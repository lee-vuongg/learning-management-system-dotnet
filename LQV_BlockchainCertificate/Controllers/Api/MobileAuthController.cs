using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LQV_BlockchainCertificate.Models.DBModel;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace LQV_BlockchainCertificate.Controllers.Api
{
    [ApiController]
    [Route("api/mobile/auth")]
    public class MobileAuthController : ControllerBase
    {
        private readonly LqvDbContext _context;

        public MobileAuthController(LqvDbContext context)
        {
            _context = context;
        }

        // ================= LOGIN =================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _context.LqvNguoiDungs
                .Include(x => x.LqvRole)
                .FirstOrDefaultAsync(x =>
                    x.LqvTenDangNhap == dto.Username ||
                    x.LqvEmail == dto.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.LqvMatKhauHash))
                return BadRequest(new { message = "Sai tài khoản hoặc mật khẩu" });

            if (!user.LqvDaXacThuc)
                return Unauthorized(new { message = "Tài khoản chưa xác thực email" });

            // 🚫 CHẶN ADMIN + GIẢNG VIÊN
            if (user.LqvRoleId != 3)
            {
                return StatusCode(403, new
                {
                    message = "Chỉ sinh viên mới được đăng nhập trên mobile"
                });
            }



            // ✅ CHỈ STUDENT ĐI QUA
            return Ok(new
            {
                userId = user.LqvId,
                username = user.LqvTenDangNhap,
                fullName = user.LqvHoTen,
                roleId = user.LqvRoleId,   // 🔥 thêm cho rõ
                roleName = user.LqvRole.LqvRoleName,
                avatar = user.LqvAvt
            });

        }

        // ================= REGISTER =================
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (_context.LqvNguoiDungs.Any(x => x.LqvTenDangNhap == dto.Username))
                return BadRequest(new { message = "Tên đăng nhập đã tồn tại" });

            if (_context.LqvNguoiDungs.Any(x => x.LqvEmail == dto.Email))
                return BadRequest(new { message = "Email đã tồn tại" });

            var user = new LqvNguoiDung
            {
                LqvTenDangNhap = dto.Username,
                LqvHoTen = dto.FullName,
                LqvEmail = dto.Email,
                LqvMatKhauHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                LqvRoleId = 3,
                LqvNgayTao = DateTime.Now,
                LqvDaXacThuc = false
            };

            _context.LqvNguoiDungs.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đăng ký thành công, vui lòng xác thực email" });
        }
    }

    // ================= DTO =================
    public class LoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class RegisterDto
    {
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
