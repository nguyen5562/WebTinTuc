using System;
using System.Collections.Generic;

namespace WebTinTuc.Models;

public partial class BinhLuan
{
    public int IdbinhLuan { get; set; }

    public int IdbaiViet { get; set; }

    public int IdnguoiDung { get; set; }

    public int? IdbinhLuanCha { get; set; }

    public string NoiDung { get; set; } = null!;

    public DateTime? NgayBinhLuan { get; set; }

    public bool DaDuyet { get; set; }

    public virtual BaiViet IdbaiVietNavigation { get; set; } = null!;

    public virtual BinhLuan? IdbinhLuanChaNavigation { get; set; }

    public virtual NguoiDung IdnguoiDungNavigation { get; set; } = null!;

    public virtual ICollection<BinhLuan> InverseIdbinhLuanChaNavigation { get; set; } = new List<BinhLuan>();
}
