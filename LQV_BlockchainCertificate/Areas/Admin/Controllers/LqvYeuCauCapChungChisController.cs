using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LQV_BlockchainCertificate.Models.DBModel;
using LQV_BlockchainCertificate.Services;

namespace LQV_BlockchainCertificate.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LqvYeuCauCapChungChisController : Controller
    {
        private readonly LqvDbContext _context;
        private readonly EthereumService _ethereumService;

        // ✅ Inject service đúng chuẩn
        public LqvYeuCauCapChungChisController(
            LqvDbContext context,
            EthereumService ethereumService)
        {
            _context = context;
            _ethereumService = ethereumService;
        }

        // ===============================
        // DANH SÁCH YÊU CẦU
        // ===============================
        public async Task<IActionResult> Index()
        {
            var data = _context.LqvYeuCauCapChungChis
                .Where(x => x.LqvTrangThai != "Đã xóa")
                .Include(x => x.LqvKhoaHoc)
                .Include(x => x.LqvNguoiDung);

            return View(await data.ToListAsync());
        }

        // ===============================
        // APPROVE + GHI BLOCKCHAIN
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var yeuCau = await _context.LqvYeuCauCapChungChis
                .Include(x => x.LqvNguoiDung)
                .Include(x => x.LqvKhoaHoc)
                .FirstOrDefaultAsync(x => x.LqvId == id);

            if (yeuCau == null) return NotFound();

            if (yeuCau.LqvTrangThai != "Chờ duyệt")
            {
                TempData["WarningMessage"] = "Trạng thái không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            // 🔍 Check trùng chứng nhận
            bool daTonTai = await _context.LqvChungNhans.AnyAsync(c =>
                c.LqvSinhVienId == yeuCau.LqvNguoiDungId &&
                c.LqvKhoaHocId == yeuCau.LqvKhoaHocId &&
                c.LqvTrangThai == "Đã cấp");

            if (daTonTai)
            {
                yeuCau.LqvTrangThai = "Đã duyệt (Trùng)";
                await _context.SaveChangesAsync();

                TempData["WarningMessage"] = "Chứng nhận đã tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            // ===============================
            // 1️⃣ TẠO CHỨNG NHẬN (DB)
            // ===============================
            string certHash = Guid.NewGuid().ToString("N");

            var chungNhan = new LqvChungNhan
            {
                LqvSinhVienId = yeuCau.LqvNguoiDungId,
                LqvKhoaHocId = yeuCau.LqvKhoaHocId,
                LqvNgayCap = DateTime.Now,
                LqvHashValue = certHash,
                LqvTrangThai = "Đã cấp",
                LqvMaChungNhanCode = $"CC-{yeuCau.LqvKhoaHocId}-{certHash[..8].ToUpper()}"
            };

            _context.LqvChungNhans.Add(chungNhan);
            await _context.SaveChangesAsync();

            // ===============================
            // 2️⃣ GHI BLOCKCHAIN
            // ===============================
            try
            {
                var txHash = await _ethereumService.IssueCertificateAsync(
                    yeuCau.LqvNguoiDung?.LqvHoTen ?? "N/A",
                    yeuCau.LqvKhoaHoc?.LqvTenKhoaHoc ?? "N/A",
                    DateTime.Now.ToString("yyyy-MM-dd"),
                    certHash
                );

                // ⛓ Chờ receipt chuẩn
                var receipt = await _ethereumService.WaitForReceiptAsync(txHash);

                // ===============================
                // 3️⃣ LƯU GIAO DỊCH BLOCKCHAIN
                // ===============================
                var giaoDich = new LqvGiaoDichBlockchain
                {
                    LqvChungNhanId = chungNhan.LqvMaChungNhan,
                    LqvTxHash = txHash,
                    LqvBlockNumber = (long)receipt.BlockNumber.Value,
                    LqvGioTao = DateTime.Now,
                    LqvStatus = "Confirmed"
                };

                _context.LqvGiaoDichBlockchains.Add(giaoDich);

                // ===============================
                // 4️⃣ UPDATE YÊU CẦU
                // ===============================
                yeuCau.LqvTrangThai = "Đã duyệt";
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] =
                    $"Duyệt thành công | TX: {txHash} | Block: {giaoDich.LqvBlockNumber}";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi ghi Blockchain: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // ===============================
        // DETAILS CHỨNG NHẬN
        // ===============================
        public async Task<IActionResult> Details(int id)
        {
            // id = LqvYeuCauCapChungChi.LqvId
            var yeuCau = await _context.LqvYeuCauCapChungChis
                .FirstOrDefaultAsync(x => x.LqvId == id);

            if (yeuCau == null)
                return NotFound();

            var chungNhan = await _context.LqvChungNhans
                .Include(x => x.LqvSinhVien)
                .Include(x => x.LqvKhoaHoc)
                .Include(x => x.LqvGiaoDichBlockchains)
                .FirstOrDefaultAsync(x =>
                    x.LqvSinhVienId == yeuCau.LqvNguoiDungId &&
                    x.LqvKhoaHocId == yeuCau.LqvKhoaHocId &&
                    x.LqvTrangThai == "Đã cấp");

            if (chungNhan == null)
                return NotFound("Chưa có chứng nhận cho yêu cầu này");

            return View(chungNhan);
        }


        // ===============================
        // REJECT
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string lqvLyDoTuChoi)
        {
            var yc = await _context.LqvYeuCauCapChungChis.FindAsync(id);
            if (yc == null) return NotFound();

            yc.LqvTrangThai = "Đã từ chối";
            yc.LqvLyDoTuChoi = lqvLyDoTuChoi;

            _context.Update(yc);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Từ chối yêu cầu thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}
