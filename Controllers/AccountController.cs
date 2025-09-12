using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using WebTinTuc.Models;

namespace WebTinTuc.Controllers
{
    public class AccountController : BaseController
    {
        private readonly ILogger<AccountController> _logger;

        public AccountController(WebTinTucContext context, ILogger<AccountController> logger) : base(context)
        {
            _logger = logger;
        }

        // GET: Account/Login
        public async Task<IActionResult> Login()
        {
            if (HttpContext.Session.GetString("UserId") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            await LoadChuDesForLayout();
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ thông tin!";
                    return View();
                }

                // Hash password để so sánh
                var hashedPassword = HashPassword(password);

                // Tìm user trong database
                var user = await _context.NguoiDungs
                    .Include(u => u.IdquyenHanNavigation)
                    .FirstOrDefaultAsync(u => u.Email == email && u.MatKhauHash == hashedPassword && u.DaKichHoat == true);

                if (user == null)
                {
                    TempData["ErrorMessage"] = "Email hoặc mật khẩu không đúng!";
                    return View();
                }

                // Lưu thông tin user vào session
                HttpContext.Session.SetString("UserId", user.IdnguoiDung.ToString());
                HttpContext.Session.SetString("UserName", user.HoTen);
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetString("UserRole", user.IdquyenHanNavigation?.TenQuyenHan ?? "");

                // Cập nhật lần đăng nhập cuối
                user.LanDangNhapCuoi = DateTime.Now;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Chào mừng {user.HoTen} quay trở lại!";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đăng nhập");
                TempData["ErrorMessage"] = "Có lỗi xảy ra, vui lòng thử lại!";
                return View();
            }
        }

        // GET: Account/Register
        public async Task<IActionResult> Register()
        {
            if (HttpContext.Session.GetString("UserId") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            await LoadChuDesForLayout();
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string hoTen, string email, string soDienThoai, string password, string confirmPassword, int quyenHan = 1)
        {
            try
            {
                // Validation
                if (string.IsNullOrEmpty(hoTen) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ thông tin!";
                    return View();
                }

                if (password != confirmPassword)
                {
                    TempData["ErrorMessage"] = "Mật khẩu xác nhận không khớp!";
                    return View();
                }

                if (password.Length < 6)
                {
                    TempData["ErrorMessage"] = "Mật khẩu phải có ít nhất 6 ký tự!";
                    return View();
                }

                // Kiểm tra email đã tồn tại chưa
                var existingUser = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == email);
                if (existingUser != null)
                {
                    TempData["ErrorMessage"] = "Email này đã được sử dụng!";
                    return View();
                }

                // Kiểm tra số điện thoại đã tồn tại chưa (nếu có)
                if (!string.IsNullOrEmpty(soDienThoai))
                {
                    var existingPhone = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.SoDienThoai == soDienThoai);
                    if (existingPhone != null)
                    {
                        TempData["ErrorMessage"] = "Số điện thoại này đã được sử dụng!";
                        return View();
                    }
                }

                // Tạo user mới
                var newUser = new NguoiDung
                {
                    HoTen = hoTen,
                    Email = email,
                    SoDienThoai = soDienThoai,
                    MatKhauHash = HashPassword(password),
                    NgayDangKy = DateTime.Now,
                    DaKichHoat = true, // Tạm thời kích hoạt luôn, sau này có thể thêm email verification
                    IdquyenHan = quyenHan // 1 = Người đọc, 2 = Tác giả, 3 = Quản trị
                };

                _context.NguoiDungs.Add(newUser);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đăng ký thành công! Bạn có thể đăng nhập ngay bây giờ.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đăng ký");
                TempData["ErrorMessage"] = "Có lỗi xảy ra, vui lòng thử lại!";
                return View();
            }
        }

        // GET: Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "Bạn đã đăng xuất thành công!";
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Profile
        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login");
            }

            await LoadChuDesForLayout();

            var user = await _context.NguoiDungs
                .Include(u => u.IdquyenHanNavigation)
                .FirstOrDefaultAsync(u => u.IdnguoiDung.ToString() == userId);

            if (user == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login");
            }

            // Tính thống kê bài viết theo trạng thái
            var publishedArticlesCount = await _context.BaiViets
                .Include(b => b.IdtrangThaiNavigation)
                .CountAsync(b => b.IdtacGia.ToString() == userId && 
                                b.IdtrangThaiNavigation.TenTrangThai == "Đã xuất bản");

            var draftArticlesCount = await _context.BaiViets
                .Include(b => b.IdtrangThaiNavigation)
                .CountAsync(b => b.IdtacGia.ToString() == userId && 
                                b.IdtrangThaiNavigation.TenTrangThai == "Bản nháp");

            var pendingArticlesCount = await _context.BaiViets
                .Include(b => b.IdtrangThaiNavigation)
                .CountAsync(b => b.IdtacGia.ToString() == userId && 
                                b.IdtrangThaiNavigation.TenTrangThai == "Chờ duyệt");

            // Tính thống kê bình luận đã duyệt
            var approvedCommentsCount = await _context.BinhLuans
                .CountAsync(c => c.IdnguoiDung.ToString() == userId && c.DaDuyet == true);

            // Lấy 6 bài viết gần đây nhất
            var recentArticles = await _context.BaiViets
                .Include(b => b.IdtrangThaiNavigation)
                .Where(b => b.IdtacGia.ToString() == userId)
                .OrderByDescending(b => b.NgayTao)
                .Take(6)
                .ToListAsync();

            ViewBag.PublishedArticlesCount = publishedArticlesCount;
            ViewBag.DraftArticlesCount = draftArticlesCount;
            ViewBag.PendingArticlesCount = pendingArticlesCount;
            ViewBag.ApprovedCommentsCount = approvedCommentsCount;
            ViewBag.RecentArticles = recentArticles;

            return View(user);
        }

        // POST: Account/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(string hoTen, string soDienThoai, string urlAnhDaiDien)
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập!" });
                }

                if (string.IsNullOrEmpty(hoTen) || hoTen.Trim().Length < 2)
                {
                    return Json(new { success = false, message = "Họ tên phải có ít nhất 2 ký tự!" });
                }

                // Kiểm tra số điện thoại đã tồn tại chưa (nếu có)
                if (!string.IsNullOrEmpty(soDienThoai))
                {
                    var existingPhone = await _context.NguoiDungs
                        .FirstOrDefaultAsync(u => u.SoDienThoai == soDienThoai && u.IdnguoiDung.ToString() != userId);
                    if (existingPhone != null)
                    {
                        return Json(new { success = false, message = "Số điện thoại này đã được sử dụng!" });
                    }
                }

                // Tìm user
                var user = await _context.NguoiDungs
                    .FirstOrDefaultAsync(u => u.IdnguoiDung.ToString() == userId);

                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin người dùng!" });
                }

                // Cập nhật thông tin
                user.HoTen = hoTen.Trim();
                user.SoDienThoai = string.IsNullOrEmpty(soDienThoai) ? null : soDienThoai.Trim();
                user.UrlanhDaiDien = string.IsNullOrEmpty(urlAnhDaiDien) ? null : urlAnhDaiDien.Trim();

                await _context.SaveChangesAsync();

                // Cập nhật session
                HttpContext.Session.SetString("UserName", user.HoTen);

                return Json(new { success = true, message = "Cập nhật thông tin thành công!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật thông tin người dùng");
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật thông tin!" });
            }
        }

        // POST: Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            try
            {
                // Kiểm tra đăng nhập
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return Json(new { success = false, message = "Bạn cần đăng nhập để thực hiện chức năng này!" });
                }

                // Validation
                if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
                {
                    return Json(new { success = false, message = "Vui lòng điền đầy đủ thông tin!" });
                }

                if (newPassword.Length < 6)
                {
                    return Json(new { success = false, message = "Mật khẩu mới phải có ít nhất 6 ký tự!" });
                }

                if (newPassword != confirmPassword)
                {
                    return Json(new { success = false, message = "Mật khẩu mới và xác nhận mật khẩu không khớp!" });
                }

                // Lấy thông tin user
                var user = await _context.NguoiDungs.FindAsync(userId);
                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin người dùng!" });
                }

                // Kiểm tra mật khẩu hiện tại
                var currentPasswordHash = HashPassword(currentPassword);
                if (user.MatKhauHash != currentPasswordHash)
                {
                    return Json(new { success = false, message = "Mật khẩu hiện tại không đúng!" });
                }

                // Cập nhật mật khẩu mới
                user.MatKhauHash = HashPassword(newPassword);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User {UserId} đã đổi mật khẩu thành công", userId);
                return Json(new { success = true, message = "Đổi mật khẩu thành công!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đổi mật khẩu cho user {UserId}", HttpContext.Session.GetInt32("UserId"));
                return Json(new { success = false, message = "Có lỗi xảy ra khi đổi mật khẩu!" });
            }
        }

        // Helper method để hash password
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
