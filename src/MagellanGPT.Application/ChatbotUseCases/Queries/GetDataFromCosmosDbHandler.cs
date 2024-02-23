using MagellanGPT.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MagellanGPT.Application.ChatbotUseCases.Queries;

public record GetDataFromCosmosDb : IRequest<object>
{

}

public class GetDataFromCosmosDbHandler : IRequestHandler<GetDataFromCosmosDb, object>
{
    private readonly IApplicationDbContext _context;

    public GetDataFromCosmosDbHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<object> Handle(GetDataFromCosmosDb request, CancellationToken cancellationToken)
    {
        var conversation = _context.Conversations.ToList();

        throw new NotImplementedException();
    }
}

