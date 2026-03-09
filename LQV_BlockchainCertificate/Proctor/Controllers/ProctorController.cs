using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using LQV_BlockchainCertificate.Hubs;
using LQV_BlockchainCertificate.Services;

namespace LQV_BlockchainCertificate.Proctor.Controllers
{
    [ApiController]
    [Route("api/proctor")]
    public class ProctorController : ControllerBase
    {
        private readonly ProctorService _proctorService;
        private readonly IHubContext<ProctorHub> _hub;

        public ProctorController(
            ProctorService proctorService,
            IHubContext<ProctorHub> hub)
        {
            _proctorService = proctorService;
            _hub = hub;
        }

        [HttpPost("frame")]
        public async Task<IActionResult> ReceiveFrame([FromBody] FrameDto dto)
        {
            var result = await _proctorService.AnalyzeFrameAsync(
                dto.BaiLamId,
                dto.Base64Image);

            if ((bool)result.GetType().GetProperty("Cheating")!.GetValue(result)!)
            {
                await _hub.Clients.Group($"exam_{dto.LichThiId}")
                    .SendAsync("ReceiveWarning", result);
            }

            return Ok(result);
        }
    }

    public class FrameDto
    {
        public int BaiLamId { get; set; }
        public int LichThiId { get; set; }
        public string Base64Image { get; set; } = string.Empty;
    }
}