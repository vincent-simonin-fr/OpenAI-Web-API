using System.Linq.Expressions;
using MagellanGPT.Domain.Entities;

namespace MagellanGPT.Application.Common.Models;

public class ConversationDTO
{
    public string Id { get; set; }
    public string ConversationId { get; set; }
    public List<DialogDTO> Dialogs { get; set; }
    public int? Token { get; set; }

    public static Expression<Func<Conversation, ConversationDTO>> Projection { get; } = conversation
            => new ConversationDTO
            {
                Id = conversation.Id,
                ConversationId = conversation.ConversationId,
                Dialogs = conversation.Dialogs.AsQueryable().Select(DialogDTO.Projection).ToList(),
                Token = conversation.Token,
            };

    public static ConversationDTO FromEntity(Conversation conversation)
    {
        return Projection.Compile().Invoke(conversation);
    }
}

