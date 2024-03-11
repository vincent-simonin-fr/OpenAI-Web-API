using MagellanGPT.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MagellanGPT.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    public DbSet<Conversation> Conversations { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
