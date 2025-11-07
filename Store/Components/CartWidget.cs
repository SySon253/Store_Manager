using Microsoft.AspNetCore.Mvc;
using Store.Infrastructure;
using Store.Models;

namespace Store.Components
{
    public class CartWidget : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View(HttpContext.Session.GetJson<Cart>("cart"));
        }
    }
}
