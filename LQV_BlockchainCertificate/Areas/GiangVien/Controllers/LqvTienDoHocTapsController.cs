using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LQV_BlockchainCertificate.Models.DBModel;

namespace LQV_BlockchainCertificate.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    [Authorize(Roles = "GiangVien")]
    public class LqvTienDoHocTapsController : Controller
    {
        private readonly LqvDbContext _context;

        public LqvTienDoHocTapsController(LqvDbContext context)
        {
            _context = context;
        }

        // ==============================
        // 🔑 HELPER
        // ==============================
        private int GetGiangVienId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        // ==============================
        // 📌 INDEX – CHỈ XEM TIẾN ĐỘ SINH VIÊN
        // ==============================
        public async Task<IActionResult> Index()
        {
            int giangVienId = GetGiangVienId();

            var tienDos = await _context.LqvTienDoHocTaps
                .Include(x => x.LqvSinhVien)
                .Include(x => x.LqvKhoaHoc)
                .Where(x =>
                    _context.LqvDangKyLopHocs.Any(dk =>
                        dk.LqvSinhVienId == x.LqvSinhVienId &&
                        dk.LqvLopHoc.LqvGiangVienId == giangVienId
                    )
                )
                .OrderByDescending(x => x.LqvNgayCapNhat)
                .ToListAsync();

            return View(tienDos);
        }

        // ==============================
        // 📌 DETAILS – CHI TIẾT TIẾN ĐỘ 1 SINH VIÊN
        // ==============================
        public async Task<IActionResult> Details(int id)
        {
            int giangVienId = GetGiangVienId();

            var tienDo = await _context.LqvTienDoHocTaps
                .Include(x => x.LqvSinhVien)
                .Include(x => x.LqvKhoaHoc)
                .FirstOrDefaultAsync(x =>
                    x.LqvId == id &&
                    _context.LqvDangKyLopHocs.Any(dk =>
                        dk.LqvSinhVienId == x.LqvSinhVienId &&
                        dk.LqvLopHoc.LqvGiangVienId == giangVienId
                    )
                );

            if (tienDo == null)
                return NotFound();

            return View(tienDo);
        }

        // ==============================
        // ❌ CREATE – KHÔNG DÙNG
        // ==============================
        public IActionResult Create()
        {
            return Forbid();
        }

        // ==============================
        // ❌ EDIT – KHÔNG DÙNG
        // ==============================
        public IActionResult Edit(int id)
        {
            return Forbid();
        }

        // ==============================
        // ❌ DELETE – KHÔNG DÙNG
        // ==============================
        public IActionResult Delete(int id)
        {
            return Forbid();
        }
    }
}
