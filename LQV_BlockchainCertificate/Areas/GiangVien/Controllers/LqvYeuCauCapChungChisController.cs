using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using LQV_BlockchainCertificate.Models.DBModel;

namespace LQV_BlockchainCertificate.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    [Authorize(Roles = "GiangVien")]
    public class LqvYeuCauCapChungChisController : Controller
    {
        private readonly LqvDbContext _context;

        public LqvYeuCauCapChungChisController(LqvDbContext context)
        {
            _context = context;
        }

        // =========================
        // Helper: ID giảng viên
        // =========================
        private int GetGiangVienId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        // =====================================================
        // 1. DANH SÁCH YÊU CẦU (THEO GIẢNG VIÊN)
        // =====================================================
        public async Task<IActionResult> Index()
        {
            int giangVienId = GetGiangVienId();

            var data = await _context.LqvYeuCauCapChungChis
                .Include(x => x.LqvNguoiDung)
                .Include(x => x.LqvKhoaHoc)
                .Where(x => x.LqvKhoaHoc.LqvGiangVienId == giangVienId)
                .OrderByDescending(x => x.LqvNgayYeuCau)
                .ToListAsync();

            return View(data);
        }

        // =====================================================
        // 2. SINH VIÊN ĐỦ ĐIỀU KIỆN (100% + CHƯA CÓ YC / ĐÃ BỊ TỪ CHỐI)
        // =====================================================
        public async Task<IActionResult> SinhVienDuDieuKien(int khoaHocId)
        {
            int giangVienId = GetGiangVienId();

            // kiểm tra khóa học có thuộc giảng viên không
            bool isOwner = await _context.LqvKhoaHocs.AnyAsync(k =>
                k.LqvMaKhoaHoc == khoaHocId &&
                k.LqvGiangVienId == giangVienId
            );

            if (!isOwner)
                return Forbid();

            var sinhVien = await _context.LqvTienDoHocTaps
                .Include(t => t.LqvSinhVien)
               .Where(t =>
            t.LqvKhoaHocId == khoaHocId &&
            t.LqvTiLeHoanThanh >= 100 &&
            !_context.LqvYeuCauCapChungChis.Any(y =>
                y.LqvNguoiDungId == t.LqvSinhVienId &&
                y.LqvKhoaHocId == khoaHocId &&
                (y.LqvTrangThai == "Chờ duyệt" || y.LqvTrangThai.StartsWith("Đã duyệt"))
                )
            )
                .ToListAsync();

            ViewBag.KhoaHocId = khoaHocId;
            return View(sinhVien);
        }

        // =====================================================
        // 3. GIẢNG VIÊN GỬI / GỬI LẠI YÊU CẦU
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeXuat(int sinhVienId, int khoaHocId, string? lyDo)
        {
            int giangVienId = GetGiangVienId();

            bool isOwner = await _context.LqvKhoaHocs.AnyAsync(k =>
                k.LqvMaKhoaHoc == khoaHocId &&
                k.LqvGiangVienId == giangVienId
            );
            if (!isOwner) return Forbid();

            bool duDieuKien = await _context.LqvTienDoHocTaps.AnyAsync(t =>
                t.LqvSinhVienId == sinhVienId &&
                t.LqvKhoaHocId == khoaHocId &&
                t.LqvTiLeHoanThanh >= 100
            );
            if (!duDieuKien)
            {
                TempData["Error"] = "Sinh viên chưa hoàn thành khóa học.";
                return RedirectToAction(nameof(Index));
            }

            // ❌ CHẶN nếu đang chờ hoặc đã duyệt
            bool dangTonTai = await _context.LqvYeuCauCapChungChis.AnyAsync(y =>
                y.LqvNguoiDungId == sinhVienId &&
                y.LqvKhoaHocId == khoaHocId &&
                (y.LqvTrangThai == "Chờ duyệt" || y.LqvTrangThai.StartsWith("Đã duyệt"))
            );

            if (dangTonTai)
            {
                TempData["Warning"] = "Yêu cầu đang chờ duyệt hoặc đã được duyệt.";
                return RedirectToAction(nameof(Index));
            }

            // ✅ TẠO YÊU CẦU MỚI (GỬI LẠI)
            var yeuCau = new LqvYeuCauCapChungChi
            {
                LqvNguoiDungId = sinhVienId,
                LqvKhoaHocId = khoaHocId,
                LqvNgayYeuCau = DateTime.Now,
                LqvLyDoYeuCau = lyDo,
                LqvTrangThai = "Chờ duyệt",
                LqvLyDoTuChoi = null
            };

            _context.LqvYeuCauCapChungChis.Add(yeuCau);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã gửi lại yêu cầu cấp chứng chỉ.";
            return RedirectToAction(nameof(Index));
        }


        // =====================================================
        // 4. CHI TIẾT YÊU CẦU
        // =====================================================
        public async Task<IActionResult> Details(int id)
        {
            int giangVienId = GetGiangVienId();

            var data = await _context.LqvYeuCauCapChungChis
                .Include(x => x.LqvNguoiDung)
                .Include(x => x.LqvKhoaHoc)
                .FirstOrDefaultAsync(x =>
                    x.LqvId == id &&
                    x.LqvKhoaHoc.LqvGiangVienId == giangVienId
                );

            if (data == null)
                return NotFound();

            return View(data);
        }
    }
}
