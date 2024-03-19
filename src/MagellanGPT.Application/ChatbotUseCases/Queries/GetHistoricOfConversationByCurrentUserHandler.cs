using System.Collections.Concurrent;
using System.Data;
using System.Text.Json;
using MagellanGPT.Application.Common.Interfaces;
using MagellanGPT.Application.Common.Models;
using MagellanGPT.Application.Common.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
        //var users2 = _context.User.Where(chat => chat.Users).AsEnumerable().ToList();
        //var users1 = _context.Chats.Select(chat => chat.Users).FirstOrDefault();
        //var chat = _context.Chats.Select(chat => chat.Users.Where(u => u.Id.ToString() == "83f00ae7-efa3-46a6-88d0-6cbd411084cb")).FirstOrDefault();

        //var users = await _context.User
        //    .Where(user => user.ObjectId == "Test" && user.PartitionKey == "Chat")
        //    .FirstAsync(cancellationToken);

        //var user = await _context.User
        //    .Where(user => user.ObjectId == _currentUserService.UserId && EF.Property<string>(user, "PartitionKey") == "Chat")
        //    .FirstAsync(cancellationToken);

        var conversation = _context.Conversation.Where(conversation => conversation.User.ObjectId == _currentUserService.UserId).ToList();

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