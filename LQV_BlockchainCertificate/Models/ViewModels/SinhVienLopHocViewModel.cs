namespace LQV_BlockchainCertificate.Models.ViewModels
{
    public class SinhVienLopHocViewModel
    {
        public int SinhVienId { get; set; }
        public string HoTen { get; set; } = "";
        public string Email { get; set; } = "";
        public double ChuyenCan { get; set; }
        public bool DuDieuKienChungChi { get; set; }
    }

}
