using System;
using System.Collections.Generic;

namespace LQV_BlockchainCertificate.Models.DBModel;

public partial class LqvDapAn
{
    public int LqvDapAnId { get; set; }

    public int LqvCauHoiId { get; set; }

    public string? LqvNoiDung { get; set; }

    public bool LqvDung { get; set; }

    public virtual LqvCauHoi LqvCauHoi { get; set; } = null!;

    public virtual ICollection<LqvChiTietBaiLam> LqvChiTietBaiLams { get; set; } = new List<LqvChiTietBaiLam>();
}