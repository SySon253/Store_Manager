using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.Data;
using Store.Models;

namespace Store.Controllers
{
    public class ReviewController : Controller
    {
        private readonly StoreContext _context;
        public ReviewController(StoreContext context)
        {
            _context = context;
        }
        [HttpPost]
        [Authorize(Roles = "User")] // chỉ User đăng nhập mới gửi được
        public IActionResult Create(Review model)
        {
            if (ModelState.IsValid)
            {
                model.UserName = User.FindFirst("UserName")?.Value ?? User.Identity.Name;
                model.CreatedDate = DateTime.Now;
                _context.Reviews.Add(model);
                _context.SaveChanges();
            }
            return RedirectToAction("Details", "Product", new { id = model.ProductId });
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
