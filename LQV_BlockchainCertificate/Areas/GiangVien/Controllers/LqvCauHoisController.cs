using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LQV_BlockchainCertificate.Models.DBModel;

namespace LQV_BlockchainCertificate.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    [Authorize(Roles = "GiangVien")]
    public class LqvCauHoisController : Controller
    {
        private readonly LqvDbContext _context;

        public LqvCauHoisController(LqvDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // DANH SÁCH CÂU HỎI THEO BỘ
        // =====================================================
        public async Task<IActionResult> Index(int boId)
        {
            var bo = await _context.LqvBoCauHois
                .FirstOrDefaultAsync(x => x.LqvBoCauHoiId == boId);

            if (bo == null) return NotFound();

            ViewBag.BoId = boId;
            ViewBag.TenBo = bo.LqvTenBo;

            var list = await _context.LqvCauHois
                .Where(x => x.LqvBoCauHoiId == boId)
                .Include(x => x.LqvDapAns)
                .OrderBy(x => x.LqvCauHoiId)
                .ToListAsync();

            return View(list);
        }

        // =====================================================
        // CREATE - GET
        // =====================================================
        public IActionResult Create(int boId)
        {
            LoadBoCauHoiDropDown(boId);

            return View(new LqvCauHoi
            {
                LqvBoCauHoiId = boId,
                LqvLoai = "TracNghiem",
                LqvDiem = 1
            });
        }

        // =====================================================
        // CREATE - POST
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LqvCauHoi model)
        {
            if (model.LqvBoCauHoiId <= 0)
                ModelState.AddModelError(nameof(model.LqvBoCauHoiId), "Chưa chọn bộ câu hỏi");

            if (string.IsNullOrWhiteSpace(model.LqvNoiDung))
                ModelState.AddModelError(nameof(model.LqvNoiDung), "Nội dung không được rỗng");

            if (!ModelState.IsValid)
            {
                LoadBoCauHoiDropDown(model.LqvBoCauHoiId);
                return View(model);
            }

            _context.LqvCauHois.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { boId = model.LqvBoCauHoiId });
        }

        // =====================================================
        // EDIT - GET
        // =====================================================
        public async Task<IActionResult> Edit(int id)
        {
            var cauHoi = await _context.LqvCauHois
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.LqvCauHoiId == id);

            if (cauHoi == null) return NotFound();

            LoadBoCauHoiDropDown(cauHoi.LqvBoCauHoiId);
            return View(cauHoi);
        }

        // =====================================================
        // EDIT - POST
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LqvCauHoi model)
        {
            if (id != model.LqvCauHoiId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                LoadBoCauHoiDropDown(model.LqvBoCauHoiId);
                return View(model);
            }

            var entity = await _context.LqvCauHois
                .FirstOrDefaultAsync(x => x.LqvCauHoiId == id);

            if (entity == null) return NotFound();

            entity.LqvNoiDung = model.LqvNoiDung;
            entity.LqvLoai = model.LqvLoai;
            entity.LqvDiem = model.LqvDiem;
            entity.LqvBoCauHoiId = model.LqvBoCauHoiId;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { boId = entity.LqvBoCauHoiId });
        }

        // =====================================================
        // DELETE
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var cauHoi = await _context.LqvCauHois
                .Include(x => x.LqvDapAns)
                .FirstOrDefaultAsync(x => x.LqvCauHoiId == id);

            if (cauHoi == null) return NotFound();

            int boId = cauHoi.LqvBoCauHoiId;

            _context.LqvDapAns.RemoveRange(cauHoi.LqvDapAns);
            _context.LqvCauHois.Remove(cauHoi);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { boId });
        }

        // =====================================================
        // LOAD DROPDOWN BỘ CÂU HỎI
        // =====================================================
        private void LoadBoCauHoiDropDown(int? selectedId = null)
        {
            ViewBag.BoCauHois = _context.LqvBoCauHois
                .OrderBy(x => x.LqvTenBo)
                .Select(x => new SelectListItem
                {
                    Value = x.LqvBoCauHoiId.ToString(),
                    Text = x.LqvTenBo,
                    Selected = selectedId == x.LqvBoCauHoiId
                })
                .ToList();
        }
    }
}
