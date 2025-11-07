using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Store.Data;
using Store.Models;
using Store.Service;
using System.Security.Claims;

namespace Store.Controllers
{
    [Route("User")]
    public class UsersController : Controller
    {
        private UserService userService;
        private readonly StoreContext _context;

        public UsersController(UserService _userService, StoreContext context)
        {
            userService = _userService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }
        [Route("")]
        [Route("Login")]
        public IActionResult Login()
        {
            return View("Login");
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(string useremail, string userpassword)
        {
            // Kiểm tra đăng nhập qua service
            if (userService.Login(useremail, userpassword))
            {
                // Lấy thông tin user
                var user = userService.findByEmail(useremail);

                // TẠO DANH TÍNH (CLAIMS) 
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserEmail),
                    new Claim("FullName", user.UserName ?? ""),
                    new Claim(ClaimTypes.Role, user.UserRole ?? "")
                };


                var claimsIdentity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme
                );

                // CẤU HÌNH COOKIE 
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true, // Giữ đăng nhập sau khi đóng trình duyệt
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30) // Cookie hết hạn sau 30 phút
                };

                // ĐĂNG NHẬP (ghi cookie vào trình duyệt)
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties
                );

                // Chuyển hướng đến trang Welcome
                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["Msg"] = "Đăng nhập thất bại!";
                return RedirectToAction("Login");
            }
        }
        [HttpGet]
        [Route("Logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login","User");
        }
        [HttpGet]
        [Route("Register")]
        public IActionResult Register()
        {
            return View("Register", new User());
        }
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register(User user)
        {
            // 1. Kiểm tra mật khẩu trùng khớp 
            if (user.UserPassword != user.ConfirmPassword)
            {
                TempData["Msg"] = "Password and Confirm Password do not match!";
                return RedirectToAction("Register");
            }

            //  2. Kiểm tra dữ liệu hợp lệ 
            if (ModelState.IsValid)
            {
                // 3. Mã hóa mật khẩu 
                user.UserPassword = BCrypt.Net.BCrypt.HashPassword(user.UserPassword);

                // 4. Lưu vào cơ sở dữ liệu 
                if (userService.Create(user))
                {
                    // 5. Đăng nhập tự động 
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserEmail),
                new Claim("FullName", user.UserName ?? ""),
                new Claim(ClaimTypes.Role, user.UserRole ?? "")
            };

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["Msg"] = "Registration failed!";
                    return RedirectToAction("Register");
                }
            }

            TempData["Msg"] = "Invalid data!";
            return RedirectToAction("Register");
        }
        


    }
}
