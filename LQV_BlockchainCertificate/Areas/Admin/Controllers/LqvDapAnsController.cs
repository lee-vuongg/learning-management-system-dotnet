using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LQV_BlockchainCertificate.Models.DBModel;
using System;

namespace LQV_BlockchainCertificate.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LqvDapAnsController : Controller
    {
        private readonly LqvDbContext _context;

        public LqvDapAnsController(LqvDbContext context)
        {
            _context = context;
            Console.WriteLine(">>> LqvDapAnsController INIT");
        }

        // ===============================
        // VIEW: ĐÁP ÁN THEO CÂU HỎI
        // ===============================
        public async Task<IActionResult> ByCauHoi(int cauHoiId)
        {
            var cauHoi = await _context.LqvCauHois
                .Include(x => x.LqvDapAns)
                .FirstOrDefaultAsync(x => x.LqvCauHoiId == cauHoiId);

            if (cauHoi == null) return NotFound();

            return View(cauHoi);
        }


        // ===============================
        // CREATE (AJAX)
        // ===============================
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Create(int cauHoiId, string noiDung, bool lqvDung)
        {
            Console.WriteLine("===== CREATE START =====");
            Console.WriteLine($"cauHoiId={cauHoiId}, noiDung={noiDung}, lqvDung={lqvDung}");

            var dapAn = new LqvDapAn
            {
                LqvCauHoiId = cauHoiId,
                LqvNoiDung = noiDung,
                LqvDung = lqvDung
            };

            _context.LqvDapAns.Add(dapAn);
            await _context.SaveChangesAsync();

            Console.WriteLine("===== CREATE SUCCESS =====");

            return Json(new { success = true });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            Console.WriteLine("===== DELETE START =====");
            Console.WriteLine($"id={id}");

            var dapAn = await _context.LqvDapAns.FindAsync(id);
            if (dapAn == null)
            {
                Console.WriteLine("❌ NOT FOUND");
                return Json(new { success = false });
            }

            _context.LqvDapAns.Remove(dapAn);
            await _context.SaveChangesAsync();

            Console.WriteLine("===== DELETE SUCCESS =====");

            return Json(new { success = true });
        }

    }
}
