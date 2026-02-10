using System;
using System.Collections.Generic;

namespace LQV_BlockchainCertificate.Models.DBModel;

public partial class LqvKhoaHoc
{
    public int LqvMaKhoaHoc { get; set; }

    public string LqvTenKhoaHoc { get; set; } = null!;

    public string? LqvMoTa { get; set; }

    public DateTime? LqvNgayBatDau { get; set; }

    public DateTime? LqvNgayKetThuc { get; set; }

    public int? LqvGiangVienId { get; set; }

    public virtual ICollection<LqvChungNhan> LqvChungNhans { get; set; } = new List<LqvChungNhan>();

    public virtual LqvNguoiDung? LqvGiangVien { get; set; }

    public virtual ICollection<LqvLopHoc> LqvLopHocs { get; set; } = new List<LqvLopHoc>();

    public virtual ICollection<LqvTienDoHocTap> LqvTienDoHocTaps { get; set; } = new List<LqvTienDoHocTap>();

    public virtual ICollection<LqvYeuCauCapChungChi> LqvYeuCauCapChungChis { get; set; } = new List<LqvYeuCauCapChungChi>();
}