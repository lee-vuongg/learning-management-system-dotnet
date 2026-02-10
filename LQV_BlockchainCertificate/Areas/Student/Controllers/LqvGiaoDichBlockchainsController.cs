using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LQV_BlockchainCertificate.Models.DBModel;
using System.Diagnostics;
using System.Net.Http;

namespace LQV_BlockchainCertificate.Areas.Student.Controllers
{
    [Area("Student")]
    public class LqvGiaoDichBlockchainsController : Controller
    {
        private readonly LqvDbContext _context;

        public LqvGiaoDichBlockchainsController(LqvDbContext context)
        {
            _context = context;
        }

        // 🧭 Lấy ID sinh viên hiện tại (tạm thời gán 1 nếu chưa login)
        private int GetCurrentUserId()
        {
            int currentUserId = 0;
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdString, out int id))
                {
                    currentUserId = id;
                }
            }
            Debug.WriteLine($"[INFO] User ID hiện tại: {currentUserId}");
            return (currentUserId > 0) ? currentUserId : 1;
        }

        public async Task<IActionResult> Index()
        {
            int currentUserId = GetCurrentUserId();

            var sinhVienChungNhanIds = await _context.LqvChungNhans
                .Where(cn => cn.LqvSinhVienId == currentUserId)
                .Select(cn => cn.LqvMaChungNhan)
                .ToListAsync();

            Console.WriteLine($"🧩 DEBUG: SinhVienID = {currentUserId}, Số chứng nhận = {sinhVienChungNhanIds.Count}");

            // ✅ Lấy tất cả rồi lọc trong bộ nhớ để tránh lỗi OPENJSON
            var allGiaoDich = await _context.LqvGiaoDichBlockchains
                .Include(gd => gd.LqvChungNhan)
                    .ThenInclude(cn => cn.LqvKhoaHoc)
                .OrderByDescending(gd => gd.LqvGioTao)
                .AsNoTracking()
                .ToListAsync();

            var giaoDichList = allGiaoDich
                .Where(gd => sinhVienChungNhanIds.Contains(gd.LqvChungNhanId))
                .ToList();

            Console.WriteLine($"✅ DEBUG: Đã lọc {giaoDichList.Count} giao dịch của sinh viên {currentUserId}");

            return View(giaoDichList);
        }


        // 📋 Chi tiết giao dịch
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                Debug.WriteLine("[ERROR] Không có ID giao dịch truyền vào.");
                return NotFound();
            }

            int currentUserId = GetCurrentUserId();
            Debug.WriteLine($"[DEBUG] Đang lấy giao dịch có ID={id} cho sinh viên={currentUserId}");

            var giaoDich = await _context.LqvGiaoDichBlockchains
                .Include(gd => gd.LqvChungNhan)
                    .ThenInclude(cn => cn.LqvKhoaHoc)
                .FirstOrDefaultAsync(gd =>
                    gd.LqvMaGiaoDich == id &&
                    gd.LqvChungNhan.LqvSinhVienId == currentUserId
                );

            if (giaoDich == null)
            {
                Debug.WriteLine("[ERROR] Không tìm thấy giao dịch hoặc không thuộc sinh viên hiện tại.");
                return NotFound();
            }

            Debug.WriteLine($"[INFO] Giao dịch {giaoDich.LqvTxHash}, Block={giaoDich.LqvBlockNumber}");

            // 🔹 Nếu chưa có BlockNumber -> gọi API lấy
            if (giaoDich.LqvBlockNumber == null && !string.IsNullOrEmpty(giaoDich.LqvTxHash))
            {
                Debug.WriteLine("[DEBUG] Giao dịch chưa có BlockNumber, đang gọi RPC để lấy...");
                giaoDich.LqvBlockNumber = await GetBlockNumberFromTxHashAsync(giaoDich.LqvTxHash);

                if (giaoDich.LqvBlockNumber != null)
                {
                    Debug.WriteLine($"[SUCCESS] Lấy được BlockNumber = {giaoDich.LqvBlockNumber}");
                    _context.Update(giaoDich);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    Debug.WriteLine("[WARN] Không lấy được BlockNumber từ RPC!");
                }
            }

            // 🔹 Tự động tạo URL Etherscan nếu chưa có
            if (string.IsNullOrEmpty(giaoDich.LqvUrlEtherscan))
            {
                giaoDich.LqvUrlEtherscan = $"https://sepolia.etherscan.io/tx/{giaoDich.LqvTxHash}";
            }

            return View(giaoDich);
        }

        // ⚙️ Hàm helper gọi RPC để lấy BlockNumber thật
        private async Task<long?> GetBlockNumberFromTxHashAsync(string txHash)
        {
            try
            {
                string rpcUrl = "https://eth-sepolia.g.alchemy.com/v2/RzNVVCRNVkqLfmdSjrDIx"; // 🔑 thay bằng API key của bạn

                using (var httpClient = new HttpClient())
                {
                    var requestData = new
                    {
                        jsonrpc = "2.0",
                        method = "eth_getTransactionReceipt",
                        @params = new[] { txHash },
                        id = 1
                    };

                    var jsonBody = JsonSerializer.Serialize(requestData);
                    Debug.WriteLine($"[RPC] POST {rpcUrl}");
                    Debug.WriteLine($"[RPC] Body: {jsonBody}");

                    var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync(rpcUrl, content);

                    Debug.WriteLine($"[RPC] Response Status: {response.StatusCode}");

                    var json = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"[RPC] Raw JSON: {json}");

                    using (var doc = JsonDocument.Parse(json))
                    {
                        if (doc.RootElement.TryGetProperty("result", out JsonElement result))
                        {
                            if (result.TryGetProperty("blockNumber", out JsonElement blockElem))
                            {
                                string blockHex = blockElem.GetString();
                                Debug.WriteLine($"[RPC] blockNumber(hex) = {blockHex}");

                                if (!string.IsNullOrEmpty(blockHex))
                                {
                                    long blockDec = Convert.ToInt64(blockHex, 16);
                                    Debug.WriteLine($"[RPC] blockNumber(dec) = {blockDec}");
                                    return blockDec;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Lỗi khi lấy BlockNumber: {ex.Message}");
            }

            return null;
        }
    }
}
