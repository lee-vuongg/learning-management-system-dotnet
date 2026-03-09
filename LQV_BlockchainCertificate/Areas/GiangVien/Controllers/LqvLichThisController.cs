using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LQV_BlockchainCertificate.Models.DBModel;

namespace LQV_BlockchainCertificate.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    public class LqvLichThisController : Controller
    {
        private readonly LqvDbContext _context;

        public LqvLichThisController(LqvDbContext context)
        {
            _context = context;
        }

        // =============================================
        // DANH SÁCH LỊCH THI
        // =============================================
        public async Task<IActionResult> Index()
        {
            var lichThis = await _context.LqvLichThis
                .Include(x => x.LqvDeThi)
                .Include(x => x.LqvLopHoc)
                .ToListAsync();

            return View(lichThis);
        }

        // =============================================
        // XEM CHI TIẾT
        // =============================================
        public async Task<IActionResult> Details(int id)
        {
            var lichThi = await _context.LqvLichThis
                .Include(x => x.LqvDeThi)
                .Include(x => x.LqvLopHoc)
                .FirstOrDefaultAsync(x => x.LqvLichThiId == id);

            if (lichThi == null)
                return NotFound();

            return View(lichThi);
        }

        // =============================================
        // BẮT ĐẦU THI
        // =============================================
        [HttpPost]
        public async Task<IActionResult> BatDauThi(int id)
        {
            var lichThi = await _context.LqvLichThis.FindAsync(id);
            if (lichThi == null)
                return NotFound();

            // cập nhật thời gian bắt đầu = NOW
            lichThi.LqvBatDau = DateTime.Now;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // =============================================
        // KẾT THÚC THI
        // =============================================
        [HttpPost]
        public async Task<IActionResult> KetThucThi(int id)
        {
            var lichThi = await _context.LqvLichThis.FindAsync(id);
            if (lichThi == null)
                return NotFound();

            // cập nhật thời gian kết thúc = NOW
            lichThi.LqvKetThuc = DateTime.Now;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}