using System.Threading;
using MagellanGPT.Application.Common.Interfaces;
using MagellanGPT.Application.Common.Models;
using MagellanGPT.Domain.Entities;
using MediatR;

namespace MagellanGPT.Application.ChatbotUseCasesCommands;

// [Authorize(Roles = "user")]
public record CreateAICompletionSynchronously : IRequest<ResponseDto>
{
    public Guid? ConversationId { get; set; }
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
        ArgumentNullException.ThrowIfNull(_currentUserService.UserId, "ObjectId user not found");

        var initialization = await InitializeConversation(request, cancellationToken);

        if (request.SystemPrompt is not null) initialization.Conversation.SystemPromt = request.SystemPrompt;

        await _openAiService.ProcessDemandSynchronously(initialization.Conversation);

        initialization.Conversation = await UpsertConversation(initialization.Conversation, initialization.IsExistingConversation, cancellationToken);

        var dialog = initialization.Conversation.Dialogs![^1];

        return new ResponseDto {
            Id = "MagellanGPT",
            ConversationId = initialization.Conversation.Id,
            Answer = dialog.Answer,
            Tokens = (int)dialog.TokensRequest! + (int)dialog.TokensResponse!
        };
    }

    private async Task<(Conversation Conversation, User User, bool IsExistingConversation)> InitializeConversation(CreateAICompletionSynchronously request, CancellationToken cancellationToken)
    {
        var user = _currentUserService.UserId is not null ? _context.User.FirstOrDefault(user => user.ObjectId == _currentUserService.UserId) : null;

        if (user is null)
        {
            user = new User(_currentUserService.UserId ?? "fd285508-8ba1-4064-be24-30dfdea0b376");
            _context.User.Add(user);
            await _context.SaveChangesAsync(cancellationToken);
        }

        Conversation? conversation = request.ConversationId is not null ? _context.Conversation.FirstOrDefault(c => c.Id == request.ConversationId) : null;
        bool isExistingConversation;
        if (conversation is not null)
        {
            conversation.Dialogs!.Add(new Dialog
            {
                Question = request.Demand,
                Answer = null,
                LlmDeploymentName = request.LlmDeploymentName!,
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
                Id = Guid.NewGuid(),
                PartitionKey = nameof(Conversation),
                Dialogs = new List<Dialog> { new Dialog
                {
                    Question = request.Demand,
                    Answer = null,
                    LlmDeploymentName = request.LlmDeploymentName!,
                    TokensRequest = 0,
                    TokensResponse = 0,
                    CreatedAt = DateTime.Now,
                }},
                Tokens = 0,
                User = user
            };

            user.Conversations.Add(conversation);
            _context.User.Update(user);

            isExistingConversation = false;
        }

        return (conversation, user, isExistingConversation);
    }

    private async Task<Conversation> UpsertConversation(
        Conversation conversation,
        bool isExistingConversation,
        CancellationToken cancellationToken)
    {
        // var chat = _context.Chat.First();

        if (!isExistingConversation)
        {
            
            //chat.Conversations.Add(conversation);
            //_context.Chat.Update(chat);
            _context.Conversation.Add(conversation);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return conversation;
    }
}