using System;
using System.Collections.Generic;

namespace LQV_BlockchainCertificate.Models.DBModel;

public partial class LqvXacThucChungNhan
{
    public int LqvId { get; set; }

    public string LqvMaChungNhanCode { get; set; } = null!;

    public DateTime LqvThoiGianXacThuc { get; set; }

    public string? LqvDiaChiNguoiXacThuc { get; set; }

    public string LqvKetQua { get; set; } = null!;
}