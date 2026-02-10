using LQV_BlockchainCertificate.Models.DBModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace LQV_BlockchainCertificate.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    public class LqvBuoiHocsController : Controller
    {
        private readonly LqvDbContext _context;

        public LqvBuoiHocsController(LqvDbContext context)
        {
            _context = context;
            Console.WriteLine("🔥 [INIT] LqvBuoiHocsController khởi tạo");
        }

        // ================== INDEX ==================
        public async Task<IActionResult> Index()
        {
            Console.WriteLine("📄 [INDEX] Load danh sách buổi học");

            var data = await _context.LqvBuoiHocs
                .OrderByDescending(x => x.LqvNgayHoc)
                .Include(b => b.LqvLopHoc)
                .ToListAsync();

            Console.WriteLine($"📊 [INDEX] Tổng buổi học: {data.Count}");
            return View(data);
        }

        // ================== DETAILS ==================
        public async Task<IActionResult> Details(int? id)
        {
            Console.WriteLine($"🔍 [DETAILS] id = {id}");

            if (id == null)
            {
                Console.WriteLine("❌ [DETAILS] id null");
                return NotFound();
            }

            var buoiHoc = await _context.LqvBuoiHocs
                .Include(b => b.LqvLopHoc)
                .FirstOrDefaultAsync(m => m.LqvBuoiHocId == id);

            if (buoiHoc == null)
            {
                Console.WriteLine($"❌ [DETAILS] Không tìm thấy buổi học id={id}");
                return NotFound();
            }

            Console.WriteLine($"✅ [DETAILS] Buổi học: {buoiHoc.LqvBuoiHocId}");
            return View(buoiHoc);
        }

        // ================== CREATE ==================
        public IActionResult Create()
        {
            Console.WriteLine("➕ [CREATE-GET] Load form tạo buổi học");
            LoadLopHoc();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LqvBuoiHoc model)
        {
            Console.WriteLine("💾 [CREATE-POST] Nhận dữ liệu tạo buổi học");

            Console.WriteLine($"📍 GPS RAW: Lat={model.LqvViDo}, Lng={model.LqvKinhDo}, Radius={model.LqvBanKinh}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ [CREATE] ModelState INVALID");
                foreach (var err in ModelState)
                {
                    Console.WriteLine($"⚠️ {err.Key} => {string.Join(",", err.Value.Errors.Select(e => e.ErrorMessage))}");
                }
                LoadLopHoc();
                return View(model);
            }

            model.LqvDangMo = false;

            model.LqvViDo = NormalizeGps(model.LqvViDo);
            model.LqvKinhDo = NormalizeGps(model.LqvKinhDo);

            Console.WriteLine($"📍 GPS SAU NORMALIZE: Lat={model.LqvViDo}, Lng={model.LqvKinhDo}");

            _context.LqvBuoiHocs.Add(model);
            await _context.SaveChangesAsync();

            Console.WriteLine($"✅ [CREATE] Đã lưu buổi học ID={model.LqvBuoiHocId}");
            return RedirectToAction(nameof(Index));
        }

        // ================== EDIT ==================
        public async Task<IActionResult> Edit(int? id)
        {
            Console.WriteLine($"✏️ [EDIT-GET] id={id}");

            if (id == null) return NotFound();

            var buoiHoc = await _context.LqvBuoiHocs.FindAsync(id);
            if (buoiHoc == null)
            {
                Console.WriteLine("❌ [EDIT-GET] Không tìm thấy buổi học");
                return NotFound();
            }

            LoadLopHoc();

            buoiHoc.LqvViDo ??= 21.0278;
            buoiHoc.LqvKinhDo ??= 105.8342;
            buoiHoc.LqvBanKinh ??= 50;

            Console.WriteLine($"📍 [EDIT-GET] GPS: {buoiHoc.LqvViDo}, {buoiHoc.LqvKinhDo}, R={buoiHoc.LqvBanKinh}");

            return View(buoiHoc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LqvBuoiHoc model)
        {
            Console.WriteLine($"✏️ [EDIT-POST] id={id}");
            Console.WriteLine($"📍 GPS FORM: Lat={model.LqvViDo}, Lng={model.LqvKinhDo}, R={model.LqvBanKinh}");

            if (id != model.LqvBuoiHocId)
            {
                Console.WriteLine("❌ [EDIT] ID không khớp");
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ [EDIT] ModelState INVALID");
                LoadLopHoc();
                return View(model);
            }

            var buoiHoc = await _context.LqvBuoiHocs.FirstOrDefaultAsync(x => x.LqvBuoiHocId == id);
            if (buoiHoc == null)
            {
                Console.WriteLine("❌ [EDIT] Không tìm thấy DB record");
                return NotFound();
            }

            buoiHoc.LqvViDo = NormalizeGps(model.LqvViDo);
            buoiHoc.LqvKinhDo = NormalizeGps(model.LqvKinhDo);
            buoiHoc.LqvBanKinh = model.LqvBanKinh;

            Console.WriteLine($"📍 GPS SAVE: {buoiHoc.LqvViDo}, {buoiHoc.LqvKinhDo}, R={buoiHoc.LqvBanKinh}");

            await _context.SaveChangesAsync();
            Console.WriteLine("✅ [EDIT] Lưu thành công");

            return RedirectToAction(nameof(Index));
        }

        // ================== MỞ / ĐÓNG ĐIỂM DANH ==================
        [HttpPost]
        public async Task<IActionResult> MoDiemDanh(int id)
        {
            Console.WriteLine($"🔓 [MO DIEM DANH] id={id}");

            var buoiHoc = await _context.LqvBuoiHocs.FindAsync(id);
            if (buoiHoc == null) return NotFound();

            buoiHoc.LqvDangMo = true;
            await _context.SaveChangesAsync();

            Console.WriteLine("✅ Điểm danh đã MỞ");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DongDiemDanh(int id)
        {
            Console.WriteLine($"🔒 [DONG DIEM DANH] id={id}");

            var buoiHoc = await _context.LqvBuoiHocs.FindAsync(id);
            if (buoiHoc == null) return NotFound();

            if (buoiHoc.LqvGioBatDau.HasValue)
            {
                DateTime thoiGianBatDau = buoiHoc.LqvNgayHoc.ToDateTime(
                    buoiHoc.LqvGioBatDau.Value
                );

                if (DateTime.Now > thoiGianBatDau.AddMinutes(15))
                {
                    Console.WriteLine("⛔ Quá 15 phút – đã tự đóng");
                    return RedirectToAction(nameof(Index));
                }
            }

            buoiHoc.LqvDangMo = false;
            await _context.SaveChangesAsync();

            Console.WriteLine("✅ Điểm danh đã ĐÓNG");
            return RedirectToAction(nameof(Index));
        }


        // ================== DS ĐIỂM DANH ==================
        public async Task<IActionResult> DiemDanh(int id)
        {
            Console.WriteLine($"📋 [DIEM DANH] Buổi học id={id}");

            var buoiHoc = await _context.LqvBuoiHocs
                .FirstOrDefaultAsync(x => x.LqvBuoiHocId == id);

            if (buoiHoc == null)
                return NotFound();

            // ====== AUTO ĐÓNG SAU 15 PHÚT ======
            if (buoiHoc.LqvGioBatDau.HasValue)
            {
                DateTime thoiGianBatDau = buoiHoc.LqvNgayHoc.ToDateTime(
                    buoiHoc.LqvGioBatDau.Value
                );

                DateTime hanDiemDanh = thoiGianBatDau.AddMinutes(15);

                if (DateTime.Now > hanDiemDanh && buoiHoc.LqvDangMo)
                {
                    Console.WriteLine("⏱ Quá 15 phút → AUTO ĐÓNG điểm danh");

                    buoiHoc.LqvDangMo = false;
                    await _context.SaveChangesAsync();
                }

                ViewBag.ConHan = DateTime.Now <= hanDiemDanh;
                ViewBag.ThoiGianConLai = (int)(hanDiemDanh - DateTime.Now).TotalSeconds;
            }

            var ds = await _context.LqvDiemDanhGps
                .Include(x => x.LqvSinhVien)
                .Where(x => x.LqvBuoiHocId == id)
                .OrderByDescending(x => x.LqvThoiGian)
                .ToListAsync();

            Console.WriteLine($"📊 Tổng lượt điểm danh: {ds.Count}");
            ViewBag.BuoiHocId = id;
            ViewBag.DangMo = buoiHoc.LqvDangMo;

            return View(ds);
        }


        // ================== HỖ TRỢ ==================
        private void LoadLopHoc()
        {
            Console.WriteLine("📚 Load danh sách lớp học");

            ViewBag.LopHocList = _context.LqvLopHocs
                .Select(l => new SelectListItem
                {
                    Value = l.LqvLopHocId.ToString(),
                    Text = l.LqvTenLop
                }).ToList();

            Console.WriteLine($"📚 Tổng lớp: {ViewBag.LopHocList.Count}");
        }

        // ================== GPS NORMALIZE ==================
        private double? NormalizeGps(double? value)
        {
            if (!value.HasValue)
            {
                Console.WriteLine("⚠️ GPS null");
                return null;
            }

            double v = value.Value;
            Console.WriteLine($"🔧 Normalize GPS RAW = {v}");

            if (Math.Abs(v) > 180)
            {
                Console.WriteLine("⚠️ GPS bị nhân -> chia lại");
                while (Math.Abs(v) > 180)
                {
                    v /= 10;
                }
            }

            v = Math.Round(v, 6);
            Console.WriteLine($"✅ GPS sau normalize = {v}");

            return v;
        }
    }
}