using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTinTuc.Models;

namespace WebTinTuc.Controllers
{
    public class BaiVietController : BaseController
    {
        private readonly ILogger<BaiVietController> _logger;

        public BaiVietController(WebTinTucContext context, ILogger<BaiVietController> logger) : base(context)
        {
            _logger = logger;
        }

        // GET: BaiViet/Details/slug
        public async Task<IActionResult> Details(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }

            // Load chủ đề cho layout
            await LoadChuDesForLayout();

            var userRole = HttpContext.Session.GetString("UserRole");
            var isAdmin = userRole == "Quản trị";
            
            var baiViet = await _context.BaiViets
                .Include(b => b.IdtacGiaNavigation)
                .Include(b => b.IdchuDeNavigation)
                .Include(b => b.IdtrangThaiNavigation)
                .Include(b => b.BinhLuans.Where(c => isAdmin || c.DaDuyet == true))
                    .ThenInclude(c => c.IdnguoiDungNavigation)
                .Include(b => b.BinhLuans.Where(c => isAdmin || c.DaDuyet == true))
                    .ThenInclude(c => c.InverseIdbinhLuanChaNavigation.Where(cc => isAdmin || cc.DaDuyet == true))
                        .ThenInclude(cc => cc.IdnguoiDungNavigation)
                .FirstOrDefaultAsync(b => b.Slug == slug);

            if (baiViet == null)
            {
                return NotFound();
            }

            // Chỉ hiển thị bài viết đã xuất bản
            if (baiViet.IdtrangThaiNavigation.TenTrangThai != "Đã xuất bản")
            {
                return NotFound();
            }

            // Tăng lượt xem
            baiViet.LuotXem++;
            await _context.SaveChangesAsync();

            // Lấy bài viết liên quan (cùng chủ đề)
            var baiVietLienQuan = await _context.BaiViets
                .Include(b => b.IdtacGiaNavigation)
                .Include(b => b.IdchuDeNavigation)
                .Include(b => b.IdtrangThaiNavigation)
                .Where(b => b.IdchuDe == baiViet.IdchuDe && 
                           b.IdbaiViet != baiViet.IdbaiViet &&
                           b.IdtrangThaiNavigation.TenTrangThai == "Đã xuất bản")
                .OrderByDescending(b => b.NgayXuatBan ?? b.NgayTao)
                .Take(4)
                .ToListAsync();

            // Lấy bài viết mới nhất
            var baiVietMoiNhat = await _context.BaiViets
                .Include(b => b.IdtacGiaNavigation)
                .Include(b => b.IdchuDeNavigation)
                .Include(b => b.IdtrangThaiNavigation)
                .Where(b => b.IdtrangThaiNavigation.TenTrangThai == "Đã xuất bản")
                .OrderByDescending(b => b.NgayXuatBan ?? b.NgayTao)
                .Take(5)
                .ToListAsync();

            ViewBag.BaiVietLienQuan = baiVietLienQuan;
            ViewBag.BaiVietMoiNhat = baiVietMoiNhat;

            return View(baiViet);
        }

        // POST: BaiViet/AddComment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int baiVietId, string noiDung, int? binhLuanChaId = null)
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập để bình luận!" });
                }

                if (string.IsNullOrEmpty(noiDung) || noiDung.Trim().Length < 5)
                {
                    return Json(new { success = false, message = "Nội dung bình luận phải có ít nhất 5 ký tự!" });
                }

                // Kiểm tra bài viết có tồn tại không
                var baiViet = await _context.BaiViets
                    .Include(b => b.IdtrangThaiNavigation)
                    .FirstOrDefaultAsync(b => b.IdbaiViet == baiVietId);

                if (baiViet == null || baiViet.IdtrangThaiNavigation.TenTrangThai != "Đã xuất bản")
                {
                    return Json(new { success = false, message = "Bài viết không tồn tại!" });
                }

                // Tạo bình luận mới
                var binhLuan = new BinhLuan
                {
                    IdbaiViet = baiVietId,
                    IdnguoiDung = int.Parse(userId),
                    IdbinhLuanCha = binhLuanChaId,
                    NoiDung = noiDung.Trim(),
                    NgayBinhLuan = DateTime.Now,
                    DaDuyet = true // Tự động duyệt bình luận
                };

                _context.BinhLuans.Add(binhLuan);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Bình luận đã được thêm thành công!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm bình luận cho bài viết {BaiVietId}", baiVietId);
                return Json(new { success = false, message = "Có lỗi xảy ra khi thêm bình luận!" });
            }
        }

        // POST: BaiViet/ToggleCommentStatus - Toggle trạng thái duyệt bình luận (chỉ admin)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleCommentStatus(int binhLuanId)
        {
            try
            {
                var userRole = HttpContext.Session.GetString("UserRole");
                if (userRole != "Quản trị")
                {
                    return Json(new { success = false, message = "Bạn không có quyền thực hiện hành động này!" });
                }

                var binhLuan = await _context.BinhLuans
                    .Include(b => b.IdbaiVietNavigation)
                    .Include(b => b.InverseIdbinhLuanChaNavigation) // Include bình luận con
                    .FirstOrDefaultAsync(b => b.IdbinhLuan == binhLuanId);

                if (binhLuan == null)
                {
                    return Json(new { success = false, message = "Bình luận không tồn tại!" });
                }

                // Kiểm tra nếu đang cố duyệt bình luận con mà bình luận cha bị ẩn
                if (binhLuan.IdbinhLuanCha != null)
                {
                    var binhLuanCha = await _context.BinhLuans
                        .FirstOrDefaultAsync(b => b.IdbinhLuan == binhLuan.IdbinhLuanCha);
                    
                    if (binhLuanCha != null && !binhLuanCha.DaDuyet && !binhLuan.DaDuyet)
                    {
                        return Json(new { success = false, message = "Không thể duyệt bình luận con khi bình luận cha đang bị ẩn!" });
                    }
                }

                // Toggle trạng thái duyệt
                binhLuan.DaDuyet = !binhLuan.DaDuyet;

                // Nếu ẩn bình luận cha, ẩn tất cả bình luận con
                if (!binhLuan.DaDuyet && binhLuan.IdbinhLuanCha == null)
                {
                    foreach (var binhLuanCon in binhLuan.InverseIdbinhLuanChaNavigation)
                    {
                        binhLuanCon.DaDuyet = false;
                    }
                }

                await _context.SaveChangesAsync();

                var message = binhLuan.DaDuyet ? "Đã duyệt bình luận" : "Đã ẩn bình luận";
                if (!binhLuan.DaDuyet && binhLuan.IdbinhLuanCha == null && binhLuan.InverseIdbinhLuanChaNavigation.Any())
                {
                    message += " và tất cả bình luận con";
                }
                
                return Json(new { success = true, message = message, isApproved = binhLuan.DaDuyet });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thay đổi trạng thái bình luận {BinhLuanId}", binhLuanId);
                return Json(new { success = false, message = "Có lỗi xảy ra!" });
            }
        }

        // GET: BaiViet/Create
        public async Task<IActionResult> Create()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Tác giả" && userRole != "Quản trị")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền viết bài!";
                return RedirectToAction("Index", "Home");
            }

            await LoadChuDesForLayout();

            // Lấy danh sách chủ đề
            var chuDes = await _context.ChuDes
                .Where(c => c.DaKichHoat == true)
                .OrderBy(c => c.ThuTuHienThi)
                .ToListAsync();

            ViewBag.ChuDes = chuDes;

            return View();
        }

        // POST: BaiViet/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string tieuDe, string tomTat, string noiDung, int idchuDe, string actionType, string urlanhBia)
        {
            try
            {
                var userRole = HttpContext.Session.GetString("UserRole");
                if (userRole != "Tác giả" && userRole != "Quản trị")
                {
                    return Json(new { success = false, message = "Bạn không có quyền viết bài!" });
                }

                var userId = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập!" });
                }

                // Validation
                if (string.IsNullOrEmpty(tieuDe) || tieuDe.Trim().Length < 5)
                {
                    return Json(new { success = false, message = "Tiêu đề phải có ít nhất 5 ký tự!" });
                }

                if (string.IsNullOrEmpty(noiDung) || noiDung.Trim().Length < 50)
                {
                    return Json(new { success = false, message = "Nội dung phải có ít nhất 50 ký tự!" });
                }

                // Xác định trạng thái dựa trên actionType
                int idtrangThai;
                string message;
                
                if (actionType == "publish")
                {
                    // Gửi chờ duyệt
                    idtrangThai = 2; // ID của "Chờ duyệt"
                    message = "Bài viết đã được gửi chờ duyệt!";
                }
                else
                {
                    // Lưu bản nháp
                    idtrangThai = 1; // ID của "Bản nháp"
                    message = "Bài viết đã được lưu dưới dạng bản nháp!";
                }

                // Tạo slug từ tiêu đề
                var slug = GenerateSlug(tieuDe);

                // Kiểm tra slug trùng lặp
                var existingSlug = await _context.BaiViets
                    .FirstOrDefaultAsync(b => b.Slug == slug);
                if (existingSlug != null)
                {
                    slug += "-" + DateTime.Now.ToString("yyyyMMddHHmmss");
                }

                // Tạo bài viết mới
                var baiViet = new BaiViet
                {
                    TieuDe = tieuDe.Trim(),
                    Slug = slug,
                    TomTat = string.IsNullOrEmpty(tomTat) ? null : tomTat.Trim(),
                    NoiDung = noiDung.Trim(),
                    UrlanhBia = string.IsNullOrEmpty(urlanhBia) ? null : urlanhBia.Trim(),
                    IdtacGia = int.Parse(userId),
                    IdchuDe = idchuDe,
                    IdtrangThai = idtrangThai,
                    NgayTao = DateTime.Now,
                    LaTinNong = false, // Mặc định không phải tin nóng, admin sẽ quản lý sau
                    LuotXem = 0,
                    NgayChinhSuaCuoi = DateTime.Now
                };

                _context.BaiViets.Add(baiViet);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = message, baiVietId = baiViet.IdbaiViet });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo bài viết mới");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tạo bài viết!" });
            }
        }

        // GET: BaiViet/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");

            if (userRole != "Tác giả" && userRole != "Quản trị")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền chỉnh sửa bài viết!";
                return RedirectToAction("Index", "Home");
            }

            await LoadChuDesForLayout();

            var baiViet = await _context.BaiViets
                .Include(b => b.IdtacGiaNavigation)
                .Include(b => b.IdchuDeNavigation)
                .Include(b => b.IdtrangThaiNavigation)
                .FirstOrDefaultAsync(b => b.IdbaiViet == id);

            if (baiViet == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền: Tác giả chỉ có thể sửa bài của mình
            if (userRole == "Tác giả" && baiViet.IdtacGia.ToString() != userId)
            {
                TempData["ErrorMessage"] = "Bạn chỉ có thể chỉnh sửa bài viết của mình!";
                return RedirectToAction("Index", "Home");
            }

            // Lấy danh sách chủ đề
            var chuDes = await _context.ChuDes
                .Where(c => c.DaKichHoat == true)
                .OrderBy(c => c.ThuTuHienThi)
                .ToListAsync();

            ViewBag.ChuDes = chuDes;

            return View(baiViet);
        }

        // POST: BaiViet/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string tieuDe, string tomTat, string noiDung, int idchuDe, string actionType, string urlanhBia)
        {
            try
            {
                var userRole = HttpContext.Session.GetString("UserRole");
                var userId = HttpContext.Session.GetString("UserId");

                if (userRole != "Tác giả" && userRole != "Quản trị")
                {
                    return Json(new { success = false, message = "Bạn không có quyền chỉnh sửa bài viết!" });
                }

                var baiViet = await _context.BaiViets
                    .Include(b => b.IdtacGiaNavigation)
                    .FirstOrDefaultAsync(b => b.IdbaiViet == id);

                if (baiViet == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bài viết!" });
                }

                // Kiểm tra quyền: Tác giả chỉ có thể sửa bài của mình
                if (userRole == "Tác giả" && baiViet.IdtacGia.ToString() != userId)
                {
                    return Json(new { success = false, message = "Bạn chỉ có thể chỉnh sửa bài viết của mình!" });
                }

                // Validation
                if (string.IsNullOrEmpty(tieuDe) || tieuDe.Trim().Length < 5)
                {
                    return Json(new { success = false, message = "Tiêu đề phải có ít nhất 5 ký tự!" });
                }

                if (string.IsNullOrEmpty(noiDung) || noiDung.Trim().Length < 50)
                {
                    return Json(new { success = false, message = "Nội dung phải có ít nhất 50 ký tự!" });
                }

                // Xác định trạng thái dựa trên actionType
                int idtrangThai;
                string message;
                
                if (actionType == "publish")
                {
                    // Gửi chờ duyệt
                    idtrangThai = 2; // ID của "Chờ duyệt"
                    message = "Bài viết đã được gửi chờ duyệt!";
                }
                else
                {
                    // Lưu bản nháp
                    idtrangThai = 1; // ID của "Bản nháp"
                    message = "Bài viết đã được lưu dưới dạng bản nháp!";
                }

                // Cập nhật thông tin
                baiViet.TieuDe = tieuDe.Trim();
                baiViet.TomTat = string.IsNullOrEmpty(tomTat) ? null : tomTat.Trim();
                baiViet.NoiDung = noiDung.Trim();
                baiViet.UrlanhBia = string.IsNullOrEmpty(urlanhBia) ? null : urlanhBia.Trim();
                baiViet.IdchuDe = idchuDe;
                baiViet.IdtrangThai = idtrangThai;
                // LaTinNong không được thay đổi bởi tác giả, chỉ admin mới có thể quản lý
                baiViet.NgayChinhSuaCuoi = DateTime.Now;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật bài viết {BaiVietId}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật bài viết!" });
            }
        }

        // POST: BaiViet/UploadImage
        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile upload)
        {
            try
            {
                var userRole = HttpContext.Session.GetString("UserRole");
                if (userRole != "Tác giả" && userRole != "Quản trị")
                {
                    return Json(new { error = new { message = "Bạn không có quyền upload ảnh!" } });
                }

                if (upload == null || upload.Length == 0)
                {
                    return Json(new { error = new { message = "Vui lòng chọn file ảnh!" } });
                }

                // Kiểm tra định dạng file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(upload.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return Json(new { error = new { message = "Chỉ cho phép file ảnh (JPG, PNG, GIF, WebP)!" } });
                }

                // Kiểm tra kích thước file (max 5MB)
                if (upload.Length > 5 * 1024 * 1024)
                {
                    return Json(new { error = new { message = "File ảnh không được vượt quá 5MB!" } });
                }

                // Tạo tên file unique
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "images");
                
                // Tạo thư mục nếu chưa tồn tại
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                var filePath = Path.Combine(uploadsPath, fileName);

                // Lưu file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await upload.CopyToAsync(stream);
                }

                // Trả về URL ảnh
                var imageUrl = $"/uploads/images/{fileName}";
                
                return Json(new { url = imageUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi upload ảnh");
                return Json(new { error = new { message = "Có lỗi xảy ra khi upload ảnh!" } });
            }
        }

        // GET: BaiViet/Manage
        public async Task<IActionResult> Manage()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");

            if (userRole != "Tác giả" && userRole != "Quản trị")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này!";
                return RedirectToAction("Index", "Home");
            }

            await LoadChuDesForLayout();

            var query = _context.BaiViets
                .Include(b => b.IdtacGiaNavigation)
                .Include(b => b.IdchuDeNavigation)
                .Include(b => b.IdtrangThaiNavigation)
                .AsQueryable();

            // Tác giả chỉ xem được bài viết của mình
            if (userRole == "Tác giả")
            {
                query = query.Where(b => b.IdtacGia.ToString() == userId);
            }

            var baiViets = await query
                .OrderByDescending(b => b.NgayTao)
                .ToListAsync();

            _logger.LogInformation("Manage page loaded. User: {UserId}, Role: {UserRole}, Articles count: {Count}", 
                userId, userRole, baiViets.Count);

            ViewBag.UserRole = userRole;
            ViewBag.UserId = userId;

            return View(baiViets);
        }

        // POST: BaiViet/ChangeStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, int newStatus)
        {
            try
            {
                _logger.LogInformation("ChangeStatus called. ID: {Id}, NewStatus: {NewStatus}", id, newStatus);
                
                var userRole = HttpContext.Session.GetString("UserRole");
                if (userRole != "Quản trị")
                {
                    _logger.LogWarning("Unauthorized access to ChangeStatus. UserRole: {UserRole}", userRole);
                    return Json(new { success = false, message = "Bạn không có quyền thay đổi trạng thái bài viết!" });
                }

                var baiViet = await _context.BaiViets
                    .Include(b => b.IdtrangThaiNavigation)
                    .FirstOrDefaultAsync(b => b.IdbaiViet == id);

                if (baiViet == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bài viết!" });
                }

                var oldStatus = baiViet.IdtrangThai;
                
                // Kiểm tra logic chuyển trạng thái: chỉ cho phép chuyển từ "Chờ duyệt" (ID: 2)
                if (oldStatus != 2)
                {
                    return Json(new { success = false, message = "Chỉ có thể thay đổi trạng thái bài viết đang chờ duyệt!" });
                }

                // Chỉ cho phép chuyển sang "Đã xuất bản" (ID: 3) hoặc "Bị từ chối" (ID: 4)
                if (newStatus != 3 && newStatus != 4)
                {
                    return Json(new { success = false, message = "Chỉ có thể chuyển sang trạng thái 'Đã xuất bản' hoặc 'Bị từ chối'!" });
                }

                baiViet.IdtrangThai = newStatus;
                baiViet.NgayChinhSuaCuoi = DateTime.Now;

                // Nếu chuyển sang "Đã xuất bản" và chưa có ngày xuất bản
                if (newStatus == 3 && !baiViet.NgayXuatBan.HasValue)
                {
                    baiViet.NgayXuatBan = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                var statusName = await _context.TrangThaiBaiViets
                    .Where(t => t.IdtrangThai == newStatus)
                    .Select(t => t.TenTrangThai)
                    .FirstOrDefaultAsync();

                return Json(new { success = true, message = $"Đã chuyển bài viết sang trạng thái: {statusName}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thay đổi trạng thái bài viết {BaiVietId}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi thay đổi trạng thái!" });
            }
        }

        // POST: BaiViet/ToggleHot
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleHot(int id)
        {
            try
            {
                _logger.LogInformation("ToggleHot called. ID: {Id}", id);
                
                var userRole = HttpContext.Session.GetString("UserRole");
                if (userRole != "Quản trị")
                {
                    _logger.LogWarning("Unauthorized access to ToggleHot. UserRole: {UserRole}", userRole);
                    return Json(new { success = false, message = "Bạn không có quyền đánh dấu tin nóng!" });
                }

                var baiViet = await _context.BaiViets.FindAsync(id);
                if (baiViet == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bài viết!" });
                }

                baiViet.LaTinNong = !baiViet.LaTinNong;
                baiViet.NgayChinhSuaCuoi = DateTime.Now;

                await _context.SaveChangesAsync();

                var message = baiViet.LaTinNong ? "Đã đánh dấu tin nóng" : "Đã bỏ đánh dấu tin nóng";
                return Json(new { success = true, message = message, isHot = baiViet.LaTinNong });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thay đổi trạng thái tin nóng {BaiVietId}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra!" });
            }
        }

        // Helper method để tạo slug
        private string GenerateSlug(string title)
        {
            if (string.IsNullOrEmpty(title))
                return string.Empty;

            // Chuyển về chữ thường và loại bỏ dấu
            var slug = title.ToLowerInvariant();
            
            // Thay thế các ký tự đặc biệt
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", " ").Trim();
            slug = slug.Replace(" ", "-");
            
            // Loại bỏ dấu gạch ngang liên tiếp
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");
            
            return slug;
        }
    }
}
