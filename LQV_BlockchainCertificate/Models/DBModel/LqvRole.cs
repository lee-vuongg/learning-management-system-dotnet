using System;
using System.Collections.Generic;

namespace LQV_BlockchainCertificate.Models.DBModel;

public partial class LqvRole
{
    public int LqvRoleId { get; set; }

    public string LqvRoleName { get; set; } = null!;

    public virtual ICollection<LqvNguoiDung> LqvNguoiDungs { get; set; } = new List<LqvNguoiDung>();

    public virtual ICollection<LqvPhanQuyen> LqvPhanQuyens { get; set; } = new List<LqvPhanQuyen>();
}