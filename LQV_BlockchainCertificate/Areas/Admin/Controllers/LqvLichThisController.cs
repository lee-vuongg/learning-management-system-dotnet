using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LQV_BlockchainCertificate.Models.DBModel;
using Microsoft.AspNetCore.Authorization;

namespace LQV_BlockchainCertificate.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class LqvLichThisController : Controller
    {
        private readonly LqvDbContext _context;

        public LqvLichThisController(LqvDbContext context)
        {
            _context = context;
        }

        // ===================== INDEX =====================
        public async Task<IActionResult> Index()
        {
            var lichThi = await _context.LqvLichThis
                .Include(l => l.LqvDeThi)
                .Include(l => l.LqvLopHoc)
                .OrderByDescending(l => l.LqvBatDau)
                .ToListAsync();

            return View(lichThi);
        }

        // ===================== DETAILS =====================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var lichThi = await _context.LqvLichThis
                .Include(l => l.LqvDeThi)
                .Include(l => l.LqvLopHoc)
                .FirstOrDefaultAsync(l => l.LqvLichThiId == id);

            if (lichThi == null) return NotFound();

            return View(lichThi);
        }

        // ===================== CREATE (GET) =====================
        public IActionResult Create()
        {
            ViewBag.LqvDeThiId = new SelectList(
                _context.LqvDeThis.Where(d => d.LqvDaDuyet),
                "LqvDeThiId",
                "LqvTenDeThi"
            );

            ViewBag.LqvLopHocId = new SelectList(
                _context.LqvLopHocs,
                "LqvLopHocId",
                "LqvTenLop"
            );

            return View();
        }

        // ===================== CREATE (POST) =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LqvLichThi model)
        {
            // 🔧 FIX: bỏ giây + mili giây
            var nowRaw = DateTime.Now;
            var now = new DateTime(
                nowRaw.Year,
                nowRaw.Month,
                nowRaw.Day,
                nowRaw.Hour,
                nowRaw.Minute,
                0
            );

            // ================== CHECK ĐỀ THI ==================
            var deThi = await _context.LqvDeThis
                .FirstOrDefaultAsync(d => d.LqvDeThiId == model.LqvDeThiId);

            if (deThi == null || !deThi.LqvDaDuyet)
                ModelState.AddModelError("", "⛔ Đề thi chưa được duyệt");

            // ================== CHECK THỜI GIAN ==================
            if (model.LqvBatDau < now)
                ModelState.AddModelError("", "❌ Thời gian bắt đầu phải >= thời điểm hiện tại");

            if (model.LqvKetThuc <= model.LqvBatDau)
                ModelState.AddModelError("", "❌ Thời gian kết thúc phải lớn hơn thời gian bắt đầu");

            // ================== CHECK TRÙNG LỊCH CÙNG LỚP ==================
            bool trungLich = await _context.LqvLichThis.AnyAsync(l =>
                l.LqvLopHocId == model.LqvLopHocId &&
                model.LqvBatDau < l.LqvKetThuc &&
                model.LqvKetThuc > l.LqvBatDau
            );

            if (trungLich)
                ModelState.AddModelError("", "❌ Lớp này đã có lịch thi trùng thời gian");

            // ================== SAVE ==================
            if (ModelState.IsValid)
            {
                _context.LqvLichThis.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // ================== RETURN VIEW ==================
            ViewBag.LqvDeThiId = new SelectList(
                _context.LqvDeThis.Where(d => d.LqvDaDuyet),
                "LqvDeThiId",
                "LqvTenDeThi",
                model.LqvDeThiId
            );

            ViewBag.LqvLopHocId = new SelectList(
                _context.LqvLopHocs,
                "LqvLopHocId",
                "LqvTenLop",
                model.LqvLopHocId
            );

            return View(model);
        }

        // ===================== EDIT (GET) =====================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var lichThi = await _context.LqvLichThis.FindAsync(id);
            if (lichThi == null) return NotFound();

            ViewBag.LqvDeThiId = new SelectList(
                _context.LqvDeThis.Where(d => d.LqvDaDuyet),
                "LqvDeThiId",
                "LqvTenDeThi",
                lichThi.LqvDeThiId
            );

            ViewBag.LqvLopHocId = new SelectList(
                _context.LqvLopHocs,
                "LqvLopHocId",
                "LqvTenLop",
                lichThi.LqvLopHocId
            );

            return View(lichThi);
        }

        // ===================== EDIT (POST) =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LqvLichThi model)
        {
            if (id != model.LqvLichThiId) return NotFound();

            if (model.LqvKetThuc <= model.LqvBatDau)
                ModelState.AddModelError("", "Thời gian kết thúc phải lớn hơn thời gian bắt đầu");

            if (ModelState.IsValid)
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.LqvDeThiId = new SelectList(
                _context.LqvDeThis.Where(d => d.LqvDaDuyet),
                "LqvDeThiId",
                "LqvTenDeThi",
                model.LqvDeThiId
            );

            ViewBag.LqvLopHocId = new SelectList(
                _context.LqvLopHocs,
                "LqvLopHocId",
                "LqvTenLop",
                model.LqvLopHocId
            );

            return View(model);
        }

        // ===================== DELETE =====================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var lichThi = await _context.LqvLichThis
                .Include(l => l.LqvDeThi)
                .Include(l => l.LqvLopHoc)
                .FirstOrDefaultAsync(l => l.LqvLichThiId == id);

            if (lichThi == null) return NotFound();

            return View(lichThi);
        }

        // ===================== DELETE CONFIRMED =====================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lichThi = await _context.LqvLichThis.FindAsync(id);
            if (lichThi != null)
            {
                _context.LqvLichThis.Remove(lichThi);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
