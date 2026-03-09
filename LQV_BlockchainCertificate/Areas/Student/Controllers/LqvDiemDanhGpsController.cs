using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LQV_BlockchainCertificate.Models.DBModel;

namespace LQV_BlockchainCertificate.Areas.Student.Controllers
{
    [Area("Student")]
    public class LqvDiemDanhGpsController : Controller
    {
        private readonly LqvDbContext _context;

        public LqvDiemDanhGpsController(LqvDbContext context)
        {
            _context = context;
            Console.WriteLine("🔥 [INIT][STUDENT] LqvDiemDanhGpsController khởi tạo");
        }

        // ===============================
        // 🔐 ID SINH VIÊN
        // ===============================
        private int GetCurrentStudentId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Console.WriteLine($"👤 [AUTH] Claim NameIdentifier = {idStr}");

            if (!int.TryParse(idStr, out var id))
            {
                Console.WriteLine("❌ [AUTH] Không parse được StudentId");
                return 0;
            }

            return id;
        }

        // ===============================
        // 📍 LIST
        // ===============================
        public async Task<IActionResult> Index()
        {
            int studentId = GetCurrentStudentId();

            var data = await _context.LqvDiemDanhGps
                .Include(x => x.LqvBuoiHoc)
                    .ThenInclude(b => b.LqvLopHoc)
                .Where(x => x.LqvSinhVienId == studentId)
                .OrderByDescending(x => x.LqvThoiGian)
                .AsNoTracking()
                .ToListAsync();

            return View(data);
        }

        // ===============================
        // 📍 CHECKIN GET
        // ===============================
        public async Task<IActionResult> CheckIn(int buoiHocId)
        {
            int studentId = GetCurrentStudentId();

            var buoiHoc = await _context.LqvBuoiHocs
                .Include(b => b.LqvLopHoc)
                .FirstOrDefaultAsync(b => b.LqvBuoiHocId == buoiHocId);

            if (buoiHoc == null)
                return NotFound();

            if (!buoiHoc.LqvDangMo)
            {
                TempData["ErrorMessage"] = "Buổi học chưa mở hoặc đã đóng điểm danh.";
                return RedirectToAction("Details", "LqvBuoiHocs", new { id = buoiHocId });
            }

            bool daDiemDanh = await _context.LqvDiemDanhGps.AnyAsync(x =>
                x.LqvBuoiHocId == buoiHocId &&
                x.LqvSinhVienId == studentId);

            if (daDiemDanh)
            {
                TempData["ErrorMessage"] = "Bạn đã điểm danh buổi học này rồi.";
                return RedirectToAction("Details", "LqvBuoiHocs", new { id = buoiHocId });
            }

            ViewBag.BuoiHoc = buoiHoc;

            return View();
        }

        // ===============================
        // 📍 CHECKIN POST
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(
            int buoiHocId,
            string viDo,
            string kinhDo,
            IFormFile faceImage
        )
        {
            int studentId = GetCurrentStudentId();

            if (faceImage == null || faceImage.Length == 0)
            {
                TempData["ErrorMessage"] = "Chưa chụp ảnh khuôn mặt.";
                return RedirectToAction("Details", "LqvBuoiHocs", new { id = buoiHocId });
            }

            // ===== LƯU ẢNH =====
            try
            {
                string folder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "checkin-faces"
                );

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string fileName =
                    $"checkin_{studentId}_{buoiHocId}_{DateTime.Now:yyyyMMddHHmmss}.jpg";

                string path = Path.Combine(folder, fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await faceImage.CopyToAsync(stream);
                }

                Console.WriteLine($"📷 Image saved: {fileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Save image error: " + ex.Message);
            }

            // ===== LẤY BUỔI HỌC =====
            var buoiHoc = await _context.LqvBuoiHocs
                .FirstOrDefaultAsync(b => b.LqvBuoiHocId == buoiHocId);

            if (buoiHoc == null)
            {
                TempData["ErrorMessage"] = "Buổi học không tồn tại.";
                return RedirectToAction("Index");
            }

            // ===== GPS =====
            double lat = double.Parse(viDo.Replace(",", "."), CultureInfo.InvariantCulture);
            double lng = double.Parse(kinhDo.Replace(",", "."), CultureInfo.InvariantCulture);

            double distance = CalculateDistance(
                lat, lng,
                buoiHoc.LqvViDo!.Value,
                buoiHoc.LqvKinhDo!.Value
            );

            double banKinh = buoiHoc.LqvBanKinh ?? 50;

            bool hopLe = distance <= banKinh;

            var diemDanh = new LqvDiemDanhGp
            {
                LqvSinhVienId = studentId,
                LqvLopHocId = buoiHoc.LqvLopHocId,
                LqvBuoiHocId = buoiHocId,
                LqvViDo = lat,
                LqvKinhDo = lng,
                LqvThoiGian = DateTime.Now,
                LqvHopLe = hopLe
            };

            _context.LqvDiemDanhGps.Add(diemDanh);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "✔ Điểm danh thành công";

            return RedirectToAction("Details", "LqvBuoiHocs", new { id = buoiHocId });
        }

        // ===============================
        // 📐 HAVERSINE
        // ===============================
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371000;

            double dLat = ToRad(lat2 - lat1);
            double dLon = ToRad(lon2 - lon1);

            double a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            return R * (2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a)));
        }

        private double ToRad(double value) => value * Math.PI / 180;
    }
}