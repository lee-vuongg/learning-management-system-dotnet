namespace LQV_BlockchainCertificate.Models.ViewModels
{
    public class GiangVienDashboardViewModel
    {
        public int TongLopHoc { get; set; }
        public int TongSinhVien { get; set; }
        public int TongBaiTap { get; set; }
        public int TongKhoaHoc { get; set; }
        public int SoBaiTapCanCham { get; set; }
        public List<LopGanDayVM> TatCaLopHoc { get; set; }

        public List<LopGanDayVM> LopGanDays { get; set; } = new();
    }

    public class LopGanDayVM
    {
     
        public int LopId { get; set; }
        public string TenLop { get; set; } = null!;
        public int SoSinhVien { get; set; }
        public DateTime? NgayTao { get; set; }
    }
}
