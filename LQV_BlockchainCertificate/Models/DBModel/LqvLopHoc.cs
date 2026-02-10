using System;
using System.Collections.Generic;

namespace LQV_BlockchainCertificate.Models.DBModel;

public partial class LqvLopHoc
{
    public int LqvLopHocId { get; set; }

    public string LqvTenLop { get; set; } = null!;

    public int LqvKhoaHocId { get; set; }

    public int LqvGiangVienId { get; set; }

    public string? LqvMoTa { get; set; }

    public DateTime? LqvNgayTao { get; set; }
    public virtual ICollection<LqvBuoiHoc> LqvBuoiHocs { get; set; }
    = new List<LqvBuoiHoc>();
    public virtual ICollection<LqvBaiTap> LqvBaiTaps { get; set; } = new List<LqvBaiTap>();

    public virtual ICollection<LqvDangKyLopHoc> LqvDangKyLopHocs { get; set; } = new List<LqvDangKyLopHoc>();

    public virtual ICollection<LqvDiemDanhGp> LqvDiemDanhGps { get; set; } = new List<LqvDiemDanhGp>();

    public virtual LqvNguoiDung? LqvGiangVien { get; set; }
    public virtual LqvKhoaHoc? LqvKhoaHoc { get; set; }
    public virtual ICollection<LqvLichThi> LqvLichThis { get; set; } = new List<LqvLichThi>();
}