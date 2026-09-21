using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SIGEMAC_ONPE.Models;

public partial class SigemacOnpeContext : DbContext
{
    public SigemacOnpeContext()
    {
    }

    public SigemacOnpeContext(DbContextOptions<SigemacOnpeContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Asistencium> Asistencia { get; set; }

    public virtual DbSet<Capacitador> Capacitadors { get; set; }

    public virtual DbSet<MaterialCapacitacion> MaterialCapacitacions { get; set; }

    public virtual DbSet<MiembroMesa> MiembroMesas { get; set; }

    public virtual DbSet<Odpe> Odpes { get; set; }

    public virtual DbSet<SesionCapacitacion> SesionCapacitacions { get; set; }

    public virtual DbSet<SesionMaterial> SesionMaterials { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Asistencium>(entity =>
        {
            entity.HasKey(e => e.IdAsistencia).HasName("PK__Asistenc__3956DEE6DC74A459");

            entity.HasIndex(e => new { e.DniMiembroMesa, e.IdSesion }, "UQ_Asistencia_MiembroSesion").IsUnique();

            entity.Property(e => e.DniMiembroMesa)
                .HasMaxLength(8)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Observacion)
                .HasMaxLength(250)
                .IsUnicode(false);

            entity.HasOne(d => d.DniMiembroMesaNavigation).WithMany(p => p.Asistencia)
                .HasForeignKey(d => d.DniMiembroMesa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Asistencia_Miembro");

            entity.HasOne(d => d.IdSesionNavigation).WithMany(p => p.Asistencia)
                .HasForeignKey(d => d.IdSesion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Asistencia_Sesion");
        });

        modelBuilder.Entity<Capacitador>(entity =>
        {
            entity.HasKey(e => e.IdCapacitador).HasName("PK__Capacita__E4C835EFD3F71454");

            entity.ToTable("Capacitador");

            entity.HasIndex(e => e.CodigoCapacitador, "UQ__Capacita__C92D97223F14203D").IsUnique();

            entity.Property(e => e.CodigoCapacitador)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Especialidad)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Capacitadors)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Capacitador_Usuario");
        });

        modelBuilder.Entity<MaterialCapacitacion>(entity =>
        {
            entity.HasKey(e => e.IdMaterial).HasName("PK__Material__94356E58C8B7094D");

            entity.ToTable("MaterialCapacitacion");

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Tipo)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.Titulo)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.UrlRecurso)
                .HasMaxLength(300)
                .IsUnicode(false);
        });

        modelBuilder.Entity<MiembroMesa>(entity =>
        {
            entity.HasKey(e => e.Dni).HasName("PK__MiembroM__C0308574C4A4424F");

            entity.ToTable("MiembroMesa");

            entity.Property(e => e.Dni)
                .HasMaxLength(8)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Apellidos)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Cargo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.EstadoCapacitacion)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("Designado");
            entity.Property(e => e.Nombres)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.IdOdpeNavigation).WithMany(p => p.MiembroMesas)
                .HasForeignKey(d => d.IdOdpe)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MiembroMesa_Odpe");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.MiembroMesas)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK_MiembroMesa_Usuario");
        });

        modelBuilder.Entity<Odpe>(entity =>
        {
            entity.HasKey(e => e.IdOdpe).HasName("PK__ODPE__84F385CC2D199ADC");

            entity.ToTable("ODPE");

            entity.Property(e => e.Direccion)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.NombreOdpe)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Region)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<SesionCapacitacion>(entity =>
        {
            entity.HasKey(e => e.IdSesion).HasName("PK__SesionCa__22EC535B73B40E1B");

            entity.ToTable("SesionCapacitacion");

            entity.Property(e => e.Direccion)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.FechaHora).HasColumnType("datetime");
            entity.Property(e => e.Modalidad)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Sede)
                .HasMaxLength(150)
                .IsUnicode(false);

            entity.HasOne(d => d.IdCapacitadorNavigation).WithMany(p => p.SesionCapacitacions)
                .HasForeignKey(d => d.IdCapacitador)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sesion_Capacitador");
        });

        modelBuilder.Entity<SesionMaterial>(entity =>
        {
            entity.HasKey(e => new { e.IdSesion, e.IdMaterial }).HasName("PK__SesionMa__BBAF05BE3FBE68D8");

            entity.ToTable("SesionMaterial");

            entity.Property(e => e.FechaAsignacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.IdMaterialNavigation).WithMany(p => p.SesionMaterials)
                .HasForeignKey(d => d.IdMaterial)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SesionMaterial_Material");

            entity.HasOne(d => d.IdSesionNavigation).WithMany(p => p.SesionMaterials)
                .HasForeignKey(d => d.IdSesion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SesionMaterial_Sesion");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__Usuario__5B65BF9713D79CDE");

            entity.ToTable("Usuario");

            entity.HasIndex(e => e.Username, "UQ__Usuario__536C85E42F37ACC3").IsUnique();

            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(256)
                .IsUnicode(false);
            entity.Property(e => e.Rol)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
