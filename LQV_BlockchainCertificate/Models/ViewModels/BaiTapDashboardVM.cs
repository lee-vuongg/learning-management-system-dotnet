namespace LQV_BlockchainCertificate.Models.ViewModels
{
    public class BaiTapDashboardVM
    {
        public int BaiTapId { get; set; }
        public string TenBaiTap { get; set; } = string.Empty;

        public string TenMonHoc { get; set; } = string.Empty;
        public string TenLopHoc { get; set; } = string.Empty;

        public DateTime Deadline { get; set; }

        // Trang thai
        public string TrangThai { get; set; } = string.Empty;
        // SapDenHan | DaNop | QuaHan

        public bool DaNop { get; set; }

        // Button
        public string ActionText { get; set; } = "Làm bài"; // hoặc "Xem bài"
    }

}
