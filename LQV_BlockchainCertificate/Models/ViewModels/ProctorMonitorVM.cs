using LQV_BlockchainCertificate.Models.DBModel;

namespace LQV_BlockchainCertificate.Models.ViewModels
{
    public class ProctorMonitorVM
    {
        public int LichThiId { get; set; }

        public List<StudentExamVM> Students { get; set; } = new();

    }

    public class StudentExamVM
    {
        public int BaiLamId { get; set; }
        public int UserId { get; set; }

        // ====== DÙNG CHO REVIEW SAU THI ======
        public int TongRisk { get; set; }

        public List<Lqv_NhatKyViPhamThi> Violations { get; set; } = new();
        public List<Lqv_NhatKyHinhAnhThi> Images { get; set; } = new();
    }

    public class ViolationVM
    {
        public string LoaiViPham { get; set; } = "";
        public int DiemRisk { get; set; }
        public string? MoTa { get; set; }
        public DateTime ThoiGian { get; set; }

        // Ảnh snapshot nếu có
        public string? AnhChup { get; set; }
    }
}