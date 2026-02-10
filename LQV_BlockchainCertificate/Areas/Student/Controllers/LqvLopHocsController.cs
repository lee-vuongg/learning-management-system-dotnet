using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using LQV_BlockchainCertificate.Models.DBModel;

namespace LQV_BlockchainCertificate.Areas.Student.Controllers
{
    [Area("Student")]
    public class LqvLopHocsController : Controller
    {
        private readonly LqvDbContext _context;

        public LqvLopHocsController(LqvDbContext context)
        {
            _context = context;
        }

        // ===============================
        // DANH SÁCH LỚP ĐÃ ĐĂNG KÝ
        // ===============================
        public async Task<IActionResult> Index()
        {
            var sinhVienId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var lopHocs = await _context.LqvDangKyLopHocs
                .Where(dk => dk.LqvSinhVienId == sinhVienId)
                .Include(dk => dk.LqvLopHoc)
                    .ThenInclude(l => l.LqvKhoaHoc)
                .Include(dk => dk.LqvLopHoc)
                    .ThenInclude(l => l.LqvGiangVien)
                .Select(dk => dk.LqvLopHoc)
                .ToListAsync();

            return View(lopHocs);
        }

        // ===============================
        // CHI TIẾT LỚP HỌC
        // ===============================
        public async Task<IActionResult> Details(int id)
        {
            var sinhVienId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Kiểm tra sinh viên có đăng ký lớp này không
            var isRegistered = await _context.LqvDangKyLopHocs
                .AnyAsync(dk => dk.LqvLopHocId == id && dk.LqvSinhVienId == sinhVienId);

            if (!isRegistered)
                return Forbid();

            var lopHoc = await _context.LqvLopHocs
                .Include(l => l.LqvKhoaHoc)
                .Include(l => l.LqvGiangVien)
                .Include(l => l.LqvBaiTaps)
                .Include(l => l.LqvLichThis)
                .Include(l => l.LqvDiemDanhGps)
                .FirstOrDefaultAsync(l => l.LqvLopHocId == id);

            if (lopHoc == null)
                return NotFound();

            ViewBag.SoBuoiHoc = await _context.LqvBuoiHocs
                .CountAsync(b => b.LqvLopHocId == id);

            return View(lopHoc);
        }
    }
}
