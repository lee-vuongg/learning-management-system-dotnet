using System;
using System.Collections.Generic;

namespace LQV_BlockchainCertificate.Models.DBModel;

public partial class LqvDiemDanhGp
{
    public int LqvId { get; set; }

    public int LqvSinhVienId { get; set; }

    public int LqvLopHocId { get; set; }

    public double LqvViDo { get; set; }

    public double LqvKinhDo { get; set; }

    public DateTime LqvThoiGian { get; set; }

    public bool LqvHopLe { get; set; }

    public int LqvBuoiHocId { get; set; }

    public virtual LqvBuoiHoc LqvBuoiHoc { get; set; } = null!;

    public virtual LqvLopHoc LqvLopHoc { get; set; } = null!;

    public virtual LqvNguoiDung LqvSinhVien { get; set; } = null!;
}