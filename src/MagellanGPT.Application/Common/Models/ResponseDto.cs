using Microsoft.KernelMemory;

namespace MagellanGPT.Application.Common.Models;

public class ResponseDto
{
    public string Id { get; set; }
    public Guid? ConversationId { get; set; }
    public string? Answer { get; set; }
    public int Tokens { get; set; }
    public ICollection<Citation> Citations { get; set; } = new List<Citation>();
}

