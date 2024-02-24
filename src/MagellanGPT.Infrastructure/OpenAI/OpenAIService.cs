using Azure;
using Azure.AI.OpenAI;
using MagellanGPT.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace MagellanGPT.Infrastructure.OpenAI;

/// <summary>
/// Use Semantic Kernel
/// https://devblogs.microsoft.com/dotnet/demystifying-retrieval-augmented-generation-with-dotnet/
/// </summary>
public class OpenAIService : IOpenAIService
{
    private readonly IConfiguration _configuration;
    private readonly OpenAIClient _client;

    public OpenAIService(IConfiguration configuration)
    {
        _configuration = configuration;
        _client = new OpenAIClient(
          new Uri(_configuration.GetSection("OpenAi:Endpoint").Value!),
          new AzureKeyCredential(_configuration.GetSection("OpenAi:Key").Value!));
    }

    public async Task<IAsyncEnumerable<StreamingChatCompletionsUpdate>> ProcessDemand(string question)
    {
        StreamingResponse<StreamingChatCompletionsUpdate> responseStreamed = await _client.GetChatCompletionsStreamingAsync(
        new ChatCompletionsOptions()
        {
            DeploymentName = "gpt-35-turbo-1106",
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
            // NucleusSamplingFactor = (float)0.95,
            FrequencyPenalty = 0,
            PresencePenalty = 0,
        });

        return responseStreamed.EnumerateValues();

        //return responseStreamed.GetRawResponse().ContentStream!;
    }

    public async Task<string> ProcessDemandSynchronously(string question)
    {
        ChatCompletions responseWithoutStream = await _client.GetChatCompletionsAsync(
        new ChatCompletionsOptions()
        {
            DeploymentName = "gpt-35-turbo-1106",
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
            // NucleusSamplingFactor = (float)0.95,
            FrequencyPenalty = 0,
            PresencePenalty = 0,
        });

        return responseWithoutStream.Choices[0].Message.Content;
    }
}

