using System;
using System.Collections.Generic;

namespace LQV_BlockchainCertificate.Models.DBModel;

public partial class LqvNhatKyHoatDong
{
    public int LqvId { get; set; }

    public string? LqvTaiKhoan { get; set; }

    public string? LqvHanhDong { get; set; }

    public string? LqvChiTiet { get; set; }

    public DateTime? LqvThoiGian { get; set; }

    public string? LqvIp { get; set; }
}