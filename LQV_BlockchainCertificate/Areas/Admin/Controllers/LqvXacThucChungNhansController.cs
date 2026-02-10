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
    public class LqvXacThucChungNhansController : Controller
    {
        private readonly LqvDbContext _context;

        public LqvXacThucChungNhansController(LqvDbContext context)
        {
            _context = context;
        }

        // GET: Admin/LqvXacThucChungNhans (Đổi tên thành Lịch sử Tra cứu)
        public async Task<IActionResult> LichSuTraCuu()
        {
            // Bao gồm thông tin liên quan nếu cần (như chứng nhận)
            var history = await _context.LqvXacThucChungNhans
                // .Include(x => x.LqvChungNhan) // Nếu có liên kết FK
                .OrderByDescending(x => x.LqvThoiGianXacThuc)
                .ToListAsync();

            return View("LichSuTraCuu", history); // Vẫn sử dụng View Index hiện tại
        }

        // GET: Admin/LqvXacThucChungNhans/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lqvXacThucChungNhan = await _context.LqvXacThucChungNhans
                // Bao gồm thông tin liên quan nếu cần
                // .Include(x => x.LqvChungNhan) 
                .FirstOrDefaultAsync(m => m.LqvId == id);

            if (lqvXacThucChungNhan == null)
            {
                return NotFound();
            }

            return View(lqvXacThucChungNhan);
        }

        // ------------------------------------------------------------------
        // LOẠI BỎ CÁC ACTION KHÔNG CẦN THIẾT CHO DỮ LIỆU NHẬT KÝ
        // Đã loại bỏ: GET/POST Create, GET/POST Edit
        // ------------------------------------------------------------------

        // GET: Admin/LqvXacThucChungNhans/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lqvXacThucChungNhan = await _context.LqvXacThucChungNhans
                .FirstOrDefaultAsync(m => m.LqvId == id);

            if (lqvXacThucChungNhan == null)
            {
                return NotFound();
            }

            return View(lqvXacThucChungNhan);
        }

        // POST: Admin/LqvXacThucChungNhans/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lqvXacThucChungNhan = await _context.LqvXacThucChungNhans.FindAsync(id);
            if (lqvXacThucChungNhan != null)
            {
                _context.LqvXacThucChungNhans.Remove(lqvXacThucChungNhan);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(LichSuTraCuu)); // Chuyển hướng về action mới
        }

        private bool LqvXacThucChungNhanExists(int id)
        {
            return _context.LqvXacThucChungNhans.Any(e => e.LqvId == id);
        }
    }
}