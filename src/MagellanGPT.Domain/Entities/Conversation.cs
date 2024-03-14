namespace MagellanGPT.Domain.Entities;

public class Conversation
{
    public string Id { get; set; }
    public string ConversationId { get; set; }
    public string? Title { get; set; }
    public List<Dialog>? Dialogs { get; set; } = new List<Dialog>();
    public string LlmDeploymentName { get; set; }
    public int? Tokens { get; set; }


}
