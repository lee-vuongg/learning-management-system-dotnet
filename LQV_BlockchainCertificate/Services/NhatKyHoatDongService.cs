using LQV_BlockchainCertificate.Models.DBModel;

namespace LQV_BlockchainCertificate.Services
{
    public class NhatKyHoatDongService
    {
        private readonly LqvDbContext _context;

        public NhatKyHoatDongService(LqvDbContext context)
        {
            _context = context;
        }

        public void GhiNhatKy(string taiKhoan, string hanhDong, string chiTiet, string? ip = null)
        {
            var log = new LqvNhatKyHoatDong
            {
                LqvTaiKhoan = taiKhoan,
                LqvHanhDong = hanhDong,
                LqvChiTiet = chiTiet,
                LqvThoiGian = DateTime.Now,
                LqvIp = ip
            };

            _context.LqvNhatKyHoatDongs.Add(log);
            _context.SaveChanges();
        }

        public List<LqvNhatKyHoatDong> LayHoatDongGanDay(int soLuong = 5)
        {
            return _context.LqvNhatKyHoatDongs
                .OrderByDescending(x => x.LqvThoiGian)
                .Take(soLuong)
                .ToList();
        }
    }
}
