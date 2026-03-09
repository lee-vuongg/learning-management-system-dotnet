using System;
using System.Collections.Generic;
namespace LQV_BlockchainCertificate.Models.DBModel
{
    public class Lqv_NhatKyHinhAnhThi
    {
        public int Lqv_NhatKyHinhAnhThiId { get; set; }

        public int Lqv_BaiLamId { get; set; }

        public string Lqv_DuongDanAnh { get; set; } = "";

        public string? Lqv_KetQuaAI { get; set; }

        public DateTime Lqv_ThoiGian { get; set; } = DateTime.Now;

        public LqvBaiLam? Lqv_BaiLam { get; set; }
    }
}
