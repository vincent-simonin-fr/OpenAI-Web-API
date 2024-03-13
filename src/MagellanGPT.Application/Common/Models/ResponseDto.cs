namespace MagellanGPT.Application.Common.Models;

public class ResponseDto
{
    public string Id { get; set; }
    public string ConversationId { get; set; }
    public string? Answer { get; set; }
    public int Tokens { get; set; }
}

