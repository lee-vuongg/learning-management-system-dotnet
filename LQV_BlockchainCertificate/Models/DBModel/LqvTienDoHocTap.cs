using System;
using System.Collections.Generic;

namespace LQV_BlockchainCertificate.Models.DBModel;

public partial class LqvTienDoHocTap
{
    public int LqvId { get; set; }

    public int LqvSinhVienId { get; set; }

    public int LqvKhoaHocId { get; set; }

    public double LqvTiLeHoanThanh { get; set; }

    public DateTime? LqvNgayCapNhat { get; set; }

    public virtual LqvKhoaHoc LqvKhoaHoc { get; set; } = null!;

    public virtual LqvNguoiDung LqvSinhVien { get; set; } = null!;
}