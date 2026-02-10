using System;
using System.Collections.Generic;

namespace LQV_BlockchainCertificate.Models.DBModel;

public partial class LqvChiTietBaiLam
{
    public int LqvId { get; set; }

    public int LqvBaiLamId { get; set; }

    public int LqvCauHoiId { get; set; }

    public int? LqvDapAnId { get; set; }

    public string? LqvTraLoiTuLuan { get; set; }

    public double? LqvDiem { get; set; }

    public bool LqvDaCham { get; set; }

    public virtual LqvBaiLam LqvBaiLam { get; set; } = null!;

    public virtual LqvCauHoi LqvCauHoi { get; set; } = null!;

    public virtual LqvDapAn? LqvDapAn { get; set; }
}