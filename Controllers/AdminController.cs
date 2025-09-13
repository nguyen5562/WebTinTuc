using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTinTuc.Models;
using System.Security.Cryptography;
using System.Text;

namespace WebTinTuc.Controllers
{
    public class AdminController : BaseController
    {
        private readonly WebTinTucContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(WebTinTucContext context, ILogger<AdminController> logger) : base(context)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin - Trang chủ quản trị
        public async Task<IActionResult> Index()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Quản trị")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này!";
                return RedirectToAction("Index", "Home");
            }

            // Load chủ đề cho header và footer
            await LoadChuDesForLayout();

            // Lấy dữ liệu thống kê
            var totalArticles = await _context.BaiViets.CountAsync();
            var publishedArticles = await _context.BaiViets.CountAsync(b => b.IdtrangThai == 3); // Đã xuất bản
            var totalUsers = await _context.NguoiDungs.CountAsync();
            var totalTopics = await _context.ChuDes.CountAsync();

            ViewBag.TotalArticles = totalArticles;
            ViewBag.PublishedArticles = publishedArticles;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalTopics = totalTopics;

            return View();
        }

        // GET: Admin/ChuDe - Quản lý chủ đề
        public async Task<IActionResult> ChuDe()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Quản trị")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này!";
                return RedirectToAction("Index", "Home");
            }

            // Load chủ đề cho header và footer
            await LoadChuDesForLayout();

            var chuDes = await _context.ChuDes
                .Include(c => c.BaiViets)
                .OrderBy(c => c.ThuTuHienThi ?? 999)
                .ThenBy(c => c.TenChuDe)
                .ToListAsync();

            return View(chuDes);
        }

        // GET: Admin/ChuDe/Create - Tạo chủ đề mới
        public async Task<IActionResult> CreateChuDe()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Quản trị")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này!";
                return RedirectToAction("Index", "Home");
            }

            // Load chủ đề cho header và footer
            await LoadChuDesForLayout();

            return View();
        }

        // POST: Admin/ChuDe/Create - Xử lý tạo chủ đề mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateChuDe(ChuDe chuDe)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Quản trị")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền thực hiện hành động này!";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                if (ModelState.IsValid)
                {
                    // Kiểm tra tên chủ đề đã tồn tại chưa
                    var existingChuDe = await _context.ChuDes
                        .FirstOrDefaultAsync(c => c.TenChuDe.ToLower() == chuDe.TenChuDe.ToLower());
                    
                    if (existingChuDe != null)
                    {
                        ModelState.AddModelError("TenChuDe", "Tên chủ đề đã tồn tại!");
                        return View(chuDe);
                    }

                    // Tự động tạo slug nếu chưa có
                    if (string.IsNullOrEmpty(chuDe.Slug))
                    {
                        chuDe.Slug = chuDe.TenChuDe.ToLower()
                            .Replace(" ", "-")
                            .Replace("đ", "d")
                            .Replace("Đ", "D")
                            .Replace("á", "a")
                            .Replace("à", "a")
                            .Replace("ả", "a")
                            .Replace("ã", "a")
                            .Replace("ạ", "a")
                            .Replace("ă", "a")
                            .Replace("ắ", "a")
                            .Replace("ằ", "a")
                            .Replace("ẳ", "a")
                            .Replace("ẵ", "a")
                            .Replace("ặ", "a")
                            .Replace("â", "a")
                            .Replace("ấ", "a")
                            .Replace("ầ", "a")
                            .Replace("ẩ", "a")
                            .Replace("ẫ", "a")
                            .Replace("ậ", "a")
                            .Replace("é", "e")
                            .Replace("è", "e")
                            .Replace("ẻ", "e")
                            .Replace("ẽ", "e")
                            .Replace("ẹ", "e")
                            .Replace("ê", "e")
                            .Replace("ế", "e")
                            .Replace("ề", "e")
                            .Replace("ể", "e")
                            .Replace("ễ", "e")
                            .Replace("ệ", "e")
                            .Replace("í", "i")
                            .Replace("ì", "i")
                            .Replace("ỉ", "i")
                            .Replace("ĩ", "i")
                            .Replace("ị", "i")
                            .Replace("ó", "o")
                            .Replace("ò", "o")
                            .Replace("ỏ", "o")
                            .Replace("õ", "o")
                            .Replace("ọ", "o")
                            .Replace("ô", "o")
                            .Replace("ố", "o")
                            .Replace("ồ", "o")
                            .Replace("ổ", "o")
                            .Replace("ỗ", "o")
                            .Replace("ộ", "o")
                            .Replace("ơ", "o")
                            .Replace("ớ", "o")
                            .Replace("ờ", "o")
                            .Replace("ở", "o")
                            .Replace("ỡ", "o")
                            .Replace("ợ", "o")
                            .Replace("ú", "u")
                            .Replace("ù", "u")
                            .Replace("ủ", "u")
                            .Replace("ũ", "u")
                            .Replace("ụ", "u")
                            .Replace("ư", "u")
                            .Replace("ứ", "u")
                            .Replace("ừ", "u")
                            .Replace("ử", "u")
                            .Replace("ữ", "u")
                            .Replace("ự", "u")
                            .Replace("ý", "y")
                            .Replace("ỳ", "y")
                            .Replace("ỷ", "y")
                            .Replace("ỹ", "y")
                            .Replace("ỵ", "y");
                    }
                    
                    // Set giá trị mặc định cho ThuTuHienThi nếu chưa có
                    if (chuDe.ThuTuHienThi == null)
                    {
                        var maxOrder = await _context.ChuDes.MaxAsync(c => (int?)c.ThuTuHienThi) ?? 0;
                        chuDe.ThuTuHienThi = maxOrder + 1;
                    }
                    
                    // Set giá trị mặc định cho DaKichHoat nếu chưa có
                    if (chuDe.DaKichHoat == null)
                    {
                        chuDe.DaKichHoat = true;
                    }
                    
                    _context.Add(chuDe);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Admin đã tạo chủ đề mới: {TenChuDe}", chuDe.TenChuDe);
                    TempData["SuccessMessage"] = "Tạo chủ đề thành công!";
                    return RedirectToAction("ChuDe");
                }

                return View(chuDe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo chủ đề mới");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tạo chủ đề!";
                return View(chuDe);
            }
        }

        // GET: Admin/ChuDe/Edit - Chỉnh sửa chủ đề
        public async Task<IActionResult> EditChuDe(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Quản trị")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này!";
                return RedirectToAction("Index", "Home");
            }

            // Load chủ đề cho header và footer
            await LoadChuDesForLayout();

            var chuDe = await _context.ChuDes
                .Include(c => c.BaiViets)
                .FirstOrDefaultAsync(c => c.IdchuDe == id);
            
            if (chuDe == null)
            {
                TempData["ErrorMessage"] = "Chủ đề không tồn tại!";
                return RedirectToAction("ChuDe");
            }

            ViewBag.BaiVietCount = chuDe.BaiViets.Count;
            return View(chuDe);
        }

        // POST: Admin/ChuDe/Edit - Xử lý chỉnh sửa chủ đề
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditChuDe(int id, ChuDe chuDe)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Quản trị")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền thực hiện hành động này!";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                if (id != chuDe.IdchuDe)
                {
                    TempData["ErrorMessage"] = "ID không khớp!";
                    return RedirectToAction("ChuDe");
                }

                if (ModelState.IsValid)
                {
                    // Kiểm tra tên chủ đề đã tồn tại chưa (trừ chủ đề hiện tại)
                    var existingChuDe = await _context.ChuDes
                        .FirstOrDefaultAsync(c => c.TenChuDe.ToLower() == chuDe.TenChuDe.ToLower() && c.IdchuDe != id);
                    
                    if (existingChuDe != null)
                    {
                        ModelState.AddModelError("TenChuDe", "Tên chủ đề đã tồn tại!");
                        ViewBag.BaiVietCount = await _context.BaiViets.CountAsync(b => b.IdchuDe == id);
                        return View(chuDe);
                    }

                    // Cập nhật chỉ các trường cần thiết (không bao gồm DaKichHoat)
                    var chuDeToUpdate = await _context.ChuDes.FindAsync(id);
                    if (chuDeToUpdate != null)
                    {
                        chuDeToUpdate.TenChuDe = chuDe.TenChuDe;
                        chuDeToUpdate.Slug = chuDe.Slug;
                        chuDeToUpdate.ThuTuHienThi = chuDe.ThuTuHienThi;
                        // Không cập nhật DaKichHoat - chỉ quản lý qua toggle button
                    }
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Admin đã cập nhật chủ đề: {TenChuDe}", chuDe.TenChuDe);
                    TempData["SuccessMessage"] = "Cập nhật chủ đề thành công!";
                    return RedirectToAction("ChuDe");
                }

                ViewBag.BaiVietCount = await _context.BaiViets.CountAsync(b => b.IdchuDe == id);
                return View(chuDe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật chủ đề {Id}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật chủ đề!";
                ViewBag.BaiVietCount = await _context.BaiViets.CountAsync(b => b.IdchuDe == id);
                return View(chuDe);
            }
        }

        // POST: Admin/ChuDe/Delete - Xóa chủ đề
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteChuDe(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Quản trị")
            {
                return Json(new { success = false, message = "Bạn không có quyền thực hiện hành động này!" });
            }

            try
            {
                var chuDe = await _context.ChuDes
                    .Include(c => c.BaiViets)
                    .FirstOrDefaultAsync(c => c.IdchuDe == id);

                if (chuDe == null)
                {
                    return Json(new { success = false, message = "Chủ đề không tồn tại!" });
                }

                // Kiểm tra xem chủ đề có bài viết không
                if (chuDe.BaiViets.Any())
                {
                    return Json(new { success = false, message = "Không thể xóa chủ đề có bài viết! Hãy xóa hoặc chuyển các bài viết trước." });
                }

                _context.ChuDes.Remove(chuDe);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Admin đã xóa chủ đề: {TenChuDe}", chuDe.TenChuDe);
                return Json(new { success = true, message = "Xóa chủ đề thành công!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa chủ đề {Id}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa chủ đề!" });
            }
        }

        // GET: Admin/NguoiDung - Quản lý người dùng
        public async Task<IActionResult> NguoiDung()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Quản trị")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này!";
                return RedirectToAction("Index", "Home");
            }

            // Load chủ đề cho header và footer
            await LoadChuDesForLayout();

            var nguoiDungs = await _context.NguoiDungs
                .Include(n => n.IdquyenHanNavigation)
                .Include(n => n.BaiVietIdtacGiaNavigations)
                .OrderBy(n => n.HoTen)
                .ToListAsync();

            return View(nguoiDungs);
        }

        // POST: Admin/NguoiDung/ToggleStatus - Toggle trạng thái người dùng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleNguoiDungStatus(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Quản trị")
            {
                return Json(new { success = false, message = "Bạn không có quyền thực hiện hành động này!" });
            }

            try
            {
                var nguoiDung = await _context.NguoiDungs.FindAsync(id);
                if (nguoiDung == null)
                {
                    return Json(new { success = false, message = "Người dùng không tồn tại!" });
                }

                // Không cho phép vô hiệu hóa chính mình
                var currentUserId = HttpContext.Session.GetString("UserId");
                if (currentUserId == id.ToString())
                {
                    return Json(new { success = false, message = "Không thể vô hiệu hóa tài khoản của chính mình!" });
                }

                nguoiDung.DaKichHoat = !nguoiDung.DaKichHoat;
                await _context.SaveChangesAsync();

                var message = nguoiDung.DaKichHoat == true ? "Đã kích hoạt tài khoản" : "Đã vô hiệu hóa tài khoản";
                _logger.LogInformation("Admin đã {Action} người dùng: {HoTen}", message, nguoiDung.HoTen);
                
                return Json(new { success = true, message = message, isActive = nguoiDung.DaKichHoat });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thay đổi trạng thái người dùng {Id}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra!" });
            }
        }

        // POST: Admin/NguoiDung/ChangeRole - Thay đổi quyền hạn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeNguoiDungRole(int id, int newRoleId)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Quản trị")
            {
                return Json(new { success = false, message = "Bạn không có quyền thực hiện hành động này!" });
            }

            try
            {
                var nguoiDung = await _context.NguoiDungs.FindAsync(id);
                if (nguoiDung == null)
                {
                    return Json(new { success = false, message = "Người dùng không tồn tại!" });
                }

                // Không cho phép thay đổi quyền của chính mình
                var currentUserId = HttpContext.Session.GetString("UserId");
                if (currentUserId == id.ToString())
                {
                    return Json(new { success = false, message = "Không thể thay đổi quyền hạn của chính mình!" });
                }

                var oldRole = nguoiDung.IdquyenHanNavigation?.TenQuyenHan ?? "Không xác định";
                nguoiDung.IdquyenHan = newRoleId;
                await _context.SaveChangesAsync();

                var newRole = await _context.QuyenHans.FindAsync(newRoleId);
                var newRoleName = newRole?.TenQuyenHan ?? "Không xác định";

                _logger.LogInformation("Admin đã thay đổi quyền hạn của {HoTen} từ {OldRole} thành {NewRole}", 
                    nguoiDung.HoTen, oldRole, newRoleName);
                
                return Json(new { success = true, message = $"Đã thay đổi quyền hạn thành: {newRoleName}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thay đổi quyền hạn người dùng {Id}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra!" });
            }
        }

        // POST: Admin/ChuDe/UpdateOrder - Cập nhật thứ tự hiển thị
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrder(string orders)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Quản trị")
            {
                return Json(new { success = false, message = "Bạn không có quyền thực hiện hành động này!" });
            }

            try
            {
                var orderList = System.Text.Json.JsonSerializer.Deserialize<List<dynamic>>(orders);
                
                foreach (var item in orderList)
                {
                    var id = (int)item.GetProperty("id").GetInt32();
                    var order = (int)item.GetProperty("order").GetInt32();
                    
                    var chuDe = await _context.ChuDes.FindAsync(id);
                    if (chuDe != null)
                    {
                        chuDe.ThuTuHienThi = order;
                    }
                }
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Admin đã cập nhật thứ tự hiển thị chủ đề");
                return Json(new { success = true, message = "Đã cập nhật thứ tự hiển thị!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật thứ tự hiển thị chủ đề");
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật thứ tự!" });
            }
        }

        // GET: Admin/CreateNguoiDung
        public async Task<IActionResult> CreateNguoiDung()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Quản trị")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này!";
                return RedirectToAction("Index", "Home");
            }

            await LoadChuDesForLayout();

            // Lấy danh sách quyền hạn
            var quyenHans = await _context.QuyenHans.ToListAsync();
            ViewBag.QuyenHans = quyenHans;

            return View();
        }

        // POST: Admin/CreateNguoiDung
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateNguoiDung(string hoTen, string email, string password, string confirmPassword, string soDienThoai, string urlAnhDaiDien, int idquyenHan)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Quản trị")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền thực hiện hành động này!";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                if (ModelState.IsValid)
                {
                    // Validation
                    if (string.IsNullOrEmpty(hoTen) || hoTen.Trim().Length < 2)
                    {
                        TempData["ErrorMessage"] = "Họ tên phải có ít nhất 2 ký tự!";
                        return RedirectToAction("CreateNguoiDung");
                    }

                    if (string.IsNullOrEmpty(email) || !IsValidEmail(email))
                    {
                        TempData["ErrorMessage"] = "Vui lòng nhập email hợp lệ!";
                        return RedirectToAction("CreateNguoiDung");
                    }

                    if (string.IsNullOrEmpty(password) || password.Length < 6)
                    {
                        TempData["ErrorMessage"] = "Mật khẩu phải có ít nhất 6 ký tự!";
                        return RedirectToAction("CreateNguoiDung");
                    }

                    if (password != confirmPassword)
                    {
                        TempData["ErrorMessage"] = "Mật khẩu xác nhận không khớp!";
                        return RedirectToAction("CreateNguoiDung");
                    }

                    // Kiểm tra email đã tồn tại chưa
                    var existingEmail = await _context.NguoiDungs
                        .FirstOrDefaultAsync(u => u.Email == email);
                    if (existingEmail != null)
                    {
                        TempData["ErrorMessage"] = "Email này đã được sử dụng!";
                        return RedirectToAction("CreateNguoiDung");
                    }

                    // Kiểm tra số điện thoại đã tồn tại chưa (nếu có)
                    if (!string.IsNullOrEmpty(soDienThoai))
                    {
                        var existingPhone = await _context.NguoiDungs
                            .FirstOrDefaultAsync(u => u.SoDienThoai == soDienThoai);
                        if (existingPhone != null)
                        {
                            TempData["ErrorMessage"] = "Số điện thoại này đã được sử dụng!";
                            return RedirectToAction("CreateNguoiDung");
                        }
                    }

                    // Tạo user mới
                    var newUser = new NguoiDung
                    {
                        HoTen = hoTen.Trim(),
                        Email = email.Trim(),
                        MatKhauHash = HashPassword(password),
                        SoDienThoai = string.IsNullOrEmpty(soDienThoai) ? null : soDienThoai.Trim(),
                        UrlanhDaiDien = string.IsNullOrEmpty(urlAnhDaiDien) ? null : urlAnhDaiDien.Trim(),
                        IdquyenHan = idquyenHan,
                        NgayDangKy = DateTime.Now,
                        DaKichHoat = true
                    };

                    _context.NguoiDungs.Add(newUser);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Tạo người dùng thành công!";
                    return RedirectToAction("NguoiDung");
                }

                // Lấy danh sách quyền hạn cho view
                var quyenHans = await _context.QuyenHans.ToListAsync();
                ViewBag.QuyenHans = quyenHans;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo người dùng mới");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tạo người dùng!";
                return RedirectToAction("CreateNguoiDung");
            }
        }

        // GET: Admin/EditNguoiDung
        public async Task<IActionResult> EditNguoiDung(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Quản trị")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này!";
                return RedirectToAction("Index", "Home");
            }

            await LoadChuDesForLayout();

            var nguoiDung = await _context.NguoiDungs
                .Include(u => u.IdquyenHanNavigation)
                .FirstOrDefaultAsync(u => u.IdnguoiDung == id);

            if (nguoiDung == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng!";
                return RedirectToAction("NguoiDung");
            }

            // Lấy danh sách quyền hạn
            var quyenHans = await _context.QuyenHans.ToListAsync();
            ViewBag.QuyenHans = quyenHans;

            return View(nguoiDung);
        }

        // POST: Admin/EditNguoiDung
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditNguoiDung(int id, string hoTen, string soDienThoai, string urlAnhDaiDien, int idquyenHan, bool daKichHoat)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Quản trị")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền thực hiện hành động này!";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var nguoiDung = await _context.NguoiDungs
                    .FirstOrDefaultAsync(u => u.IdnguoiDung == id);

                if (nguoiDung == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy người dùng!";
                    return RedirectToAction("NguoiDung");
                }

                // Validation
                if (string.IsNullOrEmpty(hoTen) || hoTen.Trim().Length < 2)
                {
                    TempData["ErrorMessage"] = "Họ tên phải có ít nhất 2 ký tự!";
                    return RedirectToAction("EditNguoiDung", new { id });
                }


                // Kiểm tra số điện thoại đã tồn tại chưa (nếu có)
                if (!string.IsNullOrEmpty(soDienThoai))
                {
                    var existingPhone = await _context.NguoiDungs
                        .FirstOrDefaultAsync(u => u.SoDienThoai == soDienThoai && u.IdnguoiDung != id);
                    if (existingPhone != null)
                    {
                        TempData["ErrorMessage"] = "Số điện thoại này đã được sử dụng!";
                        return RedirectToAction("EditNguoiDung", new { id });
                    }
                }

                // Cập nhật thông tin (không thay đổi email)
                nguoiDung.HoTen = hoTen.Trim();
                nguoiDung.SoDienThoai = string.IsNullOrEmpty(soDienThoai) ? null : soDienThoai.Trim();
                nguoiDung.UrlanhDaiDien = string.IsNullOrEmpty(urlAnhDaiDien) ? null : urlAnhDaiDien.Trim();
                nguoiDung.IdquyenHan = idquyenHan;
                nguoiDung.DaKichHoat = daKichHoat;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cập nhật thông tin người dùng thành công!";
                return RedirectToAction("NguoiDung");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật người dùng {Id}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật thông tin!";
                return RedirectToAction("EditNguoiDung", new { id });
            }
        }

        // Helper method để validate email
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        // POST: Admin/ToggleChuDeStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleChuDeStatus(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Quản trị")
            {
                return Json(new { success = false, message = "Bạn không có quyền thực hiện hành động này!" });
            }

            try
            {
                var chuDe = await _context.ChuDes.FindAsync(id);
                if (chuDe == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy chủ đề!" });
                }

                // Toggle trạng thái kích hoạt
                chuDe.DaKichHoat = !chuDe.DaKichHoat;
                await _context.SaveChangesAsync();

                var message = chuDe.DaKichHoat == true ? 
                    $"Đã kích hoạt chủ đề '{chuDe.TenChuDe}'" : 
                    $"Đã tắt kích hoạt chủ đề '{chuDe.TenChuDe}'";

                _logger.LogInformation("Admin đã {Action} chủ đề: {TenChuDe}", 
                    chuDe.DaKichHoat == true ? "kích hoạt" : "tắt kích hoạt", chuDe.TenChuDe);

                return Json(new { success = true, message = message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi toggle trạng thái chủ đề {Id}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật trạng thái!" });
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
