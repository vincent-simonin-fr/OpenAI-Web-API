namespace MagellanGPT.Application.Common.Interfaces;

public interface IAzureAiSearchService
{
    Task<int> StoreAsync(Dictionary<string, string> documents);
    Task SearchMemoryAsync(string query);
}
