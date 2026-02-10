using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LQV_BlockchainCertificate.Models.DBModel;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LQV_BlockchainCertificate.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    public class LqvChiTietBaiLamsController : Controller
    {
        private readonly LqvDbContext _context;

        public LqvChiTietBaiLamsController(LqvDbContext context)
        {
            _context = context;
        }

        // =========================================
        // XEM CHI TIẾT BÀI LÀM (THEO BÀI LÀM)
        // GiangVien/LqvChiTietBaiLams/ByBaiLam/5
        // =========================================
        public async Task<IActionResult> ByBaiLam(int baiLamId)
        {
            var baiLam = await _context.LqvBaiLams
                .Include(bl => bl.LqvNguoiDung)
                .Include(bl => bl.LqvLichThi)
                    .ThenInclude(lt => lt.LqvLopHoc)
                .Include(bl => bl.LqvChiTietBaiLams)
                    .ThenInclude(ct => ct.LqvCauHoi)
                .Include(bl => bl.LqvChiTietBaiLams)
                    .ThenInclude(ct => ct.LqvDapAn)
                .FirstOrDefaultAsync(bl => bl.LqvBaiLamId == baiLamId);

            if (baiLam == null)
                return NotFound();

            return View(baiLam);
        }

        // =========================================
        // GET: CHẤM 1 CÂU TỰ LUẬN
        // =========================================
        public async Task<IActionResult> ChamCauHoi(int id)
        {
            var chiTiet = await _context.LqvChiTietBaiLams
                .Include(ct => ct.LqvCauHoi)
                .Include(ct => ct.LqvBaiLam)
                .FirstOrDefaultAsync(ct => ct.LqvId == id);

            if (chiTiet == null)
                return NotFound();

            // Không cho chấm trắc nghiệm
            if (chiTiet.LqvCauHoi.LqvLoai == "TracNghiem")
                return BadRequest("Câu trắc nghiệm được chấm tự động");

            // Nếu bài đã chấm xong → khóa
            if (chiTiet.LqvBaiLam.LqvTrangThai == "DaCham")
                return BadRequest("Bài làm đã được chấm");

            return View(chiTiet);
        }

        // =========================================
        // POST: LƯU ĐIỂM 1 CÂU TỰ LUẬN
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChamCauHoi(int id, double diem)
        {
            var chiTiet = await _context.LqvChiTietBaiLams
                .Include(ct => ct.LqvCauHoi)
                .Include(ct => ct.LqvBaiLam)
                .FirstOrDefaultAsync(ct => ct.LqvId == id);

            if (chiTiet == null)
                return NotFound();

            if (chiTiet.LqvBaiLam.LqvTrangThai == "DaCham")
                return BadRequest("Không thể chấm lại bài đã hoàn tất");

            // Giới hạn điểm theo điểm tối đa của câu hỏi
            var diemToiDa = chiTiet.LqvCauHoi.LqvDiem;
            if (diem < 0) diem = 0;
            if (diem > diemToiDa) diem = diemToiDa;

            chiTiet.LqvDiem = Math.Round(diem, 2);
            chiTiet.LqvDaCham = true;

            await _context.SaveChangesAsync();

            // Quay về chi tiết bài làm (BaiLamController)
            return RedirectToAction(
                "Details",
                "LqvBaiLams",
                new { area = "GiangVien", id = chiTiet.LqvBaiLamId }
            );
        }
    }
}
