namespace MagellanGPT.Domain.Entities;

public class User
{
    public string ObjectId { get; set; }
    public string Conversation { get; set; } = "Conversation";
    public IEnumerable<Conversation> Conversations{ get; set; }

    public User()
    {
        Conversations = new List<Conversation>();
    }
}

