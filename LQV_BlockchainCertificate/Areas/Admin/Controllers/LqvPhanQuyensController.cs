using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LQV_BlockchainCertificate.Models.DBModel;
using X.PagedList;
using X.PagedList.Extensions;

namespace LQV_BlockchainCertificate.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LqvPhanQuyensController : Controller
    {
        private readonly LqvDbContext _context;

        public LqvPhanQuyensController(LqvDbContext context)
        {
            _context = context;
        }

        // GET: Admin/LqvPhanQuyens
        public async Task<IActionResult> Index(string searchString, int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var query = _context.LqvPhanQuyens
                .Include(p => p.LqvRole)
                .Include(p => p.LqvChucNang)
                .AsQueryable();

            // Tìm kiếm theo Role hoặc Chức năng
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p =>
                    p.LqvRole.LqvRoleName.Contains(searchString) ||
                    p.LqvChucNang.LqvTenChucNang.Contains(searchString));
            }

            // Sắp xếp giảm dần theo ID
            query = query.OrderByDescending(p => p.LqvPhanQuyenId);

            var list = await query.ToListAsync();
            var pagedList = list.ToPagedList(pageNumber, pageSize);

            return View(pagedList);
        }

        // GET: Admin/LqvPhanQuyens/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var phanQuyen = await _context.LqvPhanQuyens
                .Include(p => p.LqvRole)
                .Include(p => p.LqvChucNang)
                .FirstOrDefaultAsync(m => m.LqvPhanQuyenId == id);

            if (phanQuyen == null) return NotFound();

            return View(phanQuyen);
        }

        // GET: Admin/LqvPhanQuyens/Create
        public IActionResult Create()
        {
            ViewData["LqvRoleId"] = new SelectList(_context.LqvRoles, "LqvRoleId", "LqvTenRole");
            ViewData["LqvChucNangId"] = new SelectList(_context.LqvChucNangs, "LqvChucNangId", "LqvTenChucNang");
            return View();
        }

        // POST: Admin/LqvPhanQuyens/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LqvRoleId,LqvChucNangId,LqvChoPhep")] LqvPhanQuyen model)
        {
            // Kiểm tra trùng quyền
            var exists = await _context.LqvPhanQuyens
                .AnyAsync(p => p.LqvRoleId == model.LqvRoleId && p.LqvChucNangId == model.LqvChucNangId);
            if (exists)
            {
                ModelState.AddModelError("", "Phân quyền này đã tồn tại!");
            }

            if (ModelState.IsValid)
            {
                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["LqvRoleId"] = new SelectList(_context.LqvRoles, "LqvRoleId", "LqvTenRole", model.LqvRoleId);
            ViewData["LqvChucNangId"] = new SelectList(_context.LqvChucNangs, "LqvChucNangId", "LqvTenChucNang", model.LqvChucNangId);
            return View(model);
        }

        // GET: Admin/LqvPhanQuyens/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var phanQuyen = await _context.LqvPhanQuyens.FindAsync(id);
            if (phanQuyen == null) return NotFound();

            ViewData["LqvRoleId"] = new SelectList(_context.LqvRoles, "LqvRoleId", "LqvTenRole", phanQuyen.LqvRoleId);
            ViewData["LqvChucNangId"] = new SelectList(_context.LqvChucNangs, "LqvChucNangId", "LqvTenChucNang", phanQuyen.LqvChucNangId);
            return View(phanQuyen);
        }

        // POST: Admin/LqvPhanQuyens/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LqvPhanQuyenId,LqvRoleId,LqvChucNangId,LqvChoPhep")] LqvPhanQuyen model)
        {
            if (id != model.LqvPhanQuyenId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.LqvPhanQuyens.Any(e => e.LqvPhanQuyenId == model.LqvPhanQuyenId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["LqvRoleId"] = new SelectList(_context.LqvRoles, "LqvRoleId", "LqvTenRole", model.LqvRoleId);
            ViewData["LqvChucNangId"] = new SelectList(_context.LqvChucNangs, "LqvChucNangId", "LqvTenChucNang", model.LqvChucNangId);
            return View(model);
        }

        // GET: Admin/LqvPhanQuyens/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var phanQuyen = await _context.LqvPhanQuyens
                .Include(p => p.LqvRole)
                .Include(p => p.LqvChucNang)
                .FirstOrDefaultAsync(m => m.LqvPhanQuyenId == id);

            if (phanQuyen == null) return NotFound();

            return View(phanQuyen);
        }

        // POST: Admin/LqvPhanQuyens/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var phanQuyen = await _context.LqvPhanQuyens.FindAsync(id);
            if (phanQuyen != null)
            {
                _context.LqvPhanQuyens.Remove(phanQuyen);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
