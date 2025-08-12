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

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<QuestionOption> QuestionOptions { get; set; }

    public virtual DbSet<Response> Responses { get; set; }

    public virtual DbSet<SurveyResponse> SurveyResponses { get; set; }

    public virtual DbSet<SurveysMaster> SurveysMasters { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=103.120.179.247;Initial Catalog=padhyaso_seed;Persist Security Info=True;User ID=padhyaso_seed;Password=h2Ee8^z3mvxu9sUMg;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("padhyaso_seed");

        modelBuilder.Entity<Question>(entity =>
        {
            entity.ToTable("Questions", "dbo");
        });

        modelBuilder.Entity<QuestionOption>(entity =>
        {
            entity.ToTable("Question_Options", "dbo");
        });

        modelBuilder.Entity<Response>(entity =>
        {
            entity.ToTable("Responses", "dbo");

            entity.Property(e => e.NumericAnswer).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<SurveyResponse>(entity =>
        {
            entity.ToTable("SurveyResponses", "dbo");
        });

        modelBuilder.Entity<SurveysMaster>(entity =>
        {
            entity.ToTable("Surveys_Master", "dbo");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
