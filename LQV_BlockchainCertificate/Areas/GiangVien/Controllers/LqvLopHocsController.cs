using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using LQV_BlockchainCertificate.Models.DBModel;
using LQV_BlockchainCertificate.Models.ViewModels;
using Org.BouncyCastle.Crypto.Generators;
using System.ComponentModel;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using System.IO;
using LQV_BlockchainCertificate.Services;


namespace LQV_BlockchainCertificate.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    [Authorize(Roles = "GiangVien")]
    public class LqvLopHocsController : Controller
    {
        private readonly LqvDbContext _context;
        private readonly IEmailService _emailService;


        public LqvLopHocsController(
     LqvDbContext context,
     IEmailService emailService
 )
        {
            _context = context;
            _emailService = emailService;
        }


        // =========================================================
        // 🔐 LẤY ID GIẢNG VIÊN
        // =========================================================
        private int GetGiangVienId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        // =========================================================
        // 📌 INDEX – LỚP GIẢNG DẠY
        // =========================================================
        public async Task<IActionResult> Index()
        {
            int giangVienId = GetGiangVienId();

            var lopHocs = await _context.LqvLopHocs
                .Include(l => l.LqvKhoaHoc)
                .Include(l => l.LqvDangKyLopHocs)
                .Where(l => l.LqvGiangVienId == giangVienId)
                .OrderByDescending(l => l.LqvNgayTao)
                .ToListAsync();

            return View(lopHocs);
        }

        // =========================================================
        // 📌 SINH VIÊN TRONG LỚP
        // =========================================================
        public async Task<IActionResult> SinhVien(int id)
        {
            int giangVienId = GetGiangVienId();

            var lopHoc = await _context.LqvLopHocs
                .Include(l => l.LqvDangKyLopHocs)
                    .ThenInclude(dk => dk.LqvSinhVien)
                .FirstOrDefaultAsync(l =>
                    l.LqvLopHocId == id &&
                    l.LqvGiangVienId == giangVienId
                );

            if (lopHoc == null)
                return NotFound();

            var sinhViens = lopHoc.LqvDangKyLopHocs
                .Select(dk => new SinhVienLopHocViewModel
                {
                    SinhVienId = dk.LqvSinhVienId,
                    HoTen = dk.LqvSinhVien.LqvHoTen,
                    Email = dk.LqvSinhVien.LqvEmail,
                    ChuyenCan = TinhChuyenCan(id, dk.LqvSinhVienId),
                    DuDieuKienChungChi = DuDieuKienCapChungChi(id, dk.LqvSinhVienId)
                })
                .ToList();

            return View(new SinhVienLopHocListViewModel
            {
                LopHocId = lopHoc.LqvLopHocId,
                TenLop = lopHoc.LqvTenLop,
                SinhViens = sinhViens
            });
        }
        // =========================================================
        // ❌ XÓA SINH VIÊN KHỎI LỚP
        // =========================================================
        [HttpPost]
        [Authorize(Roles = "GiangVien")]
        public async Task<IActionResult> XoaSinhVien(int lopHocId, int sinhVienId)
        {
            int giangVienId = GetGiangVienId();

            var dangKy = await _context.LqvDangKyLopHocs
                .Include(x => x.LqvLopHoc)
                .FirstOrDefaultAsync(x =>
                    x.LqvLopHocId == lopHocId &&
                    x.LqvSinhVienId == sinhVienId &&
                    x.LqvLopHoc.LqvGiangVienId == giangVienId
                );

            if (dangKy == null)
                return NotFound();

            _context.LqvDangKyLopHocs.Remove(dangKy);
            await _context.SaveChangesAsync();

            // quay lại tab People của Classroom
            return RedirectToAction(
                "Index",
                "LqvClassroom",
                new { lopHocId, tab = "people" }
            );
        }

        // =========================================================
        // ✉️ GỬI MAIL CHO SINH VIÊN
        // =========================================================
        [HttpPost]
        [Authorize(Roles = "GiangVien")]
        public async Task<IActionResult> GuiMailSinhVien(
            int lopHocId,
            int sinhVienId,
            string noiDung)
        {
            int giangVienId = GetGiangVienId();

            var lopHoc = await _context.LqvLopHocs
                .FirstOrDefaultAsync(x =>
                    x.LqvLopHocId == lopHocId &&
                    x.LqvGiangVienId == giangVienId
                );

            if (lopHoc == null)
                return NotFound();

            var sinhVien = await _context.LqvNguoiDungs
                .FirstOrDefaultAsync(x => x.LqvId == sinhVienId);

            if (sinhVien == null)
                return NotFound();

            string subject = $"Thông báo từ lớp {lopHoc.LqvTenLop}";
            string body = $@"
        <p>Xin chào <b>{sinhVien.LqvHoTen}</b>,</p>
        <p>{noiDung}</p>
        <hr/>
        <p><i>Giảng viên {User.Identity?.Name}</i></p>
    ";

            await _emailService.SendEmailAsync(
                sinhVien.LqvEmail,
                subject,
                body
            );

            return RedirectToAction(
                "Index",
                "LqvClassroom",
                new { lopHocId, tab = "people" }
            );
        }

        // =========================================================
        // 📥 IMPORT SINH VIÊN TỪ EXCEL
        // =========================================================
        [HttpPost]
        public async Task<IActionResult> ImportSinhVien(int lopHocId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File không hợp lệ");

            var lopHoc = await _context.LqvLopHocs.FindAsync(lopHocId);
            if (lopHoc == null)
                return NotFound();

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            using var workbook = new XLWorkbook(stream);
            var ws = workbook.Worksheet(1);
            var rows = ws.RangeUsed().RowsUsed().Skip(1);

            foreach (var row in rows)
            {
                string hoTen = row.Cell(2).GetString().Trim();
                string email = row.Cell(3).GetString().Trim();
                string tenDangNhap = row.Cell(4).GetString().Trim();
                DateTime? ngaySinh = row.Cell(5).TryGetValue(out DateTime ns) ? ns : null;
                string wallet = row.Cell(6).GetString().Trim();

                if (string.IsNullOrEmpty(tenDangNhap) || string.IsNullOrEmpty(email))
                    continue;

                bool isNewUser = false;

                var sv = await _context.LqvNguoiDungs
                    .FirstOrDefaultAsync(x => x.LqvTenDangNhap == tenDangNhap);

                if (sv == null)
                {
                    sv = new LqvNguoiDung
                    {
                        LqvTenDangNhap = tenDangNhap,
                        LqvHoTen = hoTen,
                        LqvEmail = email,
                        LqvNgaySinh = ngaySinh,
                        LqvWalletAddress = string.IsNullOrEmpty(wallet) ? null : wallet,
                        LqvRoleId = 3, // Sinh viên
                        LqvMatKhauHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                        LqvNgayTao = DateTime.Now,
                        LqvDaXacThuc = true
                    };

                    _context.LqvNguoiDungs.Add(sv);
                    await _context.SaveChangesAsync();
                    isNewUser = true;
                }

                bool exists = await _context.LqvDangKyLopHocs.AnyAsync(x =>
                    x.LqvSinhVienId == sv.LqvId &&
                    x.LqvLopHocId == lopHocId);

                if (!exists)
                {
                    _context.LqvDangKyLopHocs.Add(new LqvDangKyLopHoc
                    {
                        LqvSinhVienId = sv.LqvId,
                        LqvLopHocId = lopHocId,
                        LqvNgayDangKy = DateTime.Now
                    });

                    await _context.SaveChangesAsync();

                    string subject = "Thông báo tham gia lớp học";
                    string body = $@"
                <h3>Xin chào {sv.LqvHoTen},</h3>
                <p>Bạn đã được thêm vào lớp <b>{lopHoc.LqvTenLop}</b></p>
                <ul>
                    <li>Username: <b>{sv.LqvTenDangNhap}</b></li>
                    {(isNewUser ? "<li>Password: <b>123456</b></li>" : "")}
                </ul>
                <p>Vui lòng đổi mật khẩu sau khi đăng nhập.</p>";

                    await _emailService.SendEmailAsync(sv.LqvEmail, subject, body);
                }
            }

            return RedirectToAction(nameof(SinhVien), new { id = lopHocId });
        }

        // =========================================================
        // 📤 EXPORT SINH VIÊN RA EXCEL
        // =========================================================
        public async Task<IActionResult> ExportSinhVien(int lopHocId)
        {
            var data = await _context.LqvDangKyLopHocs
                .Include(x => x.LqvSinhVien)
                .Where(x => x.LqvLopHocId == lopHocId)
                .Select(x => new
                {
                    x.LqvSinhVien.LqvHoTen,
                    x.LqvSinhVien.LqvEmail,
                    x.LqvSinhVien.LqvTenDangNhap,
                    x.LqvSinhVien.LqvNgaySinh,
                    x.LqvSinhVien.LqvWalletAddress,
                    x.LqvSinhVien.LqvDaXacThuc,
                    x.LqvNgayDangKy
                })
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("SinhVien");

            ws.Cell(1, 1).Value = "HoTen";
            ws.Cell(1, 2).Value = "Email";
            ws.Cell(1, 3).Value = "TenDangNhap";
            ws.Cell(1, 4).Value = "NgaySinh";
            ws.Cell(1, 5).Value = "Wallet";
            ws.Cell(1, 6).Value = "DaXacThuc";
            ws.Cell(1, 7).Value = "NgayDangKy";

            int row = 2;
            foreach (var sv in data)
            {
                ws.Cell(row, 1).Value = sv.LqvHoTen;
                ws.Cell(row, 2).Value = sv.LqvEmail;
                ws.Cell(row, 3).Value = sv.LqvTenDangNhap;
                ws.Cell(row, 4).Value = sv.LqvNgaySinh?.ToString("dd/MM/yyyy");
                ws.Cell(row, 5).Value = sv.LqvWalletAddress;
                ws.Cell(row, 6).Value = sv.LqvDaXacThuc ? "Đã xác thực" : "Chưa xác thực";
                ws.Cell(row, 7).Value = sv.LqvNgayDangKy?.ToString("dd/MM/yyyy");
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "DanhSachSinhVien_Full.xlsx"
            );
        }

        // =========================================================
        // 📊 CHUYÊN CẦN
        // =========================================================
        private double TinhChuyenCan(int lopHocId, int sinhVienId)
        {
            int tong = _context.LqvDiemDanhGps
                .Where(x => x.LqvLopHocId == lopHocId)
                .Select(x => x.LqvBuoiHocId)
                .Distinct()
                .Count();

            if (tong == 0) return 0;

            int coMat = _context.LqvDiemDanhGps
                .Where(x =>
                    x.LqvLopHocId == lopHocId &&
                    x.LqvSinhVienId == sinhVienId &&
                    x.LqvHopLe == true
                )
                .Select(x => x.LqvBuoiHocId)
                .Distinct()
                .Count();

            return Math.Round((double)coMat / tong * 100, 2);
        }

        private bool DuDieuKienCapChungChi(int lopHocId, int sinhVienId)
        {
            return TinhChuyenCan(lopHocId, sinhVienId) >= 80;
        }
    }
}
