using System;
using System.Collections.Generic;

namespace LQV_BlockchainCertificate.Models.DBModel;

public partial class LqvCauHoi
{
    public int LqvCauHoiId { get; set; }

    public int LqvBoCauHoiId { get; set; }

    public string LqvNoiDung { get; set; } = null!;

    public string LqvLoai { get; set; } = null!;

    public double LqvDiem { get; set; }

    public virtual LqvBoCauHoi? LqvBoCauHoi { get; set; }

    public virtual ICollection<LqvChiTietBaiLam> LqvChiTietBaiLams { get; set; } = new List<LqvChiTietBaiLam>();

    public virtual ICollection<LqvDapAn> LqvDapAns { get; set; } = new List<LqvDapAn>();
}