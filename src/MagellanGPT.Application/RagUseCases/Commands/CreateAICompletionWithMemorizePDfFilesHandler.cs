using System.Text;
using MagellanGPT.Application.Common.Interfaces;
using MagellanGPT.Domain.Entities;
using MediatR;
using Microsoft.KernelMemory.DataFormats;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace MagellanGPT.Application.RAGUseCases.Commands;

public record CreateAICompletionWithMemorizePDfFiles : IRequest<string>
{
    public required List<string> FilePathList { get; set; }
    public string Demand { get; set; }
};

public class CreateAICompletionWithMemorizePDfFilesHandler : IRequestHandler<CreateAICompletionWithMemorizePDfFiles, string>
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
    /// ExtractContent from PDF https://github.com/microsoft/kernel-memory/blob/main/examples/205-dotnet-extract-text-from-docs/Program.cs
    /// https://stackoverflow.com/questions/77261548/how-to-appropriately-use-the-azure-ai-openai-openaiclient-getchatcompletionsstre
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Task<IAsyncEnumerable<StreamingChatCompletionsUpdate>></returns>
    public async Task<string> Handle(CreateAICompletionWithMemorizePDfFiles request, CancellationToken cancellationToken)
    {
        // S'agit il d'une conversation existante
        var existingConversation = _context.Conversations.FirstOrDefault(c => c.Id == "13" && c.ConversationId == "759f368c-c14c-49eb-8770-69881e15367f");

        // Traitement des PDF
        var documentsProcessing = await ProcessAndStorePdf(request.FilePathList);
        var documentsIds = new List<string>();
        StringBuilder document = new();
        foreach(var doc in documentsProcessing.Documents)
        {
            document.Append(doc.Value);
            documentsIds.Add(doc.Key);
        }

        // Requête de complétion de la demande
        (string Text, int TotalTokens, int RequestTokens, int ResponseTokens) response = await _openAIService.ProcessDemandWithRagSynchronously(request.Demand, document.ToString());

        // Persistence du dialogue en base de données
        await StoreDialog(existingConversation, request.Demand, response, (documentsIds, documentsProcessing.TokenCost), cancellationToken);

        return response.Text;
    }

    private async Task<(Dictionary<string, string> Documents, int TokenCost)> ProcessAndStorePdf(List<string> filePathList)
    {
        FileContent content = new();
        StringBuilder document = new();

        var documents = new Dictionary<string, string>();

        var index = 1;
        filePathList.ForEach(file => {
            content = new PdfDecoder().ExtractContent(file);
            document.Append($"Document N°{index}");
            foreach (FileSection section in content.Sections)
            {
                document.Append($"Page: {section.Number}/{content.Sections.Count}");
                document.Append(section.Content);
                document.Append("-----");
            }
            documents.Add(Guid.NewGuid().ToString(), document.ToString());
            index++;
        });
       
        var tokenCost = await _azureAiSearchService.StoreAsync(documents);

        return (documents, tokenCost);
    }

    private async Task StoreDialog(
        Conversation? existingConversation,
        string demand,
        (string Text, int TotalTokens, int RequestTokens, int ResponseTokens) response,
        (List<string> Ids, int TokenCost) documents,
        CancellationToken cancellationToken)
    {
        if (existingConversation is not null)
        {
            existingConversation.Tokens += response.TotalTokens + documents.TokenCost;
            existingConversation.Dialogs.Add(new Dialog
            {
                Question = demand,
                Answer = response.Text,
                DocumentId = documents.Ids,
                TokensRequest = response.RequestTokens,
                TokensResponse = response.ResponseTokens,
                TokensDocumentProcessing = documents.TokenCost,
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
                        DocumentId = documents.Ids,
                        TokensRequest = response.RequestTokens,
                        TokensResponse = response.ResponseTokens,
                        TokensDocumentProcessing = documents.TokenCost,
                        CreatedAt = DateTime.Now,
                    }
                },
                Tokens = response.TotalTokens + documents.TokenCost
            };
            _context.Conversations.Add(conversation);
        }

        await _context.SaveChangesAsync(cancellationToken);
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
