using Microsoft.AspNetCore.Mvc;
using Store.Data;

namespace Store.Components
{
    public class JustArrived : ViewComponent
    {
        private readonly StoreContext _context;
        public JustArrived(StoreContext context) { _context = context; }
        public IViewComponentResult Invoke()
        {
            return View(_context.Products.Where(p => p.IsArrived == true).ToList());
        }
    }
}
