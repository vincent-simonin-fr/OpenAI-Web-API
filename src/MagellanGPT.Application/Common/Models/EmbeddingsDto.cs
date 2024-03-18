
using Azure.AI.OpenAI;
namespace MagellanGPT.Application.Common.Models;

public class EmbeddingsDto
{
    public string Text { get; set; }
    public Embeddings Embeddings { get; set; }
}

