using Microsoft.EntityFrameworkCore;
using LQV_BlockchainCertificate.Models.DBModel;

namespace LQV_BlockchainCertificate.Services
{
    public class LqvTienDoHocTapService : ILqvTienDoHocTapService
    {
        private readonly LqvDbContext _context;

        private const double DIEM_DANH_WEIGHT = 0.3;
        private const double BAI_TAP_WEIGHT = 0.4;
        private const double BAI_THI_WEIGHT = 0.3;

        public LqvTienDoHocTapService(LqvDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // 🎯 TÍNH TIẾN ĐỘ
        // =========================================================
        public async Task<double> TinhTienDoHocTapAsync(int sinhVienId, int khoaHocId)
        {
            // 🔴 LẤY LỚP SINH VIÊN TRONG KHÓA
            var lopHocId = await _context.LqvDangKyLopHocs
                .Where(x => x.LqvSinhVienId == sinhVienId &&
                            x.LqvLopHoc.LqvKhoaHocId == khoaHocId)
                .Select(x => x.LqvLopHocId)
                .FirstOrDefaultAsync();

            if (lopHocId == 0)
                return 0;

            // =============================
            // 1️⃣ ĐIỂM DANH
            // =============================
            int tongBuoiHoc = await _context.LqvBuoiHocs
                .Where(b => b.LqvLopHocId == lopHocId)
                .CountAsync();

            int buoiDaDiemDanh = await (
                from dd in _context.LqvDiemDanhGps
                join bh in _context.LqvBuoiHocs
                    on dd.LqvBuoiHocId equals bh.LqvBuoiHocId
                where dd.LqvSinhVienId == sinhVienId
                      && dd.LqvHopLe == true
                      && bh.LqvLopHocId == lopHocId
                select dd
            ).CountAsync();

            double tiLeDiemDanh = tongBuoiHoc == 0 ? 0 :
                (double)buoiDaDiemDanh / tongBuoiHoc;

            // =============================
            // 2️⃣ BÀI TẬP
            // =============================
            int tongBaiTap = await _context.LqvBaiTaps
                .Where(x => x.LqvLopHocId == lopHocId)
                .CountAsync();

            int baiTapHoanThanh = await _context.LqvNopBaiTaps
                .Where(x => x.LqvSinhVienId == sinhVienId
                         && x.LqvDaCham == true
                         && x.LqvBaiTap.LqvLopHocId == lopHocId)
                .CountAsync();

            double tiLeBaiTap = tongBaiTap == 0 ? 0 :
                (double)baiTapHoanThanh / tongBaiTap;

            // =============================
            // 3️⃣ BÀI THI
            // =============================
            int tongBaiThi = await _context.LqvLichThis
                .Where(x => x.LqvLopHocId == lopHocId)
                .CountAsync();

            int baiThiHoanThanh = await _context.LqvBaiLams
                .Where(x => x.LqvUserId == sinhVienId
                         && x.LqvThoiGianNop != null
                         && x.LqvLichThi.LqvLopHocId == lopHocId)
                .CountAsync();

            double tiLeBaiThi = tongBaiThi == 0 ? 0 :
                (double)baiThiHoanThanh / tongBaiThi;

            // =============================
            // 4️⃣ TỔNG
            // =============================
            double tienDo =
                tiLeDiemDanh * DIEM_DANH_WEIGHT +
                tiLeBaiTap * BAI_TAP_WEIGHT +
                tiLeBaiThi * BAI_THI_WEIGHT;

            return Math.Round(tienDo * 100, 2);
        }

        // =========================================================
        // 💾 UPDATE DB
        // =========================================================
        public async Task CapNhatTienDoHocTapAsync(int sinhVienId, int khoaHocId)
        {
            double phanTram = await TinhTienDoHocTapAsync(sinhVienId, khoaHocId);

            var tienDo = await _context.LqvTienDoHocTaps
                .FirstOrDefaultAsync(x =>
                    x.LqvSinhVienId == sinhVienId &&
                    x.LqvKhoaHocId == khoaHocId);

            if (tienDo == null)
            {
                tienDo = new LqvTienDoHocTap
                {
                    LqvSinhVienId = sinhVienId,
                    LqvKhoaHocId = khoaHocId,
                    LqvTiLeHoanThanh = phanTram,
                    LqvNgayCapNhat = DateTime.Now
                };
                _context.LqvTienDoHocTaps.Add(tienDo);
            }
            else
            {
                tienDo.LqvTiLeHoanThanh = phanTram;
                tienDo.LqvNgayCapNhat = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }
    }
}
