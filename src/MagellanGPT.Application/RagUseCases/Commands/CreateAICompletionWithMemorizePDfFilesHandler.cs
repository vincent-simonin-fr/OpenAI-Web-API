using Azure.AI.OpenAI;
using MagellanGPT.Application.Common.Interfaces;
using MagellanGPT.Application.Common.Models;
using MediatR;
using Microsoft.KernelMemory.DataFormats;
using Microsoft.SemanticKernel;
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
    // TODO : Refactor
    private readonly IKernelBuilder _kernelBuilder;

    public CreateAICompletionWithMemorizePDfFilesHandler(IOpenAIService openAIService)
    {
        _openAIService = openAIService;
        _kernelBuilder = Kernel.CreateBuilder();
    }

    /// <summary>
    /// ExtractContent from PDF https://github.com/microsoft/kernel-memory/blob/main/examples/205-dotnet-extract-text-from-docs/Program.cs
    /// Memory & GetEmbedding https://github.com/microsoft/semantic-kernel/blob/main/dotnet/notebooks/06-memory-and-embeddings.ipynb
    /// && https://devblogs.microsoft.com/semantic-kernel/semantic-kernel-planner-improvements-with-embeddings-and-semantic-memory/
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IAsyncEnumerable<StreamingChatCompletionsUpdate>> Handle(CreateAICompletionWithMemorizePDfFiles request, CancellationToken cancellationToken)
    {
        // TODO : Refactor
        //var memoryWithACS = new MemoryBuilder()
        //    .WithOpenAITextEmbeddingGeneration("text-embedding-ada-002", "TestConfiguration.OpenAI.ApiKey")
        //    .WithMemoryStore(new AzureAISearchMemoryStore("TestConfiguration.AzureAISearch.Endpoint", "TestConfiguration.AzureAISearch.ApiKey"))
        //    .Build();

        // Process PDF files
        FileContent content = new();
        
        request.FilePathList.ForEach(file => {
            content = new PdfDecoder().ExtractContent(file);
            foreach (FileSection section in content.Sections)
            {
                Console.WriteLine($"Page: {section.Number}/{content.Sections.Count}");
                Console.WriteLine(section.Content);
                Console.WriteLine("-----");
            }
        });

        var response = await _openAIService.ProcessDemand(request.Demand);

        return response;
    }
}

// TODO : Refactor
// https://github1s.com/microsoft/kernel-memory/blob/main/service/Core/DataFormats/Pdf/PdfDecoder.cs#L7-L8
public class PdfDecoder
{
    public FileContent ExtractContent(string filename)
    {
        using var stream = File.OpenRead(filename);
        return this.ExtractContent(stream);
    }

    public FileContent ExtractContent(BinaryData data)
    {
        using var stream = data.ToStream();
        return this.ExtractContent(stream);
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
