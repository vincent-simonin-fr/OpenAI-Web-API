
using System.Reflection;
using System.Reflection.Emit;
using MagellanGPT.Application.Common.Interfaces;
using MagellanGPT.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace MagellanGPT.Infrastructure.Persistence;

/// <summary>
/// https://github.com/dotnet/EntityFramework.Docs/blob/main/samples/core/Cosmos/ModelBuilding
/// https://learn.microsoft.com/en-us/ef/core/providers/cosmos/limitations
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public DbSet<Conversation> Conversations { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(builder);
    }
}

