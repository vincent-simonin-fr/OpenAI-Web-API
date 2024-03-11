using Azure.AI.OpenAI;
using MagellanGPT.Application.Common.Interfaces;
using MediatR;

namespace MagellanGPT.Application.ChatbotUseCases.Commands.CreateAICompletion;

public record CreateAICompletion : IRequest<IAsyncEnumerable<StreamingChatCompletionsUpdate>>
{
    public string Demand { get; set; }
}

public class CreateAICompletionHandler : IRequestHandler<CreateAICompletion, IAsyncEnumerable<StreamingChatCompletionsUpdate>>
{
    private readonly IOpenAIService _openAIService;
    private readonly IApplicationDbContext _context;

    public CreateAICompletionHandler(IOpenAIService openAIService, IApplicationDbContext context)
    {
        _openAIService = openAIService;
        _context = context;
    }

    public async Task<IAsyncEnumerable<StreamingChatCompletionsUpdate>> Handle(CreateAICompletion request, CancellationToken cancellationToken)
    {
        var response = await _openAIService.ProcessDemand(request.Demand);

        return response;
    }
}

