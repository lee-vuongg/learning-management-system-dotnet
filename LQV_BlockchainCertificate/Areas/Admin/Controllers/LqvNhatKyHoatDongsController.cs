using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList; // Thư viện phân trang
using LQV_BlockchainCertificate.Models.DBModel;
using X.PagedList.Extensions;

namespace LQV_BlockchainCertificate.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LqvNhatKyHoatDongsController : Controller
    {
        private readonly LqvDbContext _context;

        public LqvNhatKyHoatDongsController(LqvDbContext context)
        {
            _context = context;
        }

        // GET: Admin/LqvNhatKyHoatDongs
        // GET: Admin/LqvNhatKyHoatDongs
        public async Task<IActionResult> Index(string searchString, int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            // Truy vấn dữ liệu
            var query = _context.LqvNhatKyHoatDongs.AsQueryable();

            // Tìm kiếm theo tài khoản hoặc hành động
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(n =>
                    n.LqvTaiKhoan.Contains(searchString) ||
                    n.LqvHanhDong.Contains(searchString) ||
                    n.LqvChiTiet.Contains(searchString));
            }

            // Sắp xếp theo thời gian mới nhất
            query = query.OrderByDescending(n => n.LqvThoiGian);

            // Dùng ToListAsync để truy xuất dữ liệu trước
            var list = await query.ToListAsync();

            // Phân trang (sử dụng X.PagedList)
            var pagedList = list.ToPagedList(pageNumber, pageSize);

            return View(pagedList);
        }


        // GET: Admin/LqvNhatKyHoatDongs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var nhatKy = await _context.LqvNhatKyHoatDongs.FirstOrDefaultAsync(m => m.LqvId == id);
            if (nhatKy == null)
                return NotFound();

            return View(nhatKy);
        }

        // GET: Admin/LqvNhatKyHoatDongs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var nhatKy = await _context.LqvNhatKyHoatDongs.FirstOrDefaultAsync(m => m.LqvId == id);
            if (nhatKy == null)
                return NotFound();

            return View(nhatKy);
        }

        // POST: Admin/LqvNhatKyHoatDongs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var nhatKy = await _context.LqvNhatKyHoatDongs.FindAsync(id);
            if (nhatKy != null)
            {
                _context.LqvNhatKyHoatDongs.Remove(nhatKy);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
