using System;
using System.Collections.Generic;

namespace LQV_BlockchainCertificate.Models.DBModel;

public partial class LqvBuoiHoc
{
    public int LqvBuoiHocId { get; set; }

    public int LqvLopHocId { get; set; }

    public DateOnly LqvNgayHoc { get; set; }

    public TimeOnly? LqvGioBatDau { get; set; }

    public TimeOnly? LqvGioKetThuc { get; set; }
    public bool LqvDangMo { get; set; }

    public double? LqvViDo { get; set; }
    public double? LqvKinhDo { get; set; }
    public double? LqvBanKinh { get; set; }

    public virtual LqvLopHoc? LqvLopHoc { get; set; }
    public virtual ICollection<LqvDiemDanhGp> LqvDiemDanhGps { get; set; } = new List<LqvDiemDanhGp>();
}