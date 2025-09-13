using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebTinTuc.Models;

public partial class WebTinTucContext : DbContext
{
    public WebTinTucContext()
    {
    }

    public WebTinTucContext(DbContextOptions<WebTinTucContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BaiViet> BaiViets { get; set; }

    public virtual DbSet<BinhLuan> BinhLuans { get; set; }

    public virtual DbSet<ChuDe> ChuDes { get; set; }

    public virtual DbSet<NguoiDung> NguoiDungs { get; set; }

    public virtual DbSet<QuyenHan> QuyenHans { get; set; }

    public virtual DbSet<TrangThaiBaiViet> TrangThaiBaiViets { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Connection string được cấu hình trong Program.cs thông qua dependency injection
        // Không cần cấu hình ở đây nữa
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BaiViet>(entity =>
        {
            entity.HasKey(e => e.IdbaiViet).HasName("PK__BaiViet__FC50A2077A3AB769");

            entity.ToTable("BaiViet");

            entity.HasIndex(e => e.Slug, "UQ__BaiViet__BC7B5FB65D30EC85").IsUnique();

            entity.Property(e => e.IdbaiViet).HasColumnName("IDBaiViet");
            entity.Property(e => e.GhiChuDuyet).HasMaxLength(500);
            entity.Property(e => e.IdchuDe).HasColumnName("IDChuDe");
            entity.Property(e => e.IdnguoiDuyet).HasColumnName("IDNguoiDuyet");
            entity.Property(e => e.IdtacGia).HasColumnName("IDTacGia");
            entity.Property(e => e.IdtrangThai).HasColumnName("IDTrangThai");
            entity.Property(e => e.LaTinNong).HasDefaultValue(false);
            entity.Property(e => e.LuotXem).HasDefaultValue(0);
            entity.Property(e => e.NgayChinhSuaCuoi).HasColumnType("datetime");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NgayXuatBan).HasColumnType("datetime");
            entity.Property(e => e.Slug)
                .HasMaxLength(280)
                .IsUnicode(false);
            entity.Property(e => e.TieuDe).HasMaxLength(255);
            entity.Property(e => e.TomTat).HasMaxLength(500);
            entity.Property(e => e.UrlanhBia)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("URLAnhBia");

            entity.HasOne(d => d.IdchuDeNavigation).WithMany(p => p.BaiViets)
                .HasForeignKey(d => d.IdchuDe)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BaiViet_ChuDe");

            entity.HasOne(d => d.IdnguoiDuyetNavigation).WithMany(p => p.BaiVietIdnguoiDuyetNavigations)
                .HasForeignKey(d => d.IdnguoiDuyet)
                .HasConstraintName("FK_BaiViet_NguoiDung_NguoiDuyet");

            entity.HasOne(d => d.IdtacGiaNavigation).WithMany(p => p.BaiVietIdtacGiaNavigations)
                .HasForeignKey(d => d.IdtacGia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BaiViet_NguoiDung_TacGia");

            entity.HasOne(d => d.IdtrangThaiNavigation).WithMany(p => p.BaiViets)
                .HasForeignKey(d => d.IdtrangThai)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BaiViet_TrangThaiBaiViet");
        });

        modelBuilder.Entity<BinhLuan>(entity =>
        {
            entity.HasKey(e => e.IdbinhLuan).HasName("PK__BinhLuan__5CDBC03C6126055F");

            entity.ToTable("BinhLuan");

            entity.Property(e => e.IdbinhLuan).HasColumnName("IDBinhLuan");
            entity.Property(e => e.DaDuyet).HasDefaultValue(true);
            entity.Property(e => e.IdbaiViet).HasColumnName("IDBaiViet");
            entity.Property(e => e.IdbinhLuanCha).HasColumnName("IDBinhLuanCha");
            entity.Property(e => e.IdnguoiDung).HasColumnName("IDNguoiDung");
            entity.Property(e => e.NgayBinhLuan)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NoiDung).HasMaxLength(1000);

            entity.HasOne(d => d.IdbaiVietNavigation).WithMany(p => p.BinhLuans)
                .HasForeignKey(d => d.IdbaiViet)
                .HasConstraintName("FK_BinhLuan_BaiViet");

            entity.HasOne(d => d.IdbinhLuanChaNavigation).WithMany(p => p.InverseIdbinhLuanChaNavigation)
                .HasForeignKey(d => d.IdbinhLuanCha)
                .HasConstraintName("FK_BinhLuan_BinhLuanCha");

            entity.HasOne(d => d.IdnguoiDungNavigation).WithMany(p => p.BinhLuans)
                .HasForeignKey(d => d.IdnguoiDung)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BinhLuan_NguoiDung");
        });

        modelBuilder.Entity<ChuDe>(entity =>
        {
            entity.HasKey(e => e.IdchuDe).HasName("PK__ChuDe__5C130CB860E86E70");

            entity.ToTable("ChuDe");

            entity.HasIndex(e => e.TenChuDe, "UQ__ChuDe__19B17CFB05B99F8B").IsUnique();

            entity.HasIndex(e => e.Slug, "UQ__ChuDe__BC7B5FB63F297360").IsUnique();

            entity.Property(e => e.IdchuDe).HasColumnName("IDChuDe");
            entity.Property(e => e.DaKichHoat).HasDefaultValue(true);
            entity.Property(e => e.Slug)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.TenChuDe).HasMaxLength(100);
            entity.Property(e => e.ThuTuHienThi).HasDefaultValue(0);
        });

        modelBuilder.Entity<NguoiDung>(entity =>
        {
            entity.HasKey(e => e.IdnguoiDung).HasName("PK__NguoiDun__FCD7DB094B2A2510");

            entity.ToTable("NguoiDung");

            entity.HasIndex(e => e.SoDienThoai, "UQ__NguoiDun__0389B7BD4AE18716").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__NguoiDun__A9D10534F006F6EC").IsUnique();

            entity.Property(e => e.IdnguoiDung).HasColumnName("IDNguoiDung");
            entity.Property(e => e.DaKichHoat).HasDefaultValue(true);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.IdquyenHan).HasColumnName("IDQuyenHan");
            entity.Property(e => e.LanDangNhapCuoi).HasColumnType("datetime");
            entity.Property(e => e.MatKhauHash)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.NgayDangKy)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UrlanhDaiDien)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("URLAnhDaiDien");

            entity.HasOne(d => d.IdquyenHanNavigation).WithMany(p => p.NguoiDungs)
                .HasForeignKey(d => d.IdquyenHan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NguoiDung_QuyenHan");
        });

        modelBuilder.Entity<QuyenHan>(entity =>
        {
            entity.HasKey(e => e.IdquyenHan).HasName("PK__QuyenHan__75DDDDCD795297F7");

            entity.ToTable("QuyenHan");

            entity.HasIndex(e => e.TenQuyenHan, "UQ__QuyenHan__DC5279DA86D1C2B3").IsUnique();

            entity.Property(e => e.IdquyenHan).HasColumnName("IDQuyenHan");
            entity.Property(e => e.MoTa).HasMaxLength(255);
            entity.Property(e => e.TenQuyenHan).HasMaxLength(50);
        });

        modelBuilder.Entity<TrangThaiBaiViet>(entity =>
        {
            entity.HasKey(e => e.IdtrangThai).HasName("PK__TrangTha__556586002D576410");

            entity.ToTable("TrangThaiBaiViet");

            entity.HasIndex(e => e.TenTrangThai, "UQ__TrangTha__9489EF66C0A03846").IsUnique();

            entity.Property(e => e.IdtrangThai).HasColumnName("IDTrangThai");
            entity.Property(e => e.MoTa).HasMaxLength(255);
            entity.Property(e => e.TenTrangThai).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
