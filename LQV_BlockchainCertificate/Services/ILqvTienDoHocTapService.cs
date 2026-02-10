using System.Threading.Tasks;

namespace LQV_BlockchainCertificate.Services
{
    public interface ILqvTienDoHocTapService
    {
        Task<double> TinhTienDoHocTapAsync(int sinhVienId, int khoaHocId);

        Task CapNhatTienDoHocTapAsync(int sinhVienId, int khoaHocId);
    }
}
