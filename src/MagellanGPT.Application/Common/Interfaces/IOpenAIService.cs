using Azure.AI.OpenAI;

namespace MagellanGPT.Application.Common.Interfaces;

public interface IOpenAIService
{
    Task<IAsyncEnumerable<StreamingChatCompletionsUpdate>> ProcessDemand(string question);
    Task<string> ProcessDemandSynchronously(string question);
}

