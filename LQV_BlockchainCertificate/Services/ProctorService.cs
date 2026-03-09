using LQV_BlockchainCertificate.Models.DBModel;

namespace LQV_BlockchainCertificate.Services
{
    public class ProctorService
    {
        private readonly GeminiService _geminiService;
        private readonly RiskService _riskService;

        public ProctorService(GeminiService geminiService, RiskService riskService)
        {
            _geminiService = geminiService;
            _riskService = riskService;
        }

        public async Task<object> AnalyzeFrameAsync(int baiLamId, string base64Image)
        {
            var prompt = @"
            You are an AI exam proctor.
            Detect:
            - Multiple faces
            - Looking away
            - Phone usage
            - No face
            Return JSON:
            {
                ""cheating"": true/false,
                ""confidence"": 0-1,
                ""reason"": """"
            }";

            var result = await _geminiService.AnalyzeImageAsync(base64Image, prompt);

            double risk = result.Cheating ? result.Confidence * 10 : 0;

            double totalRisk = _riskService.AddRisk(baiLamId, risk);

            return new
            {
                result.Cheating,
                result.Confidence,
                result.Reason,
                TotalRisk = totalRisk
            };
        }
    }
}