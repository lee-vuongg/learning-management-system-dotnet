using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LQV_BlockchainCertificate.Models.DBModel;
using LQV_BlockchainCertificate.Services;
using ClosedXML.Excel;
using System.IO;


namespace LQV_BlockchainCertificate.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    public class LqvDiemDanhGpsController : Controller
    {
        private readonly LqvDbContext _context;
        private readonly ILqvTienDoHocTapService _tienDoService;
        private readonly IWebHostEnvironment _env;

        public LqvDiemDanhGpsController(
            LqvDbContext context,
            ILqvTienDoHocTapService tienDoService,
            IWebHostEnvironment env)
        {
            _context = context;
            _tienDoService = tienDoService;
            _env = env;
        }

        // ================== GPS UTILS ==================
        private double TinhKhoangCach(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371000; // mét
            var dLat = (lat2 - lat1) * Math.PI / 180;
            var dLon = (lon2 - lon1) * Math.PI / 180;

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1 * Math.PI / 180) *
                    Math.Cos(lat2 * Math.PI / 180) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        // ================== INDEX ==================
        public async Task<IActionResult> Index(int? buoiHocId)
        {
            var query = _context.LqvDiemDanhGps
                .Include(d => d.LqvSinhVien)
                .Include(d => d.LqvBuoiHoc)
                    .ThenInclude(b => b.LqvLopHoc)
                .AsNoTracking()
                .AsQueryable();

            if (buoiHocId.HasValue)
                query = query.Where(x => x.LqvBuoiHocId == buoiHocId);

            var data = await query.ToListAsync();

            // ✅ GROUP CHUẨN – 1 BUỔI HỌC + 1 LỚP = 1 CARD DUY NHẤT
            var groupedData = data
      .GroupBy(x => new
      {
          x.LqvBuoiHoc.LqvNgayHoc,
          x.LqvBuoiHoc.LqvLopHocId
      })
      .Select(g => new LqvDiemDanhGroupVM
      {
          NgayHoc = g.Key.LqvNgayHoc,
          TenLop = g.First().LqvBuoiHoc.LqvLopHoc.LqvTenLop,
          Items = g
              .OrderBy(x => x.LqvThoiGian)
              .ToList()
      })
      .OrderByDescending(x => x.NgayHoc)
      .ToList();


            return View(groupedData);
        }

        public class LqvDiemDanhGroupVM
        {
            public int LqvBuoiHocId { get; set; }
            public DateOnly NgayHoc { get; set; }
            public string TenLop { get; set; }
            public List<LqvDiemDanhGp> Items { get; set; }
        }

        public async Task<IActionResult> Details(int id)
        {
            Console.WriteLine($"🔍 [GV][DETAILS] DiemDanhId={id}");

            var diemDanh = await _context.LqvDiemDanhGps
                .Include(x => x.LqvSinhVien)
                .Include(x => x.LqvBuoiHoc)
                    .ThenInclude(b => b.LqvLopHoc)
                .FirstOrDefaultAsync(x => x.LqvId == id);

            if (diemDanh == null)
            {
                Console.WriteLine("❌ [DETAILS] Không tìm thấy bản ghi điểm danh");
                return NotFound();
            }

            Console.WriteLine($"✅ [DETAILS] SinhVien={diemDanh.LqvSinhVien?.LqvHoTen}");
            Console.WriteLine($"📍 GPS: {diemDanh.LqvViDo}, {diemDanh.LqvKinhDo}");
            Console.WriteLine($"✔ Hợp lệ: {diemDanh.LqvHopLe}");

            return View(diemDanh);
        }

        // ================== CREATE (SV GỬI GPS) ==================
        [HttpPost]
        public async Task<IActionResult> Create(
     int LqvSinhVienId,
     int LqvBuoiHocId,
     double LqvViDo,
     double LqvKinhDo)
        {
            var buoiHoc = await _context.LqvBuoiHocs
                .FirstOrDefaultAsync(b => b.LqvBuoiHocId == LqvBuoiHocId);

            if (buoiHoc == null)
                return BadRequest("Không tìm thấy buổi học");

            // ❌ CHƯA MỞ ĐIỂM DANH
            if (!buoiHoc.LqvDangMo)
                return BadRequest("Buổi học chưa mở điểm danh");

            // ❌ CHƯA CẤU HÌNH THỜI GIAN
            if (!buoiHoc.LqvGioBatDau.HasValue || !buoiHoc.LqvGioKetThuc.HasValue)
                return BadRequest("Buổi học chưa cấu hình thời gian");

            // ❌ CHƯA CẤU HÌNH GPS
            if (!buoiHoc.LqvViDo.HasValue ||
                !buoiHoc.LqvKinhDo.HasValue ||
                !buoiHoc.LqvBanKinh.HasValue)
                return BadRequest("Buổi học chưa cấu hình GPS");

            // ⏰ THỜI GIAN
            var batDau = buoiHoc.LqvNgayHoc.ToDateTime(buoiHoc.LqvGioBatDau.Value);
            var ketThuc = buoiHoc.LqvNgayHoc.ToDateTime(buoiHoc.LqvGioKetThuc.Value);
            var now = DateTime.Now;

            if (now < batDau || now > ketThuc)
                return BadRequest("Ngoài thời gian điểm danh");

            // 📍 GPS
            var khoangCach = TinhKhoangCach(
                buoiHoc.LqvViDo.Value,
                buoiHoc.LqvKinhDo.Value,
                LqvViDo,
                LqvKinhDo
            );

            bool hopLe = khoangCach <= buoiHoc.LqvBanKinh.Value;

            var diemDanh = new LqvDiemDanhGp
            {
                LqvSinhVienId = LqvSinhVienId,
                LqvLopHocId = buoiHoc.LqvLopHocId,
                LqvBuoiHocId = buoiHoc.LqvBuoiHocId,
                LqvViDo = LqvViDo,
                LqvKinhDo = LqvKinhDo,
                LqvThoiGian = now,
                LqvHopLe = hopLe
            };

            _context.LqvDiemDanhGps.Add(diemDanh);
            await _context.SaveChangesAsync();

            // 🔥 UPDATE TIẾN ĐỘ (CHỈ KHI HỢP LỆ)
            if (hopLe)
            {
                var khoaHocId = await _context.LqvLopHocs
                    .Where(lh => lh.LqvLopHocId == buoiHoc.LqvLopHocId)
                    .Select(lh => lh.LqvKhoaHocId)
                    .FirstAsync();

                await _tienDoService.CapNhatTienDoHocTapAsync(
                    LqvSinhVienId,
                    khoaHocId
                );
            }

            return Ok(new
            {
                success = true,
                hopLe,
                khoangCach
            });
        }
        [HttpGet]
        public async Task<IActionResult> ExportExcel(int buoiHocId)
        {
            var data = await _context.LqvDiemDanhGps
                .Include(x => x.LqvSinhVien)
                .Include(x => x.LqvBuoiHoc)
                    .ThenInclude(b => b.LqvLopHoc)
                .Where(x => x.LqvBuoiHocId == buoiHocId)
                .OrderBy(x => x.LqvThoiGian)
                .ToListAsync();

            if (!data.Any())
                return BadRequest("Không có dữ liệu");

            var buoiHoc = data.First().LqvBuoiHoc;

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("DiemDanh");

            int row = 1;

            // ===== HEADER TRƯỜNG =====
            ws.Cell(row, 1).Value = "TRƯỜNG …………………………………";
            ws.Range(row, 1, row, 5).Merge().Style.Font.Bold = true;
            row++;

            ws.Cell(row, 1).Value = "KHOA / BỘ MÔN …………………………";
            ws.Range(row, 1, row, 5).Merge();
            row += 2;

            // ===== TITLE =====
            ws.Cell(row, 1).Value = "DANH SÁCH ĐIỂM DANH SINH VIÊN";
            ws.Range(row, 1, row, 5).Merge();
            ws.Row(row).Style.Font.Bold = true;
            ws.Row(row).Style.Font.FontSize = 14;
            ws.Row(row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row += 2;

            // ===== INFO =====
            ws.Cell(row, 1).Value = $"Lớp: {buoiHoc.LqvLopHoc.LqvTenLop}";
            row++;

            ws.Cell(row, 1).Value = $"Ngày học: {buoiHoc.LqvNgayHoc:dd/MM/yyyy}";
            row++;

            ws.Cell(row, 1).Value =
                $"Giờ học: {buoiHoc.LqvGioBatDau:hh\\:mm} - {buoiHoc.LqvGioKetThuc:hh\\:mm}";
            row += 2;

            // ===== TABLE HEADER =====
            ws.Cell(row, 1).Value = "STT";
            ws.Cell(row, 2).Value = "Mã SV";
            ws.Cell(row, 3).Value = "Họ và tên";
            ws.Cell(row, 4).Value = "Thời gian điểm danh";
            ws.Cell(row, 5).Value = "Kết quả";

            ws.Range(row, 1, row, 5).Style.Font.Bold = true;
            ws.Range(row, 1, row, 5).Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;
            ws.Range(row, 1, row, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Range(row, 1, row, 5).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            row++;

            // ===== DATA =====
            int stt = 1;
            foreach (var d in data)
            {
                ws.Cell(row, 1).Value = stt++;
                ws.Cell(row, 2).Value = d.LqvSinhVienId;
                ws.Cell(row, 3).Value = d.LqvSinhVien.LqvHoTen;
                ws.Cell(row, 4).Value = d.LqvThoiGian.ToString("HH:mm:ss");
                ws.Cell(row, 5).Value = d.LqvHopLe ? "Hợp lệ" : "Không hợp lệ";

                ws.Range(row, 1, row, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws.Range(row, 1, row, 5).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                row++;
            }

            row += 2;

            // ===== SIGNATURE =====
            ws.Cell(row, 4).Value = $"Ngày … tháng … năm …";
            ws.Range(row, 4, row, 5).Merge();
            row += 2;

            ws.Cell(row, 2).Value = "GIẢNG VIÊN";
            ws.Cell(row, 4).Value = "TRƯỞNG KHOA";

            ws.Range(row, 2, row, 3).Merge();
            ws.Range(row, 4, row, 5).Merge();

            ws.Range(row, 2, row, 5).Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            row += 4;

            ws.Cell(row, 2).Value = "(Ký, ghi rõ họ tên)";
            ws.Cell(row, 4).Value = "(Ký, ghi rõ họ tên)";

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            wb.SaveAs(stream);

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"DiemDanh_{buoiHoc.LqvLopHoc.LqvTenLop}_{buoiHoc.LqvNgayHoc:ddMMyyyy}.xlsx"
            );
        }

        [HttpGet]
        public async Task<IActionResult> ExportExcelByLop(int lopHocId)
        {
            var data = await _context.LqvDiemDanhGps
                .Include(x => x.LqvSinhVien)
                .Include(x => x.LqvBuoiHoc)
                    .ThenInclude(b => b.LqvLopHoc)
                .Where(x => x.LqvLopHocId == lopHocId)
                .OrderBy(x => x.LqvBuoiHoc.LqvNgayHoc)
                .ThenBy(x => x.LqvThoiGian)
                .ToListAsync();

            if (!data.Any())
                return BadRequest("Không có dữ liệu");

            var lop = data.First().LqvBuoiHoc.LqvLopHoc;
            var ngayHoc = data.First().LqvBuoiHoc.LqvNgayHoc;

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("DiemDanhTong");

            /* ================= HEADER ================= */
            ws.Cell(1, 1).Value = "DANH SÁCH ĐIỂM DANH";
            ws.Range(1, 1, 1, 6).Merge();
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 16;
            ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Cell(2, 1).Value = $"Lớp: {lop.LqvTenLop}";
            ws.Range(2, 1, 2, 3).Merge();

            ws.Cell(2, 4).Value = $"Ngày học: {ngayHoc:dd/MM/yyyy}";
            ws.Range(2, 4, 2, 6).Merge();

            /* ================= TABLE HEADER ================= */
            int headerRow = 4;

            ws.Cell(headerRow, 1).Value = "STT";
            ws.Cell(headerRow, 2).Value = "Mã SV";
            ws.Cell(headerRow, 3).Value = "Họ tên";
            ws.Cell(headerRow, 4).Value = "Ngày học";
            ws.Cell(headerRow, 5).Value = "Giờ điểm danh";
            ws.Cell(headerRow, 6).Value = "Kết quả";

            ws.Range(headerRow, 1, headerRow, 6).Style.Font.Bold = true;
            ws.Range(headerRow, 1, headerRow, 6)
                .Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            /* ================= DATA ================= */
            int row = headerRow + 1;
            int stt = 1;

            foreach (var d in data)
            {
                ws.Cell(row, 1).Value = stt++;
                ws.Cell(row, 2).Value = d.LqvSinhVienId;
                ws.Cell(row, 3).Value = d.LqvSinhVien.LqvHoTen;

                ws.Cell(row, 4).Value = d.LqvBuoiHoc.LqvNgayHoc
                .ToDateTime(TimeOnly.MinValue);

                ws.Cell(row, 4).Style.DateFormat.Format = "dd/MM/yyyy";


                ws.Cell(row, 5).Value = d.LqvThoiGian.ToString("HH:mm");
                ws.Cell(row, 6).Value = d.LqvHopLe ? "Hợp lệ" : "Không hợp lệ";

                row++;
            }

            /* ================= BORDER ================= */
            ws.Range(headerRow, 1, row - 1, 6)
                .Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            ws.Range(headerRow, 1, row - 1, 6)
                .Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            ws.Columns().AdjustToContents();

            /* ================= SIGNATURE ================= */
            row += 2;

            ws.Cell(row, 2).Value = "Giảng viên";
            ws.Cell(row, 5).Value = "Trưởng khoa";

            ws.Range(row, 2, row, 3).Merge();
            ws.Range(row, 5, row, 6).Merge();

            ws.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            row += 3;

            ws.Cell(row, 2).Value = "(Ký, ghi rõ họ tên)";
            ws.Cell(row, 5).Value = "(Ký, ghi rõ họ tên)";

            /* ================= SAVE ================= */
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"DiemDanh_{lop.LqvTenLop}_{ngayHoc:ddMMyyyy}.xlsx"
            );
        }
        [HttpGet]
        public async Task<IActionResult> ExportExcelAllClasses()
        {
            var data = await _context.LqvDiemDanhGps
                .Include(x => x.LqvSinhVien)
                .Include(x => x.LqvBuoiHoc)
                    .ThenInclude(b => b.LqvLopHoc)
                .AsNoTracking()
                .OrderBy(x => x.LqvBuoiHoc.LqvLopHoc.LqvTenLop)
                .ThenBy(x => x.LqvBuoiHoc.LqvNgayHoc)
                .ThenBy(x => x.LqvThoiGian)
                .ToListAsync();

            if (!data.Any())
                return BadRequest("Không có dữ liệu điểm danh");

            using var workbook = new XLWorkbook();

            // ===== GROUP THEO LỚP (ID + TÊN) =====
            var groupByLop = data.GroupBy(x => new
            {
                x.LqvBuoiHoc.LqvLopHoc.LqvLopHocId,
                x.LqvBuoiHoc.LqvLopHoc.LqvTenLop
            });

            foreach (var lopGroup in groupByLop)
            {
                var lopTen = lopGroup.Key.LqvTenLop;

                // ===== FIX TRÙNG SHEET NAME =====
                var sheetName = lopTen;
                int index = 1;
                while (workbook.Worksheets.Any(w => w.Name == sheetName))
                {
                    sheetName = $"{lopTen}_{index++}";
                }

                var ws = workbook.Worksheets.Add(sheetName);

                int row = 1;

                // ===== HEADER =====
                ws.Cell(row, 1).Value = "DANH SÁCH ĐIỂM DANH SINH VIÊN";
                ws.Range(row, 1, row, 6).Merge();
                ws.Row(row).Style.Font.Bold = true;
                ws.Row(row).Style.Font.FontSize = 15;
                ws.Row(row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                row += 2;

                ws.Cell(row, 1).Value = $"Lớp: {lopTen}";
                ws.Range(row, 1, row, 3).Merge();
                row += 2;

                // ===== GROUP THEO NGÀY HỌC =====
                var groupByNgay = lopGroup.GroupBy(x => x.LqvBuoiHoc.LqvNgayHoc);

                foreach (var ngayGroup in groupByNgay)
                {
                    ws.Cell(row, 1).Value =
                        $"Ngày học: {ngayGroup.Key.ToDateTime(TimeOnly.MinValue):dd/MM/yyyy}";
                    ws.Range(row, 1, row, 6).Merge();
                    ws.Row(row).Style.Font.Bold = true;
                    row++;

                    // ===== TABLE HEADER =====
                    ws.Cell(row, 1).Value = "STT";
                    ws.Cell(row, 2).Value = "Mã SV";
                    ws.Cell(row, 3).Value = "Họ tên";
                    ws.Cell(row, 4).Value = "Giờ điểm danh";
                    ws.Cell(row, 5).Value = "Kết quả";
                    ws.Cell(row, 6).Value = "Ghi chú";

                    ws.Range(row, 1, row, 6).Style.Font.Bold = true;
                    ws.Range(row, 1, row, 6)
                        .Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Range(row, 1, row, 6)
                        .Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    ws.Range(row, 1, row, 6)
                        .Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    row++;

                    int stt = 1;
                    foreach (var d in ngayGroup)
                    {
                        ws.Cell(row, 1).Value = stt++;
                        ws.Cell(row, 2).Value = d.LqvSinhVienId;
                        ws.Cell(row, 3).Value = d.LqvSinhVien.LqvHoTen;
                        ws.Cell(row, 4).Value = d.LqvThoiGian.ToString("HH:mm:ss");
                        ws.Cell(row, 5).Value = d.LqvHopLe ? "Hợp lệ" : "Không hợp lệ";
                        ws.Cell(row, 6).Value = d.LqvHopLe ? "" : "Ngoài vùng GPS";

                        ws.Range(row, 1, row, 6)
                            .Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        ws.Range(row, 1, row, 6)
                            .Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                        row++;
                    }

                    row += 2;
                }

                // ===== SIGNATURE =====
                ws.Cell(row, 2).Value = "Giảng viên";
                ws.Cell(row, 5).Value = "Trưởng khoa";

                ws.Range(row, 2, row, 3).Merge();
                ws.Range(row, 5, row, 6).Merge();

                ws.Range(row, 2, row, 6)
                    .Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                row += 3;

                ws.Cell(row, 2).Value = "(Ký, ghi rõ họ tên)";
                ws.Cell(row, 5).Value = "(Ký, ghi rõ họ tên)";

                ws.Columns().AdjustToContents();
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"TongHopDiemDanh_{DateTime.Now:yyyyMMdd}.xlsx"
            );
        }


        // ================== MAP VIEW ==================
        public async Task<IActionResult> Map(int buoiHocId)
        {
            Console.WriteLine($"🗺️ [MAP] BuoiHocId={buoiHocId}");

            var buoiHoc = await _context.LqvBuoiHocs
                .Include(x => x.LqvLopHoc)
                .FirstOrDefaultAsync(x => x.LqvBuoiHocId == buoiHocId);

            if (buoiHoc == null)
            {
                Console.WriteLine("❌ [MAP] Không tìm thấy buổi học");
                return NotFound();
            }

            Console.WriteLine($"📍 TÂM GPS: {buoiHoc.LqvViDo}, {buoiHoc.LqvKinhDo}");
            Console.WriteLine($"📏 BÁN KÍNH: {buoiHoc.LqvBanKinh}m");

            ViewBag.BuoiHoc = buoiHoc;

            var diemDanh = await _context.LqvDiemDanhGps
                .Include(x => x.LqvSinhVien)
                .Where(x => x.LqvBuoiHocId == buoiHocId)
                .OrderBy(x => x.LqvThoiGian)
                .ToListAsync();

            Console.WriteLine($"👥 Tổng lượt điểm danh: {diemDanh.Count}");

            foreach (var d in diemDanh)
            {
                Console.WriteLine(
                    $"➡️ SV={d.LqvSinhVienId} | Lat={d.LqvViDo} | Lng={d.LqvKinhDo} | HopLe={d.LqvHopLe}");
            }

            return View(diemDanh);
        }

        // ================= GET FACE IMAGE =================
        [HttpGet]
        public IActionResult GetCheckinImage(string prefix)
        {
            Console.WriteLine($"🖼️ [GET FACE] prefix={prefix}");

            var folder = Path.Combine(_env.WebRootPath, "checkin-faces");

            Console.WriteLine("📂 PATH = " + folder);

            if (!Directory.Exists(folder))
            {
                Console.WriteLine("❌ Folder không tồn tại");
                return Content("");
            }

            var file = Directory
                .GetFiles(folder, prefix + "*.jpg")
                .OrderByDescending(f => f)
                .FirstOrDefault();

            if (file == null)
            {
                Console.WriteLine("❌ Không tìm thấy ảnh theo prefix");
                return Content("");
            }

            var fileName = Path.GetFileName(file);

            Console.WriteLine($"✅ Tìm thấy ảnh: {fileName}");

            return Content("/checkin-faces/" + fileName);
        }
    }
}