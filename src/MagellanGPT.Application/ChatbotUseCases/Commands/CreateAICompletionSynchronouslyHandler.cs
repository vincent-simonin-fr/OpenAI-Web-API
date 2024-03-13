using MagellanGPT.Application.Common.Interfaces;
using MagellanGPT.Domain.Entities;
using MediatR;

namespace MagellanGPT.Application.ChatbotUseCasesCommands;

public record CreateAICompletionSynchronously : IRequest<string>
{
    public string Demand { get; set; }
}

public class CreateAICompletionSynchronouslyHandler : IRequestHandler<CreateAICompletionSynchronously, string>
{
    private readonly IOpenAIService _openAIService;
    private readonly IApplicationDbContext _context;

    public CreateAICompletionSynchronouslyHandler(IOpenAIService openAIService, IApplicationDbContext context)
    {
        _openAIService = openAIService;
        _context = context;
    }

    public async Task<string> Handle(CreateAICompletionSynchronously request, CancellationToken cancellationToken)
    {
        var existingConversation = _context.Conversations.FirstOrDefault(c => c.Id == "13" && c.ConversationId == "759f368c-c14c-49eb-8770-69881e15367f");
        (string Text, int TotalTokens, int RequestTokens, int ResponseTokens) response = _openAIService.ProcessDemandSynchronously(request.Demand).Result;

        await StoreDialog(existingConversation, request.Demand, response, cancellationToken);

        return response.Text;
    }

    private async Task StoreDialog(Conversation? existingConversation, string demand, (string Text, int TotalTokens, int RequestTokens, int ResponseTokens) response, CancellationToken cancellationToken)
    {
        if (existingConversation is not null)
        {
            existingConversation.Tokens += response.TotalTokens;
            existingConversation.Dialogs.Add(new Dialog
            {
                Question = demand,
                Answer = response.Text,
                DocumentId = null,
                TokensRequest = response.RequestTokens,
                TokensResponse = response.ResponseTokens,
                CreatedAt = DateTime.Now,
            });
        }
        else
        {
            var conversation = new Conversation
            {
                Id = "13",
                ConversationId = Guid.NewGuid().ToString(),
                LlmDeploymentName = "ChatGPT35Turbo",
                Title = "Test",
                Dialogs = new List<Dialog> { new Dialog
                {
                        Question = demand,
                        Answer = response.Text,
                        DocumentId = null,
                        TokensRequest = response.RequestTokens,
                        TokensResponse = response.ResponseTokens,
                        CreatedAt = DateTime.Now,
                    }
                },
                Tokens = response.TotalTokens
            };
            _context.Conversations.Add(conversation);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}