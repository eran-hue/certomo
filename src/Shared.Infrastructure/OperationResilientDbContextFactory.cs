using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data;

namespace Shared.Infrastructure;

public class OperationResilientDbContextFactory : IDesignTimeDbContextFactory<OperationResilientDbContext>
{
    public OperationResilientDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OperationResilientDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=OperationResilientNumbers;Username=postgres;Password=postgres");

        return new OperationResilientDbContext(optionsBuilder.Options);
    }
}
