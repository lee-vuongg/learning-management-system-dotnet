using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using LQV_BlockchainCertificate.Models.DBModel;
using LQV_BlockchainCertificate.Services;

namespace LQV_BlockchainCertificate.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    public class LqvNopBaiTapsController : Controller
    {
        private readonly LqvDbContext _context;
        private readonly ILqvTienDoHocTapService _tienDoService;

        public LqvNopBaiTapsController(
            LqvDbContext context,
            ILqvTienDoHocTapService tienDoService)
        {
            _context = context;
            _tienDoService = tienDoService;
        }

        // =====================================================
        // VIEW MODEL (ĐẶT CHUNG TRONG CONTROLLER)
        // =====================================================
        public class LqvChamBaiViewModel
        {
            public LqvBaiTap BaiTap { get; set; }

            public List<LqvNopBaiTap> DanhSachNop { get; set; }

            public LqvNopBaiTap? BaiDangXem { get; set; }
        }

        // ================== 1. DANH SÁCH BÀI TẬP ==================
        public async Task<IActionResult> Index()
        {
            var data = await _context.LqvBaiTaps
                .Include(b => b.LqvLopHoc)
                .Include(b => b.LqvNopBaiTaps)
                .OrderByDescending(b => b.LqvHanNop)
                .ToListAsync();

            var groupByLop = data
                .GroupBy(b => b.LqvLopHoc)
                .ToList();

            return View(groupByLop);
        }


        // ================== 2. CLASSROOM ==================
        // DS SINH VIÊN NỘP + CHI TIẾT + FORM CHẤM
        public async Task<IActionResult> DanhSachNop(int baiTapId, int? nopBaiId)
        {
            // Lấy bài tập
            var baiTap = await _context.LqvBaiTaps
                .FirstOrDefaultAsync(x => x.LqvBaiTapId == baiTapId);

            if (baiTap == null)
                return NotFound();

            // Danh sách sinh viên đã nộp
            var dsNop = await _context.LqvNopBaiTaps
                .Include(x => x.LqvSinhVien)
                .Where(x => x.LqvBaiTapId == baiTapId)
                .OrderByDescending(x => x.LqvThoiGianNop)
                .ToListAsync();

            // Bài đang xem (nếu có)
            LqvNopBaiTap? baiDangXem = null;
            if (nopBaiId.HasValue)
            {
                baiDangXem = await _context.LqvNopBaiTaps
                    .Include(x => x.LqvSinhVien)
                    .Include(x => x.LqvBaiTap)
                    .FirstOrDefaultAsync(x => x.LqvId == nopBaiId);
            }

            var vm = new LqvChamBaiViewModel
            {
                BaiTap = baiTap,
                DanhSachNop = dsNop,
                BaiDangXem = baiDangXem
            };

            return View(vm);
        }

        // ================== 3. LƯU ĐIỂM ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChamDiem(int id, double diem, string nhanXet)
        {
            var nopBai = await _context.LqvNopBaiTaps
                .Include(x => x.LqvBaiTap)
                    .ThenInclude(bt => bt.LqvLopHoc)
                .FirstOrDefaultAsync(x => x.LqvId == id);

            if (nopBai == null)
                return NotFound();

            // Cập nhật điểm
            nopBai.LqvDiem = diem;
            nopBai.LqvNhanXet = nhanXet;
            nopBai.LqvDaCham = true;

            await _context.SaveChangesAsync();

            // 🔥 Cập nhật tiến độ học tập
            await _tienDoService.CapNhatTienDoHocTapAsync(
                nopBai.LqvSinhVienId,
                nopBai.LqvBaiTap.LqvLopHoc.LqvKhoaHocId
            );

            // Quay lại classroom, giữ đúng bài đang chấm
            return RedirectToAction(nameof(DanhSachNop), new
            {
                baiTapId = nopBai.LqvBaiTapId,
                nopBaiId = nopBai.LqvId
            });
        }
    }
}
