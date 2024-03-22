namespace MagellanGPT.Domain.Entities;

public class User : BaseEntity
{
    public string ObjectId { get; set; }
    public string PartitionKey { get; set; } = "User";
    public List<Conversation> Conversations{ get; set; } 
    public string? SystemPrompt { get; set; }
    public int? Tokens { get; set; }
    public Quota Quota { get; set; }

    public User()
    {
        Quota = new Quota();
        Conversations = new List<Conversation>();
    }

    public User(string objectId) : this()
    {
        Id = Guid.Parse(objectId);
        ObjectId = objectId;
        Tokens = 0;
    }
}