namespace MagellanGPT.Domain.Entities;

public class User : BaseEntity
{
    public string ObjectId { get; set; }
    public string PartitionKey { get; set; } = "User";
    public List<Conversation> Conversations{ get; set; } = new List<Conversation>();
    public string? SystemPrompt { get; set; }
    public int? Tokens { get; set; }

    public User()
    {
    }

    public User(string objectId) : this()
    {
        Id = Guid.Parse(objectId);
        ObjectId = objectId;
        Tokens = 0;
    }
}