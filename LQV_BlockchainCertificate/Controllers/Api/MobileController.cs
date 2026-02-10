using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LQV_BlockchainCertificate.Models.DBModel;
using LQV_BlockchainCertificate.Services;

namespace LQV_BlockchainCertificate.Controllers.Api
{
    [ApiController]
    [Route("api/mobile")]
    public class MobileController : ControllerBase
    {
        private readonly ILogger<MobileController> _logger;
        private readonly LqvDbContext _context;
        private readonly GeminiService _gemini;

        public MobileController(
            ILogger<MobileController> logger,
            LqvDbContext context,
            GeminiService gemini)
        {
            _logger = logger;
            _context = context;
            _gemini = gemini;
        }

        // =====================================================
        // ✅ TEST API (PING)
        // =====================================================
       
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new
            {
                status = "success",
                message = "Mobile API OK",
                time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")
            });
        }

        // =====================================================
        // 🔎 TRA CỨU CHỨNG CHỈ (MOBILE)
        // =====================================================
        [HttpGet("certificate/{code}")]
        public async Task<IActionResult> GetCertificate(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return BadRequest(new
                {
                    error = "Mã chứng chỉ không hợp lệ"
                });
            }

            _logger.LogInformation("📱 Mobile verify chứng chỉ: {Code}", code);

            var chungNhan = await LoadChungNhan(code);

            if (chungNhan == null)
            {
                return NotFound(new
                {
                    valid = false,
                    message = "Chứng chỉ không tồn tại hoặc chưa được cấp"
                });
            }

            var txHash = chungNhan.LqvGiaoDichBlockchains?
                .OrderByDescending(x => x.LqvGioTao)
                .FirstOrDefault()
                ?.LqvTxHash;

            return Ok(new
            {
                valid = true,
                maChungChi = chungNhan.LqvMaChungNhanCode,
                sinhVien = chungNhan.LqvSinhVien?.LqvHoTen,
                khoaHoc = chungNhan.LqvKhoaHoc?.LqvTenKhoaHoc,
                ngayCap = chungNhan.LqvNgayCap,
                txHash = txHash,
                blockchainUrl = string.IsNullOrWhiteSpace(txHash)
                    ? null
                    : $"https://sepolia.etherscan.io/tx/{txHash}"
            });
        }

        // =====================================================
        // 💬 AI CHAT (MOBILE)
        // =====================================================
        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return Ok(new
                {
                    reply = "Bạn cứ hỏi về hệ thống, sinh viên, khóa học hoặc chứng chỉ nhé 🙂"
                });
            }

            var msg = request.Message.ToLower().Trim();

            // 🚀 Trả lời nhanh (không gọi AI)
            if (msg.Contains("hôm nay") && msg.Contains("ngày"))
            {
                return Ok(new
                {
                    reply = $"📅 Hôm nay là ngày {DateTime.Now:dd/MM/yyyy}"
                });
            }

            if (msg == "xin chào" || msg == "chào")
            {
                return Ok(new
                {
                    reply = "👋 Xin chào! Bạn muốn tra cứu chứng chỉ hay tìm hiểu hệ thống?"
                });
            }

            if (msg.Contains("bao nhiêu sinh viên"))
            {
                var totalSinhVien = await _context.LqvNguoiDungs
                    .CountAsync(x => x.LqvRoleId == 3);

                return Ok(new
                {
                    reply = $"👨‍🎓 Hệ thống hiện có {totalSinhVien} sinh viên."
                });
            }

            // =========================
            // 📊 DỮ LIỆU CHUNG
            // =========================
            var totalChungNhan = await _context.LqvChungNhans.CountAsync();
            var totalSinhVienAI = await _context.LqvNguoiDungs
                .Where(x => x.LqvRoleId == 3)
                .CountAsync();
            var totalKhoaHoc = await _context.LqvKhoaHocs.CountAsync();

            var prompt = $@"
Bạn là trợ lý AI của hệ thống Blockchain Certificate Verification.
Trả lời ngắn gọn, dễ hiểu, thân thiện.

📊 Thống kê:
- Tổng chứng chỉ: {totalChungNhan}
- Tổng sinh viên: {totalSinhVienAI}
- Tổng khóa học: {totalKhoaHoc}

❓ Người dùng hỏi:
{request.Message}
";

            var answer = await _gemini.AskAsync(prompt);

            return Ok(new
            {
                reply = answer ?? "🤖 Trợ lý AI đang bận, bạn thử lại sau nhé 🙂"
            });
        }

        // =====================================================
        // 📦 LOAD CHỨNG NHẬN (PRIVATE)
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
    }

    // =====================================================
    // 📩 DTO CHAT
    // =====================================================
    public class ChatRequest
    {
        public string Message { get; set; } = "";
    }
}
