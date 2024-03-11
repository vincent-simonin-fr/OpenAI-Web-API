using System.Dynamic;
using System.Text.Json;
using MagellanGPT.Application.Common.Interfaces;
using MagellanGPT.Application.Common.Models;
using MagellanGPT.Domain.Entities;
using MediatR;

namespace MagellanGPT.Application.ChatbotUseCases.Queries;

public record GetDataFromCosmosDb : IRequest<IEnumerable<ConversationDTO>>;

public class GetDataFromCosmosDbHandler : IRequestHandler<GetDataFromCosmosDb, IEnumerable<ConversationDTO>>
{
    private readonly IApplicationDbContext _context;

    public GetDataFromCosmosDbHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ConversationDTO>> Handle(GetDataFromCosmosDb request, CancellationToken cancellationToken)
    {
        var conversation = _context.Conversations.AsQueryable();

        return conversation.Select(ConversationDTO.Projection).ToList();
    }

    // TODO : Pour étude parser à refactoriser
    /// <summary>
    /// var dynamicObject = JsonSerializer.Deserialize<dynamic>(conversation.First().ConversationId)!;
    /// var test = UsingJsonElement(conversation.First().ConversationId);
    /// </summary>
    /// <param name="jsonString"></param>
    /// <returns></returns>
    public static (string? Genre, int Imdb, double Rotten) UsingJsonElement(string jsonString)
    {
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(jsonString);
        return FromJsonElement(jsonElement);
    }

    public static (string? Genre, int Imdb, double Rotten) FromJsonElement(JsonElement jsonElement)
    {
        var users = jsonElement
            .GetProperty("users");

        var array = users.EnumerateArray();

        var username = array.First().GetProperty("username").GetString();

        var imdb = jsonElement
            .GetProperty("users")
            .EnumerateArray()
            .First()
            .GetProperty("id")
            .GetInt16();

        var rotten = 2.23;
        return (username, imdb, rotten);
    }
}