namespace LQV_BlockchainCertificate.Services
{
    public class RiskService
    {
        private static Dictionary<int, double> _riskScores = new();

        public double AddRisk(int baiLamId, double score)
        {
            if (!_riskScores.ContainsKey(baiLamId))
                _riskScores[baiLamId] = 0;

            _riskScores[baiLamId] += score;
            return _riskScores[baiLamId];
        }

        public double GetRisk(int baiLamId)
        {
            return _riskScores.ContainsKey(baiLamId)
                ? _riskScores[baiLamId]
                : 0;
        }
    }
}