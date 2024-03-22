namespace MagellanGPT.Domain.Entities;

public class Quota
{
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public int Token { get; set; } = 0;
}