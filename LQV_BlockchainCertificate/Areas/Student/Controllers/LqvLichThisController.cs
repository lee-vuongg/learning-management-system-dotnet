using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LQV_BlockchainCertificate.Models.DBModel;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System;

namespace LQV_BlockchainCertificate.Areas.Student.Controllers
{
    [Area("Student")]
    public class LqvLichThisController : Controller
    {
        private readonly LqvDbContext _context;

        public LqvLichThisController(LqvDbContext context)
        {
            _context = context;
        }

        // ===============================
        // DANH SÁCH LỊCH THI CỦA SINH VIÊN
        // ===============================
        public async Task<IActionResult> Index()
        {
            int sinhVienId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)
            );

            var lichThis = await _context.LqvLichThis
                .Include(lt => lt.LqvDeThi)
                .Include(lt => lt.LqvLopHoc)
                    .ThenInclude(lh => lh.LqvDangKyLopHocs)
                .Where(lt =>
                    lt.LqvLopHoc.LqvDangKyLopHocs
                        .Any(dk => dk.LqvSinhVienId == sinhVienId)
                )
                .OrderBy(lt => lt.LqvBatDau)
                .ToListAsync();

            return View(lichThis);
        }

        // ===============================
        // CHI TIẾT LỊCH THI
        // ===============================
        public async Task<IActionResult> Details(int id)
        {
            int sinhVienId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)
            );

            var lichThi = await _context.LqvLichThis
                .Include(lt => lt.LqvDeThi)
                .Include(lt => lt.LqvLopHoc)
                    .ThenInclude(lh => lh.LqvDangKyLopHocs)
                .FirstOrDefaultAsync(lt =>
                    lt.LqvLichThiId == id &&
                    lt.LqvLopHoc.LqvDangKyLopHocs
                        .Any(dk => dk.LqvSinhVienId == sinhVienId)
                );

            if (lichThi == null)
                return NotFound();

            return View(lichThi);
        }

        // ===============================
        // VÀO THI
        // ===============================
        public async Task<IActionResult> VaoThi(int id)
        {
            int sinhVienId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)
            );

            var lichThi = await _context.LqvLichThis
                .Include(lt => lt.LqvDeThi)
                .Include(lt => lt.LqvLopHoc)
                    .ThenInclude(lh => lh.LqvDangKyLopHocs)
                .FirstOrDefaultAsync(lt =>
                    lt.LqvLichThiId == id &&
                    lt.LqvLopHoc.LqvDangKyLopHocs
                        .Any(dk => dk.LqvSinhVienId == sinhVienId)
                );

            if (lichThi == null)
                return NotFound();

            var now = DateTime.Now;

            if (now < lichThi.LqvBatDau)
                return BadRequest("Chưa đến giờ thi");

            if (now > lichThi.LqvKetThuc)
                return BadRequest("Đã hết giờ thi");

            // 👉 Điều hướng sang bài làm
            return RedirectToAction(
                 "Start",
                 "LqvLamBai",
                 new { area = "Student", lichThiId = id }
             );

        }
    }
}
