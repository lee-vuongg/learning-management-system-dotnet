using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LQV_BlockchainCertificate.Models;
using LQV_BlockchainCertificate.Models.DBModel;
using LQV_BlockchainCertificate.Services;

namespace LQV_BlockchainCertificate.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly LqvDbContext _context;
        private readonly GeminiService _gemini;

        // =====================================================
        // 🔧 CONSTRUCTOR (DI)
        // =====================================================
        public HomeController(
            ILogger<HomeController> logger,
            LqvDbContext context,
            GeminiService gemini)
        {
            _logger = logger;
            _context = context;
            _gemini = gemini;
        }

        // =====================================================
        // 🏠 TRANG CHỦ
        // =====================================================
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // =====================================================
        // 🔎 TRA CỨU CHỨNG CHỈ
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                ViewBag.Error = "Vui lòng nhập mã chứng chỉ.";
                return View();
            }

            _logger.LogInformation("Guest verify chứng chỉ: {Code}", code);

            var chungNhan = await LoadChungNhan(code);

            if (chungNhan == null)
            {
                ViewBag.Invalid = true;
                ViewBag.Code = code;
                _logger.LogWarning("Chứng chỉ KHÔNG HỢP LỆ: {Code}", code);
                return View();
            }

            ViewBag.VerifyUrl = Url.Action(
                nameof(VerifyByQr),
                "Home",
                new { code = code },
                Request.Scheme
            );

            var txHash = chungNhan.LqvGiaoDichBlockchains?
                .OrderByDescending(x => x.LqvGioTao)
                .FirstOrDefault()
                ?.LqvTxHash;

            ViewBag.BlockchainUrl = string.IsNullOrWhiteSpace(txHash)
                ? null
                : $"https://sepolia.etherscan.io/tx/{txHash}";


            ViewBag.Code = code;

            _logger.LogInformation("Chứng chỉ HỢP LỆ: {Code}", code);

            return View(chungNhan);
        }

        // =====================================================
        // 🔎 VERIFY TỪ QR / LINK NGOÀI
        // =====================================================
        // =====================================================
        // 🔎 VERIFY TỪ QR → REDIRECT SANG BLOCKCHAIN
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> VerifyByQr(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return RedirectToAction(nameof(Index));

            _logger.LogInformation("Verify từ QR: {Code}", code);

            var chungNhan = await LoadChungNhan(code);

            if (chungNhan == null)
            {
                _logger.LogWarning("QR verify thất bại – mã không hợp lệ: {Code}", code);
                ViewBag.Invalid = true;
                return View("Index");
            }

            // 🔗 LẤY TX HASH MỚI NHẤT
            var txHash = chungNhan.LqvGiaoDichBlockchains?
                .OrderByDescending(x => x.LqvGioTao)
                .FirstOrDefault()
                ?.LqvTxHash;

            if (string.IsNullOrWhiteSpace(txHash))
            {
                _logger.LogWarning("Chứng chỉ không có txHash blockchain: {Code}", code);
                ViewBag.Error = "Chứng chỉ chưa được ghi lên blockchain.";
                return View("Index", chungNhan);
            }

            var blockchainUrl = $"https://sepolia.etherscan.io/tx/{txHash}";

            _logger.LogInformation("Redirect sang blockchain: {Url}", blockchainUrl);

            // 🚀 REDIRECT THẲNG
            return Redirect(blockchainUrl);
        }
        // =====================================================
        // 📦 LOAD CHỨNG NHẬN
        // =====================================================
        private async Task<LqvChungNhan?> LoadChungNhan(string code)
        {
            return await _context.LqvChungNhans
                .Include(x => x.LqvKhoaHoc)
                .Include(x => x.LqvSinhVien)
                .Include(x => x.LqvGiaoDichBlockchains)
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.LqvMaChungNhanCode == code &&
                    x.LqvTrangThai == "Đã cấp"
                );
        }

        // =====================================================
        // 💬 AI CHAT – TÌM HIỂU HỆ THỐNG
        // =====================================================
        [HttpPost]
        public async Task<IActionResult> Chat(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return Json(new { reply = "Bạn cứ hỏi về hệ thống, sinh viên, khóa học hoặc chứng chỉ nhé 🙂" });
            }

            var msg = message.ToLower().Trim();

            // =========================
            // 🚀 TRẢ LỜI NHANH – KHÔNG GỌI AI
            // =========================

            // 📅 Ngày tháng
            if (msg.Contains("hôm nay") && msg.Contains("ngày"))
            {
                return Json(new
                {
                    reply = $"📅 Hôm nay là ngày {DateTime.Now:dd/MM/yyyy}"
                });
            }

            // 👋 Chào hỏi
            if (msg == "xin chào" || msg == "chào")
            {
                return Json(new
                {
                    reply = "👋 Xin chào! Bạn muốn tra cứu chứng chỉ, sinh viên hay khóa học?"
                });
            }

            // 👨‍🎓 Bao nhiêu sinh viên
            if (msg.Contains("bao nhiêu sinh viên"))
            {
                var totalSinhVien = await _context.LqvNguoiDungs
                    .CountAsync(x => x.LqvRoleId == 3);

                return Json(new
                {
                    reply = $"👨‍🎓 Hệ thống hiện có {totalSinhVien} sinh viên."
                });
            }

            // =========================
            // 🔹 DỮ LIỆU CHUNG (CHO AI)
            // =========================
            var totalChungNhan = await _context.LqvChungNhans.CountAsync();
            var totalSinhVienAI = await _context.LqvNguoiDungs
                .Where(x => x.LqvRoleId == 3)
                .CountAsync();
            var totalKhoaHoc = await _context.LqvKhoaHocs.CountAsync();

            // =========================
            // 🔹 DỮ LIỆU THEO NGỮ CẢNH
            // =========================
            string contextData = "";

            // 👉 Sinh viên
            if (msg.Contains("sinh viên"))
            {
                var sinhViens = await _context.LqvNguoiDungs
                    .Where(x => x.LqvRoleId == 3)
                    .Include(x => x.LqvChungNhans)
                    .OrderByDescending(x => x.LqvNgayTao)
                    .Take(5)
                    .Select(x => new
                    {
                        x.LqvHoTen,
                        SoChungChi = x.LqvChungNhans.Count
                    })
                    .ToListAsync();

                contextData = "👤 Một số sinh viên tiêu biểu:\n" +
                    string.Join("\n", sinhViens.Select(x =>
                        $"- {x.LqvHoTen}: {x.SoChungChi} chứng chỉ"));
            }
            // 👉 Khóa học
            else if (msg.Contains("khóa học"))
            {
                var khoaHocs = await _context.LqvKhoaHocs
                    .OrderByDescending(x => x.LqvNgayBatDau)
                    .Take(5)
                    .Select(x => x.LqvTenKhoaHoc)
                    .ToListAsync();

                contextData = "📚 Một số khóa học gần đây:\n" +
                    string.Join("\n", khoaHocs.Select(x => "- " + x));
            }
            // 👉 Chứng chỉ
            else if (msg.Contains("chứng chỉ") || msg.Contains("mã"))
            {
                contextData =
        @"📜 Thông tin chứng chỉ:
- Tra cứu bằng mã chứng chỉ
- Chỉ chứng chỉ **Đã cấp** mới hợp lệ
- Có thể xác thực bằng blockchain để chống giả mạo";
            }
            else
            {
                contextData =
        @"ℹ️ Bạn có thể hỏi:
- Hệ thống này dùng để làm gì?
- Sinh viên trong hệ thống
- Các khóa học hiện có
- Cách tra cứu chứng chỉ";
            }

            // =========================
            // 🧠 PROMPT (GỌN – TIẾT KIỆM QUOTA)
            // =========================
            var prompt = $@"
Bạn là trợ lý AI của hệ thống Blockchain Certificate Verification.
Trả lời ngắn gọn, dễ hiểu, thân thiện, không nói kỹ thuật sâu.

📊 Thống kê:
- Tổng chứng chỉ: {totalChungNhan}
- Tổng sinh viên: {totalSinhVienAI}
- Tổng khóa học: {totalKhoaHoc}

{contextData}

❓ Người dùng hỏi:
{message}
";

            var answer = await _gemini.AskAsync(prompt);

            return Json(new
            {
                reply = answer ?? "🤖 Trợ lý AI đang bận, bạn thử lại sau nhé 🙂"
            });
        }


        // =====================================================
        // ❌ ERROR
        // =====================================================
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
