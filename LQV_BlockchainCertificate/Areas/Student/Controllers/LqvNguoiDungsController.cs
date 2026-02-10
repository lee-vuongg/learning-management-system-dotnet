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
    public class LqvNguoiDungsController : Controller
    {
        private readonly LqvDbContext _context;

        public LqvNguoiDungsController(LqvDbContext context)
        {
            _context = context;
        }

        // GET: Student/LqvNguoiDungs
        public async Task<IActionResult> Index()
        {
            var lqvDbContext = _context.LqvNguoiDungs.Include(l => l.LqvRole);
            return View(await lqvDbContext.ToListAsync());
        }

        // GET: Student/LqvNguoiDungs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lqvNguoiDung = await _context.LqvNguoiDungs
                .Include(l => l.LqvRole)
                .FirstOrDefaultAsync(m => m.LqvId == id);
            if (lqvNguoiDung == null)
            {
                return NotFound();
            }

            return View(lqvNguoiDung);
        }

        // GET: Student/LqvNguoiDungs/Create
        public IActionResult Create()
        {
            ViewData["LqvRoleId"] = new SelectList(_context.LqvRoles, "LqvRoleId", "LqvRoleId");
            return View();
        }

        // POST: Student/LqvNguoiDungs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LqvId,LqvTenDangNhap,LqvMatKhauHash,LqvHoTen,LqvEmail,LqvAvt,LqvRoleId,LqvWalletAddress,LqvNgayTao,LqvDaXacThuc")] LqvNguoiDung lqvNguoiDung)
        {
            if (ModelState.IsValid)
            {
                _context.Add(lqvNguoiDung);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["LqvRoleId"] = new SelectList(_context.LqvRoles, "LqvRoleId", "LqvRoleId", lqvNguoiDung.LqvRoleId);
            return View(lqvNguoiDung);
        }

        // GET: Student/LqvNguoiDungs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lqvNguoiDung = await _context.LqvNguoiDungs.FindAsync(id);
            if (lqvNguoiDung == null)
            {
                return NotFound();
            }
            ViewData["LqvRoleId"] = new SelectList(_context.LqvRoles, "LqvRoleId", "LqvRoleId", lqvNguoiDung.LqvRoleId);
            return View(lqvNguoiDung);
        }

        // POST: Student/LqvNguoiDungs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LqvId,LqvTenDangNhap,LqvMatKhauHash,LqvHoTen,LqvEmail,LqvAvt,LqvRoleId,LqvWalletAddress,LqvNgayTao,LqvDaXacThuc")] LqvNguoiDung lqvNguoiDung)
        {
            if (id != lqvNguoiDung.LqvId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(lqvNguoiDung);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LqvNguoiDungExists(lqvNguoiDung.LqvId))
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
            ViewData["LqvRoleId"] = new SelectList(_context.LqvRoles, "LqvRoleId", "LqvRoleId", lqvNguoiDung.LqvRoleId);
            return View(lqvNguoiDung);
        }

        // GET: Student/LqvNguoiDungs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lqvNguoiDung = await _context.LqvNguoiDungs
                .Include(l => l.LqvRole)
                .FirstOrDefaultAsync(m => m.LqvId == id);
            if (lqvNguoiDung == null)
            {
                return NotFound();
            }

            return View(lqvNguoiDung);
        }

        // POST: Student/LqvNguoiDungs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lqvNguoiDung = await _context.LqvNguoiDungs.FindAsync(id);
            if (lqvNguoiDung != null)
            {
                _context.LqvNguoiDungs.Remove(lqvNguoiDung);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LqvNguoiDungExists(int id)
        {
            return _context.LqvNguoiDungs.Any(e => e.LqvId == id);
        }
    }
}
