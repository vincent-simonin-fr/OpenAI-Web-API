using System.Reflection;
using MagellanGPT.Application.Common.Security;
using MediatR;

namespace MagellanGPT.Application.Common.Behaviours;

public class AuthorizationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public AuthorizationBehaviour()
    {
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var authorizeAttributes = request.GetType().GetCustomAttributes<AuthorizeAttribute>();

        if (authorizeAttributes.Any())
        {
        }

        // User is authorized / authorization not required
        return await next();
    }
}

