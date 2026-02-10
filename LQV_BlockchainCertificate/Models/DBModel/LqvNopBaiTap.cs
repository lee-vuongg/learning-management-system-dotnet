using System;
using System.Collections.Generic;

namespace LQV_BlockchainCertificate.Models.DBModel;

public partial class LqvNopBaiTap
{
    public int LqvId { get; set; }

    public int LqvBaiTapId { get; set; }

    public int LqvSinhVienId { get; set; }

    public string? LqvNoiDung { get; set; }

    public string? LqvFile { get; set; }

    public DateTime LqvThoiGianNop { get; set; }

    public double? LqvDiem { get; set; }

    public string? LqvNhanXet { get; set; }

    public bool LqvDaCham { get; set; }

    public virtual LqvBaiTap LqvBaiTap { get; set; } = null!;

    public virtual LqvNguoiDung LqvSinhVien { get; set; } = null!;
}