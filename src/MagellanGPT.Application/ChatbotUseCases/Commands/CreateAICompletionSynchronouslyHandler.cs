using Azure.AI.OpenAI;
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
        var existingConversation = _context.Conversations.FirstOrDefault(c => c.Id == "12" && c.ConversationId == "243e4e8e-af44-4548-8027-ac872bfc81bb");
        var response = _openAIService.ProcessDemandSynchronously(request.Demand).Result;

        if (existingConversation is not null)
        {
            existingConversation.Dialogs.Add(new Dialog
            {
                Question = request.Demand,
                Answer = response,
                DocumentId = null,
                Token = 100,
                CreatedAt = DateTime.Now,
            });
        }
        else
        {
            var conversation = new Conversation
            {
                Id = "12",
                ConversationId = Guid.NewGuid().ToString(),
                LlmDeploymentName = "ChatGPT35Turbo",
                Dialogs = new List<Dialog> { new Dialog
                    {
                        Question = request.Demand,
                        Answer = response,
                        DocumentId = null,
                        Token = 100,
                        CreatedAt = DateTime.Now,
                    }
                }
            };
            _context.Conversations.Add(conversation);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return response;
    }
}

