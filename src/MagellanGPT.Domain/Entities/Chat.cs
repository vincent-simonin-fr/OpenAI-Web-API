namespace MagellanGPT.Domain.Entities;

public class Chat
{
    public string Id { get; set; } = "MagellanGPT";
    public string PartitionKey { get; set; }
    public List<Conversation> Conversations { get; set; }

    public Chat()
    {
        Conversations = new List<Conversation>();
    }
}

