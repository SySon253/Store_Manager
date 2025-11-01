using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Store.Data;
using Store.Models;

namespace Store.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly StoreContext _context;

        public CategoriesController(StoreContext context)
        {
            _context = context;
        }

        // GET: Admin/Categories
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categories.ToListAsync());
        }

        // GET: Admin/Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Admin/Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                // Xử lý upload ảnh
                if (category.ImageUpload != null)
                {
                    // Lấy tên file gốc và phần mở rộng
                    string fileName = Path.GetFileNameWithoutExtension(category.ImageUpload.FileName);
                    string extension = Path.GetExtension(category.ImageUpload.FileName);

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
                        await category.ImageUpload.CopyToAsync(fileStream);
                    }

                    // Lưu tên file ảnh vào DB
                    category.CategoryImage = fileName;
                }
                else
                {
                    // Nếu không upload ảnh, giữ mặc định là noimage.jpg
                    category.CategoryImage = "noimage.jpg";
                }
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Admin/Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Admin/Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.CategoryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy dữ liệu Category hiện có trong DB
                    var existingCategory = await _context.Categories.FindAsync(id);
                    if (existingCategory == null)
                    {
                        return NotFound();
                    }

                    // Cập nhật các thuộc tính thông thường
                    existingCategory.CategoryName = category.CategoryName;

                    // ✅ Xử lý upload ảnh mới (nếu có)
                    if (category.ImageUpload != null)
                    {
                        string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

                        if (!Directory.Exists(uploadPath))
                        {
                            Directory.CreateDirectory(uploadPath);
                        }

                        // Tạo tên file mới tránh trùng
                        string fileName = Path.GetFileNameWithoutExtension(category.ImageUpload.FileName);
                        string extension = Path.GetExtension(category.ImageUpload.FileName);
                        fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;

                        string filePath = Path.Combine(uploadPath, fileName);

                        // Ghi file ảnh mới
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await category.ImageUpload.CopyToAsync(fileStream);
                        }

                        // Xóa ảnh cũ nếu có (và không phải ảnh mặc định)
                        if (!string.IsNullOrEmpty(existingCategory.CategoryImage) && existingCategory.CategoryImage != "noimage.jpg")
                        {
                            string oldPath = Path.Combine(uploadPath, existingCategory.CategoryImage);
                            if (System.IO.File.Exists(oldPath))
                            {
                                System.IO.File.Delete(oldPath);
                            }
                        }

                        // Cập nhật tên file mới vào DB
                        existingCategory.CategoryImage = fileName;
                    }

                    // Cập nhật lại vào DB
                    _context.Update(existingCategory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Categories.Any(e => e.CategoryId == category.CategoryId))
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

            return View(category);
        }


        // GET: Admin/Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Admin/Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        
    }
}
