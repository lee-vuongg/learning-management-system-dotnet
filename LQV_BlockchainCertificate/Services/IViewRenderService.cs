using System.Threading.Tasks;

namespace LQV_BlockchainCertificate.Services
{
    public interface IViewRenderService
    {
        Task<string> RenderToStringAsync(string viewName, object model);
    }
}