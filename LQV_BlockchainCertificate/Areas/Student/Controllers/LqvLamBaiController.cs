using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LQV_BlockchainCertificate.Models.DBModel;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using LQV_BlockchainCertificate.Hubs; // đúng namespace Hub của bạn

namespace LQV_BlockchainCertificate.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "SinhVien")]
    public class LqvLamBaiController : Controller
    {
        private readonly LqvDbContext _context;
        private readonly IHubContext<ProctorHub> _hubContext;

        public LqvLamBaiController(LqvDbContext context, IHubContext<ProctorHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // ===============================
        // DANH SÁCH LỊCH THI
        // ===============================
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
                    lt.LqvLopHoc.LqvDangKyLopHocs
                        .Any(dk => dk.LqvSinhVienId == sinhVienId)
                    && lt.LqvBatDau <= now
                    && lt.LqvKetThuc >= now
                )
                .OrderBy(lt => lt.LqvBatDau)
                .AsNoTracking()
                .ToListAsync();

            return View(lichThis);
        }

        // ===============================
        // START
        // ===============================
        public async Task<IActionResult> Start(int lichThiId)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var lichThi = await _context.LqvLichThis
                .FirstOrDefaultAsync(x => x.LqvLichThiId == lichThiId);

            if (lichThi == null)
                return NotFound();

            if (DateTime.Now < lichThi.LqvBatDau ||
                DateTime.Now > lichThi.LqvKetThuc)
                return BadRequest("Chưa đến giờ thi hoặc đã hết giờ.");

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

        // ===============================
        // DO EXAM (SAFE + CHECK KHÓA)
        // ===============================
        public async Task<IActionResult> DoExam(int baiLamId)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var baiLam = await _context.LqvBaiLams
                .Include(x => x.LqvLichThi)
                    .ThenInclude(x => x.LqvDeThi)
                        .ThenInclude(x => x.LqvBoCauHoi)
                            .ThenInclude(x => x.LqvCauHois)
                                .ThenInclude(x => x.LqvDapAns)
                .FirstOrDefaultAsync(x =>
                    x.LqvBaiLamId == baiLamId &&
                    x.LqvUserId == userId);

            if (baiLam == null)
                return NotFound();

            // 🔥 Nếu bị khóa do gian lận
            if (baiLam.LqvTrangThai == "KhoaDoGianLan")
                return RedirectToAction("Result", new { id = baiLamId });

            if (baiLam.LqvTrangThai != "DangLam")
                return RedirectToAction("Result", new { id = baiLamId });

            if (!baiLam.LqvThoiGianBatDau.HasValue)
                return RedirectToAction("Result", new { id = baiLamId });

            int thoiGianThi = baiLam.LqvLichThi.LqvDeThi.LqvThoiGianThi;

            DateTime hetHan =
                baiLam.LqvThoiGianBatDau.Value.AddMinutes(thoiGianThi);

            if (DateTime.Now >= hetHan)
                return RedirectToAction("AutoSubmit", new { baiLamId });

            int conLai = (int)(hetHan - DateTime.Now).TotalSeconds;
            ViewBag.ThoiGianConLai = conLai > 0 ? conLai : 0;

            return View(baiLam);
        }

        // ===============================
        // AUTO SUBMIT
        // ===============================
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
        // ===============================
        // SUBMIT BÀI
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(
    int baiLamId,
    Dictionary<int, int> answers,
    Dictionary<int, string> tuLuan)
        {
            var baiLam = await _context.LqvBaiLams
                .Include(b => b.LqvLichThi)
                    .ThenInclude(l => l.LqvDeThi)
                        .ThenInclude(d => d.LqvBoCauHoi)
                            .ThenInclude(bc => bc.LqvCauHois)
                                .ThenInclude(ch => ch.LqvDapAns)
                .FirstOrDefaultAsync(b => b.LqvBaiLamId == baiLamId);

            if (baiLam == null)
                return NotFound();

            if (baiLam.LqvTrangThai != "DangLam")
                return RedirectToAction("Result", new { id = baiLamId });

            double tongDiem = 0;

            var cauHois = baiLam.LqvLichThi.LqvDeThi.LqvBoCauHoi.LqvCauHois;

            foreach (var ch in cauHois)
            {
                int? dapAnId = null;
                string? traLoiTuLuan = null;
                double diem = 0;

                // ================= TRẮC NGHIỆM =================
                if (ch.LqvLoai == "TracNghiem")
                {
                    if (answers != null && answers.ContainsKey(ch.LqvCauHoiId))
                    {
                        dapAnId = answers[ch.LqvCauHoiId];

                        var dapAnDung = ch.LqvDapAns.FirstOrDefault(d => d.LqvDung);

                        if (dapAnDung != null &&
                            dapAnId == dapAnDung.LqvDapAnId)
                        {
                            diem = ch.LqvDiem;
                            tongDiem += diem;
                        }
                    }
                }

                // ================= TỰ LUẬN =================
                else
                {
                    if (tuLuan != null && tuLuan.ContainsKey(ch.LqvCauHoiId))
                    {
                        traLoiTuLuan = tuLuan[ch.LqvCauHoiId];
                    }
                }

                // ================= LƯU CHI TIẾT =================
                var chiTiet = new LqvChiTietBaiLam
                {
                    LqvBaiLamId = baiLamId,
                    LqvCauHoiId = ch.LqvCauHoiId,
                    LqvDapAnId = dapAnId,
                    LqvTraLoiTuLuan = traLoiTuLuan,
                    LqvDiem = diem,
                    LqvDaCham = ch.LqvLoai == "TracNghiem"
                };

                _context.LqvChiTietBaiLams.Add(chiTiet);
            }

            baiLam.LqvTrangThai = "DaNop";
            baiLam.LqvThoiGianNop = DateTime.Now;
            baiLam.LqvDiem = tongDiem;

            await _context.SaveChangesAsync();

            return RedirectToAction("Result", new { id = baiLamId });
        }
        // ===============================
        // RESULT
        // ===============================
        public async Task<IActionResult> Result(int id)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var baiLam = await _context.LqvBaiLams
                .Include(b => b.LqvChiTietBaiLams)
                    .ThenInclude(ct => ct.LqvCauHoi)
                .FirstOrDefaultAsync(b =>
                    b.LqvBaiLamId == id &&
                    b.LqvUserId == userId);

            if (baiLam == null)
                return NotFound();

            return View(baiLam);
        }
        // ======================================================
        // API BÁO VI PHẠM TỪ CLIENT AI
        // ======================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReportViolation(
            [FromBody] ViolationRequest model)
        {
            Console.WriteLine("===== REPORT VIOLATION CALLED =====");
            Console.WriteLine("BaiLamId: " + model.BaiLamId);
            Console.WriteLine("LoaiViPham: " + model.LoaiViPham);
            Console.WriteLine("DiemRisk: " + model.DiemRisk);
            Console.WriteLine("Time: " + DateTime.Now);
            Console.WriteLine("====================================");

            var baiLam = await _context.LqvBaiLams
                .FirstOrDefaultAsync(x => x.LqvBaiLamId == model.BaiLamId);

            if (baiLam == null)
            {
                Console.WriteLine("❌ BaiLam NOT FOUND!");
                return NotFound();
            }

            Console.WriteLine("TrangThai hiện tại: " + baiLam.LqvTrangThai);

            // ===================== LƯU ẢNH =====================
            if (!string.IsNullOrEmpty(model.DuongDanAnh))
            {
                var nhatKyAnh = new Lqv_NhatKyHinhAnhThi
                {
                    Lqv_BaiLamId = model.BaiLamId,
                    Lqv_DuongDanAnh = model.DuongDanAnh,
                    Lqv_KetQuaAI = model.LoaiViPham,
                    Lqv_ThoiGian = DateTime.Now
                };

                _context.Add(nhatKyAnh);

                Console.WriteLine("📸 Ảnh đã được thêm vào DB (chưa SaveChanges)");
            }

            // ===================== LƯU VI PHẠM =====================
            var viPham = new Lqv_NhatKyViPhamThi
            {
                Lqv_BaiLamId = model.BaiLamId,
                Lqv_LoaiViPham = model.LoaiViPham,
                Lqv_DiemRisk = model.DiemRisk,
                Lqv_MoTa = "Phát hiện bởi AI Client",
                Lqv_ThoiGian = DateTime.Now
            };

            _context.Add(viPham);

            Console.WriteLine("🚨 Vi phạm đã được thêm vào DB (chưa SaveChanges)");

            // ===================== TÍNH TỔNG RISK =====================
            var tongRiskTruoc = await _context.Set<Lqv_NhatKyViPhamThi>()
                .Where(x => x.Lqv_BaiLamId == model.BaiLamId)
                .SumAsync(x => (int?)x.Lqv_DiemRisk) ?? 0;

            Console.WriteLine("Tổng Risk TRƯỚC khi cộng: " + tongRiskTruoc);

            var tongRiskSau = tongRiskTruoc + model.DiemRisk;

            Console.WriteLine("Tổng Risk SAU khi cộng: " + tongRiskSau);

            // ===================== KIỂM TRA KHÓA =====================
            if (tongRiskSau >= 150)
            {
                Console.WriteLine("⛔ ĐỦ 150 RISK → KHÓA BÀI!");

                baiLam.LqvTrangThai = "KhoaDoGianLan";
                baiLam.LqvThoiGianNop = DateTime.Now;
            }
            else
            {
                Console.WriteLine("✔ Chưa đủ 150 Risk → Chưa khóa");
            }

            await _context.SaveChangesAsync();
            // 🔥 GỬI REALTIME CHO GIÁO VIÊN
            var examRoom = "exam_" + baiLam.LqvLichThiId;

            await _hubContext.Clients.Group(examRoom)
                .SendAsync("ReceiveViolation", new
                {
                    userId = baiLam.LqvUserId,
                    message = model.LoaiViPham,
                    riskScore = tongRiskSau
                });

            Console.WriteLine("📡 Sent ReceiveViolation to room: " + examRoom);
            Console.WriteLine("💾 SaveChanges DONE");
            Console.WriteLine("TrangThai sau cùng: " + baiLam.LqvTrangThai);
            Console.WriteLine("====================================");

            return Ok(new
            {
                tongRisk = tongRiskSau,
                biKhoa = baiLam.LqvTrangThai == "KhoaDoGianLan"
            });
        }
    }
}