using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; // Cần thiết để lấy ID người dùng
using LQV_BlockchainCertificate.Models.DBModel;
using LQV_BlockchainCertificate.Models.ViewModels; 
namespace LQV_BlockchainCertificate.Areas.Student.Controllers
{
    [Area("Student")]
    // [Authorize] // Bỏ comment khi triển khai hệ thống xác thực người dùng
    public class LqvChungNhansController : Controller
    {
        private readonly LqvDbContext _context;

        public LqvChungNhansController(LqvDbContext context)
        {
            _context = context;
        }

        // Phương thức helper để lấy ID người dùng hiện tại
        private int GetCurrentUserId()
        {
            int currentUserId = 0;
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdString, out int id))
                {
                    currentUserId = id;
                }
            }
            // Dùng ID giả định (1) nếu không tìm thấy ID thực tế (chỉ dùng cho thử nghiệm)
            return (currentUserId > 0) ? currentUserId : 1;
        }

        // GET: Student/LqvChungNhans
        // Chỉ hiển thị chứng nhận của sinh viên đang đăng nhập
        public async Task<IActionResult> Index()
        {
            int currentUserId = GetCurrentUserId();

            // Truy vấn lấy danh sách chứng nhận của Sinh viên hiện tại
            var chungNhans = await _context.LqvChungNhans
                .Where(cn => cn.LqvSinhVienId == currentUserId)
                .Include(cn => cn.LqvKhoaHoc) // Join với Khóa học để lấy tên
                .AsNoTracking()
                .OrderByDescending(cn => cn.LqvNgayCap)
                .Select(cn => new ChungNhanHienThiVM // Ánh xạ sang ViewModel để hiển thị
                {
                    LqvMaChungNhan = cn.LqvMaChungNhan,
                    MaChungNhanCode = cn.LqvMaChungNhanCode,
                    TenChungNhan = cn.LqvKhoaHoc.LqvTenKhoaHoc,
                    NgayCap = cn.LqvNgayCap,
                    TrangThaiXacThuc = cn.LqvTrangThai ?? "Chưa ghi chain",
                    // Bạn có thể thêm link đến Details hoặc Verify ở đây
                })
                .ToListAsync();

            // Truyền danh sách ViewModel đã lọc
            return View(chungNhans);
        }

        // GET: Student/LqvChungNhans/Details/5
        // Chỉ cho phép xem chi tiết chứng nhận của chính mình
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int currentUserId = GetCurrentUserId();

            var lqvChungNhan = await _context.LqvChungNhans
                .Include(l => l.LqvKhoaHoc)
                .Include(l => l.LqvSinhVien)
                // LỌC: Đảm bảo chứng nhận thuộc về sinh viên hiện tại VÀ khớp ID
                .FirstOrDefaultAsync(m => m.LqvMaChungNhan == id && m.LqvSinhVienId == currentUserId);

            if (lqvChungNhan == null)
            {
                // Nếu không tìm thấy hoặc chứng nhận không thuộc về sinh viên này
                return NotFound();
            }

            // Có thể ánh xạ sang ViewModel chi tiết hơn nếu cần
            return View(lqvChungNhan);
        }

        
    }
}