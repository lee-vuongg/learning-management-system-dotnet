using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LQV_BlockchainCertificate.Models.DBModel;
using X.PagedList;
using X.PagedList.Mvc.Core;
using X.PagedList.Extensions;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting; // Cần thiết để xử lý file
using System.IO; // Cần thiết để xử lý file
using Microsoft.AspNetCore.Http; // Cần thiết để xử lý IFormFile

namespace LQV_BlockchainCertificate.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LqvNguoiDungsController : Controller
    {
        private readonly LqvDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment; // 1. KHAI BÁO IWebHostEnvironment

        // 2. INJECT IWebHostEnvironment VÀO CONSTRUCTOR
        public LqvNguoiDungsController(LqvDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // ================== DANH SÁCH NGƯỜI DÙNG ==================
        public async Task<IActionResult> Index(string searchString, int? roleId, int? page)
        {
            Console.WriteLine($"\n--- 🚀 Bắt đầu Index | {DateTime.Now:HH:mm:ss} ---");
            Console.WriteLine($"🟢 [Index] Params: search='{searchString}', roleId={roleId}, page={page}");

            int pageSize = 10;
            int pageNumber = page ?? 1;

            try
            {
                // 🔹 Truy vấn dữ liệu có kèm Role
                var users = _context.LqvNguoiDungs
                    .Include(u => u.LqvRole)
                    .AsQueryable();

                Console.WriteLine($"✅ Tổng người dùng BAN ĐẦU: {users.Count()}");

                // 🔹 Tìm kiếm theo tên đăng nhập hoặc email
                if (!string.IsNullOrEmpty(searchString))
                {
                    users = users.Where(u =>
                        u.LqvTenDangNhap.Contains(searchString) ||
                        u.LqvEmail.Contains(searchString));
                    Console.WriteLine($"🔍 Sau tìm kiếm ('{searchString}'): {users.Count()}");
                }

                // 🔹 Lọc theo vai trò
                if (roleId.HasValue && roleId > 0)
                {
                    users = users.Where(u => u.LqvRoleId == roleId);
                    Console.WriteLine($"🎯 Sau lọc vai trò ({roleId}): {users.Count()}");
                }

                // 🔹 Sắp xếp mới nhất lên đầu
                var orderedUsers = users.OrderByDescending(u => u.LqvNgayTao);

                // 🔹 Phân trang
                var pagedList = orderedUsers.ToPagedList(pageNumber, pageSize);

                Console.WriteLine($"📄 Trang {pageNumber} (Kích thước {pageSize}): {pagedList.Count} bản ghi");

                // 🔹 Tải danh sách vai trò
                var roles = await _context.LqvRoles.ToListAsync();
                Console.WriteLine($"📦 Số vai trò tải: {roles.Count}");

                // 🔹 Truyền dữ liệu cho View
                ViewBag.CurrentFilter = searchString;
                ViewBag.RoleList = new SelectList(roles, "LqvRoleId", "LqvRoleName", roleId);

                Console.WriteLine("--- ✅ Hoàn thành Index ---");
                return View(pagedList);
            }
            catch (Exception ex)
            {
                Console.WriteLine("--- ❌ LỖI Index() ---");
                Console.WriteLine("Chi tiết lỗi: " + ex.Message);
                throw;
            }
        }

        // ================== TẠO NGƯỜI DÙNG (GET) ==================
        public IActionResult Create()
        {
            Console.WriteLine($"\n--- 🚀 Bắt đầu Create (GET) | {DateTime.Now:HH:mm:ss} ---");
            ViewData["LqvRoleId"] = new SelectList(_context.LqvRoles, "LqvRoleId", "LqvRoleName");
            Console.WriteLine("--- ✅ Hoàn thành Create (GET) ---");
            return View();
        }

        // ================== TẠO NGƯỜI DÙNG (POST) ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Lưu ý: Nếu có xử lý file trong Create, bạn cũng cần thêm IFormFile và logic xử lý
        public async Task<IActionResult> Create([Bind("LqvId,LqvTenDangNhap,LqvHoTen,LqvEmail,LqvMatKhauHash,LqvRoleId,LqvDaXacThuc,LqvWalletAddress,LqvAvt,LqvNgaySinh")] LqvNguoiDung lqvNguoiDung)
        {
            Console.WriteLine($"\n--- 🚀 Bắt đầu Create (POST) | {DateTime.Now:HH:mm:ss} ---");
            Console.WriteLine($"🟢 [Create POST] Tên ĐN: {lqvNguoiDung.LqvTenDangNhap}, Email: {lqvNguoiDung.LqvEmail}, Valid: {ModelState.IsValid}");

            // Loại bỏ validation cho LqvAvt và LqvMatKhauHash nếu chưa được điền ở đây
            ModelState.Remove("LqvAvt");
            ModelState.Remove("LqvMatKhauHash");

            if (ModelState.IsValid)
            {
                lqvNguoiDung.LqvNgayTao = DateTime.Now;
                // Lưu ý: Cần thêm logic Hash mật khẩu trước khi lưu tại đây nếu bạn có trường LqvMatKhauHash
                _context.Add(lqvNguoiDung);
                await _context.SaveChangesAsync();

                // Giả sử GhiNhatKy là phương thức có sẵn
                // await GhiNhatKy("Thêm người dùng", $"Tạo tài khoản: {lqvNguoiDung.LqvTenDangNhap}");

                Console.WriteLine("✅ Thêm thành công. Chuyển hướng về Index.");
                Console.WriteLine("--- ✅ Hoàn thành Create (POST) ---");
                return RedirectToAction(nameof(Index));
            }

            Console.WriteLine("❌ Lỗi ModelState. Chuyển về View.");
            ViewData["LqvRoleId"] = new SelectList(_context.LqvRoles, "LqvRoleId", "LqvRoleName", lqvNguoiDung.LqvRoleId);
            Console.WriteLine("--- ❌ Thất bại Create (POST) ---");
            return View(lqvNguoiDung);
        }

        // ================== HIỂN THỊ CHI TIẾT (GET) ==================
        public async Task<IActionResult> Details(int? id)
        {
            Console.WriteLine($"\n--- 🚀 Bắt đầu Details | {DateTime.Now:HH:mm:ss} ---");
            Console.WriteLine($"🟢 [Details] ID cần tìm: {id ?? 0}");

            if (id == null)
            {
                Console.WriteLine("❌ Lỗi: ID là null. Trả về NotFound.");
                return NotFound();
            }

            var lqvNguoiDung = await _context.LqvNguoiDungs
                .Include(n => n.LqvRole)
                .FirstOrDefaultAsync(m => m.LqvId == id);

            if (lqvNguoiDung == null)
            {
                Console.WriteLine($"❌ Lỗi: Không tìm thấy Người Dùng với ID={id}. Trả về NotFound.");
                return NotFound();
            }

            Console.WriteLine($"✅ Tìm thấy Người Dùng: {lqvNguoiDung.LqvTenDangNhap}. RoleId: {lqvNguoiDung.LqvRoleId}");
            Console.WriteLine("--- ✅ Hoàn thành Details ---");
            return View(lqvNguoiDung);
        }


        // ================== CHỈNH SỬA NGƯỜI DÙNG (GET) ==================
        public async Task<IActionResult> Edit(int? id)
        {
            Console.WriteLine($"\n--- 🚀 Bắt đầu Edit (GET) | {DateTime.Now:HH:mm:ss} ---");
            Console.WriteLine($"🟢 [Edit GET] ID cần chỉnh sửa: {id ?? 0}");

            if (id == null)
            {
                Console.WriteLine("❌ Lỗi: ID là null. Trả về NotFound.");
                return NotFound();
            }

            var user = await _context.LqvNguoiDungs.FindAsync(id);
            if (user == null)
            {
                Console.WriteLine($"❌ Lỗi: Không tìm thấy Người Dùng với ID={id}. Trả về NotFound.");
                return NotFound();
            }

            Console.WriteLine($"✅ Tìm thấy Người Dùng: {user.LqvTenDangNhap}. RoleId hiện tại: {user.LqvRoleId}");
            ViewData["LqvRoleId"] = new SelectList(_context.LqvRoles, "LqvRoleId", "LqvRoleName", user.LqvRoleId);
            Console.WriteLine("--- ✅ Hoàn thành Edit (GET) ---");
            return View(user);
        }

        // ================== CHỈNH SỬA NGƯỜI DÙNG (POST) - CẬP NHẬT XỬ LÝ FILE AVATAR ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
             // LqvAvt bị loại bỏ khỏi Bind vì giá trị mới được nhận từ IFormFile hoặc giữ lại giá trị cũ từ DB
             [Bind("LqvId,LqvTenDangNhap,LqvHoTen,LqvEmail,LqvRoleId,LqvWalletAddress,LqvDaXacThuc,LqvNgaySinh")] LqvNguoiDung lqvNguoiDung,
             string? LqvMatKhauMoi,
             IFormFile? LqvAvtFile) // 3. THÊM THAM SỐ IFormFile
        {
            Console.WriteLine($"\n--- 🚀 Bắt đầu Edit (POST) | {DateTime.Now:HH:mm:ss} ---");
            Console.WriteLine($"🟢 [Edit POST] ID URL: {id}. ID Model: {lqvNguoiDung.LqvId}. Tên ĐN: {lqvNguoiDung.LqvTenDangNhap}. Valid: {ModelState.IsValid}");

            if (id != lqvNguoiDung.LqvId)
            {
                Console.WriteLine($"❌ Lỗi: ID trong URL ({id}) khác ID trong Model ({lqvNguoiDung.LqvId}). Trả về NotFound.");
                return NotFound();
            }

            // ⚠️ BƯỚC QUAN TRỌNG: Lấy dữ liệu cũ từ DB (Mật khẩu, Ngày tạo, Avt cũ)
            var userFromDb = await _context.LqvNguoiDungs
                                   .AsNoTracking() // Dùng AsNoTracking để tránh xung đột
                                   .FirstOrDefaultAsync(u => u.LqvId == id);

            if (userFromDb == null)
            {
                Console.WriteLine($"❌ Lỗi: Không tìm thấy Người Dùng với ID={id} trong DB.");
                return NotFound();
            }

            // Xóa lỗi validation liên quan đến các trường không nhận từ form
            ModelState.Remove("LqvRole"); // Khắc phục lỗi "The LqvRole field is required."
            ModelState.Remove("LqvMatKhauHash");
            ModelState.Remove("LqvAvt"); // Loại bỏ validation cho LqvAvt

            if (ModelState.IsValid)
            {
                try
                {
                    // 4. XỬ LÝ UPLOAD FILE AVATAR MỚI (IFormFile)
                    if (LqvAvtFile != null && LqvAvtFile.Length > 0)
                    {
                        Console.WriteLine("⬆️ Phát hiện file Avatar mới. Đang xử lý lưu trữ...");

                        // 4a. Xử lý đường dẫn và tên file
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "avatars");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        // Tạo tên file duy nhất (nên dùng ID người dùng để tiện quản lý)
                        string extension = Path.GetExtension(LqvAvtFile.FileName);
                        string fileName = $"{lqvNguoiDung.LqvId}{extension}";
                        string filePath = Path.Combine(uploadsFolder, fileName);

                        // 4b. Lưu file vào hệ thống
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await LqvAvtFile.CopyToAsync(stream);
                        }

                        // 4c. Cập nhật đường dẫn lưu vào DB (dùng đường dẫn tương đối)
                        lqvNguoiDung.LqvAvt = $"/uploads/avatars/{fileName}";
                        Console.WriteLine($"   -> Đã gán đường dẫn Avatar mới: {lqvNguoiDung.LqvAvt}");
                    }
                    else
                    {
                        // KHÔNG CÓ FILE MỚI ĐƯỢC UPLOAD -> Giữ lại đường dẫn ảnh cũ từ DB
                        lqvNguoiDung.LqvAvt = userFromDb.LqvAvt;
                        Console.WriteLine("   -> Không có file mới. Giữ lại Avatar cũ.");
                    }


                    // 5. BẢO TOÀN DỮ LIỆU BẮT BUỘC/KHÔNG ĐƯỢC THAY ĐỔI

                    // a) Bảo toàn Mật khẩu Hash cũ (trừ khi có mật khẩu mới)
                    if (!string.IsNullOrEmpty(LqvMatKhauMoi))
                    {
                        lqvNguoiDung.LqvMatKhauHash =
                            BCrypt.Net.BCrypt.HashPassword(LqvMatKhauMoi);

                        Console.WriteLine("🔐 Đã cập nhật mật khẩu mới (bcrypt).");
                    }
                    else
                    {
                        lqvNguoiDung.LqvMatKhauHash = userFromDb.LqvMatKhauHash;
                    }


                    // b) Giữ lại Ngày tạo gốc
                    lqvNguoiDung.LqvNgayTao = userFromDb.LqvNgayTao;

                    // 6. CẬP NHẬT THÔNG TIN VÀ LƯU
                    // Đánh dấu đối tượng là đã được chỉnh sửa
                    _context.Entry(lqvNguoiDung).State = EntityState.Modified;

                    await _context.SaveChangesAsync();

                    await GhiNhatKy("Cập nhật người dùng", $"Sửa thông tin tài khoản: {lqvNguoiDung.LqvTenDangNhap} (RoleID mới: {lqvNguoiDung.LqvRoleId})");

                    Console.WriteLine("✅ Cập nhật thành công. Chuyển hướng về Index.");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    Console.WriteLine("❌ Lỗi Concurrency: " + ex.Message);
                    if (!_context.LqvNguoiDungs.Any(e => e.LqvId == id))
                        return NotFound();
                    else throw;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("❌ Lỗi Xử lý File/DB: " + ex.Message);
                    // Thêm lỗi vào ModelState để hiển thị trong View
                    ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi lưu: " + ex.Message);
                }

                if (!ModelState.IsValid)
                {
                    // Nếu có lỗi Exception đã được thêm vào ModelState, chuyển đến phần hiển thị lỗi
                }
                else
                {
                    Console.WriteLine("--- ✅ Hoàn thành Edit (POST) ---");
                    return RedirectToAction(nameof(Index));
                }
            }

            // ================== DEBUG LỖI MODELSTATE ==================
            Console.WriteLine("❌ Lỗi ModelState. Chi tiết các lỗi:");
            foreach (var modelStateKey in ModelState.Keys)
            {
                var modelStateEntry = ModelState[modelStateKey];
                if (modelStateEntry != null && modelStateEntry.Errors.Count > 0)
                {
                    Console.WriteLine($"    - Trường '{modelStateKey}':");
                    foreach (var error in modelStateEntry.Errors)
                    {
                        Console.WriteLine($"      > {error.ErrorMessage}");
                    }
                }
            }
            // ==========================================================

            Console.WriteLine("❌ Chuyển về View Edit.");
            ViewData["LqvRoleId"] = new SelectList(_context.LqvRoles, "LqvRoleId", "LqvRoleName", lqvNguoiDung.LqvRoleId);
            Console.WriteLine("--- ❌ Thất bại Edit (POST) ---");

            // Đảm bảo Avt cũ vẫn được hiển thị trong View khi có lỗi
            lqvNguoiDung.LqvAvt = userFromDb.LqvAvt;

            return View(lqvNguoiDung);
        }

        // ================== XÓA NGƯỜI DÙNG (GET) ==================
        public async Task<IActionResult> Delete(int? id)
        {
            Console.WriteLine($"\n--- 🚀 Bắt đầu Delete (GET) | {DateTime.Now:HH:mm:ss} ---");
            Console.WriteLine($"🟢 [Delete GET] ID cần xóa: {id ?? 0}");

            if (id == null)
            {
                Console.WriteLine("❌ Lỗi: ID là null. Trả về NotFound.");
                return NotFound();
            }

            var user = await _context.LqvNguoiDungs
                .Include(u => u.LqvRole)
                .FirstOrDefaultAsync(m => m.LqvId == id);

            if (user == null)
            {
                Console.WriteLine($"❌ Lỗi: Không tìm thấy Người Dùng với ID={id}. Trả về NotFound.");
                return NotFound();
            }

            Console.WriteLine($"✅ Xác nhận xóa Người Dùng: {user.LqvTenDangNhap}");
            Console.WriteLine("--- ✅ Hoàn thành Delete (GET) ---");
            return View(user);
        }

        // ================== XÓA NGƯỜI DÙNG (POST) ==================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Console.WriteLine($"\n--- 🚀 Bắt đầu DeleteConfirmed (POST) | {DateTime.Now:HH:mm:ss} ---");
            Console.WriteLine($"🟢 [DeleteConfirmed POST] ID đang xử lý: {id}");

            var user = await _context.LqvNguoiDungs.FindAsync(id);
            if (user != null)
            {
                // Xóa file Avatar cũ nếu tồn tại
                if (!string.IsNullOrEmpty(user.LqvAvt))
                {
                    string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, user.LqvAvt.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                        Console.WriteLine($"   -> Đã xóa Avatar cũ: {oldFilePath}");
                    }
                }

                _context.LqvNguoiDungs.Remove(user);
                await _context.SaveChangesAsync();

                await GhiNhatKy("Xóa người dùng", $"Xóa tài khoản: {user.LqvTenDangNhap}");
                Console.WriteLine($"✅ Xóa thành công Người Dùng ID={id}.");
            }
            else
            {
                Console.WriteLine($"⚠️ Cảnh báo: Không tìm thấy Người Dùng ID={id} để xóa.");
            }

            Console.WriteLine("--- ✅ Hoàn thành DeleteConfirmed (POST) ---");
            return RedirectToAction(nameof(Index));
        }

        // ================== GHI NHẬT KÝ ==================
        private async Task GhiNhatKy(string hanhDong, string chiTiet)
        {
            Console.WriteLine($"\n--- 📝 Ghi Nhật Ký: {hanhDong} ---");
            // Giả định LqvNhatKyHoatDong là một Model hợp lệ
            var log = new LqvNhatKyHoatDong
            {
                LqvTaiKhoan = User.Identity?.Name ?? "Admin",
                LqvHanhDong = hanhDong,
                LqvChiTiet = chiTiet,
                LqvThoiGian = DateTime.Now,
                LqvIp = HttpContext.Connection.RemoteIpAddress?.ToString()
            };

            // Giả định _context.LqvNhatKyHoatDongs là DbSet hợp lệ
            // _context.LqvNhatKyHoatDongs.Add(log);
            // await _context.SaveChangesAsync();
            Console.WriteLine("--- ✅ Ghi Nhật Ký thành công (Chức năng này cần được kích hoạt) ---");
        }
    }
}