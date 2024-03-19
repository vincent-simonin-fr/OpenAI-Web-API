using MagellanGPT.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MagellanGPT.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    public DbSet<Chat> Chat { get; set; }
    public DbSet<User> User { get; set; }
    public DbSet<Conversation> Conversation { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
