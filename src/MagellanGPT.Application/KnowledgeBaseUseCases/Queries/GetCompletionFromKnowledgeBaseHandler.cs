using System.Diagnostics;
using System.Text;
using MagellanGPT.Application.Common.Interfaces;
using MagellanGPT.Application.Common.Models;
using MagellanGPT.Application.RAGUseCases.Commands;
using MagellanGPT.Domain.Entities;
using MediatR;

namespace MagellanGPT.Application.KnowledgeBaseUseCases.Queries;

public record GetCompletionFromKnowledgeBase : IRequest<ResponseDto>
{
    public string Demand { get; set; }
}

public class GetCompletionFromKnowledgeBaseHandler : IRequestHandler<GetCompletionFromKnowledgeBase, ResponseDto>
{
    private readonly IAzureAiSearchService _azureAiSearchService;
    private readonly IOpenAIService _openAIService;

    public GetCompletionFromKnowledgeBaseHandler(IAzureAiSearchService azureAiSearchService, IOpenAIService openAIService)
    {
        _azureAiSearchService = azureAiSearchService;
        _openAIService = openAIService;
    }

    public async Task<ResponseDto> Handle(GetCompletionFromKnowledgeBase request, CancellationToken cancellationToken)
    {
        var searchResult = await _azureAiSearchService.SearchMemoryAsync(request.Demand);

        // build document
        var citations = searchResult.Results.Select(result => new { result.SourceName, result.Partitions }).ToList();

        StringBuilder document = new();

        citations.ForEach(citation =>
        {
            document.Append($"###{citation.SourceName}###{String.Join(" ", citation.Partitions.Select(p => p.Text).ToList())}");
        });

        var conversation = InitializeConversation(request.Demand, document.ToString());

        conversation = await _openAIService.ProcessDemandSynchronously(conversation);

        return new ResponseDto
        {
            Id = "Diiage2024",
            ConversationId = conversation.Id,
            Answer = conversation.Dialogs![^1].Answer,
            Tokens = (int)conversation.Dialogs![^1].TokensRequest!
            + (int)conversation.Dialogs![^1].TokensResponse!,
            Citations = searchResult.Results
        };
    }

    private Conversation InitializeConversation(string query, string document)
    {
        var dialogs = new List<Dialog> {
            new Dialog
            {
                Question = "Genere moi une réponse en " +
                "utilisant La liste de citations suivantes :" +
                "###REFERENCE_DOCUMENT_1###CONTENU_DOCUMENT_1" +
                "###REFERENCE_DOCUMENT_2###CONTENU_DOCUMENT_2",
                Answer = "TEXT 1 TEXT 2 TEXT \n\n Citations:\n1.REFERENCE_DOCUMENT_1\n2.REFERENCE_DOCUMENT_2,",
                TokensRequest = 0,
                TokensResponse = 0,
                CreatedAt = DateTime.Now,
            },
            new Dialog
            {
                Question = query,
                Answer = null,
                TokensRequest = 0,
                TokensResponse = 0,
                CreatedAt = DateTime.Now,
            }};

        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            Title = string.Empty,
            Dialogs = dialogs,
            Tokens = 0,
            SystemPrompt = "Tu es un expert du domaine lié à mon message." +
                $"Ta tâche est de m'aider à répondre à une question " +
                $"en utilisant la liste de citations suivantes " +
                $"délimité par ----" +
                $"----{document}----\n" +
                $"Chaque sitation à une référence délimité par ### suivi d'un contenu" +
                $". Génère une réponse. "
        };

        return conversation;
    }
}

