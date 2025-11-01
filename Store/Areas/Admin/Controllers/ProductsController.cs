using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Store.Data;
using Store.Models;

namespace Store.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly StoreContext _context;

        public ProductsController(StoreContext context)
        {
            _context = context;
        }

        // GET: Admin/Products
        public async Task<IActionResult> Index()
        {
            var storeContext = _context.Products.Include(p => p.Brand).Include(p => p.Category);
            return View(await storeContext.ToListAsync());
        }

        // GET: Admin/Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Admin/Products/Create
        public IActionResult Create()
        {
            ViewData["BrandId"] = new SelectList(_context.Brands, "BrandId", "BrandName");
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                // Xử lý upload ảnh
                if (product.ImageUpload != null)
                {
                    // Lấy tên file gốc và phần mở rộng
                    string fileName = Path.GetFileNameWithoutExtension(product.ImageUpload.FileName);
                    string extension = Path.GetExtension(product.ImageUpload.FileName);

                    // Tạo tên file mới tránh trùng
                    fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;

                    // Đường dẫn thư mục lưu ảnh (wwwroot/images)
                    string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

                    // Tạo thư mục nếu chưa có
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    // Đường dẫn đầy đủ tới file
                    string filePath = Path.Combine(uploadPath, fileName);

                    // Ghi file vào ổ đĩa
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await product.ImageUpload.CopyToAsync(fileStream);
                    }

                    // Lưu tên file ảnh vào DB
                    product.ProductImage = fileName;
                }
                else
                {
                    // Nếu không upload ảnh, giữ mặc định là noimage.jpg
                    product.ProductImage = "noimage.jpg";
                }

                // Lưu sản phẩm vào DB
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["BrandId"] = new SelectList(_context.Brands, "BrandId", "BrandName", product.BrandId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
            return View(product);
        }


        // GET: Admin/Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["BrandId"] = new SelectList(_context.Brands, "BrandId", "BrandName", product.BrandId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
            return View(product);
        }

        // POST: Admin/Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy bản ghi cũ trong DB
                    var existingProduct = await _context.Products.FindAsync(id);

                    if (existingProduct == null)
                    {
                        return NotFound();
                    }

                    // Cập nhật các trường thông tin
                    existingProduct.ProductName = product.ProductName;
                    existingProduct.ProductDescription = product.ProductDescription;
                    existingProduct.ProductPrice = product.ProductPrice;
                    existingProduct.ProductDiscount = product.ProductDiscount;
                    existingProduct.BrandId = product.BrandId;
                    existingProduct.CategoryId = product.CategoryId;
                    existingProduct.IsTopSelling = product.IsTopSelling;
                    existingProduct.IsArrived = product.IsArrived;

                    // ✅ Xử lý upload ảnh mới
                    if (product.ImageUpload != null)
                    {
                        string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

                        if (!Directory.Exists(uploadPath))
                        {
                            Directory.CreateDirectory(uploadPath);
                        }

                        string fileName = Path.GetFileNameWithoutExtension(product.ImageUpload.FileName);
                        string extension = Path.GetExtension(product.ImageUpload.FileName);
                        fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;

                        string filePath = Path.Combine(uploadPath, fileName);

                        // Lưu ảnh mới
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await product.ImageUpload.CopyToAsync(fileStream);
                        }

                        // Xóa ảnh cũ nếu có
                        if (!string.IsNullOrEmpty(existingProduct.ProductImage))
                        {
                            string oldPath = Path.Combine(uploadPath, existingProduct.ProductImage);
                            if (System.IO.File.Exists(oldPath))
                            {
                                System.IO.File.Delete(oldPath);
                            }
                        }

                        // Cập nhật tên file mới
                        existingProduct.ProductImage = fileName;
                    }

                    // Lưu thay đổi
                    _context.Update(existingProduct);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Products.Any(e => e.ProductId == product.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["BrandId"] = new SelectList(_context.Brands, "BrandId", "BrandName", product.BrandId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
            return View(product);
        }

        // GET: Admin/Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        
    }
}
