using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LQV_BlockchainCertificate.Models.ViewModels;
using LQV_BlockchainCertificate.Models.DBModel;
using System.Linq;
using System.Security.Claims;
using ClosedXML.Excel;
using System.IO;
using System;
using System.Collections.Generic;

namespace LQV_BlockchainCertificate.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    [Authorize(Roles = "GiangVien")]
    public class DashboardController : Controller
    {
        private readonly LqvDbContext _context;

        public DashboardController(LqvDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            int giangVienId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var tongBaiNop = _context.LqvNopBaiTaps
                .Count(x => x.LqvBaiTap.LqvGiangVienId == giangVienId);

            var baiDaCham = _context.LqvNopBaiTaps
                .Count(x => x.LqvBaiTap.LqvGiangVienId == giangVienId && x.LqvDaCham);

            var vm = new GiangVienDashboardViewModel
            {
                TongLopHoc = _context.LqvLopHocs.Count(x => x.LqvGiangVienId == giangVienId),
                TongSinhVien = _context.LqvDangKyLopHocs
                    .Where(x => x.LqvLopHoc.LqvGiangVienId == giangVienId)
                    .Select(x => x.LqvSinhVienId).Distinct().Count(),
                TongBaiTap = _context.LqvBaiTaps.Count(x => x.LqvGiangVienId == giangVienId),
                TongKhoaHoc = _context.LqvKhoaHocs.Count(x => x.LqvGiangVienId == giangVienId),
                SoBaiTapCanCham = tongBaiNop - baiDaCham,

                LopGanDays = _context.LqvLopHocs
                    .Where(x => x.LqvGiangVienId == giangVienId)
                    .OrderByDescending(x => x.LqvNgayTao)
                    .Take(5)
                    .Select(x => new LopGanDayVM
                    {
                        LopId = x.LqvLopHocId,
                        TenLop = x.LqvTenLop,
                        NgayTao = x.LqvNgayTao,
                        SoSinhVien = x.LqvDangKyLopHocs.Count
                    }).ToList(),

                TatCaLopHoc = _context.LqvLopHocs
                    .Where(x => x.LqvGiangVienId == giangVienId)
                    .OrderBy(x => x.LqvTenLop)
                    .Select(x => new LopGanDayVM
                    {
                        LopId = x.LqvLopHocId,
                        TenLop = x.LqvTenLop
                    }).ToList()
            };

            ViewBag.DaCham = baiDaCham;
            ViewBag.ChuaCham = tongBaiNop - baiDaCham;

            return View(vm);
        }
        private double TinhDiemThang10(double diemDatDuoc, double diemToiDa)
        {
            if (diemToiDa <= 0) return 0;
            return Math.Round(diemDatDuoc / diemToiDa * 10, 2);
        }

        private (double diem4, string diemChu) QuyDoiThang4(double diem10)
        {
            if (diem10 >= 8.5) return (4.0, "A");
            if (diem10 >= 8.0) return (3.5, "B+");
            if (diem10 >= 7.0) return (3.0, "B");
            if (diem10 >= 6.5) return (2.5, "C+");
            if (diem10 >= 5.5) return (2.0, "C");
            if (diem10 >= 5.0) return (1.5, "D+");
            if (diem10 >= 4.0) return (1.0, "D");
            return (0.0, "F");
        }

        // ====================== EXPORT TỔNG ĐIỂM ======================
        // ====================== EXPORT TỔNG ĐIỂM ======================
        [HttpGet]
        public IActionResult ExportTongDiem(int lopHocId)
        {
            int giangVienId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var lopHoc = _context.LqvLopHocs
                .FirstOrDefault(x => x.LqvLopHocId == lopHocId && x.LqvGiangVienId == giangVienId);

            if (lopHoc == null) return NotFound();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(lopHoc.LqvTenLop);

            var sinhViens = _context.LqvDangKyLopHocs
                .Where(x => x.LqvLopHocId == lopHocId)
                .Select(x => x.LqvSinhVien)
                .ToList();

            int row = 1;

            ws.Cell(row, 1).Value = "BẢNG TỔNG HỢP ĐIỂM SINH VIÊN";
            ws.Range(row, 1, row, 11).Merge().Style
                .Font.SetBold().Font.SetFontSize(16)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            row += 2;

            ws.Cell(row, 1).Value = $"Lớp: {lopHoc.LqvTenLop}";
            ws.Cell(row, 8).Value = $"Ngày xuất: {DateTime.Now:dd/MM/yyyy}";
            row += 2;

            string[] headers = {
                "STT", "MSSV", "Họ tên", "Lớp",
                "Điểm BT", "Điểm Thi (10)",
                "Tổng (10)", "Thang 4", "Điểm chữ", "Kết quả"
            };

            for (int i = 0; i < headers.Length; i++)
                ws.Cell(row, i + 1).Value = headers[i];

            ws.Range(row, 1, row, headers.Length).Style.Font.Bold = true;
            row++;

            int stt = 1;

            foreach (var sv in sinhViens)
            {
                // Tổng điểm bài tập (giả sử BT đã chấm theo thang 10)
                double diemBT = _context.LqvNopBaiTaps
                    .Where(x => x.LqvSinhVienId == sv.LqvId
                             && x.LqvBaiTap.LqvLopHocId == lopHocId
                             && x.LqvDaCham)
                    .Sum(x => (double?)x.LqvDiem) ?? 0;

                // ====== ĐIỂM THI CHUẨN ĐẠI HỌC ======
                var baiThi = _context.LqvBaiLams
                    .Where(x => x.LqvUserId == sv.LqvId
                             && x.LqvLichThi.LqvLopHocId == lopHocId
                             && x.LqvTrangThai == "DaCham")
                    .Select(x => new
                    {
                        DiemDat = x.LqvDiem ?? 0,
                        DiemToiDa = x.LqvLichThi.LqvDeThi.LqvTongDiem
                    })
                    .FirstOrDefault();

                double diemThi10 = baiThi != null
                    ? TinhDiemThang10(baiThi.DiemDat, baiThi.DiemToiDa)
                    : 0;

                double tong10 = Math.Round((diemBT + diemThi10) / 2, 2);
                var (diem4, diemChu) = QuyDoiThang4(tong10);

                ws.Cell(row, 1).Value = stt++;
                ws.Cell(row, 2).Value = sv.LqvId;
                ws.Cell(row, 3).Value = sv.LqvHoTen;
                ws.Cell(row, 4).Value = lopHoc.LqvTenLop;
                ws.Cell(row, 5).Value = diemBT;
                ws.Cell(row, 6).Value = diemThi10;
                ws.Cell(row, 7).Value = tong10;
                ws.Cell(row, 8).Value = diem4;
                ws.Cell(row, 9).Value = diemChu;
                ws.Cell(row, 10).Value = tong10 >= 4 ? "Đạt" : "Không đạt";

                if (tong10 < 4)
                    ws.Range(row, 1, row, 10).Style.Font.FontColor = XLColor.Red;

                row++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            wb.SaveAs(stream);

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"TongHopDiem_{lopHoc.LqvTenLop}.xlsx"
            );
        }

        // ====================== EXPORT CHI TIẾT ======================
        [HttpGet]
        public IActionResult ExportChiTietDiem(int lopHocId)
        {
            int giangVienId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var lopHoc = _context.LqvLopHocs
                .FirstOrDefault(x => x.LqvLopHocId == lopHocId && x.LqvGiangVienId == giangVienId);

            if (lopHoc == null) return NotFound();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add($"{lopHoc.LqvTenLop}_ChiTiet");

            var data = new List<ExportChiTietDiemRowVM>();

            var sinhViens = _context.LqvDangKyLopHocs
                .Where(x => x.LqvLopHocId == lopHocId)
                .Select(x => x.LqvSinhVien)
                .ToList();

            foreach (var sv in sinhViens)
            {
                data.AddRange(
                    _context.LqvNopBaiTaps
                    .Where(x => x.LqvSinhVienId == sv.LqvId
                             && x.LqvBaiTap.LqvLopHocId == lopHocId
                             && x.LqvDaCham)
                    .Select(x => new ExportChiTietDiemRowVM
                    {
                        MaSV = sv.LqvId.ToString(),
                        HoTen = sv.LqvHoTen,
                        Loai = "Bài tập",
                        TenBai = x.LqvBaiTap.LqvTieuDe,
                        Diem = x.LqvDiem ?? 0,
                        NgayNop = x.LqvThoiGianNop
                    })
                );

                data.AddRange(
                    _context.LqvBaiLams
                    .Where(x => x.LqvUserId == sv.LqvId
                             && x.LqvLichThi.LqvLopHocId == lopHocId
                             && x.LqvTrangThai == "DaCham")
                    .Select(x => new ExportChiTietDiemRowVM
                    {
                        MaSV = sv.LqvId.ToString(),
                        HoTen = sv.LqvHoTen,
                        Loai = "Bài thi",
                        TenBai = x.LqvLichThi.LqvDeThi.LqvTenDeThi,
                        Diem = x.LqvDiem ?? 0,
                        NgayNop = x.LqvThoiGianNop
                    })
                );
            }

            int row = 1;

            ws.Cell(row, 1).Value = "BẢNG CHI TIẾT ĐIỂM SINH VIÊN";
            ws.Range(row, 1, row, 7).Merge().Style
                .Font.SetBold().Font.SetFontSize(16)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            row += 2;

            ws.Cell(row, 1).Value = $"Lớp: {lopHoc.LqvTenLop}";
            ws.Cell(row, 5).Value = $"Ngày xuất: {DateTime.Now:dd/MM/yyyy}";
            row += 2;

            string[] headers = { "STT", "MSSV", "Họ tên", "Loại", "Tên bài", "Điểm", "Ngày nộp" };
            for (int i = 0; i < headers.Length; i++)
                ws.Cell(row, i + 1).Value = headers[i];

            ws.Range(row, 1, row, 7).Style.Font.Bold = true;
            row++;

            int stt = 1;
            foreach (var d in data.OrderBy(x => x.MaSV).ThenBy(x => x.Loai))
            {
                ws.Cell(row, 1).Value = stt++;
                ws.Cell(row, 2).Value = d.MaSV;
                ws.Cell(row, 3).Value = d.HoTen;
                ws.Cell(row, 4).Value = d.Loai;
                ws.Cell(row, 5).Value = d.TenBai;
                ws.Cell(row, 6).Value = d.Diem;
                ws.Cell(row, 7).Value = d.NgayNop?.ToString("dd/MM/yyyy");
                row++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            wb.SaveAs(stream);

            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"ChiTietDiem_{lopHoc.LqvTenLop}.xlsx");
        }

        public class ExportTongDiemRowVM
        {
            public string MaSV { get; set; }
            public string HoTen { get; set; }
            public string Lop { get; set; }
            public double DiemBaiTap { get; set; }
            public double DiemBaiThi10 { get; set; }
            public double TongDiem10 { get; set; }
            public double DiemThang4 { get; set; }
            public string DiemChu { get; set; }
            public string KetQua { get; set; }
        }

        public class ExportChiTietDiemRowVM
        {
            public string MaSV { get; set; }
            public string HoTen { get; set; }
            public string Loai { get; set; }
            public string TenBai { get; set; }
            public double Diem { get; set; }
            public DateTime? NgayNop { get; set; }
        }



    }
}
