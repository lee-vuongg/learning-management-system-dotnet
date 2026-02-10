using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LQV_BlockchainCertificate.Models.DBModel;
using LQV_BlockchainCertificate.Models.ViewModels;

namespace LQV_BlockchainCertificate.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "SinhVien")]
    public class LqvBaiLamsController : Controller
    {
        private readonly LqvDbContext _context;

        public LqvBaiLamsController(LqvDbContext context)
        {
            _context = context;
        }

        private int GetSinhVienId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        // =========================
        // 📌 INDEX
        // =========================
        public async Task<IActionResult> Index()
        {
            int sinhVienId = GetSinhVienId();

            var baiLams = await _context.LqvBaiLams
                .Include(b => b.LqvLichThi)
                    .ThenInclude(lt => lt.LqvLopHoc)
                .Include(b => b.LqvLichThi)
                    .ThenInclude(lt => lt.LqvDeThi)
                .Where(b => b.LqvUserId == sinhVienId)
                .OrderByDescending(b => b.LqvThoiGianNop)
                .AsNoTracking()
                .ToListAsync();

            var result = baiLams
                .GroupBy(b => new
                {
                    b.LqvLichThi.LqvLopHoc.LqvLopHocId,
                    b.LqvLichThi.LqvLopHoc.LqvTenLop
                })
                .Select(lop => new BaiLamGroupViewModel
                {
                    LopId = lop.Key.LqvLopHocId,
                    TenLop = lop.Key.LqvTenLop,
                    DeThis = lop
                        .GroupBy(b => new
                        {
                            b.LqvLichThi.LqvDeThi.LqvDeThiId,
                            b.LqvLichThi.LqvDeThi.LqvTenDeThi
                        })
                        .Select(de => new DeThiGroupViewModel
                        {
                            DeThiId = de.Key.LqvDeThiId,
                            TenDeThi = de.Key.LqvTenDeThi,
                            BaiLams = de.ToList()
                        })
                        .ToList()
                })
                .ToList();

            return View(result);
        }

        // =========================
        // 📌 DETAILS
        // =========================
        public async Task<IActionResult> Details(int id)
        {
            int sinhVienId = GetSinhVienId();

            var baiLam = await _context.LqvBaiLams
                .Include(b => b.LqvLichThi)
                    .ThenInclude(lt => lt.LqvDeThi)
                .Include(b => b.LqvLichThi)
                    .ThenInclude(lt => lt.LqvLopHoc)
                .Include(b => b.LqvChiTietBaiLams)
                    .ThenInclude(ct => ct.LqvCauHoi)
                .Include(b => b.LqvChiTietBaiLams)
                    .ThenInclude(ct => ct.LqvDapAn)
                .FirstOrDefaultAsync(b =>
                    b.LqvBaiLamId == id &&
                    b.LqvUserId == sinhVienId
                );

            if (baiLam == null)
                return NotFound();

            return View(baiLam);
        }
    }
}
