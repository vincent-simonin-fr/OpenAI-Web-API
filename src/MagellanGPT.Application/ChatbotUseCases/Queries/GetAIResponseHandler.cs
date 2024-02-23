using Azure;
using Azure.AI.OpenAI;
using MediatR;

namespace MagellanGPT.Application.ChatbotUseCases.Queries;

public record GetAIResponse : IRequest<string>
{
    public string Answer { get; set; }
}

public class GetAIResponseHandler : IRequestHandler<GetAIResponse, string>
{
    public GetAIResponseHandler()
    {
    }

    public async Task<string> Handle(GetAIResponse request, CancellationToken cancellationToken)
    {
        OpenAIClient client = new OpenAIClient(
          new Uri("https://di3p32024.openai.azure.com/"),
          new AzureKeyCredential("279ce121a54b436cb5a130f340d95e30"));

        Response<ChatCompletions> responseWithoutStream = await client.GetChatCompletionsAsync(
        "gpt-35-turbo-1106",
        new ChatCompletionsOptions()
        {
            Messages =
          {
              new ChatMessage(ChatRole.System, @"You are an AI assistant that helps people find information.
                    Pour information:
                    la date d'anniversaire de victor levy est le 7 octobre"),
              new ChatMessage(ChatRole.User, @"quand est l'anniversaire de victor levy ?"),
                new ChatMessage(ChatRole.Assistant, @"L'anniversaire de Victor Levy est le 7 octobre."),
                new ChatMessage(ChatRole.User, @"et jules perrodon ?"),
                new ChatMessage(ChatRole.Assistant, @"Je suis désolé, mais je n'ai pas d'informations sur la date d'anniversaire de Jules Perrodon."),
                new ChatMessage(ChatRole.User, request.Answer),
          },
            Temperature = 1,
            MaxTokens = 800,
            // NucleusSamplingFactor = (float)0.95,
            FrequencyPenalty = 0,
            PresencePenalty = 0,
        });

        ChatCompletions response = responseWithoutStream.Value;

        return response.Choices.First().Message.Content;
    }
}

