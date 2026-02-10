using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LQV_BlockchainCertificate.Models.DBModel;

namespace LQV_BlockchainCertificate.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LqvDeThisController : Controller
    {
        private readonly LqvDbContext _context;

        public LqvDeThisController(LqvDbContext context)
        {
            _context = context;
        }

        // ================== INDEX ==================
        public async Task<IActionResult> Index()
        {
            var data = await _context.LqvDeThis
                .Include(d => d.LqvBoCauHoi)
                .Include(d => d.LqvGiangVien)
                .ToListAsync();

            return View(data);
        }

        // ================== DETAILS ==================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var deThi = await _context.LqvDeThis
                .Include(d => d.LqvBoCauHoi)
                .ThenInclude(b => b.LqvCauHois)
                .FirstOrDefaultAsync(m => m.LqvDeThiId == id);

            if (deThi == null) return NotFound();

            return View(deThi);
        }

      
        // ================== CREATE (GET) ==================
        public IActionResult Create()
        {
            Console.WriteLine(">>> [DeThi] CREATE GET");

            var boCauHois = _context.LqvBoCauHois.ToList();
            Console.WriteLine($">>> SỐ BỘ CÂU HỎI: {boCauHois.Count}");

            if (!boCauHois.Any())
            {
                Console.WriteLine("!!! CẢNH BÁO: KHÔNG CÓ BỘ CÂU HỎI NÀO TRONG DB");
            }

            ViewBag.LqvBoCauHoiId = new SelectList(
                boCauHois,
                "LqvBoCauHoiId",
                "LqvTenBo"
            );

            return View();
        }

        // ================== CREATE (POST) ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LqvDeThi model)
        {
            Console.WriteLine(">>> [DeThi] CREATE POST");
            Console.WriteLine($">>> TenDeThi     = {model.LqvTenDeThi}");
            Console.WriteLine($">>> BoCauHoiId   = {model.LqvBoCauHoiId}");
            Console.WriteLine($">>> ThoiGianThi  = {model.LqvThoiGianThi}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("!!! MODELSTATE INVALID");

                foreach (var error in ModelState)
                {
                    foreach (var err in error.Value.Errors)
                    {
                        Console.WriteLine($"❌ FIELD: {error.Key} | ERROR: {err.ErrorMessage}");
                    }
                }

                ViewBag.LqvBoCauHoiId = new SelectList(
                    _context.LqvBoCauHois,
                    "LqvBoCauHoiId",
                    "LqvTenBo",
                    model.LqvBoCauHoiId
                );

                return View(model);
            }

            Console.WriteLine(">>> MODEL VALID – BẮT ĐẦU TÍNH TỔNG ĐIỂM");

            // 🔥 TÍNH TỔNG ĐIỂM
            try
            {
                model.LqvTongDiem = await TinhTongDiemBoCauHoi(model.LqvBoCauHoiId);
                Console.WriteLine($">>> TỔNG ĐIỂM = {model.LqvTongDiem}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("!!! LỖI KHI TÍNH TỔNG ĐIỂM");
                Console.WriteLine(ex.Message);
                throw;
            }

            _context.Add(model);
            await _context.SaveChangesAsync();

            Console.WriteLine(">>> LƯU ĐỀ THI THÀNH CÔNG");

            return RedirectToAction(nameof(Index));
        }

        // ================== EDIT ==================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var deThi = await _context.LqvDeThis
                .Include(d => d.LqvBoCauHoi)
                .FirstOrDefaultAsync(d => d.LqvDeThiId == id);

            if (deThi == null)
                return NotFound();

            // 🚫 KHÓA SỬA NẾU ĐÃ DUYỆT
            if (deThi.LqvDaDuyet)
            {
                TempData["Error"] = "❌ Đề thi đã được duyệt, không thể chỉnh sửa!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.LqvBoCauHoiId = new SelectList(
                _context.LqvBoCauHois,
                "LqvBoCauHoiId",
                "LqvTenBo",
                deThi.LqvBoCauHoiId
            );

            return View(deThi);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LqvDeThi model)
        {
            if (id != model.LqvDeThiId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.LqvBoCauHoiId = new SelectList(
                    _context.LqvBoCauHois,
                    "LqvBoCauHoiId",
                    "LqvTenBo",
                    model.LqvBoCauHoiId
                );
                return View(model);
            }

            // 🔥 LẤY ĐỀ THI GỐC TỪ DB
            var deThi = await _context.LqvDeThis
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.LqvDeThiId == id);

            if (deThi == null)
                return NotFound();

            try
            {
                // 🔥 TÍNH LẠI TỔNG ĐIỂM
                var tongDiem = await TinhTongDiemBoCauHoi(model.LqvBoCauHoiId);

                // 👉 GÁN TỪNG FIELD ĐƯỢC PHÉP SỬA
                deThi.LqvTenDeThi = model.LqvTenDeThi;
                deThi.LqvBoCauHoiId = model.LqvBoCauHoiId;
                deThi.LqvThoiGianThi = model.LqvThoiGianThi;
                deThi.LqvTongDiem = tongDiem;

                // 👉 LOGIC DUYỆT (OPTION)
                // Nếu đổi bộ câu hỏi → bắt duyệt lại
                if (deThi.LqvBoCauHoiId != model.LqvBoCauHoiId)
                {
                    deThi.LqvDaDuyet = false;
                    deThi.LqvNgayDuyet = null;
                }

                _context.Update(deThi);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LqvDeThiExists(model.LqvDeThiId))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }


        // ================== DELETE ==================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var deThi = await _context.LqvDeThis
                .Include(d => d.LqvBoCauHoi)
                .FirstOrDefaultAsync(m => m.LqvDeThiId == id);

            if (deThi == null) return NotFound();

            if (deThi.LqvDaDuyet)
            {
                TempData["Error"] = "❌ Đề thi đã được duyệt, không thể xoá!";
                return RedirectToAction(nameof(Index));
            }

            return View(deThi);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deThi = await _context.LqvDeThis.FindAsync(id);
            if (deThi == null)
                return NotFound();

            if (deThi.LqvDaDuyet)
            {
                TempData["Error"] = "❌ Đề thi đã được duyệt, không thể xoá!";
                return RedirectToAction(nameof(Index));
            }

            _context.LqvDeThis.Remove(deThi);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duyet(int id)
        {
            var deThi = await _context.LqvDeThis.FindAsync(id);
            if (deThi == null) return NotFound();

            if (deThi.LqvGiangVienId == null)
            {
                TempData["Error"] = "❌ Chưa gán giảng viên cho đề thi!";
                return RedirectToAction(nameof(Index));
            }

            deThi.LqvDaDuyet = true;
            deThi.LqvNgayDuyet = DateTime.Now;

            _context.Update(deThi);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ================== HÀM TÍNH ĐIỂM ==================
        private async Task<double> TinhTongDiemBoCauHoi(int boCauHoiId)
        {
            return await _context.LqvCauHois
                .Where(c => c.LqvBoCauHoiId == boCauHoiId)
                .SumAsync(c => c.LqvDiem);
        }

        private bool LqvDeThiExists(int id)
        {
            return _context.LqvDeThis.Any(e => e.LqvDeThiId == id);
        }
    }
}
