using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LQV_BlockchainCertificate.Models.DBModel;

namespace LQV_BlockchainCertificate.Areas.Student.Controllers
{
    [Area("Student")]
    public class LqvDeThisController : Controller
    {
        private readonly LqvDbContext _context;

        public LqvDeThisController(LqvDbContext context)
        {
            _context = context;
        }

        // GET: Student/LqvDeThis
        public async Task<IActionResult> Index()
        {
            var lqvDbContext = _context.LqvDeThis.Include(l => l.LqvBoCauHoi).Include(l => l.LqvGiangVien);
            return View(await lqvDbContext.ToListAsync());
        }

        // GET: Student/LqvDeThis/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lqvDeThi = await _context.LqvDeThis
                .Include(l => l.LqvBoCauHoi)
                .Include(l => l.LqvGiangVien)
                .FirstOrDefaultAsync(m => m.LqvDeThiId == id);
            if (lqvDeThi == null)
            {
                return NotFound();
            }

            return View(lqvDeThi);
        }

        // GET: Student/LqvDeThis/Create
        public IActionResult Create()
        {
            ViewData["LqvBoCauHoiId"] = new SelectList(_context.LqvBoCauHois, "LqvBoCauHoiId", "LqvBoCauHoiId");
            ViewData["LqvGiangVienId"] = new SelectList(_context.LqvNguoiDungs, "LqvId", "LqvId");
            return View();
        }

        // POST: Student/LqvDeThis/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LqvDeThiId,LqvTenDeThi,LqvBoCauHoiId,LqvThoiGianThi,LqvTongDiem,LqvDaDuyet,LqvNgayDuyet,LqvGiangVienId")] LqvDeThi lqvDeThi)
        {
            if (ModelState.IsValid)
            {
                _context.Add(lqvDeThi);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["LqvBoCauHoiId"] = new SelectList(_context.LqvBoCauHois, "LqvBoCauHoiId", "LqvBoCauHoiId", lqvDeThi.LqvBoCauHoiId);
            ViewData["LqvGiangVienId"] = new SelectList(_context.LqvNguoiDungs, "LqvId", "LqvId", lqvDeThi.LqvGiangVienId);
            return View(lqvDeThi);
        }

        // GET: Student/LqvDeThis/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lqvDeThi = await _context.LqvDeThis.FindAsync(id);
            if (lqvDeThi == null)
            {
                return NotFound();
            }
            ViewData["LqvBoCauHoiId"] = new SelectList(_context.LqvBoCauHois, "LqvBoCauHoiId", "LqvBoCauHoiId", lqvDeThi.LqvBoCauHoiId);
            ViewData["LqvGiangVienId"] = new SelectList(_context.LqvNguoiDungs, "LqvId", "LqvId", lqvDeThi.LqvGiangVienId);
            return View(lqvDeThi);
        }

        // POST: Student/LqvDeThis/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LqvDeThiId,LqvTenDeThi,LqvBoCauHoiId,LqvThoiGianThi,LqvTongDiem,LqvDaDuyet,LqvNgayDuyet,LqvGiangVienId")] LqvDeThi lqvDeThi)
        {
            if (id != lqvDeThi.LqvDeThiId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(lqvDeThi);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LqvDeThiExists(lqvDeThi.LqvDeThiId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["LqvBoCauHoiId"] = new SelectList(_context.LqvBoCauHois, "LqvBoCauHoiId", "LqvBoCauHoiId", lqvDeThi.LqvBoCauHoiId);
            ViewData["LqvGiangVienId"] = new SelectList(_context.LqvNguoiDungs, "LqvId", "LqvId", lqvDeThi.LqvGiangVienId);
            return View(lqvDeThi);
        }

        // GET: Student/LqvDeThis/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lqvDeThi = await _context.LqvDeThis
                .Include(l => l.LqvBoCauHoi)
                .Include(l => l.LqvGiangVien)
                .FirstOrDefaultAsync(m => m.LqvDeThiId == id);
            if (lqvDeThi == null)
            {
                return NotFound();
            }

            return View(lqvDeThi);
        }

        // POST: Student/LqvDeThis/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lqvDeThi = await _context.LqvDeThis.FindAsync(id);
            if (lqvDeThi != null)
            {
                _context.LqvDeThis.Remove(lqvDeThi);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LqvDeThiExists(int id)
        {
            return _context.LqvDeThis.Any(e => e.LqvDeThiId == id);
        }
    }
}
