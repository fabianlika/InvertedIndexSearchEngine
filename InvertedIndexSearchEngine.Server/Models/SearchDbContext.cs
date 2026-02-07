using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace InvertedIndexSearchEngine.Server.Models;

public partial class SearchDbContext : DbContext
{
    public SearchDbContext()
    {
    }

    public SearchDbContext(DbContextOptions<SearchDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Document> Documents { get; set; }

    public virtual DbSet<InvertedIndex> InvertedIndices { get; set; }

    public virtual DbSet<Term> Terms { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost\\MSSQLSERVER07;Database=InvertedIndexDB;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Document__3214EC071FF5E892");

            entity.Property(e => e.Title).HasMaxLength(255);
        });

        modelBuilder.Entity<InvertedIndex>(entity =>
        {
            entity.HasKey(e => new { e.TermId, e.DocumentId }).HasName("PK__Inverted__A0A1CF55CEC06456");

            entity.ToTable("InvertedIndex");

            entity.HasOne(d => d.Document).WithMany(p => p.InvertedIndices)
                .HasForeignKey(d => d.DocumentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__InvertedI__Docum__3D5E1FD2");

            entity.HasOne(d => d.Term).WithMany(p => p.InvertedIndices)
                .HasForeignKey(d => d.TermId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__InvertedI__TermI__3C69FB99");
        });

        modelBuilder.Entity<Term>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Terms__3214EC076C3C41A4");

            entity.HasIndex(e => e.Word, "UQ__Terms__95B50108380BC580").IsUnique();

            entity.Property(e => e.Word).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
