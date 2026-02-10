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
    public class LqvDeThisController : Controller
    {
        private readonly LqvDbContext _context;

        public LqvDeThisController(LqvDbContext context)
        {
            _context = context;
        }

        // ================== HÀM LẤY ID GIẢNG VIÊN ==================
        private int GetGiangVienId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        // ================== INDEX ==================
        // CHỈ HIỂN THỊ ĐỀ THI CỦA GIẢNG VIÊN ĐANG ĐĂNG NHẬP
        public async Task<IActionResult> Index()
        {
            int giangVienId = GetGiangVienId();

            var data = await _context.LqvDeThis
                .Include(d => d.LqvBoCauHoi)
                .Where(d => d.LqvGiangVienId == giangVienId)
                .OrderByDescending(d => d.LqvDeThiId)
                .ToListAsync();

            return View(data);
        }

        // ================== DETAILS ==================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            int giangVienId = GetGiangVienId();

            var deThi = await _context.LqvDeThis
                .Include(d => d.LqvBoCauHoi)
                .ThenInclude(b => b.LqvCauHois)
                .FirstOrDefaultAsync(d =>
                    d.LqvDeThiId == id &&
                    d.LqvGiangVienId == giangVienId);

            if (deThi == null) return NotFound();

            return View(deThi);
        }

        // ================== CREATE (GET) ==================
        public IActionResult Create()
        {
            ViewBag.LqvBoCauHoiId = new SelectList(
                _context.LqvBoCauHois,
                "LqvBoCauHoiId",
                "LqvTenBo"
            );

            return View();
        }

        // ================== CREATE (POST) ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LqvDeThi model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.LqvBoCauHoiId = new SelectList(
                    _context.LqvBoCauHois,
                    "LqvBoCauHoiId",
                    "LqvTenBo",
                    model.LqvBoCauHoiId
                );
                return View(model);
            }

            model.LqvGiangVienId = GetGiangVienId();
            model.LqvDaDuyet = false;
            model.LqvNgayDuyet = null;
            model.LqvTongDiem = await TinhTongDiemBoCauHoi(model.LqvBoCauHoiId);

            _context.LqvDeThis.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ================== EDIT (GET) ==================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            int giangVienId = GetGiangVienId();

            var deThi = await _context.LqvDeThis
                .FirstOrDefaultAsync(d =>
                    d.LqvDeThiId == id &&
                    d.LqvGiangVienId == giangVienId);

            if (deThi == null) return NotFound();

            if (deThi.LqvDaDuyet)
            {
                TempData["Error"] = "❌ Đề thi đã được duyệt, không thể chỉnh sửa!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.LqvBoCauHoiId = new SelectList(
                _context.LqvBoCauHois,
                "LqvBoCauHoiId",
                "LqvTenBo",
                deThi.LqvBoCauHoiId
            );

            return View(deThi);
        }

        // ================== EDIT (POST) ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LqvDeThi model)
        {
            if (id != model.LqvDeThiId) return NotFound();

            int giangVienId = GetGiangVienId();

            var deThi = await _context.LqvDeThis
                .FirstOrDefaultAsync(d =>
                    d.LqvDeThiId == id &&
                    d.LqvGiangVienId == giangVienId);

            if (deThi == null) return NotFound();

            if (deThi.LqvDaDuyet)
            {
                TempData["Error"] = "❌ Đề thi đã được duyệt!";
                return RedirectToAction(nameof(Index));
            }

            int oldBoCauHoiId = deThi.LqvBoCauHoiId;

            deThi.LqvTenDeThi = model.LqvTenDeThi;
            deThi.LqvBoCauHoiId = model.LqvBoCauHoiId;
            deThi.LqvThoiGianThi = model.LqvThoiGianThi;
            deThi.LqvTongDiem = await TinhTongDiemBoCauHoi(model.LqvBoCauHoiId);

            if (oldBoCauHoiId != model.LqvBoCauHoiId)
            {
                deThi.LqvDaDuyet = false;
                deThi.LqvNgayDuyet = null;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ================== DELETE ==================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            int giangVienId = GetGiangVienId();

            var deThi = await _context.LqvDeThis
                .Include(d => d.LqvBoCauHoi)
                .FirstOrDefaultAsync(d =>
                    d.LqvDeThiId == id &&
                    d.LqvGiangVienId == giangVienId);

            if (deThi == null) return NotFound();

            if (deThi.LqvDaDuyet)
            {
                TempData["Error"] = "❌ Đề thi đã được duyệt, không thể xoá!";
                return RedirectToAction(nameof(Index));
            }

            return View(deThi);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            int giangVienId = GetGiangVienId();

            var deThi = await _context.LqvDeThis
                .FirstOrDefaultAsync(d =>
                    d.LqvDeThiId == id &&
                    d.LqvGiangVienId == giangVienId);

            if (deThi == null) return NotFound();

            if (deThi.LqvDaDuyet)
            {
                TempData["Error"] = "❌ Đề thi đã được duyệt!";
                return RedirectToAction(nameof(Index));
            }

            _context.LqvDeThis.Remove(deThi);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ================== GỬI DUYỆT ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuiDuyet(int id)
        {
            int giangVienId = GetGiangVienId();

            var deThi = await _context.LqvDeThis
                .FirstOrDefaultAsync(d =>
                    d.LqvDeThiId == id &&
                    d.LqvGiangVienId == giangVienId);

            if (deThi == null) return NotFound();

            if (deThi.LqvDaDuyet)
            {
                TempData["Error"] = "Đề thi đã được duyệt!";
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = "✅ Đã gửi đề thi lên Admin để duyệt";
            return RedirectToAction(nameof(Index));
        }

        // ================== HÀM TÍNH TỔNG ĐIỂM ==================
        private async Task<double> TinhTongDiemBoCauHoi(int boCauHoiId)
        {
            return await _context.LqvCauHois
                .Where(c => c.LqvBoCauHoiId == boCauHoiId)
                .SumAsync(c => c.LqvDiem);
        }
    }
}
