using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTinTuc.Models;

namespace WebTinTuc.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, WebTinTucContext context) : base(context)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            // Load chủ đề cho layout
            await LoadChuDesForLayout();
            
            // Lấy các chủ đề đã kích hoạt
            var chuDes = await _context.ChuDes
                .Where(c => c.DaKichHoat == true)
                .OrderBy(c => c.ThuTuHienThi)
                .ToListAsync();

            // Lấy các bài viết hot (có nhãn HOT và đã xuất bản)
            var baiVietHot = await _context.BaiViets
                .Include(b => b.IdtacGiaNavigation)
                .Include(b => b.IdchuDeNavigation)
                .Include(b => b.IdtrangThaiNavigation)
                .Where(b => b.LaTinNong == true && b.IdtrangThaiNavigation.TenTrangThai == "Đã xuất bản")
                .OrderByDescending(b => b.NgayXuatBan)
                .Take(5) // Lấy tối đa 5 tin nóng
                .ToListAsync();

            // Lấy bài viết theo từng chủ đề (3-4 bài mới nhất mỗi chủ đề)
            var baiVietTheoChuDe = new Dictionary<ChuDe, List<BaiViet>>();
            
            foreach (var chuDe in chuDes)
            {
                var baiViets = await _context.BaiViets
                    .Include(b => b.IdtacGiaNavigation)
                    .Include(b => b.IdtrangThaiNavigation)
                    .Where(b => b.IdchuDe == chuDe.IdchuDe && 
                               b.IdtrangThaiNavigation.TenTrangThai == "Đã xuất bản")
                    .OrderByDescending(b => b.NgayXuatBan)
                    .Take(4)
                    .ToListAsync();
                
                if (baiViets.Any())
                {
                    baiVietTheoChuDe[chuDe] = baiViets;
                }
            }

            ViewBag.ChuDes = chuDes;
            ViewBag.BaiVietHot = baiVietHot; // Bây giờ là List<BaiViet> thay vì BaiViet
            ViewBag.BaiVietTheoChuDe = baiVietTheoChuDe;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
