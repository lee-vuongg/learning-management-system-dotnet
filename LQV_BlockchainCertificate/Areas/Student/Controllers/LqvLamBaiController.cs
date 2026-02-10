using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LQV_BlockchainCertificate.Models.DBModel;
using System.Security.Claims;

namespace LQV_BlockchainCertificate.Areas.Student.Controllers
{
    [Area("Student")]
    public class LqvLamBaiController : Controller
    {
        private readonly LqvDbContext _context;

        public LqvLamBaiController(LqvDbContext context)
        {
            _context = context;
        }

        // =========================================
        // 1. DANH SÁCH LỊCH THI
        // =========================================
        public async Task<IActionResult> Index()
        {
            int sinhVienId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)
            );

            var now = DateTime.Now;

            var lichThis = await _context.LqvLichThis
                .Include(lt => lt.LqvDeThi)
                .Include(lt => lt.LqvLopHoc)
                    .ThenInclude(lh => lh.LqvDangKyLopHocs)
                .Where(lt =>
                    // ✅ SINH VIÊN ĐÃ ĐĂNG KÝ LỚP
                    lt.LqvLopHoc.LqvDangKyLopHocs
                        .Any(dk => dk.LqvSinhVienId == sinhVienId)

                    // ✅ LỊCH THI ĐANG DIỄN RA
                    && lt.LqvBatDau <= now
                    && lt.LqvKetThuc >= now
                )
                .OrderBy(lt => lt.LqvBatDau)
                .AsNoTracking()
                .ToListAsync();

            return View(lichThis);
        }


        // =========================================
        // 2. START – TẠO / LẤY BÀI LÀM
        // =========================================
        public async Task<IActionResult> Start(int lichThiId)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var baiLam = await _context.LqvBaiLams
                .FirstOrDefaultAsync(x =>
                    x.LqvLichThiId == lichThiId &&
                    x.LqvUserId == userId
                );

            if (baiLam == null)
            {
                baiLam = new LqvBaiLam
                {
                    LqvLichThiId = lichThiId,
                    LqvUserId = userId,
                    LqvThoiGianBatDau = DateTime.Now,
                    LqvTrangThai = "DangLam"
                };

                _context.LqvBaiLams.Add(baiLam);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("DoExam", new { baiLamId = baiLam.LqvBaiLamId });
        }

        // =========================================
        // 3. LÀM BÀI + RANDOM CÂU HỎI & ĐÁP ÁN
        // =========================================
        public async Task<IActionResult> DoExam(int baiLamId)
        {
            var baiLam = await _context.LqvBaiLams
                .Include(x => x.LqvLichThi)
                    .ThenInclude(x => x.LqvDeThi)
                        .ThenInclude(x => x.LqvBoCauHoi)
                            .ThenInclude(x => x.LqvCauHois)
                                .ThenInclude(x => x.LqvDapAns)
                .FirstOrDefaultAsync(x => x.LqvBaiLamId == baiLamId);

            if (baiLam == null)
                return NotFound();

            if (baiLam.LqvTrangThai != "DangLam")
                return RedirectToAction("Result", new { id = baiLamId });

            // ⏱ THỜI GIAN CÒN LẠI
            int thoiGianThi = baiLam.LqvLichThi.LqvDeThi.LqvThoiGianThi;
            DateTime hetHan = baiLam.LqvThoiGianBatDau!.Value.AddMinutes(thoiGianThi);

            if (DateTime.Now >= hetHan)
                return RedirectToAction("AutoSubmit", new { baiLamId });

            ViewBag.ThoiGianConLai =
                (int)(hetHan - DateTime.Now).TotalSeconds;

            // =============================
            // 🔥 RANDOM CÂU HỎI
            // =============================
            var rnd = new Random();

            baiLam.LqvLichThi.LqvDeThi.LqvBoCauHoi.LqvCauHois =
                baiLam.LqvLichThi.LqvDeThi.LqvBoCauHoi.LqvCauHois
                    .OrderBy(x => rnd.Next())
                    .ToList();

            // =============================
            // 🔥 RANDOM ĐÁP ÁN TỪNG CÂU
            // =============================
            foreach (var cauHoi in baiLam.LqvLichThi.LqvDeThi.LqvBoCauHoi.LqvCauHois)
            {
                cauHoi.LqvDapAns = cauHoi.LqvDapAns
                    .OrderBy(x => rnd.Next())
                    .ToList();
            }

            return View(baiLam);
        }

        // =========================================
        // 4. NỘP BÀI
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(
            int baiLamId,
            Dictionary<int, int> answers,
            Dictionary<int, string> tuLuan)
        {
            var baiLam = await _context.LqvBaiLams
                .Include(x => x.LqvLichThi)
                    .ThenInclude(x => x.LqvDeThi)
                .FirstOrDefaultAsync(x => x.LqvBaiLamId == baiLamId);

            if (baiLam == null || baiLam.LqvTrangThai != "DangLam")
                return BadRequest("Bài làm không hợp lệ");

            var old = _context.LqvChiTietBaiLams
                .Where(x => x.LqvBaiLamId == baiLamId);
            _context.LqvChiTietBaiLams.RemoveRange(old);

            double tongDiem = 0;

            // TRẮC NGHIỆM
            if (answers != null)
            {
                foreach (var item in answers)
                {
                    var cauHoi = await _context.LqvCauHois
                        .Include(x => x.LqvDapAns)
                        .FirstAsync(x => x.LqvCauHoiId == item.Key);

                    var dapAnDung = cauHoi.LqvDapAns.FirstOrDefault(x => x.LqvDung);

                    double diem =
                        dapAnDung != null && dapAnDung.LqvDapAnId == item.Value
                        ? cauHoi.LqvDiem
                        : 0;

                    tongDiem += diem;

                    _context.LqvChiTietBaiLams.Add(new LqvChiTietBaiLam
                    {
                        LqvBaiLamId = baiLamId,
                        LqvCauHoiId = item.Key,
                        LqvDapAnId = item.Value,
                        LqvDiem = diem,
                        LqvDaCham = true
                    });
                }
            }

            // TỰ LUẬN
            if (tuLuan != null)
            {
                foreach (var item in tuLuan)
                {
                    _context.LqvChiTietBaiLams.Add(new LqvChiTietBaiLam
                    {
                        LqvBaiLamId = baiLamId,
                        LqvCauHoiId = item.Key,
                        LqvTraLoiTuLuan = item.Value,
                        LqvDaCham = false
                    });
                }
            }

            baiLam.LqvDiem = tongDiem;
            baiLam.LqvTrangThai = "DaNop";
            baiLam.LqvThoiGianNop = DateTime.Now;

            await _context.SaveChangesAsync();
            return RedirectToAction("Result", new { id = baiLamId });
        }

        // =========================================
        // AUTO SUBMIT
        // =========================================
        public async Task<IActionResult> AutoSubmit(int baiLamId)
        {
            var baiLam = await _context.LqvBaiLams
                .FirstOrDefaultAsync(b => b.LqvBaiLamId == baiLamId);

            if (baiLam == null)
                return NotFound();

            baiLam.LqvTrangThai = "DaNop";
            baiLam.LqvThoiGianNop = DateTime.Now;

            await _context.SaveChangesAsync();
            return RedirectToAction("Result", new { id = baiLamId });
        }

        // =========================================
        // KẾT QUẢ
        // =========================================
        public async Task<IActionResult> Result(int id)
        {
            var baiLam = await _context.LqvBaiLams
                .Include(x => x.LqvChiTietBaiLams)
                    .ThenInclude(x => x.LqvCauHoi)
                .FirstOrDefaultAsync(x => x.LqvBaiLamId == id);

            return View(baiLam);
        }
    }
}
