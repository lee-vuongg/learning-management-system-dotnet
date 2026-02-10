using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using LQV_BlockchainCertificate.Models.DBModel;
using LQV_BlockchainCertificate.Models.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace LQV_BlockchainCertificate.Areas.Student.Controllers
{
    [Area("Student")]
    public class HomeController : Controller
    {
        private readonly LqvDbContext _context;

        public HomeController(LqvDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // =================================================
            // ===== LẤY ID SINH VIÊN ==========================
            // =================================================
            int currentUserId = 0;

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                int.TryParse(userIdString, out currentUserId);
            }

            if (currentUserId <= 0)
                currentUserId = 1; // DEV MODE

            // =================================================
            // ===== THÔNG TIN SINH VIÊN =======================
            // =================================================
            var nguoiDung = await _context.LqvNguoiDungs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.LqvId == currentUserId);

            if (nguoiDung == null)
                return NotFound();

            // =================================================
            // ===== CHỨNG NHẬN ================================
            // =================================================
            var chungNhans = await _context.LqvChungNhans
                .Where(cn => cn.LqvSinhVienId == currentUserId)
                .Include(cn => cn.LqvKhoaHoc)
                .AsNoTracking()
                .ToListAsync();

            // =================================================
            // ===== ĐIỂM DANH =================================
            // =================================================
            var diemDanhs = await _context.LqvDiemDanhGps
                .Where(dd => dd.LqvSinhVienId == currentUserId)
                .Include(dd => dd.LqvBuoiHoc)
                .OrderByDescending(dd => dd.LqvThoiGian)
                .AsNoTracking()
                .ToListAsync();

            // =================================================
            // ===== 🔥 BÀI TẬP – JOIN THUẦN (KHÔNG WITH) ======
            // =================================================
            var baiTaps = await (
                from bt in _context.LqvBaiTaps
                join dk in _context.LqvDangKyLopHocs
                    on bt.LqvLopHocId equals dk.LqvLopHocId
                where dk.LqvSinhVienId == currentUserId
                select bt
            )
            .Include(bt => bt.LqvLopHoc)
            .AsNoTracking()
            .ToListAsync();

            // =================================================
            // ===== BÀI TẬP ĐÃ NỘP ============================
            // =================================================
            var baiTapDaNopIds = await _context.LqvNopBaiTaps
                .Where(nb => nb.LqvSinhVienId == currentUserId)
                .Select(nb => nb.LqvBaiTapId)
                .ToListAsync();

            var now = DateTime.Now;

            var danhSachBaiTapVM = baiTaps
                .Select(bt =>
                {
                    bool daNop = baiTapDaNopIds.Contains(bt.LqvBaiTapId);

                    string trangThai;
                    string actionText;

                    if (daNop)
                    {
                        trangThai = "Đã nộp";
                        actionText = "Xem bài";
                    }
                    else if (bt.LqvHanNop < now)
                    {
                        trangThai = "Quá hạn";
                        actionText = "Xem chi tiết";
                    }
                    else if ((bt.LqvHanNop - now).TotalDays <= 2)
                    {
                        trangThai = "Sắp đến hạn";
                        actionText = "Làm bài";
                    }
                    else
                    {
                        trangThai = "Chưa nộp";
                        actionText = "Làm bài";
                    }

                    return new BaiTapDashboardVM
                    {
                        BaiTapId = bt.LqvBaiTapId,
                        TenBaiTap = bt.LqvTieuDe,
                        TenLopHoc = bt.LqvLopHoc.LqvTenLop,
                        TenMonHoc = bt.LqvLopHoc.LqvTenLop, // sau có bảng môn sửa 1 dòng
                        Deadline = bt.LqvHanNop,
                        DaNop = daNop,
                        TrangThai = trangThai,
                        ActionText = actionText
                    };
                })
                .OrderBy(bt => bt.Deadline)
                .Take(10)
                .ToList();

            // =================================================
            // ===== VIEWMODEL CUỐI ============================
            // =================================================
            var viewModel = new StudentDashboardViewModel
            {
                HoTen = nguoiDung.LqvHoTen,
                MaSoSinhVien = nguoiDung.LqvTenDangNhap,
                Email = nguoiDung.LqvEmail ?? string.Empty,
                AvtUrl = nguoiDung.LqvAvt ?? "/img/default-avatar.jpg",
                NgaySinh = nguoiDung.LqvNgaySinh,

                TongChungNhan = chungNhans.Count,

                ChungNhanMoiNhat = chungNhans
                    .OrderByDescending(cn => cn.LqvNgayCap)
                    .Take(5)
                    .Select(cn => new ChungNhanHienThiVM
                    {
                        LqvMaChungNhan = cn.LqvMaChungNhan,
                        MaChungNhanCode = cn.LqvMaChungNhanCode,
                        TenChungNhan = cn.LqvKhoaHoc.LqvTenKhoaHoc,
                        NgayCap = cn.LqvNgayCap,
                        TrangThaiXacThuc = cn.LqvTrangThai ?? "Chưa ghi chain"
                    })
                    .ToList(),

                DanhSachBaiTap = danhSachBaiTapVM,

                LichSuDiemDanh = diemDanhs
                    .Take(5)
                    .Select(dd => new LichSuDiemDanhVM
                    {
                        TenBuoiHoc = $"Buổi học #{dd.LqvBuoiHocId}",
                        ThoiGianDiemDanh = dd.LqvThoiGian,
                        TrangThai = dd.LqvHopLe ? "Đúng giờ" : "Không hợp lệ"
                    })
                    .ToList(),

                TongBuoiDaDiemDanh = diemDanhs.Count(dd => dd.LqvHopLe),
                TongBuoiVang = diemDanhs.Count(dd => !dd.LqvHopLe)
            };

            return View(viewModel);
        }
    }
}
