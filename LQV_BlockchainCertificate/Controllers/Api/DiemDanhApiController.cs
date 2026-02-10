using LQV_BlockchainCertificate.Models.DBModel;
using LQV_BlockchainCertificate.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LQV_BlockchainCertificate.Controllers.Api
{
    [ApiController]
    [Route("api/student/diemdanh")]
    public class DiemDanhApiController : ControllerBase
    {
        private readonly LqvDbContext _context;

        public DiemDanhApiController(LqvDbContext context)
        {
            _context = context;
        }

        [HttpPost("checkin")]
        public async Task<IActionResult> CheckIn([FromBody] DiemDanhRequestVM model)
        {
            var buoiHoc = await _context.LqvBuoiHocs
                .FirstOrDefaultAsync(x => x.LqvBuoiHocId == model.BuoiHocId);

            if (buoiHoc == null)
                return BadRequest("Buổi học không tồn tại");

            // 📍 tính khoảng cách
            double distance = CalculateDistance(
                     buoiHoc.LqvViDo.Value,
                     buoiHoc.LqvKinhDo.Value,
                     model.Latitude,
                     model.Longitude
                 );


            bool hopLe = distance <= buoiHoc.LqvBanKinh;

            var diemDanh = new LqvDiemDanhGp
            {
                LqvBuoiHocId = buoiHoc.LqvBuoiHocId,
                LqvSinhVienId = model.StudentId,
                LqvLopHocId = buoiHoc.LqvLopHocId, // ✅ LẤY TỪ BUỔI HỌC
                LqvViDo = model.Latitude,
                LqvKinhDo = model.Longitude,
                LqvHopLe = hopLe,
                LqvThoiGian = DateTime.Now
            };

            _context.LqvDiemDanhGps.Add(diemDanh);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                hopLe,
                distance
            });
        }

        // ===============================
        // 📐 HAVERSINE
        // ===============================
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371000; // mét
            double dLat = (lat2 - lat1) * Math.PI / 180;
            double dLon = (lon2 - lon1) * Math.PI / 180;

            double a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) *
                Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            return R * (2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a)));
        }

        public class DiemDanhRequestVM
        {
            public int StudentId { get; set; }
            public int BuoiHocId { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }
    }
}
