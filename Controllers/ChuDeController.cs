using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTinTuc.Models;

namespace WebTinTuc.Controllers
{
    public class ChuDeController : BaseController
    {
        private readonly ILogger<ChuDeController> _logger;

        public ChuDeController(WebTinTucContext context, ILogger<ChuDeController> logger) : base(context)
        {
            _logger = logger;
        }

        // GET: ChuDe/Index?slug=the-gioi
        public async Task<IActionResult> Index(string slug, string search, int page = 1, int pageSize = 10)
        {
            try
            {
                // Load chủ đề cho layout
                await LoadChuDesForLayout();
                
                // Lấy chủ đề theo slug
                var chuDe = await _context.ChuDes
                    .FirstOrDefaultAsync(c => c.Slug == slug && c.DaKichHoat == true);

                if (chuDe == null)
                {
                    return NotFound();
                }

                // Query bài viết theo chủ đề
                var query = _context.BaiViets
                    .Include(b => b.IdtacGiaNavigation)
                    .Include(b => b.IdchuDeNavigation)
                    .Include(b => b.IdtrangThaiNavigation)
                    .Where(b => b.IdchuDe == chuDe.IdchuDe && 
                               b.IdtrangThaiNavigation.TenTrangThai == "Đã xuất bản");

                // Tìm kiếm theo tiêu đề, tóm tắt và nội dung nếu có
                if (!string.IsNullOrEmpty(search))
                {
                    var searchTerm = search.Trim();
                    query = query.Where(b => EF.Functions.Like(b.TieuDe, $"%{searchTerm}%") || 
                                            EF.Functions.Like(b.TomTat, $"%{searchTerm}%") || 
                                            EF.Functions.Like(b.NoiDung, $"%{searchTerm}%"));
                }

                // Đếm tổng số bài viết
                var totalItems = await query.CountAsync();

                // Phân trang
                var baiViets = await query
                    .OrderByDescending(b => b.NgayXuatBan ?? b.NgayTao)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Lấy các chủ đề khác để hiển thị sidebar
                var chuDesKhac = await _context.ChuDes
                    .Where(c => c.DaKichHoat == true && c.IdchuDe != chuDe.IdchuDe)
                    .OrderBy(c => c.ThuTuHienThi)
                    .Take(8)
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

                // Tính toán phân trang
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
                var hasPreviousPage = page > 1;
                var hasNextPage = page < totalPages;

                ViewBag.ChuDe = chuDe;
                ViewBag.ChuDesKhac = chuDesKhac;
                ViewBag.BaiVietMoiNhat = baiVietMoiNhat;
                ViewBag.Search = search;
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalItems = totalItems;
                ViewBag.HasPreviousPage = hasPreviousPage;
                ViewBag.HasNextPage = hasNextPage;
                ViewBag.PageSize = pageSize;

                return View(baiViets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi load trang chủ đề: {Slug}", slug);
                return View("Error");
            }
        }

        // GET: ChuDe/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Load chủ đề cho layout
            await LoadChuDesForLayout();

            var baiViet = await _context.BaiViets
                .Include(b => b.IdtacGiaNavigation)
                .Include(b => b.IdchuDeNavigation)
                .Include(b => b.IdtrangThaiNavigation)
                .Include(b => b.BinhLuans)
                    .ThenInclude(c => c.IdnguoiDungNavigation)
                .FirstOrDefaultAsync(b => b.IdbaiViet == id);

            if (baiViet == null)
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

            ViewBag.BaiVietLienQuan = baiVietLienQuan;

            return View(baiViet);
        }
    }
}
