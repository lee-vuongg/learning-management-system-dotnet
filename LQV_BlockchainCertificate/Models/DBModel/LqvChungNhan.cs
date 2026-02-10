using System;
using System.Collections.Generic;

namespace LQV_BlockchainCertificate.Models.DBModel;

public partial class LqvChungNhan
{
    public int LqvMaChungNhan { get; set; }

    public string LqvMaChungNhanCode { get; set; } = null!;

    public int LqvSinhVienId { get; set; }

    public int LqvKhoaHocId { get; set; }

    public DateTime LqvNgayCap { get; set; }

    public string? LqvHashValue { get; set; }

    public string? LqvTrangThai { get; set; }

    public virtual ICollection<LqvGiaoDichBlockchain> LqvGiaoDichBlockchains { get; set; } = new List<LqvGiaoDichBlockchain>();

    public virtual LqvKhoaHoc LqvKhoaHoc { get; set; } = null!;

    public virtual LqvNguoiDung LqvSinhVien { get; set; } = null!;
}