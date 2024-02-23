using MagellanGPT.Application.Common.Interfaces;

namespace MagellanGPT.API.Services;

// Build the CurrentUser from HttpContext
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => AccessToken is null || VerifyJwt(AccessToken).Result is null ? null : VerifyJwt(AccessToken)?.Result.Value.userId;

    public string? AccessToken => _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty);

    public string[]? Roles => VerifyJwt(AccessToken).Result.Value.roles;

    /// <summary>
    /// Allows to validate a token.
    /// </summary>
    /// <param name="token">token.</param>
    /// <returns>User ID and user Roles.</returns>
    public async Task<(string? userId, string[] roles)?> VerifyJwt(string token)
    {
        if (token == null)
        {
            return null;
        }

        try
        {
            return null;
        }
        catch (Exception ex)
        {
            // return null if validation fails
            return null;
        }
    }
}


