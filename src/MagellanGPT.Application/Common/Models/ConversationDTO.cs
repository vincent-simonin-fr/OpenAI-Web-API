using System.Linq.Expressions;
using MagellanGPT.Application.Common.Interfaces;

namespace MagellanGPT.Application.Common.Models;

public class ConversationDTO
{
    public string Id { get; set; }
    public string Question { get; set; }
    public string? Answer { get; set; }
    public int? Token { get; set; }

    public static Expression<Func<Conversation, ConversationDTO>> Projection { get; } = conversation
            => new ConversationDTO
            {
                Id = conversation.Id,
                Question = conversation.Question,
                Answer = conversation.Answer,
                Token = conversation.Token,
            };

    public static ConversationDTO FromEntity(Conversation conversation)
    {
        return Projection.Compile().Invoke(conversation);
    }
}

