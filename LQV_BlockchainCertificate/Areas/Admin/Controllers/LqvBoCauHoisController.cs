using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LQV_BlockchainCertificate.Models.DBModel;
using System;

namespace LQV_BlockchainCertificate.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LqvBoCauHoisController : Controller
    {
        private readonly LqvDbContext _context;

        public LqvBoCauHoisController(LqvDbContext context)
        {
            _context = context;
            Console.WriteLine(">>> LqvBoCauHoisController INIT");
        }

        // ===============================
        // LIST
        // ===============================
        public async Task<IActionResult> Index()
        {
            var data = await _context.LqvBoCauHois
                .Include(x => x.LqvGiangVien)
                .OrderByDescending(x => x.LqvNgayTao)
                .ToListAsync();

            Console.WriteLine($"SỐ BỘ CÂU HỎI: {data.Count}");
            return View(data);
        }

        // ===============================
        // CREATE - GET
        // ===============================
        public IActionResult Create()
        {
            Console.WriteLine(">>> CREATE GET");
            LoadGiangVienDropDown();
            return View();
        }

        // ===============================
        // CREATE - POST
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LqvBoCauHoi model)
        {
            Console.WriteLine("===== CREATE POST =====");
            Console.WriteLine($"TenBo = {model.LqvTenBo}");
            Console.WriteLine($"GiangVienId = {model.LqvGiangVienId}");

            if (model.LqvGiangVienId <= 0)
            {
                ModelState.AddModelError("LqvGiangVienId", "Chưa chọn giảng viên");
            }

            Console.WriteLine($"ModelState.Valid = {ModelState.IsValid}");

            foreach (var item in ModelState)
            {
                foreach (var err in item.Value.Errors)
                {
                    Console.WriteLine($"❌ {item.Key}: {err.ErrorMessage}");
                }
            }

            if (!ModelState.IsValid)
            {
                LoadGiangVienDropDown(model.LqvGiangVienId);
                return View(model);
            }

            model.LqvNgayTao = DateTime.Now;

            _context.LqvBoCauHois.Add(model);

            Console.WriteLine(">>> BEFORE SAVE");
            var result = await _context.SaveChangesAsync();

            Console.WriteLine($">>> SAVE RESULT = {result}");
            Console.WriteLine($">>> NEW BO ID = {model.LqvBoCauHoiId}");

            return RedirectToAction(nameof(Index));
        }


        // ===============================
        // EDIT - GET
        // ===============================
        public async Task<IActionResult> Edit(int id)
        {
            var bo = await _context.LqvBoCauHois.FindAsync(id);
            if (bo == null) return NotFound();

            LoadGiangVienDropDown(bo.LqvGiangVienId);
            return View(bo);
        }

        // ===============================
        // EDIT - POST
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LqvBoCauHoi model)
        {
            if (id != model.LqvBoCauHoiId)
                return BadRequest();

            if (model.LqvGiangVienId <= 0)
            {
                ModelState.AddModelError("LqvGiangVienId", "Chưa chọn giảng viên");
            }

            if (!ModelState.IsValid)
            {
                LoadGiangVienDropDown(model.LqvGiangVienId);
                return View(model);
            }

            _context.Update(model);
            await _context.SaveChangesAsync();

            Console.WriteLine("✅ EDIT SUCCESS");
            return RedirectToAction(nameof(Index));
        }

        // ===============================
        // DELETE
        // ===============================
        public async Task<IActionResult> Delete(int id)
        {
            var bo = await _context.LqvBoCauHois
                .Include(x => x.LqvGiangVien)
                .FirstOrDefaultAsync(x => x.LqvBoCauHoiId == id);

            if (bo == null) return NotFound();
            return View(bo);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bo = await _context.LqvBoCauHois.FindAsync(id);
            if (bo == null) return NotFound();

            _context.LqvBoCauHois.Remove(bo);
            await _context.SaveChangesAsync();

            Console.WriteLine("✅ DELETE SUCCESS");
            return RedirectToAction(nameof(Index));
        }
        // ===============================
        // DANH SÁCH CÂU HỎI THEO BỘ
        // ===============================
        public async Task<IActionResult> Questions(int id)
        {
            Console.WriteLine($">>> LOAD QUESTIONS OF BO ID = {id}");

            var boCauHoi = await _context.LqvBoCauHois
                .Include(x => x.LqvCauHois)
                .FirstOrDefaultAsync(x => x.LqvBoCauHoiId == id);

            if (boCauHoi == null)
                return NotFound();

            ViewBag.TenBo = boCauHoi.LqvTenBo;
            ViewBag.BoId = boCauHoi.LqvBoCauHoiId;

            return View(boCauHoi.LqvCauHois.ToList());
        }

        // ===============================
        // LOAD GIẢNG VIÊN (ROLE = 2)
        // ===============================
        private void LoadGiangVienDropDown(int? selectedId = null)
        {
            Console.WriteLine(">>> LOAD GIẢNG VIÊN");

            ViewBag.LqvGiangVienId = _context.LqvNguoiDungs
                .Where(x => x.LqvRoleId == 2) // GIẢNG VIÊN
                .Select(x => new SelectListItem
                {
                    Value = x.LqvId.ToString(),
                    Text = x.LqvHoTen,
                    Selected = selectedId.HasValue && x.LqvId == selectedId
                })
                .ToList();

            Console.WriteLine($"SỐ GIẢNG VIÊN: {ViewBag.LqvGiangVienId.Count}");
        }

    }
}
