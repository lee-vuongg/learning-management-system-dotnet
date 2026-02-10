using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LQV_BlockchainCertificate.Models.DBModel;
using X.PagedList;
using X.PagedList.Extensions;
using LQV_BlockchainCertificate.Services;





namespace LQV_BlockchainCertificate.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LqvGiaoDichBlockchainsController : Controller
    {
        private readonly LqvDbContext _context;
        private readonly BlockchainService _blockchainService;

        public LqvGiaoDichBlockchainsController(LqvDbContext context)
        {
            _context = context;
        }

        // GET: Admin/LqvGiaoDichBlockchains
        public async Task<IActionResult> Index(string searchString, int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var giaoDichQuery = _context.LqvGiaoDichBlockchains
                .Include(g => g.LqvChungNhan)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                // tránh ToString() trong LINQ -> tách logic tìm số và tìm chuỗi
                if (long.TryParse(searchString, out var n))
                {
                    giaoDichQuery = giaoDichQuery.Where(g =>
                        g.LqvBlockNumber == n ||
                        (g.LqvChungNhan != null && g.LqvChungNhan.LqvMaChungNhan == n));
                }
                else
                {
                    giaoDichQuery = giaoDichQuery.Where(g =>
                        g.LqvTxHash.Contains(searchString));
                }
            }

            giaoDichQuery = giaoDichQuery.OrderByDescending(g => g.LqvGioTao);

            // materialize async
            var list = await giaoDichQuery.ToListAsync();

            // phân trang trên IEnumerable (đồng bộ)
            var pagedList = list.ToPagedList(pageNumber, pageSize);

            return View(pagedList);
        }


        // GET: Admin/LqvGiaoDichBlockchains/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var giaoDich = await _context.LqvGiaoDichBlockchains
                .Include(g => g.LqvChungNhan)
                .FirstOrDefaultAsync(g => g.LqvMaGiaoDich == id);

            if (giaoDich == null) return NotFound();

            return View(giaoDich);
        }

        // GET: Admin/LqvGiaoDichBlockchains/Create
        public IActionResult Create()
        {
            ViewData["LqvChungNhanId"] = new SelectList(_context.LqvChungNhans, "LqvMaChungNhan", "LqvMaChungNhanCode");
            return View();
        }

        // POST: Admin/LqvGiaoDichBlockchains/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LqvChungNhanId,LqvTxHash,LqvBlockNumber,LqvGioTao,LqvStatus")] LqvGiaoDichBlockchain giaoDich)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra trùng TxHash
                var existed = await _context.LqvGiaoDichBlockchains
                    .AnyAsync(x => x.LqvTxHash == giaoDich.LqvTxHash);
                if (existed)
                {
                    ModelState.AddModelError("LqvTxHash", "Mã giao dịch (TxHash) đã tồn tại!");
                }
                else
                {
                    giaoDich.LqvGioTao = DateTime.Now;
                    _context.Add(giaoDich);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            ViewData["LqvChungNhanId"] = new SelectList(_context.LqvChungNhans, "LqvMaChungNhan", "LqvMaChungNhanCode", giaoDich.LqvChungNhanId);
            return View(giaoDich);
        }

        // GET: Admin/LqvGiaoDichBlockchains/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var giaoDich = await _context.LqvGiaoDichBlockchains.FindAsync(id);
            if (giaoDich == null) return NotFound();

            ViewData["LqvChungNhanId"] = new SelectList(_context.LqvChungNhans, "LqvMaChungNhan", "LqvMaChungNhanCode", giaoDich.LqvChungNhanId);
            return View(giaoDich);
        }

        // POST: Admin/LqvGiaoDichBlockchains/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LqvMaGiaoDich,LqvChungNhanId,LqvTxHash,LqvBlockNumber,LqvGioTao,LqvStatus")] LqvGiaoDichBlockchain giaoDich)
        {
            if (id != giaoDich.LqvMaGiaoDich) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(giaoDich);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LqvGiaoDichBlockchainExists(giaoDich.LqvMaGiaoDich))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["LqvChungNhanId"] = new SelectList(_context.LqvChungNhans, "LqvMaChungNhan", "LqvMaChungNhanCode", giaoDich.LqvChungNhanId);
            return View(giaoDich);
        }

        // GET: Admin/LqvGiaoDichBlockchains/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var giaoDich = await _context.LqvGiaoDichBlockchains
                .Include(g => g.LqvChungNhan)
                .FirstOrDefaultAsync(g => g.LqvMaGiaoDich == id);

            if (giaoDich == null) return NotFound();

            return View(giaoDich);
        }

        // POST: Admin/LqvGiaoDichBlockchains/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var giaoDich = await _context.LqvGiaoDichBlockchains.FindAsync(id);
            if (giaoDich != null)
            {
                // Chỉ cho phép xóa nếu chưa xác nhận trên blockchain
                if (giaoDich.LqvStatus == "Pending" || giaoDich.LqvStatus == "Lỗi")
                {
                    _context.LqvGiaoDichBlockchains.Remove(giaoDich);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể xóa giao dịch đã xác nhận trên blockchain.";
                }
            }
            return RedirectToAction(nameof(Index));
        }

        private bool LqvGiaoDichBlockchainExists(int id)
        {
            return _context.LqvGiaoDichBlockchains.Any(e => e.LqvMaGiaoDich == id);
        }
    }
}
