using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LQV_BlockchainCertificate.Models.DBModel;
using Microsoft.AspNetCore.Authorization;

namespace LQV_BlockchainCertificate.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    [Authorize(Roles = "GiangVien")]
    public class LqvDapAnsController : Controller
    {
        private readonly LqvDbContext _context;

        public LqvDapAnsController(LqvDbContext context)
        {
            _context = context;
        }

        // ===============================
        // VIEW: ĐÁP ÁN THEO CÂU HỎI
        // ===============================
        [HttpGet]
        public async Task<IActionResult> ByCauHoi(int cauHoiId)
        {
            Console.WriteLine("===== ByCauHoi HIT =====");
            Console.WriteLine($"cauHoiId = {cauHoiId}");

            if (cauHoiId <= 0)
            {
                Console.WriteLine("❌ cauHoiId <= 0");
                return BadRequest("cauHoiId invalid");
            }

            var cauHoi = await _context.LqvCauHois
                .Include(x => x.LqvDapAns)
                .FirstOrDefaultAsync(x => x.LqvCauHoiId == cauHoiId);

            if (cauHoi == null)
            {
                Console.WriteLine("❌ Không tìm thấy câu hỏi");
                return NotFound();
            }

            Console.WriteLine($"✅ Load câu hỏi: {cauHoi.LqvCauHoiId}");
            Console.WriteLine($"👉 Số đáp án: {cauHoi.LqvDapAns.Count}");

            return View(cauHoi);
        }

        // ===============================
        // CREATE ĐÁP ÁN (AJAX)
        // ===============================
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Create(
            int cauHoiId,
            string noiDung,
            bool dung)
        {
            Console.WriteLine("===== CREATE HIT =====");
            Console.WriteLine($"cauHoiId = {cauHoiId}");
            Console.WriteLine($"noiDung = {noiDung}");
            Console.WriteLine($"dung = {dung}");

            if (cauHoiId <= 0)
            {
                Console.WriteLine("❌ cauHoiId <= 0");
                return BadRequest("cauHoiId invalid");
            }

            if (string.IsNullOrWhiteSpace(noiDung))
            {
                Console.WriteLine("❌ noiDung rỗng");
                return BadRequest("noiDung empty");
            }

            var da = new LqvDapAn
            {
                LqvCauHoiId = cauHoiId,
                LqvNoiDung = noiDung,
                LqvDung = dung
            };

            Console.WriteLine("👉 Chuẩn bị add DB");

            _context.LqvDapAns.Add(da);
            await _context.SaveChangesAsync();

            Console.WriteLine($"✅ INSERT OK – DapAnId = {da.LqvDapAnId}");

            return Ok(new { success = true });
        }

        // ===============================
        // DELETE
        // ===============================
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            Console.WriteLine("===== DELETE HIT =====");
            Console.WriteLine($"id = {id}");

            var da = await _context.LqvDapAns.FindAsync(id);
            if (da == null)
            {
                Console.WriteLine("❌ Không tìm thấy đáp án");
                return NotFound();
            }

            _context.LqvDapAns.Remove(da);
            await _context.SaveChangesAsync();

            Console.WriteLine("✅ DELETE OK");

            return Ok(new { success = true });
        }

        // ===============================
        // TOGGLE ĐÚNG / SAI
        // ===============================
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> ToggleDung(int id)
        {
            Console.WriteLine("===== TOGGLE HIT =====");
            Console.WriteLine($"id = {id}");

            var da = await _context.LqvDapAns.FindAsync(id);
            if (da == null)
            {
                Console.WriteLine("❌ Không tìm thấy đáp án");
                return NotFound();
            }

            da.LqvDung = !da.LqvDung;
            await _context.SaveChangesAsync();

            Console.WriteLine($"✅ Toggle OK – LqvDung = {da.LqvDung}");

            return Ok(new { success = true });
        }

    }

}
