using System.Collections.Generic;
using LQV_BlockchainCertificate.Models.DBModel;

namespace LQV_BlockchainCertificate.Models.ViewModels
{
    public class BaiLamGroupViewModel
    {
        public int LopId { get; set; }
        public string TenLop { get; set; } = string.Empty;

        public List<DeThiGroupViewModel> DeThis { get; set; } = new();
    }

    public class DeThiGroupViewModel
    {
        public int DeThiId { get; set; }
        public string TenDeThi { get; set; } = string.Empty;

        public List<LqvBaiLam> BaiLams { get; set; } = new();
    }
}
