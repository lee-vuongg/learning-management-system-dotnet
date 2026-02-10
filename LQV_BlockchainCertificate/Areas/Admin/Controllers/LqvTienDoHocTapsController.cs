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
    public class LqvTienDoHocTapsController : Controller
    {
        private readonly LqvDbContext _context;

        public LqvTienDoHocTapsController(LqvDbContext context)
        {
            _context = context;
        }

        // --- PHƯƠNG THỨC TRỢ GIÚP (HELPER METHODS) ---

        // Phương thức trợ giúp để tạo SelectList cho Khóa học (Hiển thị Mã + Tên)
        private SelectList GetKhoaHocSelectList(object selectedValue = null)
        {
            var khoaHocList = _context.LqvKhoaHocs
                .Select(k => new
                {
                    Value = k.LqvMaKhoaHoc,
                    Text = $"[{k.LqvMaKhoaHoc}] {k.LqvTenKhoaHoc}" // Kết hợp Mã và Tên
                })
                .ToList();

            // Sử dụng "Value" là LqvMaKhoaHoc và "Text" là chuỗi kết hợp
            return new SelectList(khoaHocList, "Value", "Text", selectedValue);
        }

        // Phương thức trợ giúp để tạo SelectList cho Sinh viên (Hiển thị Họ Tên + ID)
        private SelectList GetSinhVienSelectList(object selectedValue = null)
        {
            var sinhVienList = _context.LqvNguoiDungs
                .OrderBy(u => u.LqvHoTen)
                .Select(u => new
                {
                    Value = u.LqvId,
                    Text = $"{u.LqvHoTen} (ID: {u.LqvId})" // Kết hợp Họ Tên và ID
                })
                .ToList();

            // Sử dụng "Value" là LqvId và "Text" là chuỗi kết hợp
            return new SelectList(sinhVienList, "Value", "Text", selectedValue);
        }

        // --- CÁC ACTION METHOD ---

        // GET: Admin/LqvTienDoHocTaps
        // Không cần thay đổi ở đây, vì việc hiển thị Mã + Tên Khóa học trong Index
        // sẽ được xử lý trong Index.cshtml bằng cách sử dụng l.LqvKhoaHoc.LqvMaKhoaHoc và l.LqvKhoaHoc.LqvTenKhoaHoc
        public async Task<IActionResult> Index()
        {
            // Bao gồm LqvKhoaHoc và LqvSinhVien để View Index có thể truy cập Mã Khóa học và Tên Sinh viên
            var lqvDbContext = _context.LqvTienDoHocTaps
                .Include(l => l.LqvKhoaHoc)
                .Include(l => l.LqvSinhVien);
            return View(await lqvDbContext.ToListAsync());
        }

        // GET: Admin/LqvTienDoHocTaps/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lqvTienDoHocTap = await _context.LqvTienDoHocTaps
                .Include(l => l.LqvKhoaHoc)
                .Include(l => l.LqvSinhVien)
                .FirstOrDefaultAsync(m => m.LqvId == id);

            if (lqvTienDoHocTap == null)
            {
                return NotFound();
            }

            return View(lqvTienDoHocTap);
        }

        // GET: Admin/LqvTienDoHocTaps/Create
        public IActionResult Create()
        {
            // Sử dụng phương thức trợ giúp mới để hiển thị Mã và Tên Khóa học
            ViewData["LqvKhoaHocId"] = GetKhoaHocSelectList();

            // Sử dụng phương thức trợ giúp mới để hiển thị Họ Tên và ID Sinh viên
            ViewData["LqvSinhVienId"] = GetSinhVienSelectList();

            return View();
        }

        // POST: Admin/LqvTienDoHocTaps/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LqvId,LqvSinhVienId,LqvKhoaHocId,LqvTiLeHoanThanh,LqvNgayCapNhat")] LqvTienDoHocTap lqvTienDoHocTap)
        {
            if (ModelState.IsValid)
            {
                // Tự động gán ngày cập nhật nếu cần
                if (lqvTienDoHocTap.LqvNgayCapNhat == DateTime.MinValue)
                {
                    lqvTienDoHocTap.LqvNgayCapNhat = DateTime.Now;
                }

                _context.Add(lqvTienDoHocTap);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Tải lại dữ liệu cho Dropdown List sử dụng helper methods
            ViewData["LqvKhoaHocId"] = GetKhoaHocSelectList(lqvTienDoHocTap.LqvKhoaHocId);
            ViewData["LqvSinhVienId"] = GetSinhVienSelectList(lqvTienDoHocTap.LqvSinhVienId);

            return View(lqvTienDoHocTap);
        }

        // GET: Admin/LqvTienDoHocTaps/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lqvTienDoHocTap = await _context.LqvTienDoHocTaps.FindAsync(id);

            if (lqvTienDoHocTap == null)
            {
                return NotFound();
            }

            // Sử dụng phương thức trợ giúp mới để hiển thị Mã và Tên Khóa học
            ViewData["LqvKhoaHocId"] = GetKhoaHocSelectList(lqvTienDoHocTap.LqvKhoaHocId);

            // Sử dụng phương thức trợ giúp mới để hiển thị Họ Tên và ID Sinh viên
            ViewData["LqvSinhVienId"] = GetSinhVienSelectList(lqvTienDoHocTap.LqvSinhVienId);

            return View(lqvTienDoHocTap);
        }

        // POST: Admin/LqvTienDoHocTaps/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LqvId,LqvSinhVienId,LqvKhoaHocId,LqvTiLeHoanThanh,LqvNgayCapNhat")] LqvTienDoHocTap lqvTienDoHocTap)
        {
            if (id != lqvTienDoHocTap.LqvId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Cập nhật ngày cập nhật mỗi khi chỉnh sửa
                lqvTienDoHocTap.LqvNgayCapNhat = DateTime.Now;

                try
                {
                    _context.Update(lqvTienDoHocTap);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LqvTienDoHocTapExists(lqvTienDoHocTap.LqvId))
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

            // Tải lại dữ liệu cho Dropdown List sử dụng helper methods
            ViewData["LqvKhoaHocId"] = GetKhoaHocSelectList(lqvTienDoHocTap.LqvKhoaHocId);
            ViewData["LqvSinhVienId"] = GetSinhVienSelectList(lqvTienDoHocTap.LqvSinhVienId);

            return View(lqvTienDoHocTap);
        }

        // GET: Admin/LqvTienDoHocTaps/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lqvTienDoHocTap = await _context.LqvTienDoHocTaps
                .Include(l => l.LqvKhoaHoc)
                .Include(l => l.LqvSinhVien)
                .FirstOrDefaultAsync(m => m.LqvId == id);

            if (lqvTienDoHocTap == null)
            {
                return NotFound();
            }

            return View(lqvTienDoHocTap);
        }

        // POST: Admin/LqvTienDoHocTaps/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lqvTienDoHocTap = await _context.LqvTienDoHocTaps.FindAsync(id);
            if (lqvTienDoHocTap != null)
            {
                _context.LqvTienDoHocTaps.Remove(lqvTienDoHocTap);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LqvTienDoHocTapExists(int id)
        {
            return _context.LqvTienDoHocTaps.Any(e => e.LqvId == id);
        }
    }
}