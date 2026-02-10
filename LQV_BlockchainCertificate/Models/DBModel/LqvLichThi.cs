using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace LQV_BlockchainCertificate.Models.DBModel;

public partial class LqvLichThi
{
    public int LqvLichThiId { get; set; }

    public int LqvDeThiId { get; set; }

    public int LqvLopHocId { get; set; }

    public DateTime LqvBatDau { get; set; }

    public DateTime LqvKetThuc { get; set; }

    [ValidateNever]
    public virtual LqvDeThi LqvDeThi { get; set; } = null!;

    [ValidateNever]
    public virtual LqvLopHoc LqvLopHoc { get; set; } = null!;
}