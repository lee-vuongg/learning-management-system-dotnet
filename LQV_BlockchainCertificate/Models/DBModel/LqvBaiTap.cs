using System;
using System.Collections.Generic;

namespace LQV_BlockchainCertificate.Models.DBModel;

public partial class LqvBaiTap
{
    public int LqvBaiTapId { get; set; }

    public int LqvLopHocId { get; set; }

    public int LqvGiangVienId { get; set; }

    public string LqvTieuDe { get; set; } = null!;

    public string? LqvMoTa { get; set; }

    public DateTime LqvHanNop { get; set; }

    public DateTime LqvNgayTao { get; set; }

    public string? LqvTrangThai { get; set; }

    public virtual LqvNguoiDung LqvGiangVien { get; set; } = null!;

    public virtual LqvLopHoc LqvLopHoc { get; set; } = null!;

    public virtual ICollection<LqvNopBaiTap> LqvNopBaiTaps { get; set; } = new List<LqvNopBaiTap>();
}