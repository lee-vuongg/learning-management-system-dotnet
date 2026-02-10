using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LQV_BlockchainCertificate.Models.DBModel;
using LQV_BlockchainCertificate.Services;

namespace LQV_BlockchainCertificate.Areas.Student.Controllers
{
    [Area("Student")]
    public class LqvDiemDanhGpsController : Controller
    {
        private readonly LqvDbContext _context;
        private readonly GeminiService _gemini;

        public LqvDiemDanhGpsController(LqvDbContext context, GeminiService gemini)
        {
            _context = context;
            _gemini = gemini;
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

            Console.WriteLine($"✅ [AUTH] StudentId = {id}");
            return id;
        }
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
            Console.WriteLine($"➡️ [GET][CHECKIN] BuoiHocId = {buoiHocId}");

            int studentId = GetCurrentStudentId();

            var buoiHoc = await _context.LqvBuoiHocs
                .Include(b => b.LqvLopHoc)
                .FirstOrDefaultAsync(b => b.LqvBuoiHocId == buoiHocId);

            if (buoiHoc == null)
            {
                Console.WriteLine("❌ [GET][CHECKIN] Không tìm thấy buổi học");
                return NotFound();
            }

            Console.WriteLine($"📘 [BUOIHOC] DangMo = {buoiHoc.LqvDangMo}");
            Console.WriteLine($"📍 [BUOIHOC] Tâm GPS = {buoiHoc.LqvViDo}, {buoiHoc.LqvKinhDo}");
            Console.WriteLine($"📏 [BUOIHOC] Bán kính = {buoiHoc.LqvBanKinh}");

            if (!buoiHoc.LqvDangMo)
            {
                Console.WriteLine("⛔ [GET][CHECKIN] Buổi học đã đóng");
                TempData["ErrorMessage"] = "Buổi học chưa mở hoặc đã đóng điểm danh.";
                return RedirectToAction("Details", "LqvBuoiHocs", new { id = buoiHocId });
            }

            bool daDiemDanh = await _context.LqvDiemDanhGps.AnyAsync(x =>
                x.LqvBuoiHocId == buoiHocId &&
                x.LqvSinhVienId == studentId);

            Console.WriteLine($"🧾 [CHECK] Đã điểm danh chưa = {daDiemDanh}");

            if (daDiemDanh)
            {
                TempData["ErrorMessage"] = "Bạn đã điểm danh buổi học này rồi.";
                return RedirectToAction("Details", "LqvBuoiHocs", new { id = buoiHocId });
            }

            ViewBag.BuoiHoc = buoiHoc;
            return View();
        }
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

            // ===== FACE VERIFY =====
            if (faceImage == null || faceImage.Length == 0)
            {
                TempData["ErrorMessage"] = "Chưa quét khuôn mặt";
                return RedirectToAction("Details", "LqvBuoiHocs", new { id = buoiHocId });
            }

            using var ms = new MemoryStream();
            await faceImage.CopyToAsync(ms);

            bool faceOk = await _gemini.VerifyFaceAsync(ms.ToArray());

            if (!faceOk)
            {
                TempData["ErrorMessage"] = "AI không xác nhận khuôn mặt";
                return RedirectToAction("Details", "LqvBuoiHocs", new { id = buoiHocId });
            }

            // =========================================================
            // 🔥 THÊM ĐOẠN NÀY: LƯU ẢNH CHECKIN ĐÚNG BUỔI
            // =========================================================
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

                System.IO.File.WriteAllBytes(path, ms.ToArray());

                Console.WriteLine("------ SAVE CHECKIN IMAGE ------");
                Console.WriteLine($"BUOI: {buoiHocId}");
                Console.WriteLine($"FILE: {fileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ SAVE IMAGE ERROR: " + ex.Message);
            }
            // =========================================================

            // ===== LẤY BUỔI HỌC =====
            var buoiHoc = await _context.LqvBuoiHocs
                .AsNoTracking()
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

            TempData["success"] = "Điểm danh thành công ✔";
            TempData["SuccessMessage"] = "✔ FaceID + GPS OK";

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
