using System.Linq.Expressions;
using MagellanGPT.Domain.Entities;

namespace MagellanGPT.Application.Common.Models;

public class ConversationDto
{
    public Guid? ConversationId { get; set; }
    public List<DialogDto> Dialogs { get; set; }
    public int? Tokens { get; set; }
    public string Title { get; set; }

    public static Expression<Func<Conversation, ConversationDto>> Projection { get; } = conversation
            => new ConversationDto
            {
                ConversationId = conversation.Id,
                Dialogs = conversation.Dialogs!.AsQueryable().Select(DialogDto.Projection).ToList(),
                Tokens = conversation.Tokens,
                Title = conversation.Title ?? "",
            };

    public static ConversationDto FromEntity(Conversation conversation)
    {
        return Projection.Compile().Invoke(conversation);
    }
}

