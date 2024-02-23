
using System.Reflection;
using System.Reflection.Emit;
using MagellanGPT.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MagellanGPT.Infrastructure.Persistence;

/// <summary>
/// https://github.com/dotnet/EntityFramework.Docs/blob/main/samples/core/Cosmos/ModelBuilding
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public DbSet<Conversation> Conversations { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        builder.HasDefaultContainer("Conversation");

        builder.Entity<Conversation>()
            .ToContainer(nameof(Conversation))
            .HasPartitionKey(o => o.Id)
            .HasNoDiscriminator();

        base.OnModelCreating(builder);
    }
}

