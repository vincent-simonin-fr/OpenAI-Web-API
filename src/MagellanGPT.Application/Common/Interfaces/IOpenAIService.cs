using Azure.AI.OpenAI;
using MagellanGPT.Domain.Entities;

namespace MagellanGPT.Application.Common.Interfaces;

public interface IOpenAIService
{
    Task<IAsyncEnumerable<StreamingChatCompletionsUpdate>> ProcessDemand(string question, string? deploymentName = null);
    Task<(string Text, int TotalTokens, int RequestTokens, int ResponseTokens)> ProcessDemandSynchronously(Conversation conversation);
    Task<IAsyncEnumerable<StreamingChatCompletionsUpdate>> ProcessDemandWithRag(string question, string document, string? deploymentName = null);
    Task<(ReadOnlyMemory<float> EmbeddingArray, int TotalTokens)> GetEmbeddingsAsync(string document);
    Task<(string Text, int TotalTokens, int RequestTokens, int ResponseTokens)> ProcessDemandWithRagSynchronously(string question, string document, string? deploymentName = null);
}

