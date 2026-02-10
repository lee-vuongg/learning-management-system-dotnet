using LQV_BlockchainCertificate.Models.DBModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/student/dashboard")]
public class StudentDashboardApiController : ControllerBase
{
    private readonly LqvDbContext _context;

    public StudentDashboardApiController(LqvDbContext context)
    {
        _context = context;
    }

    [HttpGet("{studentId}")]
    public async Task<IActionResult> GetDashboard(int studentId)
    {
        var nguoiDung = await _context.LqvNguoiDungs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.LqvId == studentId);

        if (nguoiDung == null)
            return NotFound(new { message = "Sinh viên không tồn tại" });

        var chungNhans = await _context.LqvChungNhans
            .Where(x => x.LqvSinhVienId == studentId)
            .Include(x => x.LqvKhoaHoc)
            .OrderByDescending(x => x.LqvNgayCap)
            .Take(5)
            .Select(x => new
            {
                x.LqvMaChungNhan,
                x.LqvMaChungNhanCode,
                TenKhoaHoc = x.LqvKhoaHoc.LqvTenKhoaHoc,
                x.LqvNgayCap,
                TrangThai = x.LqvTrangThai
            })
            .ToListAsync();

        var diemDanh = await _context.LqvDiemDanhGps
            .Where(x => x.LqvSinhVienId == studentId)
            .OrderByDescending(x => x.LqvThoiGian)
            .Take(5)
            .Select(x => new
            {
                x.LqvThoiGian,
                TrangThai = x.LqvHopLe ? "Đúng giờ" : "Không hợp lệ"
            })
            .ToListAsync();

        return Ok(new
        {
            sinhVien = new
            {
                nguoiDung.LqvHoTen,
                nguoiDung.LqvTenDangNhap,
                nguoiDung.LqvEmail,
                Avt = nguoiDung.LqvAvt ?? "/img/default-avatar.jpg"
            },
            tongChungNhan = chungNhans.Count,
            chungNhanMoiNhat = chungNhans,
            lichSuDiemDanh = diemDanh
        });
    }
}
