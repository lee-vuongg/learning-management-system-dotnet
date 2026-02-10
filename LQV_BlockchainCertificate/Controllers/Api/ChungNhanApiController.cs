
using LQV_BlockchainCertificate.Models.DBModel;
using LQV_BlockchainCertificate.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LQV_BlockchainCertificate.Controllers.Api
{
    [ApiController]
    [Route("api/student/chungnhan")]
    public class ChungNhanApiController : ControllerBase
    {
        private readonly LqvDbContext _context;

        public ChungNhanApiController(LqvDbContext context)
        {
            _context = context;
        }

        [HttpGet("{studentId}")]
        public async Task<IActionResult> GetChungNhan(int studentId)
        {
            var data = await _context.LqvChungNhans
                .Where(cn => cn.LqvSinhVienId == studentId)
                .Include(cn => cn.LqvKhoaHoc)
                .Select(cn => new ChungNhanDto
                {
                    TenKhoaHoc = cn.LqvKhoaHoc.LqvTenKhoaHoc,
                    MaChungNhan = cn.LqvMaChungNhan.ToString(),
                    NgayCap = cn.LqvNgayCap.ToString("dd/MM/yyyy"),

                    // 🔥 LẤY TX HASH
                    TxHash = _context.LqvGiaoDichBlockchains
                        .Where(gd => gd.LqvChungNhanId == cn.LqvMaChungNhan)
                        .OrderByDescending(gd => gd.LqvGioTao)
                        .Select(gd => gd.LqvTxHash)
                        .FirstOrDefault() ?? ""
                })
                .ToListAsync();

            return Ok(data);
        }

    }
}
