namespace MagellanGPT.Domain.Entities;

public class Dialog
{
    public string? Question { get; set; }
    public string? Answer { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? DocumentId { get; set; }
    public int? Token { get; set; }
}