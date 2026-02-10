namespace LQV_BlockchainCertificate.Models.ViewModels
{
    public class SinhVienLopHocListViewModel
    {
        public int LopHocId { get; set; }
        public string TenLop { get; set; } = "";
        public List<SinhVienLopHocViewModel> SinhViens { get; set; } = new();
    }

}
