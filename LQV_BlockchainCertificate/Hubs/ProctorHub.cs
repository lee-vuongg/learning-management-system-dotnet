using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using LQV_BlockchainCertificate.Models.DBModel;

namespace LQV_BlockchainCertificate.Hubs
{
    public class ProctorHub : Hub
    {
        private readonly LqvDbContext _context;

        public ProctorHub(LqvDbContext context)
        {
            _context = context;
        }

        // ============================================
        // JOIN ROOM
        // ============================================
        public async Task JoinExamRoom(string examRoom)
        {
            Console.WriteLine("JoinExamRoom called: " + examRoom);
            Console.WriteLine("ConnectionId: " + Context.ConnectionId);

            await Groups.AddToGroupAsync(Context.ConnectionId, examRoom);
        }
        public async Task ManualLock(int userId, string room)
        {
            var baiLam = await _context.LqvBaiLams
                .FirstOrDefaultAsync(x =>
                    x.LqvUserId == userId &&
                    x.LqvTrangThai == "DangLam");

            if (baiLam == null)
                return;

            baiLam.LqvTrangThai = "KhoaDoGianLan";
            baiLam.LqvThoiGianNop = DateTime.Now;

            await _context.SaveChangesAsync();

            await Clients.Group(room)
                .SendAsync("ForceLockExam", new { userId });
        }
        // ============================================
        // LIVE STREAM FRAME (CHO GIẢNG VIÊN XEM)
        // ============================================
        public async Task SendLiveFrame(string userId, string image, string examRoom)
        {
            if (string.IsNullOrEmpty(userId) ||
                string.IsNullOrEmpty(image) ||
                string.IsNullOrEmpty(examRoom))
            {
                Console.WriteLine("SendLiveFrame: Invalid data");
                return;
            }

            Console.WriteLine("==== SendLiveFrame CALLED ====");
            Console.WriteLine($"UserId: {userId}");
            Console.WriteLine($"ExamRoom: {examRoom}");
            Console.WriteLine($"Image length: {image.Length}");

            await Clients.Group(examRoom)
                .SendAsync("ReceiveLiveFrame", new
                {
                    userId = userId,
                    image = image
                });
        }

        // ============================================
        // WARNING + RISK + AUTO LOCK (CHUẨN DB)
        // ============================================
        public async Task SendWarning(
            string room,
            int userId,
            string message,
            int riskIncrease = 10,
            string? image = null)
        {
            try
            {
                // 🔍 Tìm bài làm đang thi
                var baiLam = await _context.LqvBaiLams
                    .FirstOrDefaultAsync(x =>
                        x.LqvUserId == userId &&
                        x.LqvTrangThai == "DangLam");

                if (baiLam == null)
                    return;

                // Nếu đã nộp hoặc khóa rồi thì bỏ qua
                if (baiLam.LqvTrangThai != "DangLam")
                    return;

                // =============================
                // 1️⃣ LƯU ẢNH BẰNG CHỨNG (NẾU CÓ)
                // =============================
                if (!string.IsNullOrEmpty(image))
                {
                    var nhatKyAnh = new Lqv_NhatKyHinhAnhThi
                    {
                        Lqv_BaiLamId = baiLam.LqvBaiLamId,
                        Lqv_DuongDanAnh = image,
                        Lqv_KetQuaAI = message,
                        Lqv_ThoiGian = DateTime.Now
                    };

                    _context.Add(nhatKyAnh);
                }

                // =============================
                // 2️⃣ LƯU VI PHẠM
                // =============================
                var viPham = new Lqv_NhatKyViPhamThi
                {
                    Lqv_BaiLamId = baiLam.LqvBaiLamId,
                    Lqv_LoaiViPham = message,
                    Lqv_DiemRisk = riskIncrease,
                    Lqv_MoTa = "SignalR AI Detection",
                    Lqv_ThoiGian = DateTime.Now
                };

                _context.Add(viPham);
                await _context.SaveChangesAsync();

                // =============================
                // 3️⃣ TÍNH TỔNG RISK TỪ DB
                // =============================
                var tongRisk = await _context.Lqv_NhatKyViPhamThi
                    .Where(x => x.Lqv_BaiLamId == baiLam.LqvBaiLamId)
                    .SumAsync(x => (int?)x.Lqv_DiemRisk) ?? 0;

                bool biKhoa = false;

                // =============================
                // 4️⃣ AUTO LOCK >= 150
                // =============================
                if (tongRisk >= 150)
                {
                    baiLam.LqvTrangThai = "KhoaDoGianLan";
                    baiLam.LqvThoiGianNop = DateTime.Now;
                    biKhoa = true;

                    await _context.SaveChangesAsync();
                }

                // =============================
                // 5️⃣ GỬI RISK REALTIME
                // =============================
                await Clients.Group(room)
                    .SendAsync("ReceiveRiskScore", new
                    {
                        userId,
                        score = tongRisk
                    });

                // =============================
                // 6️⃣ GỬI WARNING
                // =============================
                await Clients.Group(room)
                    .SendAsync("ReceiveWarning", new
                    {
                        userId,
                        message,
                        risk = tongRisk
                    });

                // =============================
                // 7️⃣ FORCE LOCK NẾU CẦN
                // =============================
                if (biKhoa)
                {
                    await Clients.Group(room)
                        .SendAsync("ForceLockExam", new
                        {
                            userId
                        });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR SendWarning: " + ex.Message);
            }
        }
    }
}