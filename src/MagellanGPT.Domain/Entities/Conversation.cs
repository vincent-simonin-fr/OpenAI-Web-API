namespace MagellanGPT.Domain.Entities;

public class Conversation : BaseEntity
{
    public string PartitionKey { get; set; }
    public string? Title { get; set; }
    public List<Dialog> Dialogs { get; set; } = new List<Dialog>();
    public int Tokens { get; set; } = 0;
    public string? SystemPrompt { get; set; }
    public User User { get; set; }

    public Conversation()
    {
        SystemPrompt = @"You're a helpful AI assistant, generating the result of a search query for a follow-up question.
            Your answer must be simple and precise. Return only the result, don't return any other text.";
    }
}
