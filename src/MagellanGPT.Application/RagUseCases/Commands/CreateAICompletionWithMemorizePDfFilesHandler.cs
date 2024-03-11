using System.Text;
using Azure.AI.OpenAI;
using MagellanGPT.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Primitives;
using Microsoft.KernelMemory.DataFormats;
using Microsoft.SemanticKernel.Connectors.AzureAISearch;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace MagellanGPT.Application.RAGUseCases.Commands;

public record CreateAICompletionWithMemorizePDfFiles : IRequest<IAsyncEnumerable<StreamingChatCompletionsUpdate>>
{
    public required List<string> FilePathList { get; set; }
    public string Demand { get; set; }
};

public class CreateAICompletionWithMemorizePDfFilesHandler : IRequestHandler<CreateAICompletionWithMemorizePDfFiles, IAsyncEnumerable<StreamingChatCompletionsUpdate>>
{
    private readonly IOpenAIService _openAIService;
    private readonly IAzureAiSearchService _azureAiSearchService;

    public CreateAICompletionWithMemorizePDfFilesHandler(IOpenAIService openAIService, IAzureAiSearchService azureAiSearchService)
    {
        _openAIService = openAIService;
        _azureAiSearchService = azureAiSearchService;
    }

    /// <summary>
    /// ExtractContent from PDF https://github.com/microsoft/kernel-memory/blob/main/examples/205-dotnet-extract-text-from-docs/Program.cs
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IAsyncEnumerable<StreamingChatCompletionsUpdate>> Handle(CreateAICompletionWithMemorizePDfFiles request, CancellationToken cancellationToken)
    {
        // Process PDF files
        FileContent content = new();
        StringBuilder document = new();

        var index = 1;
        request.FilePathList.ForEach(async file => {
            content = new PdfDecoder().ExtractContent(file);
            document.Append($"Document N°{index}");
            foreach (FileSection section in content.Sections)
            {
                Console.WriteLine($"Page: {section.Number}/{content.Sections.Count}");
                Console.WriteLine(section.Content);
                Console.WriteLine("-----");
                document.Append($"Page: {section.Number}/{content.Sections.Count}");
                document.Append(section.Content);
                document.Append("-----");
            }
            await _azureAiSearchService.StoreAsync(new Dictionary<string, string> { { $"Document N°{index}", document.ToString() } });
            index++;
        });

        

        var response = await _openAIService.ProcessDemandWithRag(request.Demand, document.ToString());

        return response;
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
