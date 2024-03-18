using MagellanGPT.Application.Common.Interfaces;
using MagellanGPT.Application.Common.Models;
using MagellanGPT.Domain.Entities;
using MediatR;

namespace MagellanGPT.Application.ChatbotUseCasesCommands;

// [Authorize(Roles = "user")]
public record CreateAICompletionSynchronously : IRequest<ResponseDto>
{
    public string? UserId { get; set; }
    public string? ConversationId { get; set; }
    public string? LlmDeploymentName { get; set; } = "ChatGPT35Turbo";
    public required string Demand { get; set; }
    public string? SystemPrompt { get; set; }
}

public class CreateAICompletionSynchronouslyHandler : IRequestHandler<CreateAICompletionSynchronously, ResponseDto>
{
    private readonly IOpenAIService _openAiService;
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateAICompletionSynchronouslyHandler(IOpenAIService openAIService, IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _openAiService = openAIService;
        _context = context;
        _currentUserService = currentUserService;
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
        request.UserId = _currentUserService.UserId;

        var initialization = InitializeConversation(request);

        if (request.SystemPrompt is not null) initialization.Conversation.SystemPromt = request.SystemPrompt;

        await _openAiService.ProcessDemandSynchronously(initialization.Conversation);

        initialization.Conversation = await StoreDialog(initialization.Conversation, initialization.IsExistingConversation, cancellationToken);

        var dialog = initialization.Conversation.Dialogs![^1];

        return new ResponseDto {
            Id = initialization.Conversation.Id,
            ConversationId = initialization.Conversation.ConversationId,
            Answer = dialog.Answer,
            Tokens = (int)dialog.TokensRequest! + (int)dialog.TokensResponse!
        };
    }

    private (Conversation Conversation, bool IsExistingConversation) InitializeConversation(CreateAICompletionSynchronously request)
    {
        Conversation? conversation = request.ConversationId is not null ? _context.Conversations.FirstOrDefault(c => c.Id == request.UserId && c.ConversationId == request.ConversationId) : null;
        bool isExistingConversation;
        if (conversation is not null)
        {
            conversation.Dialogs!.Add(new Dialog
            {
                Question = request.Demand,
                Answer = null,
                DocumentId = null,
                TokensRequest = 0,
                TokensResponse = 0,
                CreatedAt = DateTime.Now,
            });

            isExistingConversation = true;
        }
        else
        {
            conversation = new Conversation
            {
                Id = request.UserId!,
                ConversationId = Guid.NewGuid().ToString(),
                LlmDeploymentName = request.LlmDeploymentName!,
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
        bool isExistingConversation,
        CancellationToken cancellationToken)
    {
        if(!isExistingConversation) _context.Conversations.Add(conversation);

        await _context.SaveChangesAsync(cancellationToken);

        return conversation;
    }
}