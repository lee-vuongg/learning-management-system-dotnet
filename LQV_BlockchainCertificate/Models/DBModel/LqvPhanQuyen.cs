using System;
using System.Collections.Generic;

namespace LQV_BlockchainCertificate.Models.DBModel;

public partial class LqvPhanQuyen
{
    public int LqvPhanQuyenId { get; set; }

    public int LqvRoleId { get; set; }

    public int LqvChucNangId { get; set; }

    public bool LqvChoPhep { get; set; }

    public virtual LqvChucNang LqvChucNang { get; set; } = null!;

    public virtual LqvRole LqvRole { get; set; } = null!;
}