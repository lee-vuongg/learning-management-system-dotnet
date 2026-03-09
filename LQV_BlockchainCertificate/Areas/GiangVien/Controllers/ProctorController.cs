using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LQV_BlockchainCertificate.Models.DBModel;
using LQV_BlockchainCertificate.Models.ViewModels;

namespace LQV_BlockchainCertificate.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    public class ProctorController : Controller
    {
        private readonly LqvDbContext _context;

        public ProctorController(LqvDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // LIVE MONITOR
        // =====================================================
        public async Task<IActionResult> Monitor(int lichThiId)
        {
            Console.WriteLine("==== MONITOR ACTION CALLED ====");
            Console.WriteLine("LichThiId: " + lichThiId);

            var lichThi = await _context.LqvLichThis
                .Include(x => x.LqvDeThi)
                .Include(x => x.LqvLopHoc)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.LqvLichThiId == lichThiId);

            if (lichThi == null)
            {
                Console.WriteLine("LichThi NOT FOUND");
                return NotFound();
            }

            Console.WriteLine("TenDeThi: " + lichThi.LqvDeThi?.LqvTenDeThi);
            Console.WriteLine("TenLop: " + lichThi.LqvLopHoc?.LqvTenLop);

            var students = await (
                from b in _context.LqvBaiLams.AsNoTracking()
                where b.LqvLichThiId == lichThiId &&
                      (b.LqvTrangThai == "DangLam" ||
                       b.LqvTrangThai == "KhoaDoGianLan")

                join v in _context.Lqv_NhatKyViPhamThi.AsNoTracking()
                    on b.LqvBaiLamId equals v.Lqv_BaiLamId into violationGroup

                from vg in violationGroup.DefaultIfEmpty()

                group vg by new
                {
                    b.LqvBaiLamId,
                    b.LqvUserId
                } into g

                select new StudentExamVM
                {
                    BaiLamId = g.Key.LqvBaiLamId,
                    UserId = g.Key.LqvUserId,
                    TongRisk = g.Sum(x => x != null ? x.Lqv_DiemRisk : 0)
                }

            ).ToListAsync();

            Console.WriteLine("Total Students Loaded: " + students.Count);

            foreach (var s in students)
            {
                Console.WriteLine($"Student: {s.UserId} | BaiLamId: {s.BaiLamId} | Risk: {s.TongRisk}");
            }

            var vm = new ProctorMonitorVM
            {
                LichThiId = lichThiId,
                Students = students
            };

            ViewBag.TenDeThi = lichThi.LqvDeThi?.LqvTenDeThi ?? "";
            ViewBag.TenLop = lichThi.LqvLopHoc?.LqvTenLop ?? "";

            Console.WriteLine("==== MONITOR VIEW RETURNED ====");

            return View(vm);
        }

        // =====================================================
        // REVIEW
        // =====================================================
        public async Task<IActionResult> Review(int lichThiId)
        {
            Console.WriteLine("==== REVIEW ACTION CALLED ====");
            Console.WriteLine("LichThiId: " + lichThiId);

            var baiLams = await _context.LqvBaiLams
                .Where(x => x.LqvLichThiId == lichThiId)
                .ToListAsync();

            Console.WriteLine("Total BaiLams: " + baiLams.Count);

            var baiLamIds = baiLams.Select(x => x.LqvBaiLamId).ToList();

            var allViolations = await _context.Lqv_NhatKyViPhamThi
                .Where(x => baiLamIds.Contains(x.Lqv_BaiLamId))
                .OrderBy(x => x.Lqv_ThoiGian)
                .ToListAsync();

            Console.WriteLine("Total Violations: " + allViolations.Count);

            var allImages = await _context.Lqv_NhatKyHinhAnhThi
                .Where(x => baiLamIds.Contains(x.Lqv_BaiLamId))
                .ToListAsync();

            Console.WriteLine("Total Images: " + allImages.Count);

            var students = new List<StudentExamVM>();

            foreach (var bai in baiLams)
            {
                var violations = allViolations
                    .Where(x => x.Lqv_BaiLamId == bai.LqvBaiLamId)
                    .OrderBy(x => x.Lqv_ThoiGian)
                    .ToList();

                var images = allImages
                    .Where(x => x.Lqv_BaiLamId == bai.LqvBaiLamId)
                    .ToList();

                var studentVM = new StudentExamVM
                {
                    BaiLamId = bai.LqvBaiLamId,
                    UserId = bai.LqvUserId,
                    TongRisk = violations.Sum(x => x.Lqv_DiemRisk),
                    Violations = violations,
                    Images = images
                };

                students.Add(studentVM);
            }

            Console.WriteLine("==== REVIEW VIEW RETURNED ====");

            var vm = new ProctorMonitorVM
            {
                LichThiId = lichThiId,
                Students = students
            };

            return View(vm);
        }
    }
}