namespace MagellanGPT.Domain.Entities;

public class Conversation : BaseEntity
{
    public string PartitionKey { get; set; }
    public string? Title { get; set; }
    public List<Dialog>? Dialogs { get; set; } = new List<Dialog>();
    public int? Tokens { get; set; }
    public string? SystemPrompt { get; set; }
    public User User { get; set; }
    public Chat Chat { get; set; }

    public Conversation()
    {

    }
}
