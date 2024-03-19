using Azure;
using Azure.AI.OpenAI;
using MagellanGPT.Application.Common.Interfaces;
using MagellanGPT.Application.Common.Models;
using MagellanGPT.Domain.Entities;
using MagellanGPT.Infrastructure.KeyVault;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.Text;

namespace MagellanGPT.Infrastructure.OpenAI;

/// <summary>
/// Use Semantic Kernel
/// https://devblogs.microsoft.com/dotnet/demystifying-retrieval-augmented-generation-with-dotnet/
/// </summary>
public class OpenAIService : IOpenAIService
{
    private readonly OpenAIClient _client;
    private string _deploymentName;

    public OpenAIService(IConfiguration configuration)
    {
        _client = new OpenAIClient(
          new Uri(configuration.GetSection("OpenAi:Endpoint").Value!),
          new AzureKeyCredential(SecretManager.GetInstance().OpenAiKey));

        _deploymentName = configuration.GetSection("OpenAi:DefaultDeploymentName").Value!;
    }

    public async Task<IAsyncEnumerable<StreamingChatCompletionsUpdate>> ProcessDemand(string question, string? deploymentName = null)
    {
        _deploymentName = deploymentName is not null ? deploymentName : _deploymentName;

        StreamingResponse <StreamingChatCompletionsUpdate> responseStreamed = await _client.GetChatCompletionsStreamingAsync(
        new ChatCompletionsOptions()
        {
            DeploymentName = _deploymentName,
            Messages =
          {
              new ChatRequestSystemMessage(@"You are an AI assistant that helps people find information.
                    Pour information:
                    la date d'anniversaire de victor levy est le 7 octobre"),
              new ChatRequestUserMessage(@"quand est l'anniversaire de victor levy ?"),
                new ChatRequestAssistantMessage(@"L'anniversaire de Victor Levy est le 7 octobre."),
                new ChatRequestUserMessage(@"et jules perrodon ?"),
                new ChatRequestAssistantMessage(@"Je suis désolé, mais je n'ai pas d'informations sur la date d'anniversaire de Jules Perrodon."),
                new ChatRequestUserMessage(question),
          },
            Temperature = 1,
            MaxTokens = 800,
            FrequencyPenalty = 0,
            PresencePenalty = 0,
        });

        return responseStreamed.EnumerateValues();
    }


    public async Task<IAsyncEnumerable<StreamingChatCompletionsUpdate>> ProcessDemandWithRag(string question, string document, string? deploymentName = null)
    {
        _deploymentName = deploymentName is not null ? deploymentName : _deploymentName;

        // Prompt Chaining https://www.promptingguide.ai/fr/techniques/prompt_chaining
        StreamingResponse<StreamingChatCompletionsUpdate> responseStreamed = await _client.GetChatCompletionsStreamingAsync(
        new ChatCompletionsOptions()
        {
            DeploymentName = _deploymentName,
            Messages =
            {
                new ChatRequestSystemMessage($"Tu es un expert quelque soit le domaine." +
                $"Ta tâche est d'aider à répondre à une question en utilisant un document. " +
                $"La première étape est d'extraire des informations pertinentes du document, délimité par ###" +
                $". Génère une réponse. " +
                $"### {document} ###"),
                new ChatRequestUserMessage(question),
            },
            Temperature = 1,
            MaxTokens = 800,
            FrequencyPenalty = 0,
            PresencePenalty = 0,
        });

        return responseStreamed.EnumerateValues();
    }

    // TODO: Remove if not used
    public async Task<Conversation> ProcessDemandSynchronously(Conversation conversation)
    {
        List<ChatRequestMessage> messages;
        var question = conversation.Dialogs[^1].Question;
        string? response = null;
        string? title = null;

        if (conversation.Dialogs.Count == 1)
        {
            messages = new List<ChatRequestMessage>() {
                new ChatRequestSystemMessage($"Tu es un assistant IA expert. " +
                $"La complétion devra comportée une première partie titre délimité par les balises ### et ### sans espace, par exemple ###titre###, en début de réponse, " +
                $"cette partie titre doit résumer la question suivante '{question}' en 4 mots maximum"),
            };
        }
        else
        {
            messages = new List<ChatRequestMessage>() {
                new ChatRequestSystemMessage(@"Tu es un assistant IA expert."),
            };
        }

        conversation.Dialogs.ForEach(dialog =>
        {
            if(dialog.Answer is not null)
            {
                messages.Add(new ChatRequestUserMessage(dialog.Question));
                messages.Add(new ChatRequestUserMessage(dialog.Answer));
            }
        });

        messages.Add(new ChatRequestUserMessage(question));

        var chatCompletionsOptions = new ChatCompletionsOptions()
        {
            DeploymentName = _deploymentName,
            Temperature = (float)0.7,
            MaxTokens = 800,

            NucleusSamplingFactor = (float)0.95,
            FrequencyPenalty = 0,
            PresencePenalty = 0,
        };

        messages.ForEach(chatCompletionsOptions.Messages.Add);

        ChatCompletions responseWithoutStream = await _client.GetChatCompletionsAsync(chatCompletionsOptions);

        if (responseWithoutStream.Choices[0].Message.Content.Contains("###"))
        {
            var responseWithTitle = responseWithoutStream.Choices[0].Message.Content.Split("###", StringSplitOptions.RemoveEmptyEntries);
            title = responseWithTitle[0];
            response = responseWithTitle[1].Replace("\n\n", "").Trim();

            conversation.Title = title;
            conversation.Dialogs[^1].Answer = response;
        }
        else
        {
            conversation.Dialogs[^1].Answer = responseWithoutStream.Choices[0].Message.Content;
        }

        conversation.Dialogs[^1].CreatedAt = DateTime.UtcNow;
        conversation.Dialogs[^1].TokensRequest = responseWithoutStream.Usage.PromptTokens;
        conversation.Dialogs[^1].TokensResponse = responseWithoutStream.Usage.CompletionTokens;
        conversation.Tokens += responseWithoutStream.Usage.TotalTokens;

        return conversation;
    }

    public async Task<Conversation> ProcessDemandWithRagSynchronously(Conversation conversation, string document)
    {
        _deploymentName = conversation.LlmDeploymentName is not null ? conversation.LlmDeploymentName : _deploymentName;

        // Prompt Chaining https://www.promptingguide.ai/fr/techniques/prompt_chaining
        ChatCompletions response = await _client.GetChatCompletionsAsync(
        new ChatCompletionsOptions()
        {
            DeploymentName = _deploymentName,
            Messages =
            {
                new ChatRequestSystemMessage($"Tu es un expert quelque soit le domaine." +
                $"Ta tâche est d'aider à répondre à une question en utilisant un document et tes connaissances. " +
                $"La première étape est d'extraire des informations pertinentes du document, délimité par ###" +
                $". Génère une réponse. " +
                $"### {document} ###"),
                new ChatRequestUserMessage(conversation.Dialogs![^1].Question),
            },
            Temperature = 1,
            MaxTokens = 800,
            FrequencyPenalty = 0,
            PresencePenalty = 0,
        });

        conversation.Dialogs![^1].CreatedAt = DateTime.UtcNow;
        conversation.Dialogs[^1].TokensRequest = response.Usage.PromptTokens;
        conversation.Dialogs[^1].TokensResponse = response.Usage.CompletionTokens;
        conversation.Tokens += response.Usage.TotalTokens;
        conversation.Dialogs[^1].Answer = response.Choices[0].Message.Content;

        return conversation;
    }

//    public async Task<(ReadOnlyMemory<float> EmbeddingArray, int TotalTokens)> GetEmbeddingsAsync(string document)
//    {
//        var embeddings = new Dictionary<int, Embeddings>();

//#pragma warning disable SKEXP0055
//#pragma warning disable SKEXP0050
//        var lines = TextChunker.SplitPlainTextLines(document, 40);
//        var paragraphs = TextChunker.SplitPlainTextParagraphs(lines, 120);

//        var index = 1;

//        paragraphs.ForEach(async (paragraph) =>
//        {
//            EmbeddingsOptions embeddingOptions = new()
//            {
//                DeploymentName = "text-embedding-ada-002",
//                Input = { paragraph },
//            };

//            var returnValue = await _client.GetEmbeddingsAsync(embeddingOptions);

//            embeddings.Add(index , returnValue.Value);
//        });

//        EmbeddingsOptions embeddingOptions = new()
//        {
//            DeploymentName = "text-embedding-ada-002",
//            Input = { document },
//        };

//        var returnValue = await _client.GetEmbeddingsAsync(embeddingOptions);


//        var totalTokens = returnValue.Value.Usage.TotalTokens;
//        var embeddingArray = returnValue.Value.Data[0].Embedding;

//        foreach (float item in returnValue.Value.Data[0].Embedding.ToArray())
//        {
//            Console.WriteLine(item);
//        }

//        return (embeddingArray, totalTokens);
//    }

    public async Task<Dictionary<int, EmbeddingsDto>> GetEmbeddings(string document)
    {
        var embeddings = new Dictionary<int, EmbeddingsDto>();
#pragma warning disable SKEXP0055
#pragma warning disable SKEXP0050
        var lines = TextChunker.SplitPlainTextLines(document, 40);
        var paragraphs = TextChunker.SplitPlainTextParagraphs(lines, 120);

        var index = 1;

        paragraphs.ForEach((paragraph) =>
        {
            EmbeddingsOptions embeddingOptions = new()
            {
                DeploymentName = "text-embedding-ada-002",
                Input = { paragraph },
            };

            try
            {
                var returnValue = _client.GetEmbeddings(embeddingOptions);
                embeddings.Add(index, new EmbeddingsDto() { Text = paragraph, Embeddings = returnValue.Value });

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                }

            index++;
        });

        return embeddings;
    }
}

