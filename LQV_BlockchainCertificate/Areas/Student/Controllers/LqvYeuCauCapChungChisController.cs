using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LQV_BlockchainCertificate.Models.DBModel;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Diagnostics;

namespace LQV_BlockchainCertificate.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize]
    public class LqvYeuCauCapChungChisController : Controller
    {
        private readonly LqvDbContext _context;

        public LqvYeuCauCapChungChisController(LqvDbContext context)
        {
            _context = context;
            Console.WriteLine("Controller LqvYeuCauCapChungChisController đã được khởi tạo.");
        }

        // --- Helper để lấy ID người dùng hiện tại ---
        private int GetCurrentUserId()
        {
            Console.WriteLine("-> Bắt đầu GetCurrentUserId...");
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                Console.WriteLine($"<- GetCurrentUserId: ID người dùng hiện tại là {userId}");
                return userId;
            }
            Console.WriteLine("!!! LỖI: Không thể xác định ID người dùng.");
            throw new UnauthorizedAccessException("Không thể xác định ID người dùng. Vui lòng đăng nhập lại.");
        }

        // --- Helper: Logic kiểm tra hoàn thành khóa học ---
        private async Task<bool> HasCompletedCourse(int userId, int courseId)
        {
            Console.WriteLine($"-> Bắt đầu HasCompletedCourse cho User={userId}, Course={courseId}");

            var tienDo = await _context.LqvTienDoHocTaps
                .FirstOrDefaultAsync(td => td.LqvSinhVienId == userId &&
                                         td.LqvKhoaHocId == courseId);

            if (tienDo != null)
            {
                Console.WriteLine($"   - Tìm thấy Tiến độ. Tỷ lệ Hoàn thành: {tienDo.LqvTiLeHoanThanh}");
                if (tienDo.LqvTiLeHoanThanh >= 100.0)
                {
                    Console.WriteLine("<- HasCompletedCourse: TRUE (Đã hoàn thành >= 100%).");
                    return true;
                }
            }
            else
            {
                Console.WriteLine("   - KHÔNG tìm thấy bản ghi Tiến độ học tập.");
            }

            Console.WriteLine("<- HasCompletedCourse: FALSE (Chưa hoàn thành < 100%).");
            return false;
        }

        // =========================================================
        // Xem Danh sách Yêu cầu (Đã lọc theo User ID)
        // =========================================================
        public async Task<IActionResult> Index()
        {
            Console.WriteLine(">>> Index (GET) Bắt đầu...");
            var userId = GetCurrentUserId();

            var lqvDbContext = _context.LqvYeuCauCapChungChis
                .Where(l => l.LqvNguoiDungId == userId) // LỌC theo ID người dùng hiện tại
                .Include(l => l.LqvKhoaHoc)
                .Include(l => l.LqvNguoiDung);

            var requests = await lqvDbContext.ToListAsync();
            Console.WriteLine($"--- Index (GET): Đã tải {requests.Count} yêu cầu cho User {userId}.");

            Console.WriteLine("<<< Index (GET) Kết thúc.");
            return View(requests);
        }

        // GET: Student/LqvYeuCauCapChungChis/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            Console.WriteLine($">>> Details (GET) Bắt đầu, ID: {id}");

            if (id == null)
            {
                Console.WriteLine("!!! Details: ID là null, trả về NotFound.");
                return NotFound();
            }

            var userId = GetCurrentUserId();

            var lqvYeuCauCapChungChi = await _context.LqvYeuCauCapChungChis
                .Include(l => l.LqvKhoaHoc)
                .Include(l => l.LqvNguoiDung)
                .FirstOrDefaultAsync(m => m.LqvId == id && m.LqvNguoiDungId == userId); // KIỂM TRA QUYỀN SỞ HỮU

            if (lqvYeuCauCapChungChi == null)
            {
                Console.WriteLine($"!!! Details: Không tìm thấy Yêu cầu ID={id} hoặc không thuộc về User={userId}.");
                return NotFound();
            }

            Console.WriteLine($"--- Details: Đã tải chi tiết Yêu cầu ID={id} cho khóa học {lqvYeuCauCapChungChi.LqvKhoaHocId}.");
            Console.WriteLine("<<< Details (GET) Kết thúc.");
            return View(lqvYeuCauCapChungChi);
        }

        // =========================================================
        // Tạo Yêu cầu (GET)
        // =========================================================
        public async Task<IActionResult> Create()
        {
            Console.WriteLine(">>> Create (GET) Bắt đầu...");
            var userId = GetCurrentUserId();

            // 1. Khóa học đã hoàn thành
            var completedCourseIds = await _context.LqvTienDoHocTaps
                .Where(td => td.LqvSinhVienId == userId && td.LqvTiLeHoanThanh >= 100)
                .Select(td => td.LqvKhoaHocId)
                .ToListAsync();

            Console.WriteLine($"   - Số lượng Khóa học ĐÃ HOÀN THÀNH: {completedCourseIds.Count}");

            if (!completedCourseIds.Any())
            {
                Console.WriteLine("   - Không có khóa học nào đã hoàn thành. Hiển thị thông báo.");
                ViewData["LqvKhoaHocId"] = new SelectList(Enumerable.Empty<SelectListItem>());
                TempData["InfoMessage"] = "Bạn chưa hoàn thành khóa học nào.";
                Console.WriteLine("<<< Create (GET) Kết thúc.");
                return View();
            }

            // 2. Khóa học đã yêu cầu
            var requestedCourseIds = await _context.LqvYeuCauCapChungChis
                .Where(r => r.LqvNguoiDungId == userId &&
                            (r.LqvTrangThai == "Chờ duyệt" || r.LqvTrangThai == "Đã duyệt"))
                .Select(r => r.LqvKhoaHocId)
                .ToListAsync();

            Console.WriteLine($"   - Số lượng Khóa học ĐÃ YÊU CẦU (Chờ/Duyệt): {requestedCourseIds.Count}");

            // 3. Lọc khóa học đủ điều kiện (trong C#)
            var eligibleCourseIds = completedCourseIds.Except(requestedCourseIds).ToList();
            Console.WriteLine($"   - Số lượng Khóa học ĐỦ ĐIỀU KIỆN (Mới): {eligibleCourseIds.Count}");

            // *** FIX: Chuyển List sang Array và sử dụng LINQ to Objects để tránh lỗi SQL Server OPENJSON
            var eligibleIdsArray = eligibleCourseIds.ToArray();
            Console.WriteLine($"   - Khóa học đủ điều kiện ID Array: [{string.Join(", ", eligibleIdsArray)}]");

            // B1: Tải TẤT CẢ các khóa học về bộ nhớ (LINQ to Objects)
            var allCourses = await _context.LqvKhoaHocs.ToListAsync();

            // B2: Lọc trong bộ nhớ (LINQ to Objects)
            var availableCourses = allCourses
                .Where(kh => eligibleIdsArray.Contains(kh.LqvMaKhoaHoc))
                .ToList();

            Console.WriteLine($"   - Số lượng Khóa học hiển thị trong Dropdown: {availableCourses.Count}");

            ViewData["LqvKhoaHocId"] = new SelectList(availableCourses, "LqvMaKhoaHoc", "LqvTenKhoaHoc");

            Console.WriteLine("<<< Create (GET) Kết thúc.");
            return View();
        }

        // =========================================================
        // Tạo Yêu cầu (POST)
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LqvKhoaHocId,LqvLyDoYeuCau")] LqvYeuCauCapChungChi yeuCauInput)
        {
            Console.WriteLine($">>> Create (POST) Bắt đầu. Khóa học ID: {yeuCauInput.LqvKhoaHocId}");
            var userId = GetCurrentUserId();

            // 1. Kiểm tra điều kiện hoàn thành Khóa học
            if (!await HasCompletedCourse(userId, yeuCauInput.LqvKhoaHocId))
            {
                ModelState.AddModelError(string.Empty, "Bạn chưa hoàn thành khóa học này (Tiến độ chưa đạt 100%).");
                Console.WriteLine("   - VALIDATION FAILED: Chưa hoàn thành khóa học.");
            }

            // 2. Kiểm tra đã có yêu cầu CHỜ DUYỆT hoặc ĐÃ DUYỆT cho khóa học này chưa
            bool existingRequest = await _context.LqvYeuCauCapChungChis
               .AnyAsync(r => r.LqvNguoiDungId == userId &&
                             r.LqvKhoaHocId == yeuCauInput.LqvKhoaHocId &&
                             (r.LqvTrangThai == "Chờ duyệt" || r.LqvTrangThai == "Đã duyệt"));

            if (existingRequest)
            {
                ModelState.AddModelError(string.Empty, "Đã có yêu cầu đang chờ duyệt hoặc đã được duyệt cho khóa học này.");
                Console.WriteLine("   - VALIDATION FAILED: Đã tồn tại yêu cầu đang chờ/đã duyệt.");
            }

            // 3. Gán các giá trị cần thiết (Không để sinh viên tự nhập)
            yeuCauInput.LqvNguoiDungId = userId;
            yeuCauInput.LqvNgayYeuCau = DateTime.Now;
            yeuCauInput.LqvTrangThai = "Chờ duyệt";

            // --- BƯỚC KHẮC PHỤC LỖI MODELSTATE VÌ THIẾU THÔNG TIN (Navigation Properties và LqvTrangThai) ---
            // Lỗi này xảy ra vì ModelState được tính toán sau Model Binding nhưng trước khi gán các trường
            // không được Bind (như LqvTrangThai, LqvNgayYeuCau) và các Navigation Property.
            if (ModelState.ContainsKey("LqvKhoaHoc"))
            {
                ModelState.Remove("LqvKhoaHoc");
            }
            if (ModelState.ContainsKey("LqvNguoiDung"))
            {
                ModelState.Remove("LqvNguoiDung");
            }
            // KHẮC PHỤC LỖI LqvTrangThai: Xóa lỗi cho trường này vì ta đã gán giá trị ở trên.
            if (ModelState.ContainsKey("LqvTrangThai"))
            {
                // Chỉ xóa lỗi nếu trường đó đang báo lỗi, tức là đang có trạng thái ModelError
                if (ModelState["LqvTrangThai"].Errors.Count > 0)
                {
                    ModelState.Remove("LqvTrangThai");
                }
            }


            if (ModelState.IsValid)
            {
                Console.WriteLine("   - ModelState và Logic hợp lệ. Bắt đầu lưu Database.");

                _context.Add(yeuCauInput);
                await _context.SaveChangesAsync();
                Console.WriteLine($"   - Đã lưu Yêu cầu mới (ID={yeuCauInput.LqvId}) vào DB thành công.");

                TempData["SuccessMessage"] = "Yêu cầu cấp chứng nhận đã được gửi thành công. Vui lòng chờ Admin phê duyệt!";
                Console.WriteLine("<<< Create (POST) Kết thúc, chuyển hướng về Index.");
                return RedirectToAction(nameof(Index));
            }

            Console.WriteLine("   - ModelState KHÔNG hợp lệ. Tải lại trang Create.");

            // Nếu ModelState không hợp lệ hoặc có lỗi logic, tải lại danh sách Khóa học
            var completedCourseIds = await _context.LqvTienDoHocTaps
                .Where(td => td.LqvSinhVienId == userId && td.LqvTiLeHoanThanh >= 100.0)
                .Select(td => td.LqvKhoaHocId)
                .Distinct()
                .ToListAsync();

            var requestedCourseIdsAfterError = await _context.LqvYeuCauCapChungChis
                .Where(r => r.LqvNguoiDungId == userId &&
                            (r.LqvTrangThai == "Chờ duyệt" || r.LqvTrangThai == "Đã duyệt"))
                .Select(r => r.LqvKhoaHocId)
                .ToListAsync();

            var eligibleCourseIds = completedCourseIds.Except(requestedCourseIdsAfterError).ToList();

            // *** FIX: Đảm bảo tái sử dụng logic LINQ to Objects để tải lại dropdown chính xác
            var eligibleIdsArray = eligibleCourseIds.ToArray();
            Console.WriteLine($"   - Khóa học đủ điều kiện ID Array (After Error): [{string.Join(", ", eligibleIdsArray)}]");

            // B1: Tải tất cả các khóa học
            var allCoursesForRequest = await _context.LqvKhoaHocs.ToListAsync();

            // B2: Lọc trong bộ nhớ (LINQ to Objects)
            var availableCoursesForRequest = allCoursesForRequest
                .Where(kh => eligibleIdsArray.Contains(kh.LqvMaKhoaHoc))
                .ToList();

            Console.WriteLine($"   - Tải lại Dropdown. Số khóa học đủ điều kiện: {availableCoursesForRequest.Count}");

            ViewData["LqvKhoaHocId"] = new SelectList(availableCoursesForRequest, "LqvMaKhoaHoc", "LqvTenKhoaHoc", yeuCauInput.LqvKhoaHocId);
            Console.WriteLine("<<< Create (POST) Kết thúc (View Errors).");
            return View(yeuCauInput);
        }

        // GET: Student/LqvYeuCauCapChungChis/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            Console.WriteLine($">>> Edit (GET) Bắt đầu, ID: {id}");

            if (id == null) return NotFound();
            var userId = GetCurrentUserId();

            var yeuCau = await _context.LqvYeuCauCapChungChis
                .FirstOrDefaultAsync(y => y.LqvId == id && y.LqvNguoiDungId == userId); // Lọc và kiểm tra quyền

            if (yeuCau == null)
            {
                Console.WriteLine($"!!! Edit (GET): Không tìm thấy Yêu cầu ID={id} hoặc không thuộc về User={userId}.");
                return NotFound();
            }

            Console.WriteLine($"   - Trạng thái Yêu cầu ID={id}: {yeuCau.LqvTrangThai}");

            // CHỈ CHO PHÉP SỬA khi trạng thái là "Chờ duyệt"
            if (yeuCau.LqvTrangThai != "Chờ duyệt")
            {
                Console.WriteLine("   - ACCESS DENIED: Yêu cầu không phải 'Chờ duyệt', chuyển hướng.");
                TempData["WarningMessage"] = "Không thể chỉnh sửa yêu cầu đã được xử lý.";
                Console.WriteLine("<<< Edit (GET) Kết thúc (Redirect).");
                return RedirectToAction(nameof(Details), new { id = yeuCau.LqvId });
            }

            Console.WriteLine("   - Yêu cầu đủ điều kiện SỬA.");
            // Lưu ý: Trong Edit, ta chỉ cần danh sách khóa học để hiển thị dropdown, không cần lọc lại điều kiện.
            ViewData["LqvKhoaHocId"] = new SelectList(_context.LqvKhoaHocs, "LqvMaKhoaHoc", "LqvTenKhoaHoc", yeuCau.LqvKhoaHocId);
            Console.WriteLine("<<< Edit (GET) Kết thúc (View Edit).");
            return View(yeuCau);
        }

        // POST: Student/LqvYeuCauCapChungChis/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LqvId,LqvKhoaHocId,LqvLyDoYeuCau")] LqvYeuCauCapChungChi yeuCauInput)
        {
            Console.WriteLine($">>> Edit (POST) Bắt đầu, ID: {id}, Khóa học ID: {yeuCauInput.LqvKhoaHocId}");

            if (id != yeuCauInput.LqvId)
            {
                Console.WriteLine("!!! Edit (POST): ID trong route khác ID trong input.");
                return NotFound();
            }

            var userId = GetCurrentUserId();
            // 1. LẤY ENTITY ĐANG ĐƯỢC THEO DÕI và kiểm tra quyền
            var yeuCauToUpdate = await _context.LqvYeuCauCapChungChis
                .FirstOrDefaultAsync(y => y.LqvId == id && y.LqvNguoiDungId == userId);

            if (yeuCauToUpdate == null)
            {
                Console.WriteLine($"!!! Edit (POST): Không tìm thấy Yêu cầu ID={id} hoặc không thuộc về User={userId}.");
                return NotFound();
            }

            // 2. Kiểm tra trạng thái: CHỈ CHO PHÉP SỬA khi trạng thái là "Chờ duyệt"
            if (yeuCauToUpdate.LqvTrangThai != "Chờ duyệt")
            {
                Console.WriteLine($"   - ACCESS DENIED: Yêu cầu ID={id} có trạng thái '{yeuCauToUpdate.LqvTrangThai}', không thể sửa.");
                TempData["WarningMessage"] = "Không thể chỉnh sửa yêu cầu đã được xử lý.";
                Console.WriteLine("<<< Edit (POST) Kết thúc (Redirect).");
                return RedirectToAction(nameof(Details), new { id = id });
            }

            // --- KHẮC PHỤC LỖI MODELSTATE (Tương tự Create) ---
            if (ModelState.ContainsKey("LqvKhoaHoc"))
            {
                ModelState.Remove("LqvKhoaHoc");
            }
            if (ModelState.ContainsKey("LqvNguoiDung"))
            {
                ModelState.Remove("LqvNguoiDung");
            }
            // KHẮC PHỤC LỖI LqvTrangThai: Xóa lỗi cho trường này vì giá trị cũ đã tồn tại trong yeuCauToUpdate
            if (ModelState.ContainsKey("LqvTrangThai") && ModelState["LqvTrangThai"].Errors.Count > 0)
            {
                ModelState.Remove("LqvTrangThai");
            }


            if (ModelState.IsValid)
            {
                Console.WriteLine("   - ModelState hợp lệ. Bắt đầu cập nhật.");
                try
                {
                    // 3. CẬP NHẬT TRỰC TIẾP các trường được phép sửa
                    yeuCauToUpdate.LqvKhoaHocId = yeuCauInput.LqvKhoaHocId;
                    yeuCauToUpdate.LqvLyDoYeuCau = yeuCauInput.LqvLyDoYeuCau;
                    Console.WriteLine($"   - Cập nhật Khóa học ID={yeuCauInput.LqvKhoaHocId}, Lý do='{yeuCauInput.LqvLyDoYeuCau}'.");

                    await _context.SaveChangesAsync();
                    Console.WriteLine("   - Đã lưu cập nhật vào Database thành công.");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    Console.WriteLine($"!!! LỖI DbUpdateConcurrencyException: {ex.Message}");
                    if (!LqvYeuCauCapChungChiExists(yeuCauInput.LqvId)) return NotFound();
                    else throw;
                }
                TempData["SuccessMessage"] = "Yêu cầu đã được cập nhật.";
                Console.WriteLine("<<< Edit (POST) Kết thúc, chuyển hướng về Index.");
                return RedirectToAction(nameof(Index));
            }

            Console.WriteLine("   - ModelState KHÔNG hợp lệ. Tải lại View Edit.");

            // Nếu ModelState không hợp lệ, tải lại danh sách khóa học
            ViewData["LqvKhoaHocId"] = new SelectList(_context.LqvKhoaHocs, "LqvMaKhoaHoc", "LqvTenKhoaHoc", yeuCauInput.LqvKhoaHocId);
            Console.WriteLine("<<< Edit (POST) Kết thúc (View Errors).");
            return View(yeuCauInput);
        }

        // GET: Student/LqvYeuCauCapChungChis/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            Console.WriteLine($">>> Delete (GET) Bắt đầu, ID: {id}");

            if (id == null)
            {
                Console.WriteLine("!!! Delete (GET): ID là null, trả về NotFound.");
                return NotFound();
            }

            var userId = GetCurrentUserId();

            var lqvYeuCauCapChungChi = await _context.LqvYeuCauCapChungChis
                .Include(l => l.LqvKhoaHoc)
                .Include(l => l.LqvNguoiDung)
                .FirstOrDefaultAsync(m => m.LqvId == id && m.LqvNguoiDungId == userId); // KIỂM TRA QUYỀN SỞ HỮU

            if (lqvYeuCauCapChungChi == null)
            {
                Console.WriteLine($"!!! Delete (GET): Không tìm thấy Yêu cầu ID={id} hoặc không thuộc về User={userId}.");
                return NotFound();
            }

            Console.WriteLine($"   - Trạng thái Yêu cầu ID={id}: {lqvYeuCauCapChungChi.LqvTrangThai}");

            if (lqvYeuCauCapChungChi.LqvTrangThai != "Chờ duyệt")
            {
                Console.WriteLine("   - ACCESS DENIED: Yêu cầu không phải 'Chờ duyệt', chuyển hướng.");
                TempData["WarningMessage"] = "Không thể xóa yêu cầu đã được xử lý.";
                Console.WriteLine("<<< Delete (GET) Kết thúc (Redirect).");
                return RedirectToAction(nameof(Details), new { id = lqvYeuCauCapChungChi.LqvId });
            }

            Console.WriteLine("   - Yêu cầu đủ điều kiện XÓA.");
            Console.WriteLine("<<< Delete (GET) Kết thúc (View Delete).");
            return View(lqvYeuCauCapChungChi);
        }

        // POST: Student/LqvYeuCauCapChungChis/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Console.WriteLine($">>> DeleteConfirmed (POST) Bắt đầu, ID: {id}");

            var userId = GetCurrentUserId();
            var lqvYeuCauCapChungChi = await _context.LqvYeuCauCapChungChis.FindAsync(id);

            // Kiểm tra quyền sở hữu và trạng thái trước khi xóa
            if (lqvYeuCauCapChungChi != null)
            {
                Console.WriteLine($"   - Tìm thấy Yêu cầu ID={id}. Người dùng ID: {lqvYeuCauCapChungChi.LqvNguoiDungId}, Trạng thái: {lqvYeuCauCapChungChi.LqvTrangThai}");
            }
            else
            {
                Console.WriteLine($"   - KHÔNG tìm thấy Yêu cầu ID={id}.");
            }

            if (lqvYeuCauCapChungChi != null &&
                lqvYeuCauCapChungChi.LqvNguoiDungId == userId &&
                lqvYeuCauCapChungChi.LqvTrangThai == "Chờ duyệt")
            {
                Console.WriteLine("   - ĐIỀU KIỆN HỢP LỆ: Bắt đầu xóa.");
                _context.LqvYeuCauCapChungChis.Remove(lqvYeuCauCapChungChi);
                await _context.SaveChangesAsync();
                Console.WriteLine($"   - Đã xóa thành công Yêu cầu ID={id}.");
                TempData["SuccessMessage"] = "Yêu cầu đã được hủy bỏ.";
            }
            else
            {
                Console.WriteLine("   - ĐIỀU KIỆN KHÔNG HỢP LỆ: Không thực hiện xóa.");
                TempData["ErrorMessage"] = "Không thể xóa yêu cầu này (Không tìm thấy, không phải của bạn, hoặc đã được xử lý).";
            }

            Console.WriteLine("<<< DeleteConfirmed (POST) Kết thúc, chuyển hướng về Index.");
            return RedirectToAction(nameof(Index));
        }

        private bool LqvYeuCauCapChungChiExists(int id)
        {
            var exists = _context.LqvYeuCauCapChungChis.Any(e => e.LqvId == id);
            Console.WriteLine($"--- LqvYeuCauCapChungChiExists(ID={id}): {exists}");
            return exists;
        }
    }
}