using System.Reflection;
using System.Reflection.Emit;
using MagellanGPT.Application.Common.Interfaces;
using MagellanGPT.Domain.Entities;
using MagellanGPT.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MagellanGPT.Infrastructure.Persistence;

/// <summary>
/// https://github.com/dotnet/EntityFramework.Docs/blob/main/samples/core/Cosmos/ModelBuilding
/// https://learn.microsoft.com/en-us/ef/core/providers/cosmos/limitations
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public DbSet<ApplicationUser> ApplicationUser { get; set; }
    public DbSet<User> User { get; set; }
    public DbSet<Organisation> Organisation { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        builder.Entity<ApplicationUser>()
        .ToContainer(nameof(ApplicationUser))
        .HasPartitionKey(u => u.PartitionKey)
        .HasNoDiscriminator()
        .Property(o => o.Id).ToJsonProperty("id");

        base.OnModelCreating(builder);
    }
}

