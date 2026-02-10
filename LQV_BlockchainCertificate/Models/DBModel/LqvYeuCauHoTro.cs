using System;
using System.Collections.Generic;

namespace LQV_BlockchainCertificate.Models.DBModel;

public partial class LqvYeuCauHoTro
{
    public int LqvId { get; set; }

    public int LqvNguoiDungId { get; set; }

    public string LqvNoiDung { get; set; } = null!;

    public DateTime? LqvThoiGianGui { get; set; }

    public string LqvTrangThai { get; set; } = null!;

    public string? LqvPhanHoi { get; set; }

    public virtual LqvNguoiDung LqvNguoiDung { get; set; } = null!;
}