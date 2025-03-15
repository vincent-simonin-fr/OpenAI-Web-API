using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MagellanGPT.Application.Common.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace MagellanGPT.API.Services;

// Build the CurrentUser from HttpContext
public class CurrentUserService : ICurrentUserService
{
    // Pem ou Cert sont ok
    private readonly string _certFilePath = "certificate2.pem";
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

        // Build security public key from cert file
        var certString = await File.ReadAllTextAsync(_certFilePath);
        var certByteArray = Encoding.UTF8.GetBytes(certString);
        X509Certificate2 cert = new X509Certificate2(certByteArray);
        RSA? publicKey = cert.GetRSAPublicKey();
        RsaSecurityKey securityKey = new RsaSecurityKey(publicKey);

        var jwtHandler = new JwtSecurityTokenHandler();
        try
        {
            jwtHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = securityKey,
                ValidateIssuer = false,
                ValidateAudience = false,

                // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                ClockSkew = TimeSpan.Zero,
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;

            var userId = jwtToken.Claims.First(x => x.Type == "oid").Value;
            // var jsonRoles = jwtToken.Claims.First(x => x.Type == "roles").Value;
            var claimJson = JsonSerializer.Serialize(new { roles = new string[] { "user" } });
            var claim = JsonSerializer.Deserialize<Claim>(claimJson);

            return (userId, claim.roles);
        }
        catch (Exception ex)
        {
            // return null if validation fails
            return null;
        }
    }
}

public class Claim
{
    [JsonPropertyName("roles")]
    public string[] roles { get; set; }
}

