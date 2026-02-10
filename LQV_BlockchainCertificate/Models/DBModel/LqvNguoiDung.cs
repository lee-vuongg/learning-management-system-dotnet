using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace LQV_BlockchainCertificate.Models.DBModel;

public partial class LqvNguoiDung
{
    public int LqvId { get; set; }

    public string LqvTenDangNhap { get; set; } = null!;

    public string? LqvMatKhauHash { get; set; }

    public string LqvHoTen { get; set; } = null!;

    public string? LqvEmail { get; set; }

    public int LqvRoleId { get; set; }

    public string? LqvWalletAddress { get; set; }

    public DateTime? LqvNgayTao { get; set; }

    public bool LqvDaXacThuc { get; set; }

    public string? LqvAvt { get; set; }

    public DateTime? LqvNgaySinh { get; set; }

    public virtual ICollection<LqvBaiTap> LqvBaiTaps { get; set; } = new List<LqvBaiTap>();

    public virtual ICollection<LqvBoCauHoi> LqvBoCauHois { get; set; } = new List<LqvBoCauHoi>();

    public virtual ICollection<LqvChungNhan> LqvChungNhans { get; set; } = new List<LqvChungNhan>();

    public virtual ICollection<LqvDangKyLopHoc> LqvDangKyLopHocs { get; set; } = new List<LqvDangKyLopHoc>();

    public virtual ICollection<LqvDiemDanhGp> LqvDiemDanhGps { get; set; } = new List<LqvDiemDanhGp>();

    public virtual ICollection<LqvKhoaHoc> LqvKhoaHocs { get; set; } = new List<LqvKhoaHoc>();

    public virtual ICollection<LqvLopHoc> LqvLopHocs { get; set; } = new List<LqvLopHoc>();

    public virtual ICollection<LqvNopBaiTap> LqvNopBaiTaps { get; set; } = new List<LqvNopBaiTap>();

    [ValidateNever]
    public virtual LqvRole LqvRole { get; set; } = null!;

    public virtual ICollection<LqvTienDoHocTap> LqvTienDoHocTaps { get; set; } = new List<LqvTienDoHocTap>();

    public virtual ICollection<LqvYeuCauCapChungChi> LqvYeuCauCapChungChis { get; set; } = new List<LqvYeuCauCapChungChi>();

    public virtual ICollection<LqvYeuCauHoTro> LqvYeuCauHoTros { get; set; } = new List<LqvYeuCauHoTro>();
}