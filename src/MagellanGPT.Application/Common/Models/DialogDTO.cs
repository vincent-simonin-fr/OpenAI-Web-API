using MagellanGPT.Domain.Entities;
using System.Linq.Expressions;

namespace MagellanGPT.Application.Common.Models;

public class DialogDto
{
    public string? Question { get; set; }
    public string? Answer { get; set; }
    public int? Tokens { get; set; }
    public DateTime CreatedAt { get; set; }

    public static Expression<Func<Dialog, DialogDto>> Projection { get; } = dialog
            => new DialogDto
            {
                Question = dialog.Question,
                Answer = dialog.Answer,
                Tokens = dialog.TokensRequest + dialog.TokensResponse + dialog.TokensDocumentProcessing,
                CreatedAt = dialog.CreatedAt
            };

    public static DialogDto FromEntity(Dialog dialog)
    {
        return Projection.Compile().Invoke(dialog);
    }
}
