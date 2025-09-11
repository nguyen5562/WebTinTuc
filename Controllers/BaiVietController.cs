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

        // GET: BaiViet/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
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
                .FirstOrDefaultAsync(b => b.IdbaiViet == id);

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
                    .FirstOrDefaultAsync(b => b.IdbinhLuan == binhLuanId);

                if (binhLuan == null)
                {
                    return Json(new { success = false, message = "Bình luận không tồn tại!" });
                }

                // Toggle trạng thái duyệt
                binhLuan.DaDuyet = !binhLuan.DaDuyet;
                await _context.SaveChangesAsync();

                var message = binhLuan.DaDuyet ? "Đã duyệt bình luận" : "Đã ẩn bình luận";
                return Json(new { success = true, message = message, isApproved = binhLuan.DaDuyet });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thay đổi trạng thái bình luận {BinhLuanId}", binhLuanId);
                return Json(new { success = false, message = "Có lỗi xảy ra!" });
            }
        }
    }
}
