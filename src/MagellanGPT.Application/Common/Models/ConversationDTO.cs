using System.Linq.Expressions;
using MagellanGPT.Domain.Entities;

namespace MagellanGPT.Application.Common.Models;

public class ConversationDto
{
    public string Id { get; set; }
    public string ConversationId { get; set; }
    public List<DialogDto> Dialogs { get; set; }
    public int? Tokens { get; set; }

    public static Expression<Func<Conversation, ConversationDto>> Projection { get; } = conversation
            => new ConversationDto
            {
                Id = conversation.Id,
                ConversationId = conversation.ConversationId,
                Dialogs = conversation.Dialogs!.AsQueryable().Select(DialogDto.Projection).ToList(),
                Tokens = conversation.Tokens,
            };

    public static ConversationDto FromEntity(Conversation conversation)
    {
        return Projection.Compile().Invoke(conversation);
    }
}

