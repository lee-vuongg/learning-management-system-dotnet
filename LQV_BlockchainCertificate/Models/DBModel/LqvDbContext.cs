using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace LQV_BlockchainCertificate.Models.DBModel;

public partial class LqvDbContext : DbContext
{
    public LqvDbContext()
    {
    }

    public LqvDbContext(DbContextOptions<LqvDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<LqvBaiLam> LqvBaiLams { get; set; }

    public virtual DbSet<LqvBaiTap> LqvBaiTaps { get; set; }

    public virtual DbSet<LqvBoCauHoi> LqvBoCauHois { get; set; }

    public virtual DbSet<LqvBuoiHoc> LqvBuoiHocs { get; set; }

    public virtual DbSet<LqvCauHoi> LqvCauHois { get; set; }

    public virtual DbSet<LqvChiTietBaiLam> LqvChiTietBaiLams { get; set; }

    public virtual DbSet<LqvChucNang> LqvChucNangs { get; set; }

    public virtual DbSet<LqvChungNhan> LqvChungNhans { get; set; }

    public virtual DbSet<LqvDangKyLopHoc> LqvDangKyLopHocs { get; set; }

    public virtual DbSet<LqvDapAn> LqvDapAns { get; set; }

    public virtual DbSet<LqvDeThi> LqvDeThis { get; set; }

    public virtual DbSet<LqvDiemDanhGp> LqvDiemDanhGps { get; set; }

    public virtual DbSet<LqvGiaoDichBlockchain> LqvGiaoDichBlockchains { get; set; }

    public virtual DbSet<LqvKhoaHoc> LqvKhoaHocs { get; set; }

    public virtual DbSet<LqvLichThi> LqvLichThis { get; set; }

    public virtual DbSet<LqvLopHoc> LqvLopHocs { get; set; }

    public virtual DbSet<LqvNguoiDung> LqvNguoiDungs { get; set; }

    public virtual DbSet<LqvNhatKyHoatDong> LqvNhatKyHoatDongs { get; set; }

    public virtual DbSet<LqvNopBaiTap> LqvNopBaiTaps { get; set; }

    public virtual DbSet<LqvPhanQuyen> LqvPhanQuyens { get; set; }

    public virtual DbSet<LqvRole> LqvRoles { get; set; }

    public virtual DbSet<LqvTienDoHocTap> LqvTienDoHocTaps { get; set; }

    public virtual DbSet<LqvXacThucChungNhan> LqvXacThucChungNhans { get; set; }

    public virtual DbSet<LqvXacThucEmail> LqvXacThucEmails { get; set; }

    public virtual DbSet<LqvYeuCauCapChungChi> LqvYeuCauCapChungChis { get; set; }

    public virtual DbSet<LqvYeuCauHoTro> LqvYeuCauHoTros { get; set; }

    //    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
    //        => optionsBuilder.UseSqlServer("Server=VUONG\\SQLEXPRESS;Database=LQV_DB;Trusted_Connection=True;MultipleActiveResultSets=True; TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LqvBaiLam>(entity =>
        {
            entity.HasKey(e => e.LqvBaiLamId).HasName("PK__LQV_BaiL__55B63CF62DCD3DE8");

            entity.ToTable("LQV_BaiLam");

            entity.Property(e => e.LqvBaiLamId).HasColumnName("LQV_BaiLamID");
            entity.Property(e => e.LqvDiem).HasColumnName("LQV_Diem");
            entity.Property(e => e.LqvLichThiId).HasColumnName("LQV_LichThiID");
            entity.Property(e => e.LqvThoiGianBatDau)
                .HasColumnType("datetime")
                .HasColumnName("LQV_ThoiGianBatDau");
            entity.Property(e => e.LqvThoiGianNop)
                .HasColumnType("datetime")
                .HasColumnName("LQV_ThoiGianNop");
            entity.Property(e => e.LqvTrangThai)
                .HasMaxLength(50)
                .HasColumnName("LQV_TrangThai");
            entity.Property(e => e.LqvUserId).HasColumnName("LQV_UserID");
        });

        modelBuilder.Entity<LqvBaiTap>(entity =>
        {
            entity.HasKey(e => e.LqvBaiTapId).HasName("PK__LQV_BaiT__57AA0AE6773421D4");

            entity.ToTable("LQV_BaiTap");

            entity.Property(e => e.LqvBaiTapId).HasColumnName("LQV_BaiTapID");
            entity.Property(e => e.LqvGiangVienId).HasColumnName("LQV_GiangVienID");
            entity.Property(e => e.LqvHanNop)
                .HasColumnType("datetime")
                .HasColumnName("LQV_HanNop");
            entity.Property(e => e.LqvLopHocId).HasColumnName("LQV_LopHocID");
            entity.Property(e => e.LqvMoTa).HasColumnName("LQV_MoTa");
            entity.Property(e => e.LqvNgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("LQV_NgayTao");
            entity.Property(e => e.LqvTieuDe)
                .HasMaxLength(200)
                .HasColumnName("LQV_TieuDe");
            entity.Property(e => e.LqvTrangThai)
                .HasMaxLength(50)
                .HasDefaultValue("Đang mở")
                .HasColumnName("LQV_TrangThai");

            entity.HasOne(d => d.LqvGiangVien).WithMany(p => p.LqvBaiTaps)
                .HasForeignKey(d => d.LqvGiangVienId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BaiTap_GiangVien");

            entity.HasOne(d => d.LqvLopHoc).WithMany(p => p.LqvBaiTaps)
                .HasForeignKey(d => d.LqvLopHocId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BaiTap_LopHoc");
        });

        modelBuilder.Entity<LqvBoCauHoi>(entity =>
        {
            entity.HasKey(e => e.LqvBoCauHoiId).HasName("PK__LQV_BoCa__FA85F35FF7B94957");

            entity.ToTable("LQV_BoCauHoi");

            entity.Property(e => e.LqvBoCauHoiId).HasColumnName("LQV_BoCauHoiID");
            entity.Property(e => e.LqvGiangVienId).HasColumnName("LQV_GiangVienID");
            entity.Property(e => e.LqvMoTa)
                .HasMaxLength(500)
                .HasColumnName("LQV_MoTa");
            entity.Property(e => e.LqvNgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("LQV_NgayTao");
            entity.Property(e => e.LqvTenBo)
                .HasMaxLength(200)
                .HasColumnName("LQV_TenBo");

            entity.HasOne(d => d.LqvGiangVien).WithMany(p => p.LqvBoCauHois)
                .HasForeignKey(d => d.LqvGiangVienId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BoCauHoi_GiangVien");
        });

        modelBuilder.Entity<LqvBuoiHoc>(entity =>
        {
            entity.HasKey(e => e.LqvBuoiHocId).HasName("PK__LQV_Buoi__BB4D0D02FBC7D9BA");

            entity.ToTable("LQV_BuoiHoc");

            entity.Property(e => e.LqvBuoiHocId).HasColumnName("LQV_BuoiHocID");
            entity.Property(e => e.LqvGioBatDau).HasColumnName("LQV_GioBatDau");
            entity.Property(e => e.LqvGioKetThuc).HasColumnName("LQV_GioKetThuc");
            entity.Property(e => e.LqvLopHocId).HasColumnName("LQV_LopHocID");
            entity.Property(e => e.LqvNgayHoc).HasColumnName("LQV_NgayHoc");
        });

        modelBuilder.Entity<LqvCauHoi>(entity =>
        {
            entity.HasKey(e => e.LqvCauHoiId).HasName("PK__LQV_CauH__495F3E4088D9713D");

            entity.ToTable("LQV_CauHoi");

            entity.Property(e => e.LqvCauHoiId).HasColumnName("LQV_CauHoiID");
            entity.Property(e => e.LqvBoCauHoiId).HasColumnName("LQV_BoCauHoiID");
            entity.Property(e => e.LqvDiem).HasColumnName("LQV_Diem");
            entity.Property(e => e.LqvLoai)
                .HasMaxLength(50)
                .HasColumnName("LQV_Loai");
            entity.Property(e => e.LqvNoiDung).HasColumnName("LQV_NoiDung");

            entity.HasOne(d => d.LqvBoCauHoi).WithMany(p => p.LqvCauHois)
                .HasForeignKey(d => d.LqvBoCauHoiId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CauHoi_Bo");
        });

        modelBuilder.Entity<LqvChiTietBaiLam>(entity =>
        {
            entity.HasKey(e => e.LqvId).HasName("PK__LQV_ChiT__399ABF83E2F36458");

            entity.ToTable("LQV_ChiTietBaiLam");

            entity.HasIndex(e => new { e.LqvBaiLamId, e.LqvCauHoiId }, "UQ_BaiLam_CauHoi").IsUnique();

            entity.Property(e => e.LqvId).HasColumnName("LQV_ID");
            entity.Property(e => e.LqvBaiLamId).HasColumnName("LQV_BaiLamID");
            entity.Property(e => e.LqvCauHoiId).HasColumnName("LQV_CauHoiID");
            entity.Property(e => e.LqvDaCham).HasColumnName("LQV_DaCham");
            entity.Property(e => e.LqvDapAnId).HasColumnName("LQV_DapAnID");
            entity.Property(e => e.LqvDiem).HasColumnName("LQV_Diem");
            entity.Property(e => e.LqvTraLoiTuLuan).HasColumnName("LQV_TraLoiTuLuan");

            entity.HasOne(d => d.LqvBaiLam).WithMany(p => p.LqvChiTietBaiLams)
                .HasForeignKey(d => d.LqvBaiLamId)
                .HasConstraintName("FK_ChiTietBaiLam_BaiLam");

            entity.HasOne(d => d.LqvCauHoi).WithMany(p => p.LqvChiTietBaiLams)
                .HasForeignKey(d => d.LqvCauHoiId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChiTietBaiLam_CauHoi");

            entity.HasOne(d => d.LqvDapAn).WithMany(p => p.LqvChiTietBaiLams)
                .HasForeignKey(d => d.LqvDapAnId)
                .HasConstraintName("FK_ChiTietBaiLam_DapAn");
        });

        modelBuilder.Entity<LqvChucNang>(entity =>
        {
            entity.HasKey(e => e.LqvChucNangId).HasName("PK__LQV_Chuc__FAAD8ABC81797BAC");

            entity.ToTable("LQV_ChucNang");

            entity.Property(e => e.LqvChucNangId).HasColumnName("LQV_ChucNangID");
            entity.Property(e => e.LqvDuongDan)
                .HasMaxLength(200)
                .HasColumnName("LQV_DuongDan");
            entity.Property(e => e.LqvMoTa)
                .HasMaxLength(500)
                .HasColumnName("LQV_MoTa");
            entity.Property(e => e.LqvTenChucNang)
                .HasMaxLength(150)
                .HasColumnName("LQV_TenChucNang");
        });

        modelBuilder.Entity<LqvChungNhan>(entity =>
        {
            entity.HasKey(e => e.LqvMaChungNhan).HasName("PK__LQV_Chun__7C61D26828A303A5");

            entity.ToTable("LQV_ChungNhan");

            entity.HasIndex(e => e.LqvMaChungNhanCode, "IX_LQV_ChungNhan_Code");

            entity.HasIndex(e => e.LqvMaChungNhanCode, "UQ__LQV_Chun__98F37C26981B1A53").IsUnique();

            entity.Property(e => e.LqvMaChungNhan).HasColumnName("LQV_MaChungNhan");
            entity.Property(e => e.LqvHashValue)
                .HasMaxLength(256)
                .HasColumnName("LQV_HashValue");
            entity.Property(e => e.LqvKhoaHocId).HasColumnName("LQV_KhoaHocID");
            entity.Property(e => e.LqvMaChungNhanCode)
                .HasMaxLength(100)
                .HasColumnName("LQV_MaChungNhanCode");
            entity.Property(e => e.LqvNgayCap)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("LQV_NgayCap");
            entity.Property(e => e.LqvSinhVienId).HasColumnName("LQV_SinhVienID");
            entity.Property(e => e.LqvTrangThai)
                .HasMaxLength(50)
                .HasDefaultValue("Chưa ghi chain")
                .HasColumnName("LQV_TrangThai");

            entity.HasOne(d => d.LqvKhoaHoc).WithMany(p => p.LqvChungNhans)
                .HasForeignKey(d => d.LqvKhoaHocId)
                .HasConstraintName("FK_LQV_ChungNhan_KhoaHoc");

            entity.HasOne(d => d.LqvSinhVien).WithMany(p => p.LqvChungNhans)
                .HasForeignKey(d => d.LqvSinhVienId)
                .HasConstraintName("FK_LQV_ChungNhan_SinhVien");
        });

        modelBuilder.Entity<LqvDangKyLopHoc>(entity =>
        {
            entity.HasKey(e => e.LqvId).HasName("PK__LQV_Dang__399ABF83B02B29EE");

            entity.ToTable("LQV_DangKyLopHoc");

            entity.Property(e => e.LqvId).HasColumnName("LQV_ID");
            entity.Property(e => e.LqvLopHocId).HasColumnName("LQV_LopHocID");
            entity.Property(e => e.LqvNgayDangKy)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("LQV_NgayDangKy");
            entity.Property(e => e.LqvSinhVienId).HasColumnName("LQV_SinhVienID");

            entity.HasOne(d => d.LqvLopHoc).WithMany(p => p.LqvDangKyLopHocs)
                .HasForeignKey(d => d.LqvLopHocId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DK_LopHoc");

            entity.HasOne(d => d.LqvSinhVien).WithMany(p => p.LqvDangKyLopHocs)
                .HasForeignKey(d => d.LqvSinhVienId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DK_SinhVien");
        });

        modelBuilder.Entity<LqvDapAn>(entity =>
        {
            entity.HasKey(e => e.LqvDapAnId).HasName("PK__LQV_DapA__B4CD8321589B8AF6");

            entity.ToTable("LQV_DapAn");

            entity.Property(e => e.LqvDapAnId).HasColumnName("LQV_DapAnID");
            entity.Property(e => e.LqvCauHoiId).HasColumnName("LQV_CauHoiID");
            entity.Property(e => e.LqvDung).HasColumnName("LQV_Dung");
            entity.Property(e => e.LqvNoiDung).HasColumnName("LQV_NoiDung");

            entity.HasOne(d => d.LqvCauHoi).WithMany(p => p.LqvDapAns)
                .HasForeignKey(d => d.LqvCauHoiId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DapAn_CauHoi");
        });

        modelBuilder.Entity<LqvDeThi>(entity =>
        {
            entity.HasKey(e => e.LqvDeThiId).HasName("PK__LQV_DeTh__B0DB988EF7B75BF7");

            entity.ToTable("LQV_DeThi");

            entity.Property(e => e.LqvDeThiId).HasColumnName("LQV_DeThiID");
            entity.Property(e => e.LqvBoCauHoiId).HasColumnName("LQV_BoCauHoiID");
            entity.Property(e => e.LqvTenDeThi)
                .HasMaxLength(200)
                .HasColumnName("LQV_TenDeThi");
            entity.Property(e => e.LqvThoiGianThi).HasColumnName("LQV_ThoiGianThi");
            entity.Property(e => e.LqvTongDiem).HasColumnName("LQV_TongDiem");

            entity.HasOne(d => d.LqvBoCauHoi).WithMany(p => p.LqvDeThis)
                .HasForeignKey(d => d.LqvBoCauHoiId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DeThi_BoCauHoi");
        });

        modelBuilder.Entity<LqvDiemDanhGp>(entity =>
        {
            entity.HasKey(e => e.LqvId).HasName("PK__LQV_Diem__399ABF83D79FEFCB");

            entity.ToTable("LQV_DiemDanhGPS");

            entity.Property(e => e.LqvId).HasColumnName("LQV_ID");
            entity.Property(e => e.LqvBuoiHocId).HasColumnName("LQV_BuoiHocID");
            entity.Property(e => e.LqvHopLe)
                .HasColumnName("LQV_HopLe")
                .IsRequired()
                .ValueGeneratedNever();

            entity.Property(e => e.LqvKinhDo).HasColumnName("LQV_KinhDo");
            entity.Property(e => e.LqvLopHocId).HasColumnName("LQV_LopHocID");
            entity.Property(e => e.LqvSinhVienId).HasColumnName("LQV_SinhVienID");
            entity.Property(e => e.LqvThoiGian)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("LQV_ThoiGian");
            entity.Property(e => e.LqvViDo).HasColumnName("LQV_ViDo");

            entity.HasOne(d => d.LqvBuoiHoc).WithMany(p => p.LqvDiemDanhGps)
                .HasForeignKey(d => d.LqvBuoiHocId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DiemDanh_BuoiHoc");

            entity.HasOne(d => d.LqvLopHoc).WithMany(p => p.LqvDiemDanhGps)
                .HasForeignKey(d => d.LqvLopHocId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DiemDanh_LopHoc");

            entity.HasOne(d => d.LqvSinhVien).WithMany(p => p.LqvDiemDanhGps)
                .HasForeignKey(d => d.LqvSinhVienId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DiemDanh_SinhVien");
        });

        modelBuilder.Entity<LqvGiaoDichBlockchain>(entity =>
        {
            entity.HasKey(e => e.LqvMaGiaoDich).HasName("PK__LQV_Giao__001ACB4ED5CAC3C4");

            entity.ToTable("LQV_GiaoDichBlockchain");

            entity.HasIndex(e => e.LqvTxHash, "IX_LQV_GiaoDich_TxHash");

            entity.Property(e => e.LqvMaGiaoDich).HasColumnName("LQV_MaGiaoDich");
            entity.Property(e => e.LqvBlockNumber).HasColumnName("LQV_BlockNumber");
            entity.Property(e => e.LqvChungNhanId).HasColumnName("LQV_ChungNhanID");
            entity.Property(e => e.LqvGioTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("LQV_GioTao");
            entity.Property(e => e.LqvStatus)
                .HasMaxLength(50)
                .HasDefaultValue("Pending")
                .HasColumnName("LQV_Status");
            entity.Property(e => e.LqvTxHash)
                .HasMaxLength(100)
                .HasColumnName("LQV_TxHash");

            entity.HasOne(d => d.LqvChungNhan).WithMany(p => p.LqvGiaoDichBlockchains)
                .HasForeignKey(d => d.LqvChungNhanId)
                .HasConstraintName("FK_LQV_GiaoDich_ChungNhan");
        });

        modelBuilder.Entity<LqvKhoaHoc>(entity =>
        {
            entity.HasKey(e => e.LqvMaKhoaHoc).HasName("PK__LQV_Khoa__00BA6E76ACA376DC");

            entity.ToTable("LQV_KhoaHoc");

            entity.Property(e => e.LqvMaKhoaHoc).HasColumnName("LQV_MaKhoaHoc");
            entity.Property(e => e.LqvGiangVienId).HasColumnName("LQV_GiangVienID");
            entity.Property(e => e.LqvMoTa)
                .HasMaxLength(1000)
                .HasColumnName("LQV_MoTa");
            entity.Property(e => e.LqvNgayBatDau)
                .HasColumnType("datetime")
                .HasColumnName("LQV_NgayBatDau");
            entity.Property(e => e.LqvNgayKetThuc)
                .HasColumnType("datetime")
                .HasColumnName("LQV_NgayKetThuc");
            entity.Property(e => e.LqvTenKhoaHoc)
                .HasMaxLength(200)
                .HasColumnName("LQV_TenKhoaHoc");

            entity.HasOne(d => d.LqvGiangVien).WithMany(p => p.LqvKhoaHocs)
                .HasForeignKey(d => d.LqvGiangVienId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_LQV_KhoaHoc_GiangVien");
        });

        modelBuilder.Entity<LqvLichThi>(entity =>
        {
            entity.HasKey(e => e.LqvLichThiId).HasName("PK__LQV_Lich__76CDCC5C5CE45949");

            entity.ToTable("LQV_LichThi");

            entity.Property(e => e.LqvLichThiId).HasColumnName("LQV_LichThiID");
            entity.Property(e => e.LqvBatDau)
                .HasColumnType("datetime")
                .HasColumnName("LQV_BatDau");
            entity.Property(e => e.LqvDeThiId).HasColumnName("LQV_DeThiID");
            entity.Property(e => e.LqvKetThuc)
                .HasColumnType("datetime")
                .HasColumnName("LQV_KetThuc");
            entity.Property(e => e.LqvLopHocId).HasColumnName("LQV_LopHocID");

            entity.HasOne(d => d.LqvDeThi).WithMany(p => p.LqvLichThis)
                .HasForeignKey(d => d.LqvDeThiId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LichThi_DeThi");

            entity.HasOne(d => d.LqvLopHoc).WithMany(p => p.LqvLichThis)
                .HasForeignKey(d => d.LqvLopHocId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LichThi_LopHoc");
        });

        modelBuilder.Entity<LqvLopHoc>(entity =>
        {
            entity.HasKey(e => e.LqvLopHocId).HasName("PK__LQV_LopH__B68A262EA6901F01");

            entity.ToTable("LQV_LopHoc");

            entity.Property(e => e.LqvLopHocId).HasColumnName("LQV_LopHocID");
            entity.Property(e => e.LqvGiangVienId).HasColumnName("LQV_GiangVienID");
            entity.Property(e => e.LqvKhoaHocId).HasColumnName("LQV_KhoaHocID");
            entity.Property(e => e.LqvMoTa)
                .HasMaxLength(500)
                .HasColumnName("LQV_MoTa");
            entity.Property(e => e.LqvNgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("LQV_NgayTao");
            entity.Property(e => e.LqvTenLop)
                .HasMaxLength(200)
                .HasColumnName("LQV_TenLop");

            entity.HasOne(d => d.LqvGiangVien).WithMany(p => p.LqvLopHocs)
                .HasForeignKey(d => d.LqvGiangVienId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LopHoc_GiangVien");

            entity.HasOne(d => d.LqvKhoaHoc).WithMany(p => p.LqvLopHocs)
                .HasForeignKey(d => d.LqvKhoaHocId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LopHoc_KhoaHoc");
        });

        modelBuilder.Entity<LqvNguoiDung>(entity =>
        {
            entity.HasKey(e => e.LqvId).HasName("PK__LQV_Nguo__399ABF836CDD5D3E");

            entity.ToTable("LQV_NguoiDung");

            entity.HasIndex(e => e.LqvWalletAddress, "IX_LQV_NguoiDung_Wallet");

            entity.Property(e => e.LqvId).HasColumnName("LQV_ID");
            entity.Property(e => e.LqvDaXacThuc)
                .HasDefaultValue(false)
                .HasColumnName("LQV_DaXacThuc");
            entity.Property(e => e.LqvEmail)
                .HasMaxLength(150)
                .HasColumnName("LQV_Email");
            entity.Property(e => e.LqvHoTen)
                .HasMaxLength(150)
                .HasColumnName("LQV_HoTen");
            entity.Property(e => e.LqvMatKhauHash)
                .HasMaxLength(256)
                .HasColumnName("LQV_MatKhauHash");
            entity.Property(e => e.LqvNgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("LQV_NgayTao");
            entity.Property(e => e.LqvRoleId).HasColumnName("LQV_RoleID");
            entity.Property(e => e.LqvTenDangNhap)
                .HasMaxLength(100)
                .HasColumnName("LQV_TenDangNhap");
            entity.Property(e => e.LqvWalletAddress)
                .HasMaxLength(100)
                .HasColumnName("LQV_WalletAddress");

            entity.HasOne(d => d.LqvRole).WithMany(p => p.LqvNguoiDungs)
                .HasForeignKey(d => d.LqvRoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LQV_NguoiDung_Role");
        });

        modelBuilder.Entity<LqvNhatKyHoatDong>(entity =>
        {
            entity.HasKey(e => e.LqvId).HasName("PK__LQV_Nhat__399ABFA3BAA56476");

            entity.ToTable("LQV_NhatKyHoatDong");

            entity.Property(e => e.LqvId).HasColumnName("LQV_Id");
            entity.Property(e => e.LqvChiTiet).HasColumnName("LQV_ChiTiet");
            entity.Property(e => e.LqvHanhDong)
                .HasMaxLength(255)
                .HasColumnName("LQV_HanhDong");
            entity.Property(e => e.LqvIp)
                .HasMaxLength(50)
                .HasColumnName("LQV_Ip");
            entity.Property(e => e.LqvTaiKhoan)
                .HasMaxLength(100)
                .HasColumnName("LQV_TaiKhoan");
            entity.Property(e => e.LqvThoiGian)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("LQV_ThoiGian");
        });

        modelBuilder.Entity<LqvNopBaiTap>(entity =>
        {
            entity.HasKey(e => e.LqvId).HasName("PK__LQV_NopB__399ABF83B2BDB053");

            entity.ToTable("LQV_NopBaiTap");

            entity.HasIndex(e => new { e.LqvBaiTapId, e.LqvSinhVienId }, "UQ_NopBai").IsUnique();

            entity.Property(e => e.LqvId).HasColumnName("LQV_ID");
            entity.Property(e => e.LqvBaiTapId).HasColumnName("LQV_BaiTapID");
            entity.Property(e => e.LqvDaCham).HasColumnName("LQV_DaCham");
            entity.Property(e => e.LqvDiem).HasColumnName("LQV_Diem");
            entity.Property(e => e.LqvFile)
                .HasMaxLength(255)
                .HasColumnName("LQV_File");
            entity.Property(e => e.LqvNhanXet).HasColumnName("LQV_NhanXet");
            entity.Property(e => e.LqvNoiDung).HasColumnName("LQV_NoiDung");
            entity.Property(e => e.LqvSinhVienId).HasColumnName("LQV_SinhVienID");
            entity.Property(e => e.LqvThoiGianNop)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("LQV_ThoiGianNop");

            entity.HasOne(d => d.LqvBaiTap).WithMany(p => p.LqvNopBaiTaps)
                .HasForeignKey(d => d.LqvBaiTapId)
                .HasConstraintName("FK_NopBai_BaiTap");

            entity.HasOne(d => d.LqvSinhVien).WithMany(p => p.LqvNopBaiTaps)
                .HasForeignKey(d => d.LqvSinhVienId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NopBai_SinhVien");
        });

        modelBuilder.Entity<LqvPhanQuyen>(entity =>
        {
            entity.HasKey(e => e.LqvPhanQuyenId).HasName("PK__LQV_Phan__511351DF0E3ADF8F");

            entity.ToTable("LQV_PhanQuyen");

            entity.Property(e => e.LqvPhanQuyenId).HasColumnName("LQV_PhanQuyenID");
            entity.Property(e => e.LqvChoPhep)
                .HasDefaultValue(true)
                .HasColumnName("LQV_ChoPhep");
            entity.Property(e => e.LqvChucNangId).HasColumnName("LQV_ChucNangID");
            entity.Property(e => e.LqvRoleId).HasColumnName("LQV_RoleID");

            entity.HasOne(d => d.LqvChucNang).WithMany(p => p.LqvPhanQuyens)
                .HasForeignKey(d => d.LqvChucNangId)
                .HasConstraintName("FK_LQV_PhanQuyen_ChucNang");

            entity.HasOne(d => d.LqvRole).WithMany(p => p.LqvPhanQuyens)
                .HasForeignKey(d => d.LqvRoleId)
                .HasConstraintName("FK_LQV_PhanQuyen_Role");
        });

        modelBuilder.Entity<LqvRole>(entity =>
        {
            entity.HasKey(e => e.LqvRoleId).HasName("PK__LQV_Role__1C56210837B04E10");

            entity.ToTable("LQV_Role");

            entity.HasIndex(e => e.LqvRoleName, "UQ__LQV_Role__2D9D26F63B2DA1B8").IsUnique();

            entity.Property(e => e.LqvRoleId).HasColumnName("LQV_RoleID");
            entity.Property(e => e.LqvRoleName)
                .HasMaxLength(50)
                .HasColumnName("LQV_RoleName");
        });

        modelBuilder.Entity<LqvTienDoHocTap>(entity =>
        {
            entity.HasKey(e => e.LqvId);

            entity.ToTable("LQV_TienDoHocTap");

            entity.HasIndex(e => e.LqvKhoaHocId, "IX_LQV_TienDoHocTap_KhoaHocID");

            entity.HasIndex(e => e.LqvSinhVienId, "IX_LQV_TienDoHocTap_SinhVienID");

            entity.HasIndex(e => new { e.LqvSinhVienId, e.LqvKhoaHocId }, "UQ_LQV_TienDoHocTap_SinhVien_KhoaHoc").IsUnique();

            entity.Property(e => e.LqvId).HasColumnName("LQV_Id");
            entity.Property(e => e.LqvKhoaHocId).HasColumnName("LQV_KhoaHocId");
            entity.Property(e => e.LqvNgayCapNhat)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("LQV_NgayCapNhat");
            entity.Property(e => e.LqvSinhVienId).HasColumnName("LQV_SinhVienId");
            entity.Property(e => e.LqvTiLeHoanThanh).HasColumnName("LQV_TiLeHoanThanh");

            entity.HasOne(d => d.LqvKhoaHoc).WithMany(p => p.LqvTienDoHocTaps)
                .HasForeignKey(d => d.LqvKhoaHocId)
                .HasConstraintName("FK_LQV_TienDoHocTap_KhoaHoc");

            entity.HasOne(d => d.LqvSinhVien).WithMany(p => p.LqvTienDoHocTaps)
                .HasForeignKey(d => d.LqvSinhVienId)
                .HasConstraintName("FK_LQV_TienDoHocTap_NguoiDung");
        });

        modelBuilder.Entity<LqvXacThucChungNhan>(entity =>
        {
            entity.HasKey(e => e.LqvId).HasName("PK__LQV_XacT__399ABF8304A8D876");

            entity.ToTable("LQV_XacThucChungNhan");

            entity.HasIndex(e => e.LqvMaChungNhanCode, "IX_LQV_XacThuc_MaCert");

            entity.Property(e => e.LqvId).HasColumnName("LQV_ID");
            entity.Property(e => e.LqvDiaChiNguoiXacThuc)
                .HasMaxLength(100)
                .HasColumnName("LQV_DiaChiNguoiXacThuc");
            entity.Property(e => e.LqvKetQua)
                .HasMaxLength(50)
                .HasColumnName("LQV_KetQua");
            entity.Property(e => e.LqvMaChungNhanCode)
                .HasMaxLength(100)
                .HasColumnName("LQV_MaChungNhanCode");
            entity.Property(e => e.LqvThoiGianXacThuc)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("LQV_ThoiGianXacThuc");
        });

        modelBuilder.Entity<LqvXacThucEmail>(entity =>
        {
            entity.HasKey(e => e.LqvMaXacThuc).HasName("PK__LQV_XacT__04AC4BAE82B51BB4");

            entity.ToTable("LQV_XacThucEmail");

            entity.Property(e => e.LqvMaXacThuc).HasColumnName("LQV_MaXacThuc");
            entity.Property(e => e.LqvEmail)
                .HasMaxLength(150)
                .HasColumnName("LQV_Email");
            entity.Property(e => e.LqvLoaiXacThuc)
                .HasMaxLength(50)
                .HasColumnName("LQV_LoaiXacThuc");
            entity.Property(e => e.LqvMaOtp)
                .HasMaxLength(10)
                .HasColumnName("LQV_MaOTP");
            entity.Property(e => e.LqvThoiGianHetHan)
                .HasColumnType("datetime")
                .HasColumnName("LQV_ThoiGianHetHan");
            entity.Property(e => e.LqvThoiGianTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("LQV_ThoiGianTao");
            entity.Property(e => e.LqvTrangThai).HasColumnName("LQV_TrangThai");
        });

        modelBuilder.Entity<LqvYeuCauCapChungChi>(entity =>
        {
            entity.HasKey(e => e.LqvId);

            entity.ToTable("LQV_YeuCauCapChungChi");

            entity.HasIndex(e => e.LqvKhoaHocId, "IX_LQV_YeuCauCapChungChi_LQV_KhoaHocID");

            entity.HasIndex(e => e.LqvNguoiDungId, "IX_LQV_YeuCauCapChungChi_LQV_NguoiDungID");

            entity.Property(e => e.LqvId).HasColumnName("LQV_ID");
            entity.Property(e => e.LqvKhoaHocId).HasColumnName("LQV_KhoaHocID");
            entity.Property(e => e.LqvLyDoTuChoi).HasMaxLength(1000);
            entity.Property(e => e.LqvLyDoYeuCau)
                .HasMaxLength(1000)
                .HasColumnName("LQV_LyDoYeuCau");
            entity.Property(e => e.LqvNgayYeuCau)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("LQV_NgayYeuCau");
            entity.Property(e => e.LqvNguoiDungId).HasColumnName("LQV_NguoiDungID");
            entity.Property(e => e.LqvTrangThai)
                .HasMaxLength(50)
                .HasDefaultValue("Chờ duyệt")
                .HasColumnName("LQV_TrangThai");

            entity.HasOne(d => d.LqvKhoaHoc).WithMany(p => p.LqvYeuCauCapChungChis)
                .HasForeignKey(d => d.LqvKhoaHocId)
                .HasConstraintName("FK_LQV_YeuCauCapChungChi_KhoaHoc");

            entity.HasOne(d => d.LqvNguoiDung).WithMany(p => p.LqvYeuCauCapChungChis)
                .HasForeignKey(d => d.LqvNguoiDungId)
                .HasConstraintName("FK_LQV_YeuCauCapChungChi_NguoiDung");
        });

        modelBuilder.Entity<LqvYeuCauHoTro>(entity =>
        {
            entity.HasKey(e => e.LqvId);

            entity.ToTable("LQV_YeuCauHoTro");

            entity.HasIndex(e => e.LqvNguoiDungId, "IX_LQV_YeuCauHoTro_LQV_NguoiDungID");

            entity.Property(e => e.LqvId).HasColumnName("LQV_ID");
            entity.Property(e => e.LqvNguoiDungId).HasColumnName("LQV_NguoiDungID");
            entity.Property(e => e.LqvNoiDung).HasColumnName("LQV_NoiDung");
            entity.Property(e => e.LqvPhanHoi).HasColumnName("LQV_PhanHoi");
            entity.Property(e => e.LqvThoiGianGui)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("LQV_ThoiGianGui");
            entity.Property(e => e.LqvTrangThai)
                .HasMaxLength(50)
                .HasDefaultValue("Mới")
                .HasColumnName("LQV_TrangThai");

            entity.HasOne(d => d.LqvNguoiDung).WithMany(p => p.LqvYeuCauHoTros)
                .HasForeignKey(d => d.LqvNguoiDungId)
                .HasConstraintName("FK_LQV_YeuCauHoTro_NguoiDung");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}