using System;
using System.Collections.Generic;

namespace WebTinTuc.Models;

public partial class NguoiDung
{
    public int IdnguoiDung { get; set; }

    public string HoTen { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? SoDienThoai { get; set; }

    public string MatKhauHash { get; set; } = null!;

    public string? UrlanhDaiDien { get; set; }

    public DateTime? NgayDangKy { get; set; }

    public DateTime? LanDangNhapCuoi { get; set; }

    public bool? DaKichHoat { get; set; }

    public int IdquyenHan { get; set; }

    public virtual ICollection<BaiViet> BaiVietIdnguoiDuyetNavigations { get; set; } = new List<BaiViet>();

    public virtual ICollection<BaiViet> BaiVietIdtacGiaNavigations { get; set; } = new List<BaiViet>();

    public virtual ICollection<BinhLuan> BinhLuans { get; set; } = new List<BinhLuan>();

    public virtual QuyenHan IdquyenHanNavigation { get; set; } = null!;
}
