using System;
using System.Collections.Generic;

namespace LQV_BlockchainCertificate.Models.DBModel;

public partial class LqvXacThucEmail
{
    public int LqvMaXacThuc { get; set; }

    public string LqvEmail { get; set; } = null!;

    public string LqvMaOtp { get; set; } = null!;

    public string LqvLoaiXacThuc { get; set; } = null!;

    public DateTime LqvThoiGianTao { get; set; }

    public DateTime LqvThoiGianHetHan { get; set; }

    public bool LqvTrangThai { get; set; }
}