using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LQV_BlockchainCertificate.Models.DBModel;
using X.PagedList;
using X.PagedList.Extensions;
using LQV_BlockchainCertificate.Models.ViewModels;


namespace LQV_BlockchainCertificate.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LqvLopHocsController : Controller
    {
        private readonly LqvDbContext _context;

        public LqvLopHocsController(LqvDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // 📌 INDEX – Danh sách lớp học
        // =========================================================
        public IActionResult Index(string searchString, int? khoaHocId, int? giangVienId, int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var query = _context.LqvLopHocs
                .Include(l => l.LqvKhoaHoc)
                .Include(l => l.LqvGiangVien)
                .Include(l => l.LqvDangKyLopHocs)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
                query = query.Where(x => x.LqvTenLop.Contains(searchString));

            if (khoaHocId.HasValue && khoaHocId > 0)
                query = query.Where(x => x.LqvKhoaHocId == khoaHocId);

            if (giangVienId.HasValue && giangVienId > 0)
                query = query.Where(x => x.LqvGiangVienId == giangVienId);

            ViewBag.CurrentFilter = searchString;

            ViewBag.KhoaHocId = new SelectList(
                _context.LqvKhoaHocs,
                "LqvMaKhoaHoc",
                "LqvTenKhoaHoc",
                khoaHocId
            );

            ViewBag.GiangVienId = new SelectList(
                _context.LqvNguoiDungs.Where(x => x.LqvRoleId == 2),
                "LqvId",
                "LqvHoTen",
                giangVienId
            );

            return View(query.OrderByDescending(x => x.LqvNgayTao)
                             .ToPagedList(pageNumber, pageSize));
        }

        // =========================================================
        // 📌 DANH SÁCH SINH VIÊN TRONG LỚP
        // =========================================================
        public async Task<IActionResult> SinhVien(int id)
        {
            var lopHoc = await _context.LqvLopHocs
                .Include(l => l.LqvDangKyLopHocs)
                    .ThenInclude(dk => dk.LqvSinhVien)
                .FirstOrDefaultAsync(x => x.LqvLopHocId == id);

            if (lopHoc == null) return NotFound();

            // Danh sách sinh viên viewmodel
            var svVM = lopHoc.LqvDangKyLopHocs.Select(dk => new SinhVienLopHocViewModel
            {
                SinhVienId = dk.LqvSinhVienId,
                HoTen = dk.LqvSinhVien.LqvHoTen,
                Email = dk.LqvSinhVien.LqvEmail,
                ChuyenCan = TinhChuyenCan(lopHoc.LqvLopHocId, dk.LqvSinhVienId),
                DuDieuKienChungChi = DuDieuKienCapChungChi(lopHoc.LqvLopHocId, dk.LqvSinhVienId)
            }).ToList();

            ViewBag.SinhVienChuaCoLop = await _context.LqvNguoiDungs
                .Where(x => x.LqvRoleId == 3)
                .ToListAsync();

            // Debug
            foreach (var sv in svVM)
            {
                Console.WriteLine($"SinhVienID={sv.SinhVienId}, HoTen={sv.HoTen}, %ChuyenCan={sv.ChuyenCan}, DuDieuKien={sv.DuDieuKienChungChi}");
            }

            return View(new SinhVienLopHocListViewModel
            {
                LopHocId = lopHoc.LqvLopHocId,
                TenLop = lopHoc.LqvTenLop,
                SinhViens = svVM
            });
        }


        // =========================================================
        // 📌 GÁN SINH VIÊN (AJAX)
        [HttpPost]
        public async Task<IActionResult> GanSinhVien([FromBody] GanSinhVienDto dto)
        {
            Console.WriteLine("===== GAN SINH VIEN =====");
            Console.WriteLine($"LopHocId: {dto.LopHocId}");
            Console.WriteLine($"Số SV nhận được: {dto.SinhVienIds.Count}");

            if (dto.LopHocId <= 0 || dto.SinhVienIds.Count == 0)
                return BadRequest("Dữ liệu không hợp lệ");

            foreach (var svId in dto.SinhVienIds)
            {
                bool exists = await _context.LqvDangKyLopHocs
                    .AnyAsync(x => x.LqvLopHocId == dto.LopHocId && x.LqvSinhVienId == svId);

                if (!exists)
                {
                    _context.LqvDangKyLopHocs.Add(new LqvDangKyLopHoc
                    {
                        LqvLopHocId = dto.LopHocId,
                        LqvSinhVienId = svId,
                        LqvNgayDangKy = DateTime.Now
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok();
        }



        // =========================================================
        // =========================================================
        // 📌 XÓA SINH VIÊN KHỎI LỚP (DEBUG FULL)
        // =========================================================
        [HttpPost]
        public async Task<IActionResult> XoaSinhVien([FromBody] XoaSinhVienDto dto)
        {
            Console.WriteLine("===== XOA SINH VIEN =====");
            Console.WriteLine($"LopHocId nhận được: {dto.LopHocId}");
            Console.WriteLine($"SinhVienId nhận được: {dto.SinhVienId}");

            if (dto.LopHocId <= 0 || dto.SinhVienId <= 0)
            {
                Console.WriteLine("❌ ID KHÔNG HỢP LỆ");
                return BadRequest("ID không hợp lệ");
            }

            var dk = await _context.LqvDangKyLopHocs
                .FirstOrDefaultAsync(x =>
                    x.LqvLopHocId == dto.LopHocId &&
                    x.LqvSinhVienId == dto.SinhVienId
                );

            if (dk == null)
            {
                Console.WriteLine("❌ KHÔNG TÌM THẤY BẢN GHI ĐĂNG KÝ");
                return NotFound("Không tìm thấy sinh viên trong lớp");
            }

            Console.WriteLine($"✔ Tìm thấy DK ID = {dk.LqvId}");
            Console.WriteLine("👉 Tiến hành REMOVE");

            _context.LqvDangKyLopHocs.Remove(dk);

            int rows = await _context.SaveChangesAsync();
            Console.WriteLine($"✅ SaveChangesAsync xong - Rows affected = {rows}");

            return Ok();
        }


        // =========================================================
        // 📊 TÍNH % CHUYÊN CẦN
        // =========================================================
        private double TinhChuyenCan(int lopHocId, int sinhVienId)
        {
            int tong = _context.LqvDiemDanhGps
                .Where(x => x.LqvLopHocId == lopHocId)
                .Select(x => x.LqvBuoiHocId)
                .Distinct()
                .Count();

            if (tong == 0) return 0;

            int coMat = _context.LqvDiemDanhGps
                .Where(x =>
                    x.LqvLopHocId == lopHocId &&
                    x.LqvSinhVienId == sinhVienId &&
                    x.LqvHopLe == true
                )
                .Select(x => x.LqvBuoiHocId)
                .Distinct()
                .Count();

            return Math.Round((double)coMat / tong * 100, 2);
        }

        private bool DuDieuKienCapChungChi(int lopHocId, int sinhVienId)
        {
            return TinhChuyenCan(lopHocId, sinhVienId) >= 80;
        }

        // =========================================================
        // 📌 CREATE
        // =========================================================
        public IActionResult Create()
        {
            LoadDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LqvLopHoc model)
        {
            Console.WriteLine("===== CREATE LỚP HỌC =====");

            if (model == null)
            {
                Console.WriteLine("❌ Model = NULL");
                LoadDropdowns();
                return View();
            }

            Console.WriteLine($"Tên lớp: {model.LqvTenLop}");
            Console.WriteLine($"Khóa học ID: {model.LqvKhoaHocId}");
            Console.WriteLine($"Giảng viên ID: {model.LqvGiangVienId}");
            Console.WriteLine($"Mô tả: {model.LqvMoTa}");

            // 🔍 Kiểm tra ModelState
            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ ModelState INVALID");

                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"⚠️ Lỗi field [{state.Key}]: {error.ErrorMessage}");
                    }
                }

                LoadDropdowns(model);
                return View(model);
            }

            try
            {
                model.LqvNgayTao = DateTime.Now;
                Console.WriteLine($"Ngày tạo set = {model.LqvNgayTao}");

                _context.LqvLopHocs.Add(model);
                Console.WriteLine("✔ Đã Add vào DbContext");

                await _context.SaveChangesAsync();
                Console.WriteLine("✅ SaveChangesAsync THÀNH CÔNG");

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine("🔥 LỖI KHI SAVE DATABASE");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException?.Message);

                LoadDropdowns(model);
                return View(model);
            }
        }


        // =========================================================
        // 📌 EDIT
        // =========================================================
        public async Task<IActionResult> Edit(int id)
        {
            var lopHoc = await _context.LqvLopHocs.FindAsync(id);
            if (lopHoc == null) return NotFound();

            LoadDropdowns(lopHoc);
            return View(lopHoc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LqvLopHoc model)
        {
            if (id != model.LqvLopHocId) return NotFound();

            if (!ModelState.IsValid)
            {
                LoadDropdowns(model);
                return View(model);
            }

            _context.Update(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // 📌 DELETE
        // =========================================================
        public async Task<IActionResult> Delete(int id)
        {
            var lopHoc = await _context.LqvLopHocs
                .Include(x => x.LqvKhoaHoc)
                .Include(x => x.LqvGiangVien)
                .FirstOrDefaultAsync(x => x.LqvLopHocId == id);

            if (lopHoc == null) return NotFound();
            return View(lopHoc);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lopHoc = await _context.LqvLopHocs.FindAsync(id);
            if (lopHoc != null)
            {
                _context.LqvLopHocs.Remove(lopHoc);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // 🔁 DROPDOWN
        // =========================================================
        private void LoadDropdowns(LqvLopHoc? lopHoc = null)
        {
            ViewData["LqvKhoaHocId"] = new SelectList(
                _context.LqvKhoaHocs,
                "LqvMaKhoaHoc",
                "LqvTenKhoaHoc",
                lopHoc?.LqvKhoaHocId
            );

            ViewData["LqvGiangVienId"] = new SelectList(
                _context.LqvNguoiDungs.Where(x => x.LqvRoleId == 2),
                "LqvId",
                "LqvHoTen",
                lopHoc?.LqvGiangVienId
            );
        }
    }
}
