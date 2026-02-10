using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using LQV_BlockchainCertificate.Models.DBModel;

namespace LQV_BlockchainCertificate.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    public class LqvBoCauHoisController : Controller
    {
        private readonly LqvDbContext _context;

        public LqvBoCauHoisController(LqvDbContext context)
        {
            _context = context;
        }

        // ==================================================
        // LẤY ID GIẢNG VIÊN ĐANG ĐĂNG NHẬP
        // ==================================================
        private int GetGiangVienId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        // ==================================================
        // INDEX – DANH SÁCH BỘ CÂU HỎI
        // ==================================================
        public async Task<IActionResult> Index()
        {
            int giangVienId = GetGiangVienId();

            var data = await _context.LqvBoCauHois
                .Where(x => x.LqvGiangVienId == giangVienId)
                .OrderByDescending(x => x.LqvNgayTao)
                .ToListAsync();

            return View(data);
        }

        // ==================================================
        // CREATE
        // ==================================================
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LqvBoCauHoi model)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.LqvGiangVienId = GetGiangVienId();
            model.LqvNgayTao = DateTime.Now;

            _context.LqvBoCauHois.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ==================================================
        // EDIT
        // ==================================================
        public async Task<IActionResult> Edit(int id)
        {
            int gvId = GetGiangVienId();

            var bo = await _context.LqvBoCauHois
                .FirstOrDefaultAsync(x => x.LqvBoCauHoiId == id && x.LqvGiangVienId == gvId);

            if (bo == null) return Forbid();
            return View(bo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LqvBoCauHoi model)
        {
            int gvId = GetGiangVienId();

            if (id != model.LqvBoCauHoiId) return BadRequest();

            var boDb = await _context.LqvBoCauHois
                .FirstOrDefaultAsync(x => x.LqvBoCauHoiId == id && x.LqvGiangVienId == gvId);

            if (boDb == null) return Forbid();

            boDb.LqvTenBo = model.LqvTenBo;
            boDb.LqvMoTa = model.LqvMoTa;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ==================================================
        // DELETE
        // ==================================================
        public async Task<IActionResult> Delete(int id)
        {
            int gvId = GetGiangVienId();

            var bo = await _context.LqvBoCauHois
                .FirstOrDefaultAsync(x => x.LqvBoCauHoiId == id && x.LqvGiangVienId == gvId);

            if (bo == null) return Forbid();
            return View(bo);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            int gvId = GetGiangVienId();

            var bo = await _context.LqvBoCauHois
                .FirstOrDefaultAsync(x => x.LqvBoCauHoiId == id && x.LqvGiangVienId == gvId);

            if (bo == null) return Forbid();

            _context.LqvBoCauHois.Remove(bo);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ==================================================
        // DANH SÁCH CÂU HỎI
        // ==================================================
        public async Task<IActionResult> Questions(int id)
        {
            int gvId = GetGiangVienId();

            var bo = await _context.LqvBoCauHois
                .Include(x => x.LqvCauHois)
                    .ThenInclude(c => c.LqvDapAns)
                .FirstOrDefaultAsync(x => x.LqvBoCauHoiId == id && x.LqvGiangVienId == gvId);

            if (bo == null) return Forbid();

            ViewBag.TenBo = bo.LqvTenBo;
            ViewBag.BoId = bo.LqvBoCauHoiId;

            return View(bo.LqvCauHois.ToList());
        }

        // ==================================================
        // EXPORT EXCEL
        // ==================================================
        public IActionResult ExportExcel(int id)
        {
            int gvId = GetGiangVienId();

            var bo = _context.LqvBoCauHois
                .Include(b => b.LqvCauHois)
                    .ThenInclude(c => c.LqvDapAns)
                .FirstOrDefault(b => b.LqvBoCauHoiId == id && b.LqvGiangVienId == gvId);

            if (bo == null) return Forbid();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("CauHoi");

            ws.Cell(1, 1).Value = "NoiDungCauHoi";
            ws.Cell(1, 2).Value = "Loai";
            ws.Cell(1, 3).Value = "Diem";
            ws.Cell(1, 4).Value = "NoiDungDapAn";
            ws.Cell(1, 5).Value = "Dung";

            int row = 2;

            foreach (var ch in bo.LqvCauHois)
            {
                foreach (var da in ch.LqvDapAns)
                {
                    ws.Cell(row, 1).Value = ch.LqvNoiDung;
                    ws.Cell(row, 2).Value = ch.LqvLoai;
                    ws.Cell(row, 3).Value = ch.LqvDiem;
                    ws.Cell(row, 4).Value = da.LqvNoiDung;
                    ws.Cell(row, 5).Value = da.LqvDung;
                    row++;
                }
            }

            using var stream = new MemoryStream();
            wb.SaveAs(stream);

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"BoCauHoi_{bo.LqvTenBo}.xlsx"
            );
        }

        // ==================================================
        // IMPORT EXCEL
        // ==================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportExcel(int id, IFormFile excelFile)
        {
            int gvId = GetGiangVienId();

            var bo = await _context.LqvBoCauHois
                .FirstOrDefaultAsync(x => x.LqvBoCauHoiId == id && x.LqvGiangVienId == gvId);

            if (bo == null || excelFile == null || excelFile.Length == 0)
                return BadRequest();

            using var stream = new MemoryStream();
            await excelFile.CopyToAsync(stream);

            using var wb = new XLWorkbook(stream);
            var ws = wb.Worksheets.FirstOrDefault();
            if (ws == null)
            {
                ModelState.AddModelError("", "File Excel không có sheet nào");
                return RedirectToAction(nameof(Questions), new { id });
            }


            var rows = ws.RangeUsed().RowsUsed().Skip(1);
            var cauHoiDict = new Dictionary<string, LqvCauHoi>();

            foreach (var row in rows)
            {
                string noiDungCH = row.Cell(1).GetString().Trim();
                string loai = row.Cell(2).GetString();
                double diem = row.Cell(3).GetDouble();
                string noiDungDA = row.Cell(4).GetString();
                bool dung = row.Cell(5).GetBoolean();

                if (!cauHoiDict.ContainsKey(noiDungCH))
                {
                    var ch = new LqvCauHoi
                    {
                        LqvBoCauHoiId = bo.LqvBoCauHoiId,
                        LqvNoiDung = noiDungCH,
                        LqvLoai = loai,
                        LqvDiem = diem
                    };

                    _context.LqvCauHois.Add(ch);
                    cauHoiDict[noiDungCH] = ch;
                }

                _context.LqvDapAns.Add(new LqvDapAn
                {
                    LqvNoiDung = noiDungDA,
                    LqvDung = dung,
                    LqvCauHoi = cauHoiDict[noiDungCH]
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Questions), new { id });
        }
    }
}
