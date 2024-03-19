using Microsoft.AspNetCore.Identity;

namespace MagellanGPT.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string PartitionKey { get; set; } = "ApplicationUser";
}

