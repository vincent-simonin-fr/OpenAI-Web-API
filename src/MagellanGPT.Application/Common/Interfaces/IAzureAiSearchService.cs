using Microsoft.KernelMemory;

namespace MagellanGPT.Application.Common.Interfaces;

public interface IAzureAiSearchService
{
    Task<int> StoreAsync(Dictionary<string, string> documents);
    Task<SearchResult> SearchMemoryAsync(string query);
}
