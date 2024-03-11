using Azure.AI.OpenAI;

namespace MagellanGPT.Application.Common.Interfaces;

public interface IOpenAIService
{
    Task<IAsyncEnumerable<StreamingChatCompletionsUpdate>> ProcessDemand(string question, string? deploymentName = null);
    Task<string> ProcessDemandSynchronously(string question);
}

