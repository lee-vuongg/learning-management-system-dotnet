using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LQV_BlockchainCertificate.Models.DBModel;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Security.Claims;

namespace LQV_BlockchainCertificate.Areas.Student.Controllers
{
    [Area("Student")]
    public class LqvXacThucChungNhansController : Controller
    {
        private readonly LqvDbContext _context;

        public LqvXacThucChungNhansController(LqvDbContext context)
        {
            _context = context;
            QuestPDF.Settings.License = LicenseType.Community;
        }
        private int GetSinhVienId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        // ======================================================
        // 📜 1. LỊCH SỬ XÁC THỰC
        // ======================================================
        public async Task<IActionResult> Index()
        {
            int sinhVienId = GetSinhVienId();

            var data = await (
                from xt in _context.LqvXacThucChungNhans
                join cn in _context.LqvChungNhans
                    on xt.LqvMaChungNhanCode equals cn.LqvMaChungNhanCode
                where cn.LqvSinhVienId == sinhVienId
                orderby xt.LqvThoiGianXacThuc descending
                select xt
            ).AsNoTracking().ToListAsync();

            return View(data);
        }

        // ======================================================
        // 🔍 2. FORM XÁC THỰC
        // ======================================================
        [HttpGet]
        public IActionResult VerifyForm() => View();

        // ======================================================
        // 🔎 3A. VERIFY - GET (QR / LINK)
        // ======================================================
        [HttpGet]
        public async Task<IActionResult> Verify(string maChungNhanCode)
        {
            if (string.IsNullOrWhiteSpace(maChungNhanCode))
                return RedirectToAction(nameof(VerifyForm));

            var chungNhan = await LoadChungNhan(maChungNhanCode);

            if (chungNhan == null)
            {
                TempData["ErrorMessage"] =
                    $"Thất bại: Mã chứng nhận {maChungNhanCode} không tồn tại.";
                return RedirectToAction(nameof(VerifyForm));
            }

            ViewBag.KetQuaXacThuc =
                "Hợp lệ: Thông tin khớp với bản ghi Blockchain (Sepolia).";

            ViewBag.QRCodeBase64 = GenerateBlockchainQr(chungNhan);

            await LogVerification(
                maChungNhanCode,
                ViewBag.KetQuaXacThuc
            );

            return View("VerificationResult", chungNhan);
        }

        // ======================================================
        // 🔎 3B. VERIFY - POST
        // ======================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult VerifyPost(string maChungNhanCode)
        {
            if (string.IsNullOrWhiteSpace(maChungNhanCode))
            {
                TempData["ErrorMessage"] =
                    "Vui lòng nhập Mã Chứng nhận hợp lệ.";
                return RedirectToAction(nameof(VerifyForm));
            }

            return RedirectToAction(nameof(Verify),
                new { maChungNhanCode });
        }

        // ======================================================
        // 📥 4. EXPORT PDF (QUESTPDF)
        // ======================================================
        [HttpGet]
        public async Task<IActionResult> ExportPdf(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return BadRequest("Mã chứng nhận không hợp lệ.");

            var model = await LoadChungNhan(code);
            if (model == null)
                return NotFound("Không tìm thấy chứng nhận.");

            var qrBase64 = GenerateBlockchainQr(model);
            var qrBytes = Convert.FromBase64String(qrBase64);

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(30);

                    page.Content().Column(col =>
                    {
                        col.Item().AlignCenter()
                            .Text("CHỨNG CHỈ BLOCKCHAIN")
                            .FontSize(26)
                            .Bold();

                        col.Item().PaddingTop(8)
                            .AlignCenter()
                            .Text("Ethereum Sepolia Testnet")
                            .FontSize(12)
                            .Italic();

                        col.Item().PaddingVertical(15)
                            .LineHorizontal(1);

                        col.Item().Text($"Mã chứng nhận: {model.LqvMaChungNhanCode}");
                        col.Item().Text($"Sinh viên: {model.LqvSinhVien?.LqvHoTen}");
                        col.Item().Text($"Khóa học: {model.LqvKhoaHoc?.LqvTenKhoaHoc}");
                        col.Item().Text($"Giảng viên: {model.LqvKhoaHoc?.LqvGiangVien?.LqvHoTen}");
                        col.Item().Text($"Ngày cấp: {model.LqvNgayCap:dd/MM/yyyy}");

                        col.Item().PaddingVertical(12)
                            .Text("Hợp lệ: Dữ liệu đã được ghi nhận trên Blockchain.")
                            .Bold()
                            .FontColor(Colors.Green.Darken2);

                        // ⚠️ QuestPDF: Height PHẢI đứng trước Image
                        col.Item()
                            .PaddingTop(20)
                            .AlignCenter()
                            .Height(150)
                            .Image(qrBytes, ImageScaling.FitArea);

                        col.Item().AlignCenter()
                            .PaddingTop(5)
                            .Text("Quét QR để xem giao dịch trên Sepolia Etherscan")
                            .FontSize(9)
                            .Italic();
                    });

                    page.Footer().AlignCenter()
                        .Text($"Verification Code: {code}")
                        .FontSize(9);
                });
            }).GeneratePdf();

            return File(
                pdfBytes,
                "application/pdf",
                $"ChungChi_{code}.pdf"
            );
        }

        // ======================================================
        // 🔧 LOAD CHỨNG NHẬN
        // ======================================================
        private async Task<LqvChungNhan?> LoadChungNhan(string code)
        {
            return await _context.LqvChungNhans
                .Include(x => x.LqvKhoaHoc)
                    .ThenInclude(x => x.LqvGiangVien)
                .Include(x => x.LqvSinhVien)
                .Include(x => x.LqvGiaoDichBlockchains)
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.LqvMaChungNhanCode == code
                );
        }

        // ======================================================
        // 🔗 QR → SEPOLIA ETHERSCAN
        // ======================================================
        private string GenerateBlockchainQr(LqvChungNhan chungNhan)
        {
            var txHash = chungNhan.LqvGiaoDichBlockchains?
                .OrderByDescending(x => x.LqvGioTao)
                .FirstOrDefault()
                ?.LqvTxHash;

            if (string.IsNullOrWhiteSpace(txHash))
                return string.Empty;

            // ✅ SEPOLIA TESTNET
            string explorerUrl =
                $"https://sepolia.etherscan.io/tx/{txHash}";

            using var qrGen = new QRCodeGenerator();
            using var qrData = qrGen.CreateQrCode(
                explorerUrl,
                QRCodeGenerator.ECCLevel.Q
            );
            using var qr = new PngByteQRCode(qrData);

            return Convert.ToBase64String(qr.GetGraphic(8));
        }

        // ======================================================
        // 📝 LOG XÁC THỰC
        // ======================================================
        private async Task LogVerification(string code, string result)
        {
            // 👉 DB chỉ cần ngắn gọn
            string dbResult =
                result.Contains("Hợp lệ")
                    ? "HOP_LE"
                    : "KHONG_HOP_LE";

            Console.WriteLine($"RESULT LENGTH = {result.Length}");
            Console.WriteLine($"DB RESULT     = {dbResult}");

            _context.LqvXacThucChungNhans.Add(
                new LqvXacThucChungNhan
                {
                    LqvMaChungNhanCode = code,
                    LqvThoiGianXacThuc = DateTime.Now,
                    LqvDiaChiNguoiXacThuc =
                        HttpContext.Connection.RemoteIpAddress?.ToString()
                        ?? "Unknown",
                    LqvKetQua = dbResult   // 🔥 CHỈ LƯU NGẮN
                });

            await _context.SaveChangesAsync();
        }



    }
}
