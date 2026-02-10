using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LQV_BlockchainCertificate.Models.DBModel;

namespace LQV_BlockchainCertificate.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LqvDangKyLopHocsController : Controller
    {
        private readonly LqvDbContext _context;

        public LqvDangKyLopHocsController(LqvDbContext context)
        {
            _context = context;
        }

        // GET: Admin/LqvDangKyLopHocs
        public async Task<IActionResult> Index()
        {
            var lqvDbContext = _context.LqvDangKyLopHocs.Include(l => l.LqvLopHoc).Include(l => l.LqvSinhVien);
            return View(await lqvDbContext.ToListAsync());
        }

        // GET: Admin/LqvDangKyLopHocs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lqvDangKyLopHoc = await _context.LqvDangKyLopHocs
                .Include(l => l.LqvLopHoc)
                .Include(l => l.LqvSinhVien)
                .FirstOrDefaultAsync(m => m.LqvId == id);
            if (lqvDangKyLopHoc == null)
            {
                return NotFound();
            }

            return View(lqvDangKyLopHoc);
        }

        // GET: Admin/LqvDangKyLopHocs/Create
        public IActionResult Create()
        {
            ViewData["LqvLopHocId"] = new SelectList(_context.LqvLopHocs, "LqvLopHocId", "LqvLopHocId");
            ViewData["LqvSinhVienId"] = new SelectList(_context.LqvNguoiDungs, "LqvId", "LqvId");
            return View();
        }

        // POST: Admin/LqvDangKyLopHocs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LqvId,LqvSinhVienId,LqvLopHocId,LqvNgayDangKy")] LqvDangKyLopHoc lqvDangKyLopHoc)
        {
            if (ModelState.IsValid)
            {
                _context.Add(lqvDangKyLopHoc);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["LqvLopHocId"] = new SelectList(_context.LqvLopHocs, "LqvLopHocId", "LqvLopHocId", lqvDangKyLopHoc.LqvLopHocId);
            ViewData["LqvSinhVienId"] = new SelectList(_context.LqvNguoiDungs, "LqvId", "LqvId", lqvDangKyLopHoc.LqvSinhVienId);
            return View(lqvDangKyLopHoc);
        }

        // GET: Admin/LqvDangKyLopHocs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lqvDangKyLopHoc = await _context.LqvDangKyLopHocs.FindAsync(id);
            if (lqvDangKyLopHoc == null)
            {
                return NotFound();
            }
            ViewData["LqvLopHocId"] = new SelectList(_context.LqvLopHocs, "LqvLopHocId", "LqvLopHocId", lqvDangKyLopHoc.LqvLopHocId);
            ViewData["LqvSinhVienId"] = new SelectList(_context.LqvNguoiDungs, "LqvId", "LqvId", lqvDangKyLopHoc.LqvSinhVienId);
            return View(lqvDangKyLopHoc);
        }

        // POST: Admin/LqvDangKyLopHocs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LqvId,LqvSinhVienId,LqvLopHocId,LqvNgayDangKy")] LqvDangKyLopHoc lqvDangKyLopHoc)
        {
            if (id != lqvDangKyLopHoc.LqvId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(lqvDangKyLopHoc);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LqvDangKyLopHocExists(lqvDangKyLopHoc.LqvId))
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
            ViewData["LqvLopHocId"] = new SelectList(_context.LqvLopHocs, "LqvLopHocId", "LqvLopHocId", lqvDangKyLopHoc.LqvLopHocId);
            ViewData["LqvSinhVienId"] = new SelectList(_context.LqvNguoiDungs, "LqvId", "LqvId", lqvDangKyLopHoc.LqvSinhVienId);
            return View(lqvDangKyLopHoc);
        }

        // GET: Admin/LqvDangKyLopHocs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lqvDangKyLopHoc = await _context.LqvDangKyLopHocs
                .Include(l => l.LqvLopHoc)
                .Include(l => l.LqvSinhVien)
                .FirstOrDefaultAsync(m => m.LqvId == id);
            if (lqvDangKyLopHoc == null)
            {
                return NotFound();
            }

            return View(lqvDangKyLopHoc);
        }

        // POST: Admin/LqvDangKyLopHocs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lqvDangKyLopHoc = await _context.LqvDangKyLopHocs.FindAsync(id);
            if (lqvDangKyLopHoc != null)
            {
                _context.LqvDangKyLopHocs.Remove(lqvDangKyLopHoc);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LqvDangKyLopHocExists(int id)
        {
            return _context.LqvDangKyLopHocs.Any(e => e.LqvId == id);
        }
    }
}
