using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTinTuc.Models;

namespace WebTinTuc.Controllers
{
    public class SearchController : BaseController
    {
        private readonly ILogger<SearchController> _logger;

        public SearchController(WebTinTucContext context, ILogger<SearchController> logger) : base(context)
        {
            _logger = logger;
        }

        // GET: Search/Index?q=keyword
        public async Task<IActionResult> Index(string q, int page = 1, int pageSize = 10)
        {
            try
            {
                // Load chủ đề cho layout
                await LoadChuDesForLayout();

                ViewBag.SearchQuery = q;

                if (string.IsNullOrEmpty(q) || q.Trim().Length < 2)
                {
                    ViewBag.Message = "Vui lòng nhập ít nhất 2 ký tự để tìm kiếm";
                    return View(new List<BaiViet>());
                }

                var searchTerm = q.Trim();

                // Query tìm kiếm bài viết - tìm kiếm không phân biệt hoa thường
                var query = _context.BaiViets
                    .Include(b => b.IdtacGiaNavigation)
                    .Include(b => b.IdchuDeNavigation)
                    .Include(b => b.IdtrangThaiNavigation)
                    .Where(b => b.IdtrangThaiNavigation.TenTrangThai == "Đã xuất bản" &&
                               (EF.Functions.Like(b.TieuDe, $"%{searchTerm}%") || 
                                EF.Functions.Like(b.TomTat, $"%{searchTerm}%") || 
                                EF.Functions.Like(b.NoiDung, $"%{searchTerm}%")));

                // Đếm tổng số kết quả
                var totalItems = await query.CountAsync();

                // Phân trang
                var baiViets = await query
                    .OrderByDescending(b => b.NgayXuatBan ?? b.NgayTao)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Lấy các chủ đề để hiển thị sidebar
                var chuDes = await _context.ChuDes
                    .Where(c => c.DaKichHoat == true)
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

                ViewBag.ChuDes = chuDes;
                ViewBag.BaiVietMoiNhat = baiVietMoiNhat;
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalItems = totalItems;
                ViewBag.HasPreviousPage = hasPreviousPage;
                ViewBag.HasNextPage = hasNextPage;
                ViewBag.PageSize = pageSize;
                ViewBag.SearchTerm = searchTerm;

                return View(baiViets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm: {Query}", q);
                ViewBag.Message = "Có lỗi xảy ra khi tìm kiếm, vui lòng thử lại";
                return View(new List<BaiViet>());
            }
        }

        // GET: Search/Suggest?q=keyword (cho autocomplete)
        [HttpGet]
        public async Task<IActionResult> Suggest(string q)
        {
            try
            {
                if (string.IsNullOrEmpty(q) || q.Trim().Length < 2)
                {
                    return Json(new List<object>());
                }

                var searchTerm = q.Trim();

                // Tìm kiếm gợi ý từ tiêu đề bài viết
                var suggestions = await _context.BaiViets
                    .Include(b => b.IdtrangThaiNavigation)
                    .Where(b => b.IdtrangThaiNavigation.TenTrangThai == "Đã xuất bản" &&
                               b.TieuDe.Contains(searchTerm))
                    .Select(b => new
                    {
                        title = b.TieuDe,
                        url = Url.Action("Details", "ChuDe", new { id = b.IdbaiViet }),
                        category = b.IdchuDeNavigation.TenChuDe
                    })
                    .Take(5)
                    .ToListAsync();

                return Json(suggestions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm gợi ý: {Query}", q);
                return Json(new List<object>());
            }
        }
    }
}
