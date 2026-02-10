using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LQV_BlockchainCertificate.Models.DBModel;
using X.PagedList;
using X.PagedList.Extensions;

namespace LQV_BlockchainCertificate.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class LqvKhoaHocsController : Controller
    {
        private readonly LqvDbContext _context;

        public LqvKhoaHocsController(LqvDbContext context)
        {
            _context = context;
        }

        // =========================
        // INDEX – DANH SÁCH KHÓA HỌC
        // =========================
        public IActionResult Index(string searchString, int? giangVienId, int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var query = _context.LqvKhoaHocs
                .Include(k => k.LqvGiangVien)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(k => k.LqvTenKhoaHoc.Contains(searchString));
            }

            if (giangVienId.HasValue && giangVienId > 0)
            {
                query = query.Where(k => k.LqvGiangVienId == giangVienId);
            }

            query = query.OrderByDescending(k => k.LqvNgayBatDau);

            ViewBag.LqvGiangVienId = new SelectList(
                _context.LqvNguoiDungs
                    .Include(u => u.LqvRole)
                    .Where(u => u.LqvRole.LqvRoleName == "GiangVien"),
                "LqvId",
                "LqvTenDangNhap",
                giangVienId
            );

            ViewBag.CurrentFilter = searchString;

            return View(query.ToPagedList(pageNumber, pageSize));
        }

        // =========================
        // DETAILS – CHI TIẾT
        // =========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var khoaHoc = await _context.LqvKhoaHocs
                .Include(k => k.LqvGiangVien)
                .FirstOrDefaultAsync(k => k.LqvMaKhoaHoc == id);

            if (khoaHoc == null) return NotFound();

            return View(khoaHoc);
        }

        // =========================
        // CREATE – TẠO KHÓA HỌC
        // =========================
        public IActionResult Create()
        {
            ViewData["LqvGiangVienId"] = new SelectList(
                _context.LqvNguoiDungs
                    .Include(u => u.LqvRole)
                    .Where(u => u.LqvRole.LqvRoleName == "GiangVien"),
                "LqvId",
                "LqvTenDangNhap"
            );
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LqvKhoaHoc lqvKhoaHoc)
        {
            if (!ModelState.IsValid)
            {
                ViewData["LqvGiangVienId"] = new SelectList(
                    _context.LqvNguoiDungs
                        .Include(u => u.LqvRole)
                        .Where(u => u.LqvRole.LqvRoleName == "GiangVien"),
                    "LqvId",
                    "LqvTenDangNhap",
                    lqvKhoaHoc.LqvGiangVienId
                );
                return View(lqvKhoaHoc);
            }

            if (lqvKhoaHoc.LqvNgayBatDau == default)
                lqvKhoaHoc.LqvNgayBatDau = DateTime.Now;

            _context.LqvKhoaHocs.Add(lqvKhoaHoc);
            await _context.SaveChangesAsync();

            await GhiNhatKy(
                "Thêm khóa học",
                $"Admin tạo khóa học: {lqvKhoaHoc.LqvTenKhoaHoc}"
            );

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // EDIT – CHỈNH SỬA
        // =========================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var khoaHoc = await _context.LqvKhoaHocs.FindAsync(id);
            if (khoaHoc == null) return NotFound();

            ViewData["LqvGiangVienId"] = new SelectList(
                _context.LqvNguoiDungs
                    .Include(u => u.LqvRole)
                    .Where(u => u.LqvRole.LqvRoleName == "GiangVien"),
                "LqvId",
                "LqvTenDangNhap",
                khoaHoc.LqvGiangVienId
            );

            return View(khoaHoc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LqvKhoaHoc lqvKhoaHoc)
        {
            if (id != lqvKhoaHoc.LqvMaKhoaHoc) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["LqvGiangVienId"] = new SelectList(
                    _context.LqvNguoiDungs
                        .Include(u => u.LqvRole)
                        .Where(u => u.LqvRole.LqvRoleName == "GiangVien"),
                    "LqvId",
                    "LqvTenDangNhap",
                    lqvKhoaHoc.LqvGiangVienId
                );
                return View(lqvKhoaHoc);
            }

            try
            {
                _context.Update(lqvKhoaHoc);
                await _context.SaveChangesAsync();

                await GhiNhatKy(
                    "Cập nhật khóa học",
                    $"Admin sửa khóa học: {lqvKhoaHoc.LqvTenKhoaHoc}"
                );
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.LqvKhoaHocs.Any(e => e.LqvMaKhoaHoc == id))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // DELETE – XÓA
        // =========================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var khoaHoc = await _context.LqvKhoaHocs
                .Include(k => k.LqvGiangVien)
                .FirstOrDefaultAsync(k => k.LqvMaKhoaHoc == id);

            if (khoaHoc == null) return NotFound();

            return View(khoaHoc);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var khoaHoc = await _context.LqvKhoaHocs.FindAsync(id);
            if (khoaHoc != null)
            {
                _context.LqvKhoaHocs.Remove(khoaHoc);
                await _context.SaveChangesAsync();

                await GhiNhatKy(
                    "Xóa khóa học",
                    $"Admin xóa khóa học: {khoaHoc.LqvTenKhoaHoc}"
                );
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // NHẬT KÝ HOẠT ĐỘNG
        // =========================
        private async Task GhiNhatKy(string hanhDong, string chiTiet)
        {
            var log = new LqvNhatKyHoatDong
            {
                LqvTaiKhoan = User.Identity?.Name ?? "Admin",
                LqvHanhDong = hanhDong,
                LqvChiTiet = chiTiet,
                LqvThoiGian = DateTime.Now,
                LqvIp = HttpContext.Connection.RemoteIpAddress?.ToString()
            };

            _context.LqvNhatKyHoatDongs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
