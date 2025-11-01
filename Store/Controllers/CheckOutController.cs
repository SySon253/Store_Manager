
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.Data;
using Store.Infrastructure;
using Store.Models;
using Store.Models.ViewModels;
using Store.Service;
using System.Security.Claims;

namespace Store.Controllers
{
    //[Authorize]
    [AllowAnonymous]
    public class CheckOutController : Controller
    {
        private readonly StoreContext _context;
        private readonly UserService _userService;

        public CheckOutController(StoreContext context, UserService userService)
        {
            _context = context;
            _userService = userService;
        }

        

        public IActionResult Index(string? returnUrl)
        {
            // Lấy email người dùng từ session (do bạn set sau khi login)
            var cart = HttpContext.Session.GetJson<Cart>("cart") ?? new Cart();

            if (cart.Lines == null || !cart.Lines.Any()) { TempData["Msg"] = "Giỏ hàng của bạn đang trống!"; return RedirectToAction("Index", "Checkout"); }
            return View("Checkout", cart);
        }
        


       
        [HttpPost]
        public IActionResult Confirm(CheckoutViewModel model)
        {
            var cart = HttpContext.Session.GetJson<Cart>("cart") ?? new Cart();

            if (cart == null || !cart.Lines.Any())
            {
                TempData["Msg"] = "Không thể xác nhận vì giỏ hàng trống!";
                return RedirectToAction("Index", "Cart");
            }

            // ✅ Load Product từ DB đúng theo cart
            foreach (var line in cart.Lines)
            {
                line.Product = _context.Products.FirstOrDefault(p => p.ProductId == line.ProductId);
            }

            // ✅ Lưu đơn hàng (Guest checkout)
            var order = new Order
            {
                UserId = model.User.UserId, // hoặc 0 nếu khách không có tài khoản
                OrderDate = DateTime.Now,
                TotalAmount = cart.ComputeTotalValues(),
                Status = "Đang xử lý",
                Details = cart.Lines.Select(item => new OrderDetail
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.ProductPrice * (1 - item.ProductDiscount)
                }).ToList()
            };

            _context.Orders.Add(order);


            HttpContext.Session.Remove("cart");


            HttpContext.Session.SetJson("Cart", cart);
            
            return RedirectToAction("Success", new { orderId = order.OrderId });
        }


        [HttpPost]
        public IActionResult CheckoutInfo(string SelectedProductIds, decimal TotalWithShip)
        {
            var cart = HttpContext.Session.GetJson<Cart>("cart");
            if (cart == null || !cart.Lines.Any())
                return RedirectToAction("Index", "Cart");

            // Lấy danh sách ID sản phẩm được chọn
            var ids = SelectedProductIds?
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList() ?? new List<int>();

            // Lọc ra những sản phẩm đã chọn từ giỏ hàng
            var selectedItems = cart.Lines
                .Where((item, index) => ids.Contains(index))
                .ToList();

            // Giữ lại thông tin sản phẩm được chọn nếu cần
            TempData["SelectedIds"] = SelectedProductIds;

            // ✅ Trả dữ liệu sang View bằng ViewModel
            return View(new CheckoutViewModel
            {
                Cart = new Cart { Lines = selectedItems },
                User = new User(),
                TotalAmount = TotalWithShip // ✅ Gán tổng tiền đã bao gồm ship
            });
        }

        [HttpPost]
        public IActionResult ConfirmOrder(CheckoutViewModel model, string SelectedProductIds)
        {
            var cart = HttpContext.Session.GetJson<Cart>("cart");

            var ids = SelectedProductIds.Split(',').Select(int.Parse).ToList();
            

            var selectedItems = cart.Lines.Where((item, index) => ids.Contains(index)).ToList();
            

           

            
            HttpContext.Session.Remove("cart");

            return RedirectToAction("Success");
        }

        public IActionResult Success()
        {
            return View();
        }

    }


}
