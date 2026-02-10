using LQV_BlockchainCertificate.Models.DBModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LQV_BlockchainCertificate.Controllers.Api
{
    [ApiController]
    [Route("api/student/buoihoc")]
    public class BuoiHocApiController : ControllerBase
    {
        private readonly LqvDbContext _context;

        public BuoiHocApiController(LqvDbContext context)
        {
            _context = context;
        }

        [HttpGet("{studentId}")]
        public async Task<IActionResult> GetBuoiHoc(int studentId)
        {
            var data = await (
                from bh in _context.LqvBuoiHocs
                join dk in _context.LqvDangKyLopHocs
                    on bh.LqvLopHocId equals dk.LqvLopHocId
                where dk.LqvSinhVienId == studentId
                orderby bh.LqvNgayHoc descending
                select new
                {
                    bh.LqvBuoiHocId,
                    NgayHoc = bh.LqvNgayHoc,
                    GioBatDau = bh.LqvGioBatDau,
                    bh.LqvViDo,
                    bh.LqvKinhDo,
                    bh.LqvBanKinh
                }
            ).ToListAsync();

            return Ok(data);
        }
    }
}
