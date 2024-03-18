using System.Data;
using System.Text.Json;
using MagellanGPT.Application.Common.Interfaces;
using MagellanGPT.Application.Common.Models;
using MagellanGPT.Application.Common.Security;
using MediatR;

namespace MagellanGPT.Application.ChatbotUseCases.Queries;

[Authorize(Roles = "user")]
public record GetHistoricOfConversationByCurrentUser : IRequest<IEnumerable<ConversationDto>>;

public class GetHistoricOfConversationByCurrentUserHandler : IRequestHandler<GetHistoricOfConversationByCurrentUser, IEnumerable<ConversationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetHistoricOfConversationByCurrentUserHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<ConversationDto>> Handle(GetHistoricOfConversationByCurrentUser request, CancellationToken cancellationToken)
    {
        var conversation = _context.Conversations.Where(conversation => conversation.Id == _currentUserService.UserId).ToList();

        return conversation.AsQueryable().Select(ConversationDto.Projection).ToList();
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