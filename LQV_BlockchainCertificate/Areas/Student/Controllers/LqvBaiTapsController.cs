using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LQV_BlockchainCertificate.Models.DBModel;

namespace LQV_BlockchainCertificate.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "SinhVien")]
    public class LqvBaiTapsController : Controller
    {
        private readonly LqvDbContext _context;

        public LqvBaiTapsController(LqvDbContext context)
        {
            _context = context;
        }

        // ======================================================
        // 1. DANH SÁCH BÀI TẬP THEO LỚP SINH VIÊN ĐÃ ĐĂNG KÝ
        // ======================================================
        public async Task<IActionResult> Index()
        {
            int sinhVienId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // 1️⃣ LẤY LỚP SV ĐÃ ĐĂNG KÝ
            var lopHocIds = await _context.LqvDangKyLopHocs
                .Where(x => x.LqvSinhVienId == sinhVienId)
                .Select(x => x.LqvLopHocId)
                .ToListAsync();

            if (!lopHocIds.Any())
                return View(Enumerable.Empty<LqvBaiTap>());

            // 2️⃣ LOAD TOÀN BỘ DATA CẦN THIẾT (NO FILTER DB)
            var baiTapsAll = await _context.LqvBaiTaps.AsNoTracking().ToListAsync();
            var nguoiDungsAll = await _context.LqvNguoiDungs.AsNoTracking().ToListAsync();
            var lopHocsAll = await _context.LqvLopHocs.AsNoTracking().ToListAsync();
            var nopBaisAll = await _context.LqvNopBaiTaps
                .Where(x => x.LqvSinhVienId == sinhVienId)
                .AsNoTracking()
                .ToListAsync();

            // 3️⃣ FILTER TRONG RAM
            var baiTaps = baiTapsAll
                .Where(bt => lopHocIds.Contains(bt.LqvLopHocId))
                .ToList();

            // 4️⃣ GẮN NAVIGATION
            foreach (var bt in baiTaps)
            {
                bt.LqvGiangVien = nguoiDungsAll
                    .FirstOrDefault(u => u.LqvId == bt.LqvGiangVienId);

                bt.LqvLopHoc = lopHocsAll
                    .FirstOrDefault(lh => lh.LqvLopHocId == bt.LqvLopHocId);

                bt.LqvNopBaiTaps = nopBaisAll
                    .Where(nb => nb.LqvBaiTapId == bt.LqvBaiTapId)
                    .ToList();
            }

            // 5️⃣ SORT
            baiTaps = baiTaps
                .OrderByDescending(x => x.LqvHanNop)
                .ToList();

            return View(baiTaps);
        }



        // ======================================================
        // 2. CHI TIẾT BÀI TẬP + TRẠNG THÁI NỘP
        // ======================================================
        public async Task<IActionResult> Details(int id)
        {
            int sinhVienId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var baiTap = await _context.LqvBaiTaps
                .Include(bt => bt.LqvGiangVien)
                .Include(bt => bt.LqvLopHoc)
                .FirstOrDefaultAsync(bt => bt.LqvBaiTapId == id);

            if (baiTap == null)
                return NotFound();

            var nopBai = await _context.LqvNopBaiTaps
                .FirstOrDefaultAsync(nb =>
                    nb.LqvBaiTapId == id &&
                    nb.LqvSinhVienId == sinhVienId);

            ViewBag.DaNop = nopBai != null;
            ViewBag.NopBai = nopBai;
            ViewBag.ConHan = DateTime.Now <= baiTap.LqvHanNop;

            return View(baiTap);
        }

        // ======================================================
        // 3. FORM NỘP BÀI
        // ======================================================
        public async Task<IActionResult> NopBai(int id)
        {
            int sinhVienId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var baiTap = await _context.LqvBaiTaps.FindAsync(id);
            if (baiTap == null)
                return NotFound();

            if (DateTime.Now > baiTap.LqvHanNop)
            {
                TempData["Error"] = "❌ Bài tập đã quá hạn nộp!";
                return RedirectToAction(nameof(Details), new { id });
            }

            bool daNop = await _context.LqvNopBaiTaps
                .AnyAsync(x => x.LqvBaiTapId == id && x.LqvSinhVienId == sinhVienId);

            if (daNop)
            {
                TempData["Error"] = "⚠️ Bạn đã nộp bài này rồi!";
                return RedirectToAction(nameof(Details), new { id });
            }

            ViewBag.BaiTap = baiTap;
            return View();
        }

        // ======================================================
        // 4. XỬ LÝ NỘP BÀI
        // ======================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NopBai(
     int baiTapId,
     IFormFile? fileNop,
     string? noiDung)
        {
            int sinhVienId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var baiTap = await _context.LqvBaiTaps.FindAsync(baiTapId);
            if (baiTap == null)
                return NotFound();

            // ❌ Quá hạn
            if (DateTime.Now > baiTap.LqvHanNop)
            {
                TempData["Error"] = "❌ Không thể nộp vì đã quá hạn!";
                return RedirectToAction(nameof(Details), new { id = baiTapId });
            }

            if (fileNop == null || fileNop.Length == 0)
            {
                TempData["Error"] = "⚠️ Vui lòng chọn file!";
                return RedirectToAction(nameof(Details), new { id = baiTapId });
            }

            // ===== KIỂM TRA ĐÃ NỘP CHƯA =====
            var nopBai = await _context.LqvNopBaiTaps
                .FirstOrDefaultAsync(x =>
                    x.LqvBaiTapId == baiTapId &&
                    x.LqvSinhVienId == sinhVienId);

            // ===== LƯU FILE =====
            string folderPath = Path.Combine("wwwroot/uploads/baitap", baiTapId.ToString(), sinhVienId.ToString());
            Directory.CreateDirectory(folderPath);

            string fileName = $"{DateTime.Now:yyyyMMddHHmmss}_{Path.GetFileName(fileNop.FileName)}";
            string fullPath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await fileNop.CopyToAsync(stream);
            }

            string filePathDb = $"/uploads/baitap/{baiTapId}/{sinhVienId}/{fileName}";

            // ===== CHƯA NỘP → TẠO MỚI =====
            if (nopBai == null)
            {
                nopBai = new LqvNopBaiTap
                {
                    LqvBaiTapId = baiTapId,
                    LqvSinhVienId = sinhVienId,
                    LqvNoiDung = noiDung,
                    LqvFile = filePathDb,
                    LqvThoiGianNop = DateTime.Now,
                    LqvDaCham = false
                };

                _context.LqvNopBaiTaps.Add(nopBai);
            }
            else
            {
                // 🔁 NỘP LẠI
                nopBai.LqvNoiDung = noiDung;
                nopBai.LqvFile = filePathDb;
                nopBai.LqvThoiGianNop = DateTime.Now;

                // reset chấm
                nopBai.LqvDaCham = false;
                nopBai.LqvDiem = null;
                nopBai.LqvNhanXet = null;

                _context.LqvNopBaiTaps.Update(nopBai);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "✅ Nộp bài thành công!";
            return RedirectToAction(nameof(Details), new { id = baiTapId });
        }

    }
}
