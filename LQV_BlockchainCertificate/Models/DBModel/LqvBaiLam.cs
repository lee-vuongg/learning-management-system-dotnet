using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace LQV_BlockchainCertificate.Models.DBModel;

public partial class LqvBaiLam
{
    public int LqvBaiLamId { get; set; }

    public int LqvLichThiId { get; set; }

    public int LqvUserId { get; set; }

    public DateTime? LqvThoiGianBatDau { get; set; }
    public DateTime? LqvThoiGianNop { get; set; }
    public double? LqvDiem { get; set; }
    public string? LqvTrangThai { get; set; }

    // ================= AI PROCTORING =================

    public int Lqv_TongDiemRisk { get; set; } = 0;

    public bool Lqv_BiKhoa { get; set; } = false;

    public virtual ICollection<Lqv_NhatKyViPhamThi> Lqv_NhatKyViPhamThis { get; set; }
        = new List<Lqv_NhatKyViPhamThi>();

    public virtual ICollection<Lqv_NhatKyHinhAnhThi> Lqv_NhatKyHinhAnhThis { get; set; }
        = new List<Lqv_NhatKyHinhAnhThi>();

    // ================= RELATIONSHIP =================

    [ForeignKey(nameof(LqvUserId))]
    public virtual LqvNguoiDung LqvNguoiDung { get; set; } = null!;

    public virtual LqvLichThi LqvLichThi { get; set; } = null!;

    public virtual ICollection<LqvChiTietBaiLam> LqvChiTietBaiLams { get; set; }
        = new List<LqvChiTietBaiLam>();
}
