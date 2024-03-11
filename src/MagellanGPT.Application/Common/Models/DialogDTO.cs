using MagellanGPT.Domain.Entities;
using System.Linq.Expressions;

namespace MagellanGPT.Application.Common.Models;

public class DialogDTO
{
    public string? Question { get; set; }
    public string? Answer { get; set; }
    public string? DocumentId { get; set; }
    public int? Token { get; set; }
    public DateTime CreatedAt { get; set; }

    public static Expression<Func<Dialog, DialogDTO>> Projection { get; } = dialog
            => new DialogDTO
            {
                Question = dialog.Question,
                Answer = dialog.Answer,
                DocumentId = dialog.DocumentId,
                Token = dialog.Token,
                CreatedAt = dialog.CreatedAt
            };

    public static DialogDTO FromEntity(Dialog dialog)
    {
        return Projection.Compile().Invoke(dialog);
    }
}
