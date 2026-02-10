using Microsoft.AspNetCore.Mvc;
using LQV_BlockchainCertificate.Models.DBModel;
using LQV_BlockchainCertificate.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LQV_BlockchainCertificate.Areas.Auth.Controllers
{
    [Area("Auth")]
    public class AccountController : Controller
    {
        private readonly LqvDbContext _context;
        private readonly IEmailService _emailService;

        public AccountController(LqvDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // ================= ĐĂNG KÝ =================
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(string tenDangNhap, string hoTen, string email, string matKhau)
        {
            if (_context.LqvNguoiDungs.Any(u => u.LqvTenDangNhap == tenDangNhap))
            {
                TempData["Error"] = "Tên đăng nhập đã tồn tại.";
                return View();
            }

            if (_context.LqvNguoiDungs.Any(u => u.LqvEmail == email))
            {
                TempData["Error"] = "Email đã được sử dụng.";
                return View();
            }

            // 🔐 HASH BCRYPT
            string hashPassword = HashPassword(matKhau);

            var user = new LqvNguoiDung
            {
                LqvTenDangNhap = tenDangNhap,
                LqvHoTen = hoTen,
                LqvEmail = email,
                LqvMatKhauHash = hashPassword,
                LqvRoleId = 2,
                LqvNgayTao = DateTime.Now,
                LqvDaXacThuc = false
            };

            _context.LqvNguoiDungs.Add(user);
            await _context.SaveChangesAsync();

            await SendOtpAsync(email, hoTen, "DangKy");

            TempData["Success"] = "Đã gửi mã xác thực đến email của bạn. Vui lòng kiểm tra.";
            return RedirectToAction("VerifyOtp", new { email });
        }

        // ================= XÁC THỰC OTP =================
        [HttpGet]
        public IActionResult VerifyOtp(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public IActionResult VerifyOtp(string email, string otp)
        {
            var record = _context.LqvXacThucEmails
                .Where(x => x.LqvEmail == email && x.LqvMaOtp == otp && !x.LqvTrangThai)
                .OrderByDescending(x => x.LqvThoiGianTao)
                .FirstOrDefault();

            if (record == null)
            {
                ViewBag.Error = "Mã OTP không hợp lệ.";
                ViewBag.Email = email;
                return View();
            }

            if (record.LqvThoiGianHetHan < DateTime.Now)
            {
                ViewBag.Error = "Mã OTP đã hết hạn.";
                ViewBag.Email = email;
                return View();
            }

            record.LqvTrangThai = true;

            var user = _context.LqvNguoiDungs.FirstOrDefault(u => u.LqvEmail == email);
            if (user != null) user.LqvDaXacThuc = true;

            _context.SaveChanges();

            ViewBag.Success = "Xác thực email thành công!";
            return View();
        }

        // ================= GỬI LẠI OTP =================
        [HttpPost]
        public async Task<IActionResult> ResendOtp(string email)
        {
            var user = _context.LqvNguoiDungs.FirstOrDefault(u => u.LqvEmail == email);
            if (user == null)
            {
                ViewBag.Error = "Không tìm thấy tài khoản.";
                ViewBag.Email = email;
                return View("VerifyOtp");
            }

            if (user.LqvDaXacThuc)
            {
                ViewBag.Success = "Tài khoản đã được xác thực.";
                ViewBag.Email = email;
                return View("VerifyOtp");
            }

            _context.LqvXacThucEmails.RemoveRange(
                _context.LqvXacThucEmails.Where(o => o.LqvEmail == email && !o.LqvTrangThai)
            );
            _context.SaveChanges();

            await SendOtpAsync(email, user.LqvHoTen, "DangKy");

            ViewBag.Success = "Đã gửi lại mã OTP.";
            ViewBag.Email = email;
            return View("VerifyOtp");
        }

        // ================= ĐĂNG NHẬP =================
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string tenDangNhap, string matKhau)
        {
            if (string.IsNullOrEmpty(tenDangNhap) || string.IsNullOrEmpty(matKhau))
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ thông tin.";
                return View();
            }

            var user = await _context.LqvNguoiDungs
                .Include(u => u.LqvRole)
                .FirstOrDefaultAsync(u =>
                    u.LqvTenDangNhap == tenDangNhap ||
                    u.LqvEmail == tenDangNhap
                );

            if (user == null ||
                !BCrypt.Net.BCrypt.Verify(matKhau, user.LqvMatKhauHash))
            {
                TempData["Error"] = "Sai tên đăng nhập hoặc mật khẩu.";
                return View();
            }

            if (!user.LqvDaXacThuc)
            {
                TempData["Error"] = "Tài khoản chưa xác thực email.";
                return RedirectToAction("VerifyOtp", new { email = user.LqvEmail });
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.LqvId.ToString()),
                new Claim(ClaimTypes.Name, user.LqvTenDangNhap),
                new Claim(ClaimTypes.Role, user.LqvRole?.LqvRoleName ?? "Guest"),
                new Claim("FullName", user.LqvHoTen),
                new Claim("AvatarUrl", user.LqvAvt ?? "/img/default-avatar.png")
            };

            var identity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity)
            );

            return user.LqvRoleId switch
            {
                1 => RedirectToAction("Index", "Dashboard", new { area = "Admin" }),
                2 => RedirectToAction("Index", "Dashboard", new { area = "GiangVien" }),
                3 => RedirectToAction("Index", "Home", new { area = "Student" })
            };
        }

        // ================= ĐĂNG XUẤT =================
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ================= QUÊN MẬT KHẨU =================
        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = _context.LqvNguoiDungs.FirstOrDefault(u => u.LqvEmail == email);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy tài khoản.";
                return View();
            }

            await SendOtpAsync(email, user.LqvHoTen, "QuenMatKhau");
            return RedirectToAction("VerifyOtpReset", new { email });
        }

        // ================= OTP RESET =================
        [HttpGet]
        public IActionResult VerifyOtpReset(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public IActionResult VerifyOtpReset(string email, string otp)
        {
            var record = _context.LqvXacThucEmails
                .Where(x => x.LqvEmail == email && x.LqvMaOtp == otp && !x.LqvTrangThai)
                .OrderByDescending(x => x.LqvThoiGianTao)
                .FirstOrDefault();

            if (record == null || record.LqvThoiGianHetHan < DateTime.Now)
            {
                ViewBag.Error = "OTP không hợp lệ.";
                ViewBag.Email = email;
                return View();
            }

            record.LqvTrangThai = true;
            _context.SaveChanges();

            TempData["EmailReset"] = email;
            return RedirectToAction("ResetPassword");
        }

        // ================= RESET PASSWORD =================
        [HttpGet]
        public IActionResult ResetPassword()
        {
            if (TempData["EmailReset"] == null)
                return RedirectToAction("Login");

            ViewBag.Email = TempData["EmailReset"].ToString();
            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(string email, string matKhauMoi)
        {
            var user = _context.LqvNguoiDungs.FirstOrDefault(u => u.LqvEmail == email);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy tài khoản.";
                return RedirectToAction("ForgotPassword");
            }

            // 🔐 HASH BCRYPT
            user.LqvMatKhauHash = HashPassword(matKhauMoi);
            _context.SaveChanges();

            TempData["Success"] = "Đổi mật khẩu thành công.";
            return RedirectToAction("Login");
        }

        // ================= OTP =================
        private async Task SendOtpAsync(string email, string hoTen, string loai)
        {
            string otp = new Random().Next(100000, 999999).ToString();

            _context.LqvXacThucEmails.Add(new LqvXacThucEmail
            {
                LqvEmail = email,
                LqvMaOtp = otp,
                LqvLoaiXacThuc = loai,
                LqvThoiGianTao = DateTime.Now,
                LqvThoiGianHetHan = DateTime.Now.AddMinutes(5),
                LqvTrangThai = false
            });

            await _context.SaveChangesAsync();

            await _emailService.SendEmailAsync(
                email,
                "Mã xác thực OTP",
                $"<p>Xin chào {hoTen}, OTP của bạn là <b>{otp}</b></p>"
            );
        }

        // ================= HASH PASSWORD (BCRYPT) =================
        private string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Mật khẩu không hợp lệ.");

            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}
