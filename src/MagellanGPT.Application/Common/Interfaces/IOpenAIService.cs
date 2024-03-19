using Azure.AI.OpenAI;
using MagellanGPT.Application.Common.Models;
using MagellanGPT.Domain.Entities;

namespace MagellanGPT.Application.Common.Interfaces;

public interface IOpenAIService
{
    Task<IAsyncEnumerable<StreamingChatCompletionsUpdate>> ProcessDemand(string question, string? deploymentName = null);
    Task<Conversation> ProcessDemandSynchronously(Conversation conversation);
    // Task<IAsyncEnumerable<StreamingChatCompletionsUpdate>> ProcessDemandWithRag(string question, string document, string? deploymentName = null);
    // Task<(ReadOnlyMemory<float> EmbeddingArray, int TotalTokens)> GetEmbeddingsAsync(string document);
    Task<Dictionary<int, EmbeddingsDto>> GetEmbeddings(string document);
    Task<Conversation> ProcessDemandWithRagSynchronously(Conversation conversation, string document);
}