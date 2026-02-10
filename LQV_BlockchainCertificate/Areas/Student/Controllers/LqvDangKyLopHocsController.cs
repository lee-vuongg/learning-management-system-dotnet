using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LQV_BlockchainCertificate.Models.DBModel;

namespace LQV_BlockchainCertificate.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "SinhVien")]
    public class LqvDangKyLopHocsController : Controller
    {
        private readonly LqvDbContext _context;

        public LqvDangKyLopHocsController(LqvDbContext context)
        {
            _context = context;
        }

        // =========================
        // 1️⃣ DANH SÁCH LỚP ĐÃ ĐĂNG KÝ
        // =========================
        public async Task<IActionResult> Index()
        {
            int sinhVienId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var data = await _context.LqvDangKyLopHocs
                .Where(x => x.LqvSinhVienId == sinhVienId)
                .Include(x => x.LqvLopHoc)
                    .ThenInclude(l => l.LqvKhoaHoc)
                .OrderByDescending(x => x.LqvNgayDangKy)
                .ToListAsync();

            return View(data);
        }

        // =========================
        // 2️⃣ DANH SÁCH LỚP CÓ THỂ ĐĂNG KÝ
        // =========================
        public async Task<IActionResult> DangKy()
        {
            int sinhVienId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var lopDaDangKy = _context.LqvDangKyLopHocs
                .Where(x => x.LqvSinhVienId == sinhVienId)
                .Select(x => x.LqvLopHocId);

            var lopChuaDangKy = await _context.LqvLopHocs
                .Include(l => l.LqvKhoaHoc)
                .Where(l => !lopDaDangKy.Contains(l.LqvLopHocId))
                .ToListAsync();

            return View(lopChuaDangKy);
        }

        // =========================
        // 3️⃣ XÁC NHẬN ĐĂNG KÝ LỚP
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKy(int lopHocId)
        {
            int sinhVienId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            bool daDangKy = await _context.LqvDangKyLopHocs.AnyAsync(x =>
                x.LqvSinhVienId == sinhVienId &&
                x.LqvLopHocId == lopHocId
            );

            if (daDangKy)
            {
                TempData["Error"] = "Bạn đã đăng ký lớp này rồi!";
                return RedirectToAction(nameof(DangKy));
            }

            var dangKy = new LqvDangKyLopHoc
            {
                LqvSinhVienId = sinhVienId,
                LqvLopHocId = lopHocId,
                LqvNgayDangKy = DateTime.Now
            };

            _context.LqvDangKyLopHocs.Add(dangKy);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đăng ký lớp học thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
