using Microsoft.EntityFrameworkCore;

namespace MagellanGPT.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    public DbSet<Conversation> Conversations { get; set; }
}

public class Conversation
{
    public string Id { get; set; }
    public string Question { get; set; }
    public string? Answer { get; set; }
    public int? Token { get; set; }
}