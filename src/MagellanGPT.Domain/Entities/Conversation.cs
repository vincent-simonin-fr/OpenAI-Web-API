namespace MagellanGPT.Domain.Entities;

public class Conversation
{
    public required string Id { get; set; }
    public required string ConversationId { get; set; }
    public required string Title { get; set; }
    public List<Dialog> Dialogs { get; set; } = new List<Dialog>();
    public required string LlmDeploymentName { get; set; }
    public int Token { get; set; }
}
