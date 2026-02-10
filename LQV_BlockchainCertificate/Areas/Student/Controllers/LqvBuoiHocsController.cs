using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LQV_BlockchainCertificate.Models.DBModel;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace LQV_BlockchainCertificate.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "SinhVien")]
    public class LqvBuoiHocsController : Controller
    {
        private readonly LqvDbContext _context;

        public LqvBuoiHocsController(LqvDbContext context)
        {
            _context = context;
            Console.WriteLine("🔥 [INIT][STUDENT] LqvBuoiHocsController khởi tạo");
        }

        // ================== INDEX ==================
        // GET: Student/LqvBuoiHocs
        public async Task<IActionResult> Index(int? lopHocId)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var query =
                from bh in _context.LqvBuoiHocs
                join dk in _context.LqvDangKyLopHocs
                    on bh.LqvLopHocId equals dk.LqvLopHocId
                where dk.LqvSinhVienId == userId
                select bh;

            // ✅ LỌC THEO LỚP HỌC NẾU CÓ
            if (lopHocId.HasValue)
            {
                query = query.Where(bh => bh.LqvLopHocId == lopHocId);
            }

            var data = await query
                .Include(x => x.LqvLopHoc)
                .OrderByDescending(x => x.LqvNgayHoc)
                .Distinct()
                .ToListAsync();

            ViewBag.LopHocId = lopHocId; // nếu cần dùng lại trong view

            return View(data);
        }



        // ================== DETAILS ==================
        // GET: Student/LqvBuoiHocs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            Console.WriteLine($"🔍 [STUDENT][DETAILS] id={id}");

            if (id == null)
            {
                Console.WriteLine("❌ [DETAILS] id null");
                return NotFound();
            }

            var buoiHoc = await _context.LqvBuoiHocs
                .Include(x => x.LqvLopHoc)
                .FirstOrDefaultAsync(m => m.LqvBuoiHocId == id);

            if (buoiHoc == null)
            {
                Console.WriteLine($"❌ [DETAILS] Không tìm thấy buổi học id={id}");
                return NotFound();
            }

            Console.WriteLine($"✅ [DETAILS] Buổi học tồn tại | DangMo={buoiHoc.LqvDangMo}");
            Console.WriteLine($"📍 GPS: Lat={buoiHoc.LqvViDo}, Lng={buoiHoc.LqvKinhDo}, R={buoiHoc.LqvBanKinh}");

            return View(buoiHoc);
        }

        // ================== CHECK IN ==================
        // GET: Student/LqvBuoiHocs/CheckIn/5
        public async Task<IActionResult> CheckIn(int id)
        {
            Console.WriteLine($"📍 [STUDENT][CHECKIN] id={id}");

            var buoiHoc = await _context.LqvBuoiHocs
                .Include(x => x.LqvLopHoc)
                .FirstOrDefaultAsync(x => x.LqvBuoiHocId == id);

            if (!buoiHoc.LqvGioBatDau.HasValue)
            {
                TempData["Error"] = "Buổi học chưa cấu hình giờ bắt đầu.";
                return RedirectToAction(nameof(Index));
            }

            var thoiGianBatDau = buoiHoc.LqvNgayHoc
                .ToDateTime(buoiHoc.LqvGioBatDau.Value);


            var thoiGianKetThucDiemDanh = thoiGianBatDau.AddMinutes(15);
            var now = DateTime.Now;

            if (now > thoiGianKetThucDiemDanh || buoiHoc.LqvDangMo == false)
            {
                TempData["Error"] = "⛔ Đã hết thời gian điểm danh (15 phút đầu)";
                return RedirectToAction(nameof(Details), new { id });
            }


            // ===== TRUYỀN COUNTDOWN =====
            ViewBag.ThoiGianConLai = (int)(thoiGianKetThucDiemDanh - now).TotalSeconds;

            return View(buoiHoc);
        }

    }
}