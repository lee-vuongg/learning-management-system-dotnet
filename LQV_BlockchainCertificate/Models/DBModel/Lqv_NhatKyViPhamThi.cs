using System;
using System.Collections.Generic;
namespace LQV_BlockchainCertificate.Models.DBModel
{
    public class Lqv_NhatKyViPhamThi
    {
        public int Lqv_NhatKyViPhamThiId { get; set; }

        public int Lqv_BaiLamId { get; set; }

        public string Lqv_LoaiViPham { get; set; } = "";

        public int Lqv_DiemRisk { get; set; }

        public string? Lqv_MoTa { get; set; }

        public DateTime Lqv_ThoiGian { get; set; } = DateTime.Now;

        public LqvBaiLam? Lqv_BaiLam { get; set; }
    }
}
