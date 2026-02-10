using LQV_BlockchainCertificate.Models.DBModel;

namespace LQV_BlockchainCertificate.Models.ViewModels
{
    // ================= DASHBOARD SINH VIÊN =================
    public class StudentDashboardViewModel
    {
        // ---------- Thông tin cá nhân ----------
        public string HoTen { get; set; } = string.Empty;
        public string MaSoSinhVien { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AvtUrl { get; set; } = string.Empty;
        public DateTime? NgaySinh { get; set; }

        // ---------- Thống kê ----------
        public int TongChungNhan { get; set; }
        public int TongKhoaHocDaThamGia { get; set; }
        public double TiLeHoanThanhTongThe { get; set; }

        // ---------- Danh sách ----------
        public List<ChungNhanHienThiVM> ChungNhanMoiNhat { get; set; } = new();
        public List<BaiTapDashboardVM> DanhSachBaiTap { get; set; } = new();
       

        // ✅ THÊM MỚI – LỊCH SỬ ĐIỂM DANH
        public List<LichSuDiemDanhVM> LichSuDiemDanh { get; set; } = new();

        // ✅ THÊM MỚI – THỐNG KÊ ĐIỂM DANH
        public int TongBuoiDaDiemDanh { get; set; }
        public int TongBuoiVang { get; set; }
    }

    // ================= CHỨNG NHẬN =================
    public class ChungNhanHienThiVM
    {
        public int LqvMaChungNhan { get; set; }
        public string MaChungNhanCode { get; set; } = string.Empty;
        public string TenChungNhan { get; set; } = string.Empty;
        public DateTime NgayCap { get; set; }
        public string TrangThaiXacThuc { get; set; } = string.Empty;
    }

    // ================= KHÓA HỌC & TIẾN ĐỘ =================
    public class KhoaHocVaTienDoVM
    {
        public int LqvMaKhoaHoc { get; set; }
        public string TenKhoaHoc { get; set; } = string.Empty;
        public string MoTa { get; set; } = string.Empty;
        public double TiLeHoanThanh { get; set; }
    }

    // ================= ✅ LỊCH SỬ ĐIỂM DANH =================
    public class LichSuDiemDanhVM
    {
        public string TenBuoiHoc { get; set; } = string.Empty;
        public DateTime ThoiGianDiemDanh { get; set; }
        public string TrangThai { get; set; } = string.Empty;
    }
}
