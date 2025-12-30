using Microsoft.EntityFrameworkCore;
using Shared.Core.Entities;

namespace Shared.Infrastructure.Data;

public class OperationResilientDbContext : DbContext
{
    public OperationResilientDbContext(DbContextOptions<OperationResilientDbContext> options) : base(options)
    {
    }

    public DbSet<Request> Requests { get; set; }
    public DbSet<Aggregate> Aggregates { get; set; }
    public DbSet<DataSourceResultEntity> DataSourceResults { get; set; }
    public DbSet<FailureLogEntity> FailureLogs { get; set; }

    public DbSet<CertomoBankDataEntity> CertomoBankData { get; set; }
    public DbSet<CertomoAccountEntity> CertomoAccounts { get; set; }
    public DbSet<CertomoTransactionEntity> CertomoTransactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Request>().HasKey(r => r.Id);

        modelBuilder.Entity<Aggregate>().HasKey(a => a.Id);
        modelBuilder.Entity<Aggregate>()
            .HasMany(a => a.SourceResults)
            .WithOne()
            .HasForeignKey(s => s.AggregateId);

        modelBuilder.Entity<DataSourceResultEntity>().HasKey(d => d.Id);
        modelBuilder.Entity<DataSourceResultEntity>()
            .HasIndex(d => new { d.AggregateId, d.Source })
            .IsUnique();

        modelBuilder.Entity<FailureLogEntity>().HasKey(f => f.Id);

        // Certomo Configuration
        modelBuilder.Entity<CertomoBankDataEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DateTimeOfLastUpdate).HasConversion(
                v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            
            entity.HasMany(e => e.Accounts)
                .WithOne()
                .HasForeignKey(a => a.BankDataId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(e => e.Transactions)
                .WithOne()
                .HasForeignKey(t => t.BankDataId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CertomoAccountEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.OriginalAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Rate).HasColumnType("decimal(18,4)");
        });

        modelBuilder.Entity<CertomoTransactionEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Balance).HasColumnType("decimal(18,2)");
            entity.Property(e => e.BalanceBefore).HasColumnType("decimal(18,2)");
            
            entity.Property(e => e.Date).HasConversion(
                v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            entity.Property(e => e.ValueDate).HasConversion(
                v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        });
    }
}
