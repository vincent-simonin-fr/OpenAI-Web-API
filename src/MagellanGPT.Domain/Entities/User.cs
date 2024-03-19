namespace MagellanGPT.Domain.Entities;

public class User : BaseEntity
{
    public string ObjectId { get; set; }
    public string PartitionKey { get; set; } = "User";
    public ICollection<Conversation> Conversations{ get; set; }
    public int? Tokens { get; set; }

    public User()
    {
        Conversations = new List<Conversation>();
    }

    public User(string objectId) : this()
    {
        Id = Guid.Parse(objectId);
        ObjectId = objectId;
        Tokens = 0;
    }
}