using System.Data;
using System.Text;
using MagellanGPT.Application.Common.Interfaces;
using MagellanGPT.Application.Common.Models;
using MagellanGPT.Application.Common.Security;
using MagellanGPT.Domain.Entities;
using MediatR;
using Microsoft.KernelMemory.DataFormats;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace MagellanGPT.Application.RAGUseCases.Commands;

// [Authorize(Roles = "user")]
public record CreateAICompletionWithMemorizePDfFiles : IRequest<ResponseDto>
{
    public string? UserId { get; set; } = "16";
    public string? ConversationId { get; set; } = "bc029032-c074-408b-af71-49d0d57df506";
    public string? LlmDeploymentName { get; set; } = "ChatGPT35Turbo";
    public required List<string>? FilePathList { get; set; }
    public string Demand { get; set; }
};

public class CreateAICompletionWithMemorizePDfFilesHandler : IRequestHandler<CreateAICompletionWithMemorizePDfFiles, ResponseDto>
{
    private readonly IOpenAIService _openAIService;
    private readonly IAzureAiSearchService _azureAiSearchService;
    private readonly IApplicationDbContext _context;

    public CreateAICompletionWithMemorizePDfFilesHandler(IApplicationDbContext context, IOpenAIService openAIService, IAzureAiSearchService azureAiSearchService)
    {
        _context = context;
        _openAIService = openAIService;
        _azureAiSearchService = azureAiSearchService;
    }

    /// <summary>
    /// Check if connection exist
    /// If not exist initialize new conversation
    /// Process demand
    /// Store Dialog
    /// Return ResponseDto
    /// ExtractContent from PDF https://github.com/microsoft/kernel-memory/blob/main/examples/205-dotnet-extract-text-from-docs/Program.cs
    /// https://stackoverflow.com/questions/77261548/how-to-appropriately-use-the-azure-ai-openai-openaiclient-getchatcompletionsstre
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Task<IAsyncEnumerable<StreamingChatCompletionsUpdate>></returns>
    public async Task<ResponseDto> Handle(CreateAICompletionWithMemorizePDfFiles request, CancellationToken cancellationToken)
    {
        var conversation = InitializeConversation(request);

        // Traitement des PDF
        var process = await ProcessAndStorePdf(request.FilePathList);
        var documentsIds = new List<string>();

        StringBuilder document = new();
        foreach(var doc in process.Documents)
        {
            document.Append(doc.Value);
            documentsIds.Add(doc.Key);
        }

        // Requête de complétion de la demande
        await _openAIService.ProcessDemandWithRagSynchronously(conversation, document.ToString());

        return new ResponseDto
        {
            Id = conversation.Id,
            ConversationId = conversation.ConversationId,
            Answer = conversation.Dialogs![^1].Answer,
            Tokens = (int)conversation.Dialogs![^1].TokensRequest!
            + (int)conversation.Dialogs![^1].TokensResponse!
            + process.TotalTokens
        };
    }

    private async Task<(Dictionary<string, string> Documents, int TotalTokens)> ProcessAndStorePdf(List<string> filePathList)
    {
        FileContent content = new();
        StringBuilder document = new();

        var documents = new Dictionary<string, string>();

        var index = 1;
        filePathList.ForEach(filename => {
            content = new PdfDecoder().ExtractContent(filename);
            document.Append("-----");
            document.Append($"{filename}-{index}");
            foreach (FileSection section in content.Sections)
            {
                document.Append($"Page: {section.Number}/{content.Sections.Count}");
                document.Append(section.Content);
                document.Append("-----");
            }
            documents.Add(filename, document.ToString());
            index++;
        });
       
        var tokenCost = await _azureAiSearchService.StoreAsync(documents);

        return (documents, tokenCost);
    }

    private Conversation InitializeConversation(CreateAICompletionWithMemorizePDfFiles request)
    {
        var conversation = new Conversation
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

        return conversation;
    }
}

// TODO : Refactor & manage failure
// https://github1s.com/microsoft/kernel-memory/blob/main/service/Core/DataFormats/Pdf/PdfDecoder.cs#L7-L8
public class PdfDecoder
{
    public FileContent ExtractContent(string filename)
    {
        using var stream = File.OpenRead(filename);
        var content = ExtractContent(stream);
        File.Delete(filename);
        return content;
    }

    public FileContent ExtractContent(BinaryData data)
    {
        using var stream = data.ToStream();
        return ExtractContent(stream);
    }

    public FileContent ExtractContent(Stream data)
    {
        var result = new FileContent();

        using PdfDocument? pdfDocument = PdfDocument.Open(data);
        if (pdfDocument == null) { return result; }

        foreach (Page? page in pdfDocument.GetPages().Where(x => x != null))
        {
            // Note: no trimming, use original spacing
            string pageContent = (ContentOrderTextExtractor.GetText(page) ?? string.Empty);
            result.Sections.Add(new FileSection(page.Number, pageContent, false));
        }

        return result;
    }
}
