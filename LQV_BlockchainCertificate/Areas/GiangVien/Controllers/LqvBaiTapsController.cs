using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LQV_BlockchainCertificate.Models.DBModel;

namespace LQV_BlockchainCertificate.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    [Authorize(Roles = "GiangVien")]
    public class LqvBaiTapsController : Controller
    {
        private readonly LqvDbContext _context;

        public LqvBaiTapsController(LqvDbContext context)
        {
            _context = context;
        }

        // =========================
        // 🔑 HELPER
        // =========================
        private int GetGiangVienId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        // =========================
        // 📌 INDEX – Bài tập của GV
        // =========================
        public async Task<IActionResult> Index()
        {
            int giangVienId = GetGiangVienId();

            var data = await _context.LqvBaiTaps
                .Include(b => b.LqvLopHoc)
                .Include(b => b.LqvNopBaiTaps) // 👈 cực quan trọng
                .Where(b => b.LqvGiangVienId == giangVienId)
                .AsNoTracking()
                .ToListAsync();

            var grouped = data
                .Select(b => new LqvBaiTapGroupVM
                {
                    LqvBaiTapId = b.LqvBaiTapId,
                    TieuDe = b.LqvTieuDe,
                    MoTa = b.LqvMoTa,
                    TenLop = b.LqvLopHoc.LqvTenLop,
                    HanNop = b.LqvHanNop,
                    TrangThai = b.LqvTrangThai,
                    TongBaiNop = b.LqvNopBaiTaps.Count
                })
                .OrderByDescending(x => x.HanNop)
                .ToList();

            return View(grouped);
        }

        public class LqvBaiTapGroupVM
        {
            public int LqvBaiTapId { get; set; }
            public string TieuDe { get; set; }
            public string MoTa { get; set; }
            public string TenLop { get; set; }
            public DateTime HanNop { get; set; }
            public string TrangThai { get; set; }

            public int TongBaiNop { get; set; }
        }

        // =========================
        // 📌 DETAILS
        // =========================
        public async Task<IActionResult> Details(int id)
        {
            int giangVienId = GetGiangVienId();

            var baiTap = await _context.LqvBaiTaps
                .Include(b => b.LqvLopHoc)
                .FirstOrDefaultAsync(b =>
                    b.LqvBaiTapId == id &&
                    b.LqvGiangVienId == giangVienId
                );

            if (baiTap == null)
                return NotFound();

            return View(baiTap);
        }

        // =========================
        // 📌 CREATE
        // =========================
        public async Task<IActionResult> Create()
        {
            int giangVienId = GetGiangVienId();

            // Chỉ lấy lớp của giảng viên
            var lopHocs = await _context.LqvLopHocs
                .Where(l => l.LqvGiangVienId == giangVienId)
                .Select(l => new
                {
                    l.LqvLopHocId,
                    Text = l.LqvTenLop
                })
                .ToListAsync();

            ViewData["LqvLopHocId"] = new SelectList(lopHocs, "LqvLopHocId", "Text");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LqvBaiTap model)
        {
            int giangVienId = GetGiangVienId();

            // Kiểm tra lớp có thuộc giảng viên không
            bool hopLe = await _context.LqvLopHocs.AnyAsync(l =>
                l.LqvLopHocId == model.LqvLopHocId &&
                l.LqvGiangVienId == giangVienId
            );

            if (!hopLe)
                return Forbid();

            model.LqvGiangVienId = giangVienId;
            model.LqvNgayTao = DateTime.Now;
            model.LqvTrangThai = "DangMo";

            _context.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // 📌 EDIT
        // =========================
        public async Task<IActionResult> Edit(int id)
        {
            int giangVienId = GetGiangVienId();

            var baiTap = await _context.LqvBaiTaps
                .FirstOrDefaultAsync(b =>
                    b.LqvBaiTapId == id &&
                    b.LqvGiangVienId == giangVienId
                );

            if (baiTap == null)
                return NotFound();

            // 👉 LOAD LỚP HỌC CỦA GIẢNG VIÊN
            var lopHocs = await _context.LqvLopHocs
                .Where(l => l.LqvGiangVienId == giangVienId)
                .Select(l => new
                {
                    l.LqvLopHocId,
                    l.LqvTenLop
                })
                .ToListAsync();

            ViewData["LqvLopHocId"] =
                new SelectList(lopHocs, "LqvLopHocId", "LqvTenLop", baiTap.LqvLopHocId);

            return View(baiTap);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LqvBaiTap model)
        {
            int giangVienId = GetGiangVienId();

            if (id != model.LqvBaiTapId)
                return NotFound();

            var baiTapDb = await _context.LqvBaiTaps
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.LqvBaiTapId == id &&
                    x.LqvGiangVienId == giangVienId
                );

            if (baiTapDb == null)
                return NotFound();

            // 🔒 GIỮ NGUYÊN FIELD KHÔNG CHO SỬA
            model.LqvNgayTao = baiTapDb.LqvNgayTao;
            model.LqvGiangVienId = giangVienId;

            // ❗ VALIDATE FK
            bool lopHopLe = await _context.LqvLopHocs.AnyAsync(l =>
                l.LqvLopHocId == model.LqvLopHocId &&
                l.LqvGiangVienId == giangVienId
            );

            if (!lopHopLe)
            {
                ModelState.AddModelError("LqvLopHocId", "Lớp học không hợp lệ");
            }

            if (!ModelState.IsValid)
            {
                // 🔁 LOAD LẠI DROPDOWN
                var lopHocs = await _context.LqvLopHocs
                    .Where(l => l.LqvGiangVienId == giangVienId)
                    .ToListAsync();

                ViewData["LqvLopHocId"] =
                    new SelectList(lopHocs, "LqvLopHocId", "LqvTenLop", model.LqvLopHocId);

                return View(model);
            }

            _context.Update(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // ❌ DELETE – TÙY MÀY
        // =========================
        public async Task<IActionResult> Delete(int id)
        {
            int giangVienId = GetGiangVienId();

            var baiTap = await _context.LqvBaiTaps.FirstOrDefaultAsync(b =>
                b.LqvBaiTapId == id &&
                b.LqvGiangVienId == giangVienId
            );

            if (baiTap == null)
                return NotFound();

            _context.LqvBaiTaps.Remove(baiTap);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
