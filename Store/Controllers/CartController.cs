using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.Data;
using Store.Infrastructure;
using Store.Models;
using Store.Service;
using System.Security.Claims;

namespace Store.Controllers
{
    public class CartController : Controller
    {
        public Cart? Cart { get; set; }
        private readonly StoreContext _context;
        private readonly UserService _userService;
        public CartController(StoreContext context, UserService userService)
        {
            _context = context;
            _userService = userService;
        }
        public IActionResult Index()
        {
            // Lấy giỏ hàng từ session hoặc tạo mới nếu chưa có
            var cart = HttpContext.Session.GetJson<Cart>("cart") ?? new Cart();

            // Mặc định phí ship = 10
            decimal shippingFee = 10;

            // Kiểm tra mã giảm giá (nếu có trong session)
            string? code = HttpContext.Session.GetString("CouponCode");

            if (!string.IsNullOrEmpty(code) && code.Equals("ZMobile", StringComparison.OrdinalIgnoreCase))
            {
                shippingFee = 0;
            }

            // Gán giá trị cho ViewBag
            ViewBag.ShippingFee = shippingFee;  // LUÔN có giá trị 10 hoặc 0
            ViewBag.CouponCode = code;
            ViewBag.CouponMessage = TempData["CouponMessage"];

            return View("Cart", cart);
        }


        [HttpPost]
        public IActionResult ApplyCoupon(string couponCode)
        {
            var cart = HttpContext.Session.GetJson<Cart>("cart") ?? new Cart();

            if (!string.IsNullOrEmpty(couponCode) && couponCode.Trim().Equals("ZMobile", StringComparison.OrdinalIgnoreCase))
            {
                HttpContext.Session.SetString("CouponCode", "ZMobile");
                TempData["CouponMessage"] = "🎉 Mã ZMobile được áp dụng – miễn phí ship!";
            }
            else
            {
                HttpContext.Session.Remove("CouponCode");
                TempData["CouponMessage"] = "❌ Mã không hợp lệ hoặc đã hết hạn.";
            }

            HttpContext.Session.SetJson("cart", cart);

            return RedirectToAction("Index");
        }
        public IActionResult AddToCart(int productId)
        {
            Product? product = _context.Products.FirstOrDefault(p => p.ProductId == productId);
            if (product != null)
            {
                Cart = HttpContext.Session.GetJson<Cart>("cart") ?? new Cart();
                Cart.AddItem(product, 1);
                HttpContext.Session.SetJson("cart", Cart);
            }
            return RedirectToAction("Index");
        }
        public IActionResult UpdateCart(int productId)
        {
            Product? product = _context.Products.FirstOrDefault(p => p.ProductId == productId);
            if (product != null)
            {
                Cart = HttpContext.Session.GetJson<Cart>("cart") ?? new Cart();
                Cart.AddItem(product, -1);
                HttpContext.Session.SetJson("cart", Cart);
            }
            return RedirectToAction("Index");
        }
        public IActionResult RemoveFromCart(int productId)
        {
            Product? product = _context.Products.FirstOrDefault(p => p.ProductId == productId);
            if (product != null)
            {
                Cart = HttpContext.Session.GetJson<Cart>("cart");
                Cart.RemoveLine(product);
                HttpContext.Session.SetJson("cart", Cart);
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        [Authorize]
        public IActionResult Confirm(string SelectedProductIds)
        {
            
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(userEmail))
            {
                TempData["Msg"] = "Bạn cần đăng nhập để hoàn tất đơn hàng!";
                return RedirectToAction("Login", "User");
            }

            var user = _userService.findByEmail(userEmail);
            if (user == null)
            {
                TempData["Msg"] = "Không tìm thấy thông tin người dùng!";
                return RedirectToAction("Login", "User");
            }
            if (string.IsNullOrEmpty(SelectedProductIds))
                return RedirectToAction("Index");

            var ids = SelectedProductIds.Split(',')
                                        .Select(int.Parse)
                                        .ToList();

            var cart = HttpContext.Session.GetJson<Cart>("cart");
            var selectedItems = cart.Lines
                .Where((item, index) => ids.Contains(index))
                .ToList();


            TempData["Message"] = $"Bạn đã chọn {selectedItems.Count} sản phẩm để thanh toán!";
            return RedirectToAction("CheckoutSuccess");
        }



    }
}
