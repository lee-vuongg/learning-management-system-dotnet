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
    public class LqvYeuCauHoTroesController : Controller
    {
        private readonly LqvDbContext _context;

        public LqvYeuCauHoTroesController(LqvDbContext context)
        {
            _context = context;
        }

        // GET: Admin/LqvYeuCauHoTroes
        public async Task<IActionResult> Index()
        {
            Console.WriteLine("LOG: [Index GET] Admin truy cập danh sách Yêu cầu Hỗ trợ.");
            // Tối ưu: Sắp xếp theo trạng thái "Chờ xử lý" lên đầu và sau đó theo thời gian gửi
            var lqvDbContext = _context.LqvYeuCauHoTros
                .Include(l => l.LqvNguoiDung)
                .OrderBy(l => l.LqvTrangThai == "Chờ xử lý" ? 0 : 1)
                .ThenByDescending(l => l.LqvThoiGianGui);

            return View(await lqvDbContext.ToListAsync());
        }

        // GET: Admin/LqvYeuCauHoTroes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            Console.WriteLine($"LOG: [Details GET] Bắt đầu xử lý yêu cầu ID: {id}");
            if (id == null)
            {
                Console.WriteLine("ERROR: [Details GET] ID yêu cầu null.");
                return NotFound();
            }

            var lqvYeuCauHoTro = await _context.LqvYeuCauHoTros
                .Include(l => l.LqvNguoiDung)
                .FirstOrDefaultAsync(m => m.LqvId == id);

            if (lqvYeuCauHoTro == null)
            {
                Console.WriteLine($"ERROR: [Details GET] Không tìm thấy yêu cầu ID: {id}");
                return NotFound();
            }

            // Tối ưu: Tự động chuyển trạng thái sang "Đang xử lý" nếu Admin xem chi tiết yêu cầu "Chờ xử lý"
            if (lqvYeuCauHoTro.LqvTrangThai == "Chờ xử lý")
            {
                Console.WriteLine($"LOG: [Details GET] Yêu cầu ID: {id} chuyển từ 'Chờ xử lý' sang 'Đang xử lý'.");
                lqvYeuCauHoTro.LqvTrangThai = "Đang xử lý";
                try
                {
                    await _context.SaveChangesAsync();
                    TempData["InfoMessage"] = $"Yêu cầu hỗ trợ #{id} đã được chuyển sang trạng thái **Đang xử lý**.";
                    Console.WriteLine($"SUCCESS: [Details GET] Cập nhật trạng thái thành công cho ID: {id}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"FATAL ERROR: [Details GET] Lỗi khi cập nhật trạng thái ID: {id}. Message: {ex.Message}");
                    TempData["WarningMessage"] = "Lỗi khi cập nhật trạng thái tự động.";
                }
            }

            Console.WriteLine($"LOG: [Details GET] Hiển thị View chi tiết cho ID: {id}");
            return View(lqvYeuCauHoTro);
        }

        // GET: Admin/LqvYeuCauHoTroes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            Console.WriteLine($"LOG: [Edit GET] Bắt đầu xử lý yêu cầu chỉnh sửa/phản hồi ID: {id}");
            if (id == null)
            {
                Console.WriteLine("ERROR: [Edit GET] ID yêu cầu null.");
                return NotFound();
            }

            // Đã sửa lỗi NullReferenceException bằng cách Include LqvNguoiDung
            var lqvYeuCauHoTro = await _context.LqvYeuCauHoTros
                .Include(l => l.LqvNguoiDung)
                .FirstOrDefaultAsync(m => m.LqvId == id);

            if (lqvYeuCauHoTro == null)
            {
                Console.WriteLine($"ERROR: [Edit GET] Không tìm thấy yêu cầu ID: {id}");
                return NotFound();
            }

            ViewData["LqvNguoiDungId"] = new SelectList(_context.LqvNguoiDungs, "LqvId", "LqvHoTen", lqvYeuCauHoTro.LqvNguoiDungId);
            Console.WriteLine($"LOG: [Edit GET] Hiển thị View Edit cho ID: {id}");
            return View(lqvYeuCauHoTro);
        }

        // POST: Admin/LqvYeuCauHoTroes/Edit/5
        // Trong Areas/Admin/Controllers/LqvYeuCauHoTroesController.cs

        // POST: Admin/LqvYeuCauHoTroes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LqvId,LqvNguoiDungId,LqvNoiDung,LqvThoiGianGui,LqvTrangThai,LqvPhanHoi")] LqvYeuCauHoTro lqvYeuCauHoTro)
        {
            Console.WriteLine($"LOG: [Edit POST] Bắt đầu xử lý cập nhật/phản hồi cho ID: {id}");

            // ********** ĐÃ BỔ SUNG **********
            // Loại bỏ kiểm tra ModelState cho thuộc tính điều hướng LqvNguoiDung (giải quyết lỗi hiện tại)
            ModelState.Remove("LqvNguoiDung");
            // ********************************

            if (id != lqvYeuCauHoTro.LqvId)
            {
                Console.WriteLine($"ERROR: [Edit POST] ID trong URL ({id}) không khớp với ID trong Model ({lqvYeuCauHoTro.LqvId}).");
                TempData["ErrorMessage"] = "ID yêu cầu không khớp.";
                return NotFound();
            }

            // Lấy lại yêu cầu gốc để bảo toàn các trường không được gửi từ form (AsNoTracking)
            var existingRequest = await _context.LqvYeuCauHoTros.AsNoTracking().FirstOrDefaultAsync(r => r.LqvId == id);

            if (existingRequest == null)
            {
                Console.WriteLine($"ERROR: [Edit POST] Không tìm thấy yêu cầu gốc ID: {id}");
                TempData["ErrorMessage"] = "Yêu cầu gốc không tồn tại.";
                return NotFound();
            }

            // Bảo toàn các trường không thay đổi
            lqvYeuCauHoTro.LqvThoiGianGui = existingRequest.LqvThoiGianGui;
            lqvYeuCauHoTro.LqvNguoiDungId = existingRequest.LqvNguoiDungId;
            lqvYeuCauHoTro.LqvNoiDung = existingRequest.LqvNoiDung;

            Console.WriteLine($"LOG: [Edit POST] Trạng thái mới: {lqvYeuCauHoTro.LqvTrangThai}, Phản hồi: {lqvYeuCauHoTro.LqvPhanHoi?.Length} ký tự.");

            if (ModelState.IsValid)
            {
                Console.WriteLine($"LOG: [Edit POST] ModelState hợp lệ. Tiến hành cập nhật DB.");
                try
                {
                    _context.Update(lqvYeuCauHoTro);
                    await _context.SaveChangesAsync();

                    Console.WriteLine($"SUCCESS: [Edit POST] Cập nhật DB thành công cho ID: {id}");
                    TempData["SuccessMessage"] = $"Yêu cầu hỗ trợ #{id} đã được cập nhật thành công (Trạng thái: **{lqvYeuCauHoTro.LqvTrangThai}**).";
                }
                catch (DbUpdateConcurrencyException dbce)
                {
                    Console.WriteLine($"FATAL ERROR: [Edit POST] Lỗi DbUpdateConcurrencyException cho ID: {id}. Message: {dbce.Message}");
                    if (!LqvYeuCauHoTroExists(lqvYeuCauHoTro.LqvId))
                    {
                        TempData["ErrorMessage"] = "Yêu cầu không tồn tại.";
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"FATAL ERROR: [Edit POST] Lỗi chung khi cập nhật DB ID: {id}. Message: {ex.Message}");
                    TempData["ErrorMessage"] = "Lỗi khi cập nhật yêu cầu: " + ex.Message;
                }
                return RedirectToAction(nameof(Index));
            }

            Console.WriteLine($"ERROR: [Edit POST] ModelState không hợp lệ. Số lỗi: {ModelState.ErrorCount}");

            // Log chi tiết lỗi ModelState
            foreach (var modelStateKey in ModelState.Keys)
            {
                var modelStateVal = ModelState[modelStateKey];
                if (modelStateVal.Errors.Any())
                {
                    foreach (var error in modelStateVal.Errors)
                    {
                        Console.WriteLine($"DETAIL ERROR: Trường '{modelStateKey}' có lỗi: {error.ErrorMessage}");
                    }
                }
            }

            TempData["ErrorMessage"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";
            ViewData["LqvNguoiDungId"] = new SelectList(_context.LqvNguoiDungs, "LqvId", "LqvHoTen", lqvYeuCauHoTro.LqvNguoiDungId);
            return View(lqvYeuCauHoTro);
        }

        // GET: Admin/LqvYeuCauHoTroes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            Console.WriteLine($"LOG: [Delete GET] Bắt đầu xử lý yêu cầu xóa ID: {id}");
            if (id == null)
            {
                Console.WriteLine("ERROR: [Delete GET] ID yêu cầu null.");
                return NotFound();
            }

            var lqvYeuCauHoTro = await _context.LqvYeuCauHoTros
                .Include(l => l.LqvNguoiDung)
                .FirstOrDefaultAsync(m => m.LqvId == id);

            if (lqvYeuCauHoTro == null)
            {
                Console.WriteLine($"ERROR: [Delete GET] Không tìm thấy yêu cầu ID: {id}");
                return NotFound();
            }

            Console.WriteLine($"LOG: [Delete GET] Hiển thị View Delete cho ID: {id}");
            return View(lqvYeuCauHoTro);
        }

        // POST: Admin/LqvYeuCauHoTroes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Console.WriteLine($"LOG: [Delete POST] Bắt đầu xử lý xóa xác nhận ID: {id}");

            var lqvYeuCauHoTro = await _context.LqvYeuCauHoTros.FindAsync(id);

            if (lqvYeuCauHoTro != null)
            {
                _context.LqvYeuCauHoTros.Remove(lqvYeuCauHoTro);
                try
                {
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"SUCCESS: [Delete POST] Xóa DB thành công ID: {id}");
                    TempData["SuccessMessage"] = $"Yêu cầu hỗ trợ #{id} đã được xóa thành công.";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"FATAL ERROR: [Delete POST] Lỗi khi xóa khỏi DB ID: {id}. Message: {ex.Message}");
                    TempData["ErrorMessage"] = "Lỗi khi xóa yêu cầu: " + ex.Message;
                }
            }
            else
            {
                Console.WriteLine($"WARNING: [Delete POST] Yêu cầu ID: {id} không tìm thấy để xóa.");
                TempData["WarningMessage"] = $"Yêu cầu hỗ trợ #{id} không tìm thấy để xóa.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool LqvYeuCauHoTroExists(int id)
        {
            return _context.LqvYeuCauHoTros.Any(e => e.LqvId == id);
        }
    }
}