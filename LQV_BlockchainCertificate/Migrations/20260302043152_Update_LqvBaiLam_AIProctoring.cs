using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LQV_BlockchainCertificate.Migrations
{
    public partial class Update_LqvBaiLam_AIProctoring : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ===============================
            // 1️⃣ Thêm cột vào LQV_BaiLam
            // ===============================

            migrationBuilder.AddColumn<int>(
                name: "Lqv_TongDiemRisk",
                table: "LQV_BaiLam",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Lqv_BiKhoa",
                table: "LQV_BaiLam",
                type: "bit",
                nullable: false,
                defaultValue: false);


            // ===============================
            // 2️⃣ Bảng log ảnh AI
            // ===============================

            migrationBuilder.CreateTable(
                name: "Lqv_NhatKyHinhAnhThi",
                columns: table => new
                {
                    Lqv_NhatKyHinhAnhThiId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    Lqv_BaiLamId = table.Column<int>(nullable: false),

                    Lqv_DuongDanAnh = table.Column<string>(nullable: false),

                    Lqv_KetQuaAI = table.Column<string>(nullable: true),

                    Lqv_ThoiGian = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lqv_NhatKyHinhAnhThi",
                        x => x.Lqv_NhatKyHinhAnhThiId);

                    table.ForeignKey(
                        name: "FK_Lqv_NhatKyHinhAnhThi_LQV_BaiLam",
                        column: x => x.Lqv_BaiLamId,
                        principalTable: "LQV_BaiLam",
                        principalColumn: "LQV_BaiLamID",
                        onDelete: ReferentialAction.Cascade);
                });


            // ===============================
            // 3️⃣ Bảng log vi phạm
            // ===============================

            migrationBuilder.CreateTable(
                name: "Lqv_NhatKyViPhamThi",
                columns: table => new
                {
                    Lqv_NhatKyViPhamThiId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    Lqv_BaiLamId = table.Column<int>(nullable: false),

                    Lqv_LoaiViPham = table.Column<string>(nullable: false),

                    Lqv_DiemRisk = table.Column<int>(nullable: false),

                    Lqv_MoTa = table.Column<string>(nullable: true),

                    Lqv_ThoiGian = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lqv_NhatKyViPhamThi",
                        x => x.Lqv_NhatKyViPhamThiId);

                    table.ForeignKey(
                        name: "FK_Lqv_NhatKyViPhamThi_LQV_BaiLam",
                        column: x => x.Lqv_BaiLamId,
                        principalTable: "LQV_BaiLam",
                        principalColumn: "LQV_BaiLamID",
                        onDelete: ReferentialAction.Cascade);
                });


            // ===============================
            // 4️⃣ Index tăng tốc truy vấn
            // ===============================

            migrationBuilder.CreateIndex(
                name: "IX_Lqv_NhatKyHinhAnhThi_Lqv_BaiLamId",
                table: "Lqv_NhatKyHinhAnhThi",
                column: "Lqv_BaiLamId");

            migrationBuilder.CreateIndex(
                name: "IX_Lqv_NhatKyViPhamThi_Lqv_BaiLamId",
                table: "Lqv_NhatKyViPhamThi",
                column: "Lqv_BaiLamId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Lqv_NhatKyHinhAnhThi");

            migrationBuilder.DropTable(
                name: "Lqv_NhatKyViPhamThi");

            migrationBuilder.DropColumn(
                name: "Lqv_TongDiemRisk",
                table: "LQV_BaiLam");

            migrationBuilder.DropColumn(
                name: "Lqv_BiKhoa",
                table: "LQV_BaiLam");
        }
    }
}