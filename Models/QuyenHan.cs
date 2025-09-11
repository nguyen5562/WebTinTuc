using System;
using System.Collections.Generic;

namespace WebTinTuc.Models;

public partial class QuyenHan
{
    public int IdquyenHan { get; set; }

    public string TenQuyenHan { get; set; } = null!;

    public string? MoTa { get; set; }

    public virtual ICollection<NguoiDung> NguoiDungs { get; set; } = new List<NguoiDung>();
}
