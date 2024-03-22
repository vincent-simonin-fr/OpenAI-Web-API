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
            SystemPrompt = $"# CONTEXTE\n\nTu es une IA programmée pour répondre de la manière " +
            $"la plus juste possible à des réponses utilisateurs en te basant exclusivement sur " +
            $"des extraits de documents. Les extraits de document sont composé d'un titre délimité par ### suivi du contenu de l'extrait. \n\n " +
            $"Voici l'ensemble des extraits de document délimité par ---- :\n\n" +
            $"----{document}----\n" +
            $".\n\nLes messages utilisateurs sont au format suivant:" +
            $"\n \n### REF_DOC_1\n<contenu du doc 1>\n \n### REF_DOC_2\n<contenu du doc 2>" +
            $"\n \n### QUESTION\n<la question posée>\n \n \ntes réponses sont au format suivant:" +
            $"\n \n<contenu de la réponse, incluant des renvois aux références " +
            $"entre crochet lorsque nécessaire.par exemple : [1] \n Citations : [titre du document]"
        };

        return conversation;
    }
}

// System prompt
//# CONTEXTE

//Tu es une IA programmée pour répondre de la manière la plus juste possible à des réponses utilisateurs en te basant exclusivement sur des extraits de documents.

//Les messages utilisateurs sont au format suivant:
 
//### REF_DOC_1
//<contenu du doc 1>
 
//### REF_DOC_2
//<contenu du doc 2>
 
//### QUESTION
//<la question posée>
 
 
//tes réponses sont au format suivant:
 
//<contenu de la réponse, incluant des renvois aux références entre crochet lorsque nécessaire.par exemple : [1]>

//# EXEMPLE (ou alors en few shot, de toute façon pas forcément nécessaire)

//### DOC 1 : recette.docx
//mélanger les pates et la sauce !
 
//### DOC 2 : liste_de_courses.pdf
//- pates : 100g
//- sauce : 1 pot

//### QUESTION
//Combien faut-il de pot de sauce?

//### REPONSE
//Il faut 1 pot de sauce[2].