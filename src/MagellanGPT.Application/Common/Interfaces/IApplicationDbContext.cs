using MagellanGPT.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MagellanGPT.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    public DbSet<User> User { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
