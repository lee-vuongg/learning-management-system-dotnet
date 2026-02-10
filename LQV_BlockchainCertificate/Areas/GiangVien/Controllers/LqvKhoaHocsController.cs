using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LQV_BlockchainCertificate.Models.DBModel;

namespace LQV_BlockchainCertificate.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    [Authorize(Roles = "GiangVien")]
    public class LqvKhoaHocsController : Controller
    {
        private readonly LqvDbContext _context;

        public LqvKhoaHocsController(LqvDbContext context)
        {
            _context = context;
        }

        // =========================
        // Helper: lấy ID giảng viên
        // =========================
        private int GetGiangVienId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        // =========================
        // DANH SÁCH KHÓA HỌC CỦA GV
        // =========================
        public async Task<IActionResult> Index()
        {
            int giangVienId = GetGiangVienId();

            var khoaHocs = await _context.LqvKhoaHocs
                .Include(k => k.LqvGiangVien)
                .Where(k => k.LqvGiangVienId == giangVienId)
                .ToListAsync();

            return View(khoaHocs);
        }

        // =========================
        // CHI TIẾT KHÓA HỌC
        // =========================
        public async Task<IActionResult> Details(int id)
        {
            int giangVienId = GetGiangVienId();

            var khoaHoc = await _context.LqvKhoaHocs
                .Include(k => k.LqvGiangVien)
                .FirstOrDefaultAsync(k =>
                    k.LqvMaKhoaHoc == id &&
                    k.LqvGiangVienId == giangVienId
                );

            if (khoaHoc == null)
                return NotFound();

            return View(khoaHoc);
        }
    }
}
