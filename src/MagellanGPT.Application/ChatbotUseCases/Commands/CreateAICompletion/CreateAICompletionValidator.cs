using FluentValidation;

namespace MagellanGPT.Application.ChatbotUseCases.Commands.CreateAICompletion;

public class CreateAICompletionValidator : AbstractValidator<CreateAICompletion>
{
    public CreateAICompletionValidator()
    {
        RuleFor(v => v.Demand)
            .NotEmpty();
    }
}

