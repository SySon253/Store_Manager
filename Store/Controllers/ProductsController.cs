using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Store.Data;
using Store.Models;
using Store.Models.ViewModels;

namespace Store.Controllers
{
    public class ProductsController : Controller
    {
        private readonly StoreContext _context;
        private readonly ICompositeViewEngine _viewEngine;
        public int PageSize = 9; // Mặc định mỗi trang 9 sản phẩm

        public ProductsController(StoreContext context, ICompositeViewEngine viewEngine)
        {
            _context = context;
            _viewEngine = viewEngine;
        }

        // Class hỗ trợ lọc giá
        public class PriceRange
        {
            public int Min { get; set; }
            public int Max { get; set; }
        }

        // Class nhận dữ liệu filter từ client
        public class FilterData
        {
            public List<string> PriceRanges { get; set; }
            public List<string> Brands { get; set; }

            public int Page { get; set; } = 1;      // Trang hiện tại
            public int PageSize { get; set; } = 9;  // Số sản phẩm mỗi trang
        }

        private string RenderRazorViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = _viewEngine.FindView(ControllerContext, viewName, false);
                if (!viewResult.Success)
                {
                    throw new FileNotFoundException($"View {viewName} không tìm thấy.");
                }

                var viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    ViewData,
                    TempData,
                    sw,
                    new HtmlHelperOptions()
                );

                viewResult.View.RenderAsync(viewContext).Wait();
                return sw.GetStringBuilder().ToString();
            }
        }





        // GET: Products
        public async Task<IActionResult> Index()
        {
            var storeContext = _context.Products.Include(p => p.Category).Include(p => p.Brand);
            return View(await storeContext.ToListAsync());
        }
        //GET: Products/Shop------------
        public async Task<IActionResult> Shop(int productPage = 1)
        {
            

            return View(
                new ProductListViewModel
                {
                    Products = _context.Products
                    .Include(p => p.Brand) // ✅ nên Include nếu cần dùng Brand trong View
                    .Include(p => p.Category) // (tùy bạn có Category hay không)
                    .Skip((productPage - 1) * PageSize)
                    .Take(PageSize),
                    PagingInfo = new PagingInfo
                    {
                        ItemsPerPage = PageSize,
                        CurrentPage = productPage,
                        TotalItems = _context.Products.Count()
                    }
                }
                );
        }

        [HttpPost]
        public IActionResult GetProducts([FromBody] FilterSearchData filter)
        {
            var productsQuery = _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(filter.Keywords))
                productsQuery = productsQuery.Where(p => p.ProductName.Contains(filter.Keywords));

            // Brand filter
            if (filter.Brands != null && filter.Brands.Count > 0 && !filter.Brands.Contains("all"))
                productsQuery = productsQuery.Where(p => filter.Brands.Contains(p.Brand.BrandName));

            // Load all trước khi lọc giá
            var products = productsQuery.ToList();

            // Price filter (lọc trong bộ nhớ)
            if (filter.PriceRanges != null && filter.PriceRanges.Count > 0 && !filter.PriceRanges.Contains("all"))
            {
                var priceRanges = new List<(decimal Min, decimal Max)>();
                foreach (var range in filter.PriceRanges)
                {
                    var parts = range.Split('-');
                    if (parts.Length == 2 &&
                        decimal.TryParse(parts[0], out decimal min) &&
                        decimal.TryParse(parts[1], out decimal max))
                    {
                        priceRanges.Add((min, max));
                    }
                }

                if (priceRanges.Any())
                {
                    products = products.Where(p =>
                    {
                        decimal discountValue = 0;

                        // Nếu null thì gán 0
                        if (p.ProductDiscount != null)
                        {
                            // Ép kiểu về decimal
                            var d = Convert.ToDecimal(p.ProductDiscount);
                            // Nếu > 1 thì coi là phần trăm (20 = 20%), chia cho 100
                            discountValue = d > 1 ? d / 100 : d;
                        }

                        decimal finalPrice = p.ProductPrice * (1 - discountValue);

                        return priceRanges.Any(r => finalPrice >= r.Min && finalPrice <= r.Max);
                    }).ToList();
                }

            }

            int totalProducts = products.Count();
            int totalPages = (int)Math.Ceiling(totalProducts / (double)filter.PageSize);

            var pagedProducts = products
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            var html = RenderRazorViewToString("_ReturnProducts", pagedProducts);

            return Json(new { productsHtml = html, totalPages });
        }

        public class FilterSearchData
        {
            public string Keywords { get; set; }
            public List<string> PriceRanges { get; set; } = new List<string>();
            public List<string> Brands { get; set; } = new List<string>();
            public int Page { get; set; } = 1;
            public int PageSize { get; set; } = 9;
        }


        // =================
        [HttpPost]
        public IActionResult GetProductsByCategory([FromBody] CategoryFilterRequest request)
        {
            var productsQuery = _context.Products
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .AsQueryable();

            // Lọc theo Category nếu có
            if (request.CategoryId > 0)
                productsQuery = productsQuery.Where(p => p.CategoryId == request.CategoryId);

            // Search (từ khóa)
            if (!string.IsNullOrWhiteSpace(request.Keywords))
                productsQuery = productsQuery.Where(p => p.ProductName.Contains(request.Keywords));

            // Lọc theo Brand (an toàn với Brand null)
            if (request.Brands != null && request.Brands.Count > 0 && !request.Brands.Contains("all"))
                productsQuery = productsQuery.Where(p => request.Brands.Contains(p.Brand.BrandName));


            // Load vào memory để xử lý giá (nếu cần)
            var products = productsQuery.ToList();

            // Lọc theo PriceRanges (final price sau discount)
            if (request.PriceRanges != null && request.PriceRanges.Count > 0 && !request.PriceRanges.Contains("all"))
            {
                var priceRanges = new List<(decimal Min, decimal Max)>();
                foreach (var range in request.PriceRanges)
                {
                    var parts = range.Split('-');
                    if (parts.Length == 2 &&
                        decimal.TryParse(parts[0], out decimal min) &&
                        decimal.TryParse(parts[1], out decimal max))
                    {
                        priceRanges.Add((min, max));
                    }
                }

                if (priceRanges.Any())
                {
                    products = products.Where(p =>
                    {
                        decimal discountValue = 0;

                        // Nếu null thì gán 0
                        if (p.ProductDiscount != null)
                        {
                            // Ép kiểu về decimal
                            var d = Convert.ToDecimal(p.ProductDiscount);
                            // Nếu > 1 thì coi là phần trăm (20 = 20%), chia cho 100
                            discountValue = d > 1 ? d / 100 : d;
                        }

                        decimal finalPrice = p.ProductPrice * (1 - discountValue);

                        return priceRanges.Any(r => finalPrice >= r.Min && finalPrice <= r.Max);
                    }).ToList();
                }

            }

            int totalProducts = products.Count();
            int totalPages = (int)Math.Ceiling(totalProducts / (double)request.PageSize);

            var pagedProducts = products
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var html = RenderRazorViewToString("_ReturnProducts", pagedProducts);

            return Json(new { productsHtml = html, totalPages });
        }
        public class CategoryFilterRequest
        {
            public int CategoryId { get; set; }
            public string Keywords { get; set; } 
            public List<string> PriceRanges { get; set; } = new List<string>();
            public List<string> Brands { get; set; } = new List<string>();
            public int Page { get; set; } = 1;
            public int PageSize { get; set; } = 9;
        }

        //=========================

        public async Task<IActionResult> ProductByCart(int categoryId, int productPage = 1)
        {
            int pageSize = 9;

            var productsQuery = _context.Products
                .Where(p => p.CategoryId == categoryId)
                .Include(p => p.Category)
                .Include(p => p.Brand);

            var count = await productsQuery.CountAsync();

            var products = await productsQuery
                .OrderBy(p => p.ProductId)
                .Skip((productPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new ProductListViewModel
            {
                Products = products,
                PagingInfo = new PagingInfo
                {
                    CurrentPage = productPage,
                    ItemsPerPage = pageSize,
                    TotalItems = count
                },
                CurrentCategory = categoryId
            };

            return View(viewModel);
        }

       
        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Lấy sản phẩm (giống code cũ của bạn)
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .FirstOrDefaultAsync(m => m.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            // Lấy danh sách review của sản phẩm
            var reviews = await _context.Reviews
                .Where(r => r.ProductId == id)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();

            // Gộp vào ViewModel
            var model = new ProductDetailViewModel
            {
                Product = product,
                Reviews = reviews
            };

            return View(model);
        }
        
        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> AddReview(ProductDetailViewModel model)
        {
            if (ModelState.IsValid)
            {
                var review = model.NewReview;
                review.UserName = User.Identity?.Name ?? "Anonymous";
                review.CreatedDate = DateTime.Now;

                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Details), new { id = review.ProductId });
            }

            // Nếu lỗi form, nạp lại dữ liệu
            model.Product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .FirstOrDefaultAsync(p => p.ProductId == model.Product.ProductId);

            model.Reviews = await _context.Reviews
                .Where(r => r.ProductId == model.Product.ProductId)
                .ToListAsync();

            return View("Details", model);
        }



    }
}
