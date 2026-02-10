using System;
using System.Collections.Generic;

namespace LQV_BlockchainCertificate.Models.DBModel;

public partial class LqvGiaoDichBlockchain
{
    public int LqvMaGiaoDich { get; set; }

    public int LqvChungNhanId { get; set; }

    public string LqvTxHash { get; set; } = null!;

    public long? LqvBlockNumber { get; set; }

    public DateTime LqvGioTao { get; set; }

    public string? LqvStatus { get; set; }

    public string? LqvNoiDungHash { get; set; }

    public string? LqvUrlEtherscan { get; set; }

    public virtual LqvChungNhan LqvChungNhan { get; set; } = null!;
}