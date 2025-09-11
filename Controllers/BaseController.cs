using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTinTuc.Models;

namespace WebTinTuc.Controllers
{
    public class BaseController : Controller
    {
        protected readonly WebTinTucContext _context;

        public BaseController(WebTinTucContext context)
        {
            _context = context;
        }

        protected async Task LoadChuDesForLayout()
        {
            var chuDes = await _context.ChuDes
                .Where(c => c.DaKichHoat == true)
                .OrderBy(c => c.ThuTuHienThi)
                .ToListAsync();
            
            ViewBag.ChuDes = chuDes;
        }
    }
}
