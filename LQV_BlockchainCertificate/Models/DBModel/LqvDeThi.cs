using System;
using System.Collections.Generic;

namespace LQV_BlockchainCertificate.Models.DBModel;

public partial class LqvDeThi
{
    public int LqvDeThiId { get; set; }

    public string LqvTenDeThi { get; set; } = null!;

    public int LqvBoCauHoiId { get; set; }

    public int LqvThoiGianThi { get; set; }

    public double LqvTongDiem { get; set; }

    public bool LqvDaDuyet { get; set; } = false;
    public DateTime? LqvNgayDuyet { get; set; }
    public int? LqvGiangVienId { get; set; }

    // navigation
    public virtual LqvNguoiDung? LqvGiangVien { get; set; }

    // ✅ CHO PHÉP NULL
    public virtual LqvBoCauHoi? LqvBoCauHoi { get; set; }

    public virtual ICollection<LqvLichThi> LqvLichThis { get; set; }
        = new List<LqvLichThi>();
}