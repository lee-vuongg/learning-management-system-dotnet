using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using LQV_BlockchainCertificate.Models.DBModel;

namespace LQV_BlockchainCertificate.Areas.Student.Controllers
{
    [Area("Student")]
    public class LqvKhoaHocsController : Controller
    {
        private readonly LqvDbContext _context;

        public LqvKhoaHocsController(LqvDbContext context)
        {
            _context = context;
        }

        // ============================
        // 🔐 LẤY ID SINH VIÊN HIỆN TẠI
        // ============================
        private int GetCurrentUserId()
        {
            if (!User.Identity!.IsAuthenticated)
                throw new UnauthorizedAccessException("Chưa đăng nhập");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.Parse(userId!);
        }

        // =====================================================
        // 1️⃣ DANH SÁCH KHÓA HỌC SINH VIÊN ĐÃ ĐĂNG KÝ
        // =====================================================
        public async Task<IActionResult> Index()
        {
            int studentId = GetCurrentUserId();

            var khoaHocs = await _context.LqvTienDoHocTaps
                .Where(td => td.LqvSinhVienId == studentId)
                .Include(td => td.LqvKhoaHoc)
                    .ThenInclude(kh => kh.LqvGiangVien)
                .AsNoTracking()
                .Select(td => td.LqvKhoaHoc!)
                .ToListAsync();

            // map tiến độ
            ViewBag.CourseProgress = await _context.LqvTienDoHocTaps
                .Where(td => td.LqvSinhVienId == studentId)
                .ToDictionaryAsync(td => td.LqvKhoaHocId, td => td.LqvTiLeHoanThanh);

            return View(khoaHocs);
        }

        // =====================================================
        // 2️⃣ CHI TIẾT KHÓA HỌC (ĐÃ ĐĂNG KÝ)
        // =====================================================
        public async Task<IActionResult> Details(int id)
        {
            int studentId = GetCurrentUserId();

            // kiểm tra đã đăng ký chưa
            var progress = await _context.LqvTienDoHocTaps
                .FirstOrDefaultAsync(td =>
                    td.LqvKhoaHocId == id &&
                    td.LqvSinhVienId == studentId);

            if (progress == null)
            {
                TempData["ErrorMessage"] = "Bạn chưa đăng ký khóa học này.";
                return RedirectToAction(nameof(ListAll));
            }

            var khoaHoc = await _context.LqvKhoaHocs
                .Include(kh => kh.LqvGiangVien)
                .Include(kh => kh.LqvLopHocs) // ⭐ RẤT QUAN TRỌNG
                .FirstOrDefaultAsync(kh => kh.LqvMaKhoaHoc == id);

            if (khoaHoc == null) return NotFound();

            // 🏅 kiểm tra chứng nhận
            var chungNhan = await _context.LqvChungNhans.FirstOrDefaultAsync(cn =>
                cn.LqvKhoaHocId == id &&
                cn.LqvSinhVienId == studentId);

            ViewBag.DaCapChungNhan = chungNhan != null;
            ViewBag.MaChungNhan = chungNhan?.LqvMaChungNhanCode;
            ViewBag.NgayCapChungNhan = chungNhan?.LqvNgayCap;

            // ⏱ auto complete khi hết hạn
            if (khoaHoc.LqvNgayKetThuc < DateTime.Now && progress.LqvTiLeHoanThanh < 100)
            {
                progress.LqvTiLeHoanThanh = 100;
                progress.LqvNgayCapNhat = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            return View(khoaHoc);
        }

        // =====================================================
        // 3️⃣ DANH SÁCH KHÓA HỌC CÓ THỂ ĐĂNG KÝ
        // =====================================================
        public async Task<IActionResult> ListAll()
        {
            var now = DateTime.Now;

            var khoaHocs = await _context.LqvKhoaHocs
                .Where(kh => kh.LqvNgayKetThuc > now)
                .Include(kh => kh.LqvGiangVien)
                .AsNoTracking()
                .OrderByDescending(kh => kh.LqvNgayBatDau)
                .ToListAsync();

            return View(khoaHocs);
        }

        // =====================================================
        // 4️⃣ ĐĂNG KÝ KHÓA HỌC
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(int id)
        {
            int studentId = GetCurrentUserId();

            var course = await _context.LqvKhoaHocs.FindAsync(id);
            if (course == null)
            {
                TempData["ErrorMessage"] = "Khóa học không tồn tại.";
                return RedirectToAction(nameof(ListAll));
            }

            if (course.LqvNgayKetThuc < DateTime.Now)
            {
                TempData["ErrorMessage"] = "Khóa học đã kết thúc.";
                return RedirectToAction(nameof(ListAll));
            }

            bool exists = await _context.LqvTienDoHocTaps.AnyAsync(td =>
                td.LqvKhoaHocId == id &&
                td.LqvSinhVienId == studentId);

            if (exists)
            {
                TempData["ErrorMessage"] = "Bạn đã đăng ký khóa học này.";
                return RedirectToAction(nameof(Details), new { id });
            }

            _context.LqvTienDoHocTaps.Add(new LqvTienDoHocTap
            {
                LqvKhoaHocId = id,
                LqvSinhVienId = studentId,
                LqvTiLeHoanThanh = 0,
                LqvNgayCapNhat = DateTime.Now
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đăng ký khóa học thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
