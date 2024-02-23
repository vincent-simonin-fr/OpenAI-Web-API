using MagellanGPT.API.Services;
using MagellanGPT.Application.Common.Interfaces;
using Microsoft.OpenApi.Models;

namespace MagellanGPT.API;

public static class DependencyInjection
{
    public static IServiceCollection AddAPIServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddHealthChecks();
            // .AddDbContextCheck<ApplicationDbContext>();

        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
            {
                Name = "Authorization",
                Type = SecuritySchemeType.OpenIdConnect,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Type into the textbox: Bearer {your JWT token}.",
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
             {
                 {
                       new OpenApiSecurityScheme
                         {
                             Reference = new OpenApiReference
                             {
                                 Type = ReferenceType.SecurityScheme,
                                 Id = "Bearer"
                             }
                         },
                         Array.Empty<string>()
                 }
             });
        });

        return services;
    }
}

