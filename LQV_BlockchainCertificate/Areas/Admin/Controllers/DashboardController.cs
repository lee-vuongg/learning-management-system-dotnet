using LQV_BlockchainCertificate.Models.DBModel;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace LQV_BlockchainCertificate.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        private readonly LqvDbContext _context;

        public DashboardController(LqvDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // ========== Thống kê người dùng ==========
            var totalUsers = _context.LqvNguoiDungs.Count();
            var verifiedUsers = _context.LqvNguoiDungs.Count(u => u.LqvDaXacThuc);
            var unverifiedUsers = totalUsers - verifiedUsers;

            // ========== Thống kê khóa học & chứng nhận ==========
            var totalCourses = _context.LqvKhoaHocs.Count();
            var totalCertificates = _context.LqvChungNhans.Count();

            // ========== Thống kê Blockchain ==========
            var totalTransactions = _context.LqvGiaoDichBlockchains.Count();
            var validCertificates = _context.LqvChungNhans.Count(c => c.LqvTrangThai == "Hợp lệ");
            var invalidCertificates = _context.LqvChungNhans.Count(c => c.LqvTrangThai == "Lỗi");

            // ✅ Lấy hash mới nhất từ giao dịch blockchain
            var latestBlockHash = _context.LqvGiaoDichBlockchains
                .OrderByDescending(g => g.LqvGioTao)
                .Select(g => g.LqvTxHash)
                .FirstOrDefault() ?? "Không có dữ liệu";

            // ========== Lấy hoạt động gần đây ==========
            var recentActivities = _context.LqvNhatKyHoatDongs
                .OrderByDescending(a => a.LqvThoiGian)
                .Take(5)
                .Select(a => new
                {
                    a.LqvTaiKhoan,
                    a.LqvHanhDong,
                    a.LqvChiTiet,
                    a.LqvThoiGian,
                    a.LqvIp
                })
                .ToList();

            // ========== Gửi dữ liệu sang View ==========
            ViewBag.TotalUsers = totalUsers;
            ViewBag.VerifiedUsers = verifiedUsers;
            ViewBag.UnverifiedUsers = unverifiedUsers;
            ViewBag.TotalCourses = totalCourses;
            ViewBag.TotalCertificates = totalCertificates;

            ViewBag.TotalTransactions = totalTransactions;
            ViewBag.ValidCertificates = validCertificates;
            ViewBag.InvalidCertificates = invalidCertificates;
            ViewBag.LatestBlockHash = latestBlockHash;

            ViewBag.RecentActivities = recentActivities;

            return View();
        }
    }
}
