namespace MagellanGPT.Domain.Entities;

public class Dialog : BaseEntity
{
    public Guid ConversationId { get; set; }
    public string LlmDeploymentName { get; set; }
    public string? Question { get; set; }
    public string? Answer { get; set; }
    public int? TokensRequest { get; set; }
    public int? TokensResponse { get; set; }
    public DateTime CreatedAt { get; set; }

    public Dialog()
    {

    }
}