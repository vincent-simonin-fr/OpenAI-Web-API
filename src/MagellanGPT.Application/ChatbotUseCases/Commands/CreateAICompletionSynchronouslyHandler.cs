using MagellanGPT.Application.Common.Interfaces;
using MagellanGPT.Application.Common.Models;
using MagellanGPT.Domain.Entities;
using MediatR;

namespace MagellanGPT.Application.ChatbotUseCasesCommands;

public record CreateAICompletionSynchronously : IRequest<ResponseDto>
{
    public string? UserId { get; set; } = "13";
    public string? ConversationId { get; set; } = "759f368c-c14c-49eb-8770-69881e15367f";
    public string? LlmDeploymentName { get; set; } = "ChatGPT35Turbo";
    public required string Demand { get; set; }
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

    /// <summary>
    /// Check if connection exist
    /// If not exist initialize new conversation
    /// Process demand
    /// Store Dialog
    /// Return ResponseDto
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<ResponseDto> Handle(CreateAICompletionSynchronously request, CancellationToken cancellationToken)
    {
        var initialization = InitializeConversation(request);

        (string Text, int TotalTokens, int RequestTokens, int ResponseTokens) response = await _openAIService.ProcessDemandSynchronously(initialization.Conversation);

        initialization.Conversation = await StoreDialog(initialization.Conversation, response, initialization.IsExistingConversation, cancellationToken);

        var dialog = initialization.Conversation.Dialogs![^1];

        var dialogTokenCost = (int)dialog.TokensRequest! + (int)dialog.TokensResponse!;

        return new ResponseDto {
            Id = initialization.Conversation.Id,
            ConversationId = initialization.Conversation.ConversationId,
            Answer = dialog.Answer,
            Tokens = dialogTokenCost
        };
    }

    private (Conversation Conversation, bool IsExistingConversation) InitializeConversation(CreateAICompletionSynchronously request)
    {
        Conversation conversation = _context.Conversations.FirstOrDefault(c => c.Id == request.UserId && c.ConversationId == request.ConversationId);
        bool isExistingConversation;
        if (conversation is not null)
        {
            //conversation = _context.Conversations.First(c => c.Id == request.UserId && c.ConversationId == request.ConversationId)
            //    ?? throw new InvalidDataException("La conversation n'existe pas dans CosmosDb");
            isExistingConversation = true;
            conversation.Dialogs!.Add(new Dialog
            {
                Question = request.Demand,
                Answer = null,
                DocumentId = null,
                TokensRequest = 0,
                TokensResponse = 0,
                CreatedAt = DateTime.Now,
            });
        }
        else
        {
            conversation = new Conversation
            {
                Id = request.UserId!,
                ConversationId = Guid.NewGuid().ToString(),
                LlmDeploymentName = request.LlmDeploymentName!,
                Title = "WIP",
                Dialogs = new List<Dialog> { new Dialog
                {
                    Question = request.Demand,
                    Answer = null,
                    DocumentId = null,
                    TokensRequest = 0,
                    TokensResponse = 0,
                    CreatedAt = DateTime.Now,
                }},
                Tokens = 0
            };

            isExistingConversation = false;
        }

        return (conversation, isExistingConversation);
    }

    private async Task<Conversation> StoreDialog(
        Conversation conversation,
        (string Text, int TotalTokens, int RequestTokens, int ResponseTokens) response,
        bool isExistingConversation,
        CancellationToken cancellationToken)
    {

        var lastDialog = conversation.Dialogs![conversation.Dialogs!.Count - 1];

        lastDialog.Answer = response.Text;
        lastDialog.DocumentId = null;
        lastDialog.TokensRequest = response.RequestTokens;
        lastDialog.TokensResponse = response.ResponseTokens;
        lastDialog.CreatedAt = DateTime.Now;


        conversation.Tokens += response.TotalTokens;

        if(!isExistingConversation) _context.Conversations.Add(conversation);

        await _context.SaveChangesAsync(cancellationToken);

        return conversation;
    }
}