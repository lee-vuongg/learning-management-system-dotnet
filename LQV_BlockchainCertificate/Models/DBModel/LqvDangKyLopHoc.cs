using System;
using System.Collections.Generic;

namespace LQV_BlockchainCertificate.Models.DBModel;

public partial class LqvDangKyLopHoc
{
    public int LqvId { get; set; }

    public int LqvSinhVienId { get; set; }

    public int LqvLopHocId { get; set; }

    public DateTime? LqvNgayDangKy { get; set; }

    public virtual LqvLopHoc LqvLopHoc { get; set; } = null!;

    public virtual LqvNguoiDung LqvSinhVien { get; set; } = null!;
}