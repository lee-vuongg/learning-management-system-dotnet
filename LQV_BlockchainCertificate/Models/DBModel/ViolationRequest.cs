namespace LQV_BlockchainCertificate.Models.DBModel
{
    public class ViolationRequest
    {
        public int BaiLamId { get; set; }
        public string LoaiViPham { get; set; }
        public int DiemRisk { get; set; }
        public string? DuongDanAnh { get; set; }
    }
}
