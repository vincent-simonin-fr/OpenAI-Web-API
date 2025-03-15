using MagellanGPT.Domain.Entities;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;

namespace MagellanGPT.Application.Common.Interfaces;

public interface ISemanticKernelProvider
{
    Task<ChatMessageContent> ProcessUserRequest(Conversation conversation);
    Task StoreDocumentAsync(List<string> filePathList);
    Task<SearchResult> SearchCitationAsync(string query);
}

