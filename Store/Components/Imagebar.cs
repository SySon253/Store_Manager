using Microsoft.AspNetCore.Mvc;
using Store.Data;

namespace Store.Components
{
    public class Imagebar : ViewComponent
    {
        private readonly StoreContext _context;
        public Imagebar(StoreContext context) { _context = context; }
        public IViewComponentResult Invoke()
        {
            return View("Index", _context.Categories.ToList());
        }
    }
}
