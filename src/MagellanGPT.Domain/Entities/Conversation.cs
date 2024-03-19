namespace MagellanGPT.Domain.Entities;

public class Conversation : BaseEntity
{
    public string PartitionKey { get; set; }
    public string? Title { get; set; }
    public List<Dialog>? Dialogs { get; set; } = new List<Dialog>();
    public string LlmDeploymentName { get; set; }
    public int? Tokens { get; set; }
    public string? SystemPromt { get; set; }
    public User User { get; set; }
    public Chat Chat { get; set; }
}
