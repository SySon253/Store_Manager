using Microsoft.AspNetCore.Mvc;
using Store.Data;

namespace Store.Components
{
    public class TopSelling : ViewComponent
    {
        private readonly StoreContext _context;
        public TopSelling(StoreContext context) { _context = context; }
        public IViewComponentResult Invoke()
        {
            return View(_context.Products.Where(p => p.IsTopSelling == true).ToList());
        }
    }
}
