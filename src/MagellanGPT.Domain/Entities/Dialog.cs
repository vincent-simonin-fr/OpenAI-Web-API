namespace MagellanGPT.Domain.Entities;

public class Dialog
{
    public string? Question { get; set; }
    public string? Answer { get; set; }
    public int? TokensRequest { get; set; }
    public int? TokensResponse { get; set; }
    public int? TokensDocumentProcessing { get; set; }
    public List<string>? DocumentId { get; set; }
    public DateTime CreatedAt { get; set; }
}