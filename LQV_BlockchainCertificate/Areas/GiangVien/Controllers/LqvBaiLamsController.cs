using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LQV_BlockchainCertificate.Models.DBModel;
using System.Security.Claims;
using LQV_BlockchainCertificate.Services;
using System.Collections.Generic;

namespace LQV_BlockchainCertificate.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    [Authorize(Roles = "GiangVien")]
    public class LqvBaiLamsController : Controller
    {
        private readonly LqvDbContext _context;
        private readonly ILqvTienDoHocTapService _tienDoService;

        public LqvBaiLamsController(
            LqvDbContext context,
            ILqvTienDoHocTapService tienDoService)
        {
            _context = context;
            _tienDoService = tienDoService;
        }

        // ===============================
        // 1. DANH SÁCH BÀI LÀM
        // ===============================
        public async Task<IActionResult> Index()
        {
            int giangVienId = GetGiangVienId();

            var baiLams = await _context.LqvBaiLams
                .Include(b => b.LqvNguoiDung)
                .Include(b => b.LqvLichThi)
                    .ThenInclude(lt => lt.LqvLopHoc)
                .Include(b => b.LqvLichThi)
                    .ThenInclude(lt => lt.LqvDeThi)
                .Where(b => b.LqvLichThi.LqvLopHoc.LqvGiangVienId == giangVienId)
                .OrderByDescending(b => b.LqvThoiGianNop)
                .AsNoTracking()
                .ToListAsync();

            // ✅ GROUP ĐÚNG: LỚP + ĐỀ THI
            var groupedData = baiLams
                .GroupBy(b => new
                {
                    LopId = b.LqvLichThi.LqvLopHocId,
                    TenLop = b.LqvLichThi.LqvLopHoc.LqvTenLop,
                    DeThiId = b.LqvLichThi.LqvDeThiId,
                    TenDeThi = b.LqvLichThi.LqvDeThi.LqvTenDeThi
                })
                .ToList();

            return View(groupedData);
        }


        // ===============================
        // 2. CHI TIẾT BÀI LÀM
        // ===============================
        public async Task<IActionResult> Details(int id)
        {
            int giangVienId = GetGiangVienId();

            var baiLam = await _context.LqvBaiLams
                .Include(b => b.LqvNguoiDung)
                .Include(b => b.LqvLichThi)
                    .ThenInclude(l => l.LqvLopHoc)
                .Include(b => b.LqvChiTietBaiLams)
                    .ThenInclude(ct => ct.LqvCauHoi)
                        .ThenInclude(ch => ch.LqvDapAns)
                .Include(b => b.LqvChiTietBaiLams)
                    .ThenInclude(ct => ct.LqvDapAn)
                .FirstOrDefaultAsync(b =>
                    b.LqvBaiLamId == id &&
                    b.LqvLichThi.LqvLopHoc.LqvGiangVienId == giangVienId
                );

            if (baiLam == null)
                return NotFound();

            return View(baiLam);
        }

        // ===============================
        // 3. CHẤM BÀI (GET)
        // ===============================
        public async Task<IActionResult> ChamBai(int id)
        {
            int giangVienId = GetGiangVienId();

            var baiLam = await _context.LqvBaiLams
                .Include(b => b.LqvNguoiDung)
                .Include(b => b.LqvLichThi)
                    .ThenInclude(lt => lt.LqvLopHoc)
                .Include(b => b.LqvChiTietBaiLams)
                    .ThenInclude(ct => ct.LqvCauHoi)
                        .ThenInclude(ch => ch.LqvDapAns)
                .Include(b => b.LqvChiTietBaiLams)
                    .ThenInclude(ct => ct.LqvDapAn)
                .FirstOrDefaultAsync(b =>
                    b.LqvBaiLamId == id &&
                    b.LqvLichThi.LqvLopHoc.LqvGiangVienId == giangVienId
                );

            if (baiLam == null)
                return NotFound();

            return View(baiLam);
        }

        // ===============================
        // 4. CHẤM BÀI (POST)
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChamBai(
            int baiLamId,
            Dictionary<int, double> diemTuLuan)
        {
            int giangVienId = GetGiangVienId();

            var baiLam = await _context.LqvBaiLams
                .Include(b => b.LqvLichThi)
                    .ThenInclude(lt => lt.LqvLopHoc)
                .Include(b => b.LqvChiTietBaiLams)
                    .ThenInclude(ct => ct.LqvCauHoi)
                        .ThenInclude(ch => ch.LqvDapAns)
                .Include(b => b.LqvChiTietBaiLams)
                    .ThenInclude(ct => ct.LqvDapAn)
                .FirstOrDefaultAsync(b =>
                    b.LqvBaiLamId == baiLamId &&
                    b.LqvLichThi.LqvLopHoc.LqvGiangVienId == giangVienId
                );

            if (baiLam == null)
                return NotFound();

            // ===============================
            // AUTO CHẤM TRẮC NGHIỆM
            // ===============================
            foreach (var ct in baiLam.LqvChiTietBaiLams)
            {
                if (ct.LqvCauHoi.LqvLoai == "TracNghiem" && !ct.LqvDaCham)
                {
                    var dapAnDung = ct.LqvCauHoi.LqvDapAns
                        .FirstOrDefault(x => x.LqvDung);

                    ct.LqvDiem = (ct.LqvDapAnId == dapAnDung?.LqvDapAnId)
                        ? ct.LqvCauHoi.LqvDiem
                        : 0;

                    ct.LqvDaCham = true;
                }
            }

            // ===============================
            // CHẤM TỰ LUẬN (GIẢNG VIÊN)
            // ===============================
            foreach (var ct in baiLam.LqvChiTietBaiLams)
            {
                if (ct.LqvCauHoi.LqvLoai == "TuLuan"
                    && diemTuLuan.TryGetValue(ct.LqvId, out double diem))
                {
                    var diemToiDa = ct.LqvCauHoi.LqvDiem;
                    ct.LqvDiem = Math.Min(diem, diemToiDa);
                    ct.LqvDaCham = true;
                }
            }

            // ===============================
            // TÍNH TỔNG ĐIỂM & TRẠNG THÁI
            // ===============================
            baiLam.LqvDiem = baiLam.LqvChiTietBaiLams.Sum(x => x.LqvDiem ?? 0);
            baiLam.LqvTrangThai = baiLam.LqvChiTietBaiLams.All(x => x.LqvDaCham)
                ? "DaCham"
                : "ChuaCham";

            await _context.SaveChangesAsync();

            // ===============================
            // CẬP NHẬT TIẾN ĐỘ HỌC
            // ===============================
            await _tienDoService.CapNhatTienDoHocTapAsync(
                baiLam.LqvUserId,
                baiLam.LqvLichThi.LqvLopHoc.LqvKhoaHocId
            );

            TempData["Success"] = "Đã chấm bài thành công";
            return RedirectToAction(nameof(Index));
        }

        // ===============================
        // HELPER: LẤY ID GIẢNG VIÊN
        // ===============================
        private int GetGiangVienId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                throw new Exception("Không tìm thấy UserId trong Claims");

            return int.Parse(userIdClaim.Value);
        }
    }
}
