using MagellanGPT.Application.Common.Interfaces;
using MagellanGPT.Application.Common.Models;
using MagellanGPT.Domain.Entities;
using MediatR;

namespace MagellanGPT.Application.ChatbotUseCasesCommands;

public record CreateAICompletionSynchronously : IRequest<ResponseDto>
{
    public string Demand { get; set; }
}

public class CreateAICompletionSynchronouslyHandler : IRequestHandler<CreateAICompletionSynchronously, ResponseDto>
{
    private readonly IOpenAIService _openAIService;
    private readonly IApplicationDbContext _context;

    public CreateAICompletionSynchronouslyHandler(IOpenAIService openAIService, IApplicationDbContext context)
    {
        _openAIService = openAIService;
        _context = context;
    }

    public async Task<ResponseDto> Handle(CreateAICompletionSynchronously request, CancellationToken cancellationToken)
    {
        var existingConversation = _context.Conversations.FirstOrDefault(c => c.Id == "13" && c.ConversationId == "759f368c-c14c-49eb-8770-69881e15367f");

        (string Text, int TotalTokens, int RequestTokens, int ResponseTokens) response = await _openAIService.ProcessDemandSynchronously(request.Demand);

        var conversation = await StoreDialog(existingConversation, request.Demand, response, cancellationToken);

        var dialog = conversation.Dialogs.Last();

        var dialogTokenCost = (int)dialog.TokensRequest + (int)dialog.TokensResponse;

        return new ResponseDto { Id = conversation.Id, ConversationId = conversation.ConversationId, Answer = dialog.Answer, Tokens = dialogTokenCost };
    }

    private async Task<Conversation> StoreDialog(Conversation? existingConversation, string demand, (string Text, int TotalTokens, int RequestTokens, int ResponseTokens) response, CancellationToken cancellationToken)
    {
        Conversation conversation;

        if (existingConversation is not null)
        {
            conversation = existingConversation;
            conversation.Tokens += response.TotalTokens;
            conversation.Dialogs.Add(new Dialog
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
            conversation = new Conversation
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

        return conversation;
    }
}