using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Seed_Admin.Models_Temp;

public partial class PadhyasoSeedContext : DbContext
{
    public PadhyasoSeedContext()
    {
    }

    public PadhyasoSeedContext(DbContextOptions<PadhyasoSeedContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Plant> Plants { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=103.120.179.247;Initial Catalog=padhyaso_seed;Persist Security Info=True;User ID=padhyaso_seed;Password=h2Ee8^z3mvxu9sUMg;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("padhyaso_seed");

        modelBuilder.Entity<Plant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Plant__98FE46BC328F1224");

            entity.ToTable("Plant", "dbo");

            entity.Property(e => e.AddressLine1).HasMaxLength(200);
            entity.Property(e => e.AddressLine2).HasMaxLength(200);
            entity.Property(e => e.CityId).HasColumnName("City_Id");
            entity.Property(e => e.CountryId).HasColumnName("Country_Id");
            entity.Property(e => e.DistrictId).HasColumnName("District_Id");
            entity.Property(e => e.PlantCode).HasMaxLength(50);
            entity.Property(e => e.PlantName).HasMaxLength(150);
            entity.Property(e => e.StateId).HasColumnName("State_Id");
            entity.Property(e => e.TalukaId).HasColumnName("Taluka_Id");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
