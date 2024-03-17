using System.Reflection;
using MagellanGPT.API.Services;
using MagellanGPT.Application.Common.Interfaces;
using MagellanGPT.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;

namespace MagellanGPT.API;

public static class DependencyInjection
{
    public static IServiceCollection AddAPIServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>(
                    null,
                    HealthStatus.Unhealthy,
                    null,
                    PerformCosmosHealthCheck());

        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo {
                Title = "MagellanGPT",
                Version = "v1",
                Description = "Assistant AI",
                Contact = new OpenApiContact()
                {
                    Name = "Vincent Simonin",
                    Email = "vincent.simonin@diiage.org",
                }
            });
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
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
                             },
                             Scheme = "oauth2",
                             Name = "Bearer",
                             In = ParameterLocation.Header
                         },
                         Array.Empty<string>()
                 }
             });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);
        });

        return services;
    }

    private static Func<ApplicationDbContext, CancellationToken, Task<bool>> PerformCosmosHealthCheck() =>
      async (context, _) =>
      {
          try
          {
              await context.Database.GetCosmosClient().ReadAccountAsync().ConfigureAwait(false);
          }
          catch (HttpRequestException)
          {
              return false;
          }
          return true;
      };

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
            .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,
                                                                        retryAttempt)));
    }
}

