using System;
using System.Collections.Generic;

namespace WebTinTuc.Models;

public partial class ChuDe
{
    public int IdchuDe { get; set; }

    public string TenChuDe { get; set; } = null!;

    public string? Slug { get; set; } = null!;

    public int? ThuTuHienThi { get; set; }

    public bool? DaKichHoat { get; set; }

    public virtual ICollection<BaiViet> BaiViets { get; set; } = new List<BaiViet>();
}
