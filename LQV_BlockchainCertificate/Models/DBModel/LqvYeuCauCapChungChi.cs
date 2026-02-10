using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace LQV_BlockchainCertificate.Models.DBModel;

public partial class LqvYeuCauCapChungChi
{
    public int LqvId { get; set; }

    public int LqvNguoiDungId { get; set; }

    public int LqvKhoaHocId { get; set; }

    public DateTime? LqvNgayYeuCau { get; set; }

    public string? LqvLyDoYeuCau { get; set; }

    public string LqvTrangThai { get; set; } = null!;

    public string? LqvLyDoTuChoi { get; set; }

    // ✅ FK tới bảng LqvChungNhan
    public int? LqvChungNhanId { get; set; }

    // ✅ Navigation Property
    [ForeignKey("LqvChungNhanId")]
    public virtual LqvChungNhan? LqvChungNhan { get; set; }

    public virtual LqvKhoaHoc LqvKhoaHoc { get; set; } = null!;

    public virtual LqvNguoiDung LqvNguoiDung { get; set; } = null!;
}