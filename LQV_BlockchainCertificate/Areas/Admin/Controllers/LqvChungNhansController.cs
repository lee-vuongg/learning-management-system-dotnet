using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LQV_BlockchainCertificate.Models.DBModel;

namespace LQV_BlockchainCertificate.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LqvChungNhansController : Controller
    {
        private readonly LqvDbContext _context;

        public LqvChungNhansController(LqvDbContext context)
        {
            _context = context;
        }

        // 🔹 Danh sách chứng nhận
        public async Task<IActionResult> Index()
        {
            var list = await _context.LqvChungNhans
                .Include(c => c.LqvSinhVien)
                .Include(c => c.LqvKhoaHoc)
                .OrderByDescending(c => c.LqvNgayCap)
                .ToListAsync();

            return View(list);
        }

        // 🔹 Chi tiết chứng nhận
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var chungNhan = await _context.LqvChungNhans
                .Include(c => c.LqvSinhVien)
                .Include(c => c.LqvKhoaHoc)
                .FirstOrDefaultAsync(c => c.LqvMaChungNhan == id);

            if (chungNhan == null)
                return NotFound();

            return View(chungNhan);
        }

        // 🔹 Form tạo mới
        public IActionResult Create()
        {
            ViewData["LqvSinhVienId"] = new SelectList(_context.LqvNguoiDungs, "LqvId", "LqvHoTen");
            ViewData["LqvKhoaHocId"] = new SelectList(_context.LqvKhoaHocs, "LqvMaKhoaHoc", "LqvTenKhoaHoc");
            return View();
        }

        // 🔹 Xử lý thêm chứng nhận
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LqvSinhVienId,LqvKhoaHocId")] LqvChungNhan model)
        {
            if (ModelState.IsValid)
            {
                // Sinh mã chứng nhận và hash
                model.LqvNgayCap = DateTime.Now;
                model.LqvMaChungNhanCode = "CN" + DateTime.Now.ToString("yyyyMMddHHmmss");
                model.LqvHashValue = Guid.NewGuid().ToString("N");
                model.LqvTrangThai = "Đã cấp";

                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["LqvSinhVienId"] = new SelectList(_context.LqvNguoiDungs, "LqvId", "LqvHoTen", model.LqvSinhVienId);
            ViewData["LqvKhoaHocId"] = new SelectList(_context.LqvKhoaHocs, "LqvMaKhoaHoc", "LqvTenKhoaHoc", model.LqvKhoaHocId);
            return View(model);
        }

        // 🔹 Sửa chứng nhận
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var chungNhan = await _context.LqvChungNhans.FindAsync(id);
            if (chungNhan == null)
                return NotFound();

            ViewData["LqvSinhVienId"] = new SelectList(_context.LqvNguoiDungs, "LqvId", "LqvHoTen", chungNhan.LqvSinhVienId);
            ViewData["LqvKhoaHocId"] = new SelectList(_context.LqvKhoaHocs, "LqvMaKhoaHoc", "LqvTenKhoaHoc", chungNhan.LqvKhoaHocId);
            return View(chungNhan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LqvMaChungNhan,LqvMaChungNhanCode,LqvSinhVienId,LqvKhoaHocId,LqvNgayCap,LqvHashValue,LqvTrangThai")] LqvChungNhan chungNhan)
        {
            if (id != chungNhan.LqvMaChungNhan)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chungNhan);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.LqvChungNhans.Any(e => e.LqvMaChungNhan == id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["LqvSinhVienId"] = new SelectList(_context.LqvNguoiDungs, "LqvId", "LqvHoTen", chungNhan.LqvSinhVienId);
            ViewData["LqvKhoaHocId"] = new SelectList(_context.LqvKhoaHocs, "LqvMaKhoaHoc", "LqvTenKhoaHoc", chungNhan.LqvKhoaHocId);
            return View(chungNhan);
        }

        // 🔹 Xóa chứng nhận
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var chungNhan = await _context.LqvChungNhans
                .Include(c => c.LqvSinhVien)
                .Include(c => c.LqvKhoaHoc)
                .FirstOrDefaultAsync(m => m.LqvMaChungNhan == id);

            if (chungNhan == null)
                return NotFound();

            return View(chungNhan);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var chungNhan = await _context.LqvChungNhans.FindAsync(id);
            if (chungNhan != null)
                _context.LqvChungNhans.Remove(chungNhan);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
