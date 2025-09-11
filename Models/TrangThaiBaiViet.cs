using System;
using System.Collections.Generic;

namespace WebTinTuc.Models;

public partial class TrangThaiBaiViet
{
    public int IdtrangThai { get; set; }

    public string TenTrangThai { get; set; } = null!;

    public string? MoTa { get; set; }

    public virtual ICollection<BaiViet> BaiViets { get; set; } = new List<BaiViet>();
}
