using Microsoft.AspNetCore.Mvc;
using Store.Data;

namespace Store.Components
{
    public class Navbar : ViewComponent
    {
        private readonly StoreContext _context;
        public Navbar(StoreContext context) { _context = context; }
        public IViewComponentResult Invoke()
        {
            return View(_context.Categories.ToList());
        }
    }
}
