using System;
using System.Collections.Generic;

namespace WebTinTuc.Models;

public partial class BaiViet
{
    public int IdbaiViet { get; set; }

    public string TieuDe { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? TomTat { get; set; }

    public string NoiDung { get; set; } = null!;

    public string? UrlanhBia { get; set; }

    public int IdtacGia { get; set; }

    public int IdchuDe { get; set; }

    public int IdtrangThai { get; set; }

    public DateTime? NgayTao { get; set; }

    public DateTime? NgayXuatBan { get; set; }

    public DateTime? NgayChinhSuaCuoi { get; set; }

    public bool? LaTinNong { get; set; }

    public int? LuotXem { get; set; }

    public int? IdnguoiDuyet { get; set; }

    public string? GhiChuDuyet { get; set; }

    public virtual ICollection<BinhLuan> BinhLuans { get; set; } = new List<BinhLuan>();

    public virtual ChuDe IdchuDeNavigation { get; set; } = null!;

    public virtual NguoiDung? IdnguoiDuyetNavigation { get; set; }

    public virtual NguoiDung IdtacGiaNavigation { get; set; } = null!;

    public virtual TrangThaiBaiViet IdtrangThaiNavigation { get; set; } = null!;
}
