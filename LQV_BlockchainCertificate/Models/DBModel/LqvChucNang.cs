using System;
using System.Collections.Generic;

namespace LQV_BlockchainCertificate.Models.DBModel;

public partial class LqvChucNang
{
    public int LqvChucNangId { get; set; }

    public string LqvTenChucNang { get; set; } = null!;

    public string? LqvMoTa { get; set; }

    public string? LqvDuongDan { get; set; }

    public virtual ICollection<LqvPhanQuyen> LqvPhanQuyens { get; set; } = new List<LqvPhanQuyen>();
}