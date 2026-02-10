using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LQV_BlockchainCertificate.Models.DBModel;
using System.Security.Claims;


namespace LQV_BlockchainCertificate.Areas.Student.Controllers
{
    [Area("Student")]
    public class LqvYeuCauHoTroesController : Controller
    {
        private readonly LqvDbContext _context;

        public LqvYeuCauHoTroesController(LqvDbContext context)
        {
            _context = context;
        }

        // Helper function (Giả định, bạn cần thay thế bằng logic xác thực thực tế)
        private int GetLoggedInStudentId()
        {
            // Lấy ID người dùng (user ID) từ Claims
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdClaim != null && int.TryParse(userIdClaim, out int studentId))
            {
                return studentId;
            }

            // Nếu không tìm thấy ID (người dùng chưa đăng nhập), ném ra lỗi.
            throw new UnauthorizedAccessException("Người dùng chưa được xác thực hoặc ID không hợp lệ.");
        }

        // GET: Student/LqvYeuCauHoTroes
        public async Task<IActionResult> Index()
        {
            // Trong môi trường thực tế, lỗi ở đây sẽ được xử lý bởi middleware (ví dụ: redirect to login)
            try
            {
                int studentId = GetLoggedInStudentId();
                Console.WriteLine($"LOG: [Index] Sinh viên ID: {studentId} truy cập danh sách yêu cầu.");

                var lqvDbContext = _context.LqvYeuCauHoTros
                    .Where(l => l.LqvNguoiDungId == studentId) // Chỉ lấy yêu cầu của sinh viên hiện tại
                    .Include(l => l.LqvNguoiDung)
                    .OrderByDescending(l => l.LqvThoiGianGui); // Sắp xếp yêu cầu mới nhất lên đầu

                return View(await lqvDbContext.ToListAsync());
            }
            catch (UnauthorizedAccessException)
            {
                // Giả định chuyển hướng đến trang đăng nhập nếu xác thực thất bại
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
        }

        // GET: Student/LqvYeuCauHoTroes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                Console.WriteLine("ERROR: [Details] ID yêu cầu null.");
                return NotFound();
            }

            int studentId = GetLoggedInStudentId();

            var lqvYeuCauHoTro = await _context.LqvYeuCauHoTros
                .Include(l => l.LqvNguoiDung)
                .FirstOrDefaultAsync(m => m.LqvId == id && m.LqvNguoiDungId == studentId); // Kiểm tra quyền sở hữu

            if (lqvYeuCauHoTro == null)
            {
                Console.WriteLine($"ERROR: [Details] Không tìm thấy YCHT ID: {id} hoặc không có quyền truy cập.");
                TempData["ErrorMessage"] = "Yêu cầu không tồn tại hoặc bạn không có quyền truy cập.";
                return NotFound();
            }

            Console.WriteLine($"LOG: [Details] Xem chi tiết YCHT ID: {id}.");
            return View(lqvYeuCauHoTro);
        }

        // GET: Student/LqvYeuCauHoTroes/Create
        public IActionResult Create()
        {
            Console.WriteLine("LOG: [Create GET] Hiển thị form tạo yêu cầu mới.");
            return View();
        }

        // POST: Student/LqvYeuCauHoTroes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LqvNoiDung")] LqvYeuCauHoTro lqvYeuCauHoTro)
        {
            Console.WriteLine("LOG: [Create POST] Đang xử lý tạo yêu cầu...");

            // ********** CẬP NHẬT TRỌNG TÂM **********
            // 1. Loại bỏ các trường sẽ được Controller tự động gán hoặc là Navigation Property
            ModelState.Remove("LqvNguoiDungId");
            ModelState.Remove("LqvNguoiDung"); // <-- ĐÃ THÊM: Loại bỏ thuộc tính điều hướng gây ra lỗi
            ModelState.Remove("LqvThoiGianGui");
            ModelState.Remove("LqvTrangThai");
            ModelState.Remove("LqvPhanHoi");
            // ****************************************

            // 2. Kiểm tra Nội dung
            if (string.IsNullOrWhiteSpace(lqvYeuCauHoTro.LqvNoiDung))
            {
                Console.WriteLine("ERROR: [Create POST] Nội dung yêu cầu bị trống.");
                ModelState.AddModelError("LqvNoiDung", "Nội dung yêu cầu không được để trống.");
            }

            if (ModelState.IsValid)
            {
                Console.WriteLine("LOG: [Create POST] ModelState hợp lệ. Đang gán dữ liệu mặc định.");

                // 3. Gán các giá trị cần thiết
                try
                {
                    lqvYeuCauHoTro.LqvNguoiDungId = GetLoggedInStudentId();
                    lqvYeuCauHoTro.LqvThoiGianGui = DateTime.Now;
                    lqvYeuCauHoTro.LqvTrangThai = "Chờ xử lý";
                    lqvYeuCauHoTro.LqvPhanHoi = null;

                    Console.WriteLine($"LOG: [Create POST] Dữ liệu chuẩn bị thêm: ND ID={lqvYeuCauHoTro.LqvNguoiDungId}, ND={lqvYeuCauHoTro.LqvNoiDung}");

                    _context.Add(lqvYeuCauHoTro);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"SUCCESS: [Create POST] Lưu DB thành công. ID yêu cầu mới: {lqvYeuCauHoTro.LqvId}");
                    TempData["SuccessMessage"] = "Yêu cầu hỗ trợ đã được gửi thành công. Chúng tôi sẽ phản hồi sớm nhất.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("FATAL ERROR: [Create POST] Lỗi khi lưu vào DB!");
                    Console.WriteLine($"EXCEPTION TYPE: {ex.GetType().Name}");
                    Console.WriteLine($"EXCEPTION MESSAGE: {ex.Message}");
                    Console.WriteLine($"INNER EXCEPTION: {ex.InnerException?.Message}");

                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi gửi yêu cầu: " + ex.Message;
                }
            }
            else
            {
                Console.WriteLine($"ERROR: [Create POST] ModelState không hợp lệ. Số lỗi: {ModelState.ErrorCount}");

                // In chi tiết lỗi (Giữ lại để tìm lỗi nếu có trường Required khác)
                foreach (var modelStateKey in ModelState.Keys)
                {
                    var modelStateVal = ModelState[modelStateKey];
                    if (modelStateVal.Errors.Any())
                    {
                        foreach (var error in modelStateVal.Errors)
                        {
                            Console.WriteLine($"DETAIL ERROR: Trường '{modelStateKey}' có lỗi: {error.ErrorMessage} | KeyState: {modelStateVal.ValidationState}");
                        }
                    }
                }
            }
            Console.WriteLine("LOG: [Create POST] Quay lại View Create.");
            return View(lqvYeuCauHoTro);
        }

        // GET: Student/LqvYeuCauHoTroes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                Console.WriteLine("ERROR: [Details] ID yêu cầu null.");
                return NotFound();
            }

            int studentId = GetLoggedInStudentId();
            var lqvYeuCauHoTro = await _context.LqvYeuCauHoTros.FindAsync(id);

            if (lqvYeuCauHoTro == null || lqvYeuCauHoTro.LqvNguoiDungId != studentId)
            {
                Console.WriteLine($"ERROR: [Edit GET] Không tìm thấy YCHT ID: {id} hoặc không có quyền.");
                TempData["ErrorMessage"] = "Yêu cầu không tồn tại hoặc bạn không có quyền chỉnh sửa.";
                return NotFound();
            }

            // CHỈ CHO PHÉP SỬA KHI TRẠNG THÁI LÀ "Chờ xử lý"
            if (lqvYeuCauHoTro.LqvTrangThai != "Chờ xử lý")
            {
                Console.WriteLine($"WARNING: [Edit GET] YCHT ID: {id} đang ở trạng thái '{lqvYeuCauHoTro.LqvTrangThai}', không thể sửa.");
                TempData["WarningMessage"] = "Yêu cầu này đang được xử lý hoặc đã hoàn thành, không thể chỉnh sửa.";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            Console.WriteLine($"LOG: [Edit GET] Hiển thị form Edit YCHT ID: {id}.");
            return View(lqvYeuCauHoTro);
        }

        // POST: Student/LqvYeuCauHoTroes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LqvId,LqvNoiDung")] LqvYeuCauHoTro lqvYeuCauHoTroUpdate)
        {
            Console.WriteLine($"LOG: [Edit POST] Đang xử lý cập nhật YCHT ID: {id}");

            if (id != lqvYeuCauHoTroUpdate.LqvId)
            {
                Console.WriteLine("ERROR: [Edit POST] ID trong URL khác với ID trong model.");
                return NotFound();
            }

            int studentId = GetLoggedInStudentId();

            // Lấy yêu cầu gốc từ DB
            var lqvYeuCauHoTro = await _context.LqvYeuCauHoTros
                .FirstOrDefaultAsync(m => m.LqvId == id && m.LqvNguoiDungId == studentId);

            if (lqvYeuCauHoTro == null)
            {
                Console.WriteLine($"ERROR: [Edit POST] Không tìm thấy YCHT ID: {id} hoặc không có quyền chỉnh sửa.");
                TempData["ErrorMessage"] = "Yêu cầu không tồn tại hoặc bạn không có quyền chỉnh sửa.";
                return NotFound();
            }

            if (lqvYeuCauHoTro.LqvTrangThai != "Chờ xử lý")
            {
                Console.WriteLine($"WARNING: [Edit POST] YCHT ID: {id} đã chuyển trạng thái, không thể chỉnh sửa.");
                TempData["ErrorMessage"] = "Yêu cầu đã được chuyển trạng thái, không thể chỉnh sửa nội dung.";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            // Chỉ cập nhật Nội dung (LqvNoiDung)
            if (string.IsNullOrWhiteSpace(lqvYeuCauHoTroUpdate.LqvNoiDung))
            {
                Console.WriteLine("ERROR: [Edit POST] Nội dung yêu cầu bị trống.");
                ModelState.AddModelError("LqvNoiDung", "Nội dung yêu cầu không được để trống.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Cập nhật nội dung mới
                    lqvYeuCauHoTro.LqvNoiDung = lqvYeuCauHoTroUpdate.LqvNoiDung;
                    Console.WriteLine($"LOG: [Edit POST] Nội dung mới: {lqvYeuCauHoTro.LqvNoiDung}");

                    _context.Update(lqvYeuCauHoTro);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"SUCCESS: [Edit POST] Cập nhật DB thành công YCHT ID: {id}");
                    TempData["SuccessMessage"] = $"Yêu cầu hỗ trợ #{id} đã được cập nhật thành công.";
                }
                catch (DbUpdateConcurrencyException dbce)
                {
                    Console.WriteLine($"ERROR: [Edit POST] Lỗi DbUpdateConcurrencyException YCHT ID: {id}");
                    Console.WriteLine($"EXCEPTION MESSAGE: {dbce.Message}");
                    if (!LqvYeuCauHoTroExists(lqvYeuCauHoTro.LqvId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            // Nếu ModelState không hợp lệ, hiển thị lại View với dữ liệu gốc (trừ nội dung mới)
            Console.WriteLine("LOG: [Edit POST] Quay lại View Edit do ModelState không hợp lệ.");
            return View(lqvYeuCauHoTro);
        }

        // GET: Student/LqvYeuCauHoTroes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                Console.WriteLine("ERROR: [Delete GET] ID yêu cầu null.");
                return NotFound();
            }

            int studentId = GetLoggedInStudentId();

            var lqvYeuCauHoTro = await _context.LqvYeuCauHoTros
                .Include(l => l.LqvNguoiDung)
                .FirstOrDefaultAsync(m => m.LqvId == id && m.LqvNguoiDungId == studentId); // Kiểm tra quyền sở hữu

            if (lqvYeuCauHoTro == null)
            {
                Console.WriteLine($"ERROR: [Delete GET] Không tìm thấy YCHT ID: {id} hoặc không có quyền.");
                TempData["ErrorMessage"] = "Yêu cầu không tồn tại hoặc bạn không có quyền xóa.";
                return NotFound();
            }

            // CHỈ CHO PHÉP XÓA KHI TRẠNG THÁI LÀ "Chờ xử lý"
            if (lqvYeuCauHoTro.LqvTrangThai != "Chờ xử lý")
            {
                Console.WriteLine($"WARNING: [Delete GET] YCHT ID: {id} đang ở trạng thái '{lqvYeuCauHoTro.LqvTrangThai}', không thể xóa.");
                TempData["WarningMessage"] = "Yêu cầu này đang được xử lý hoặc đã hoàn thành, không thể xóa.";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            Console.WriteLine($"LOG: [Delete GET] Hiển thị form Delete YCHT ID: {id}.");
            return View(lqvYeuCauHoTro);
        }

        // POST: Student/LqvYeuCauHoTroes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Console.WriteLine($"LOG: [Delete POST] Đang xử lý xóa YCHT ID: {id}");
            int studentId = GetLoggedInStudentId();

            var lqvYeuCauHoTro = await _context.LqvYeuCauHoTros.FindAsync(id);

            // Kiểm tra quyền sở hữu và trạng thái trước khi xóa
            if (lqvYeuCauHoTro == null || lqvYeuCauHoTro.LqvNguoiDungId != studentId)
            {
                Console.WriteLine($"ERROR: [Delete POST] Không tìm thấy YCHT ID: {id} hoặc không có quyền xóa.");
                TempData["ErrorMessage"] = "Yêu cầu không tồn tại hoặc bạn không có quyền xóa.";
                return NotFound();
            }

            if (lqvYeuCauHoTro.LqvTrangThai != "Chờ xử lý")
            {
                Console.WriteLine($"WARNING: [Delete POST] YCHT ID: {id} đã chuyển trạng thái, không thể xóa.");
                TempData["ErrorMessage"] = "Yêu cầu đã được chuyển trạng thái, không thể xóa.";
                return RedirectToAction(nameof(Index));
            }


            try
            {
                _context.LqvYeuCauHoTros.Remove(lqvYeuCauHoTro);
                await _context.SaveChangesAsync();
                Console.WriteLine($"SUCCESS: [Delete POST] Xóa DB thành công YCHT ID: {id}");
                TempData["SuccessMessage"] = $"Yêu cầu hỗ trợ #{id} đã được hủy bỏ thành công.";
            }
            catch (Exception ex)
            {
                Console.WriteLine("FATAL ERROR: [Delete POST] Lỗi khi xóa khỏi DB!");
                Console.WriteLine($"EXCEPTION MESSAGE: {ex.Message}");
                TempData["ErrorMessage"] = "Lỗi khi xóa yêu cầu: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private bool LqvYeuCauHoTroExists(int id)
        {
            int studentId = GetLoggedInStudentId();
            return _context.LqvYeuCauHoTros.Any(e => e.LqvId == id && e.LqvNguoiDungId == studentId);
        }
    }
}