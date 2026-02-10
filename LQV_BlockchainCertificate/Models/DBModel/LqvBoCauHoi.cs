using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace LQV_BlockchainCertificate.Models.DBModel;

public partial class LqvBoCauHoi
{
    public int LqvBoCauHoiId { get; set; }

    public string LqvTenBo { get; set; } = null!;

    public string? LqvMoTa { get; set; }

    public int LqvGiangVienId { get; set; }

    public DateTime? LqvNgayTao { get; set; }

    [ForeignKey(nameof(LqvGiangVienId))]
    public virtual LqvNguoiDung? LqvGiangVien { get; set; } = null!;

    public virtual ICollection<LqvCauHoi> LqvCauHois { get; set; } = new List<LqvCauHoi>();

    public virtual ICollection<LqvDeThi> LqvDeThis { get; set; } = new List<LqvDeThi>();
}