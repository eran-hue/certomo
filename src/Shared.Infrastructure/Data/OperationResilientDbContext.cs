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
    }
}
