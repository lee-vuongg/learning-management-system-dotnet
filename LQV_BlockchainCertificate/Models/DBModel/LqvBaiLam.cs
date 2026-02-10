using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace LQV_BlockchainCertificate.Models.DBModel;

public partial class LqvBaiLam
{
    public int LqvBaiLamId { get; set; }

    public int LqvLichThiId { get; set; }

    public int LqvUserId { get; set; }   // FK thật trong DB

    public DateTime? LqvThoiGianBatDau { get; set; }
    public DateTime? LqvThoiGianNop { get; set; }
    public double? LqvDiem { get; set; }
    public string? LqvTrangThai { get; set; }

    [ForeignKey(nameof(LqvUserId))]
    public virtual LqvNguoiDung LqvNguoiDung { get; set; } = null!;

    public virtual LqvLichThi LqvLichThi { get; set; } = null!;

    public virtual ICollection<LqvChiTietBaiLam> LqvChiTietBaiLams { get; set; }
        = new List<LqvChiTietBaiLam>();
}