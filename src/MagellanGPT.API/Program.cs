using Azure.Identity;
using MagellanGPT.API;
using MagellanGPT.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Load configuration from Azure App Configuration
// https://learn.microsoft.com/fr-fr/azure/azure-app-configuration/howto-integrate-azure-managed-service-identity?pivots=framework-dotnet
builder.Configuration.AddAzureAppConfiguration(builder.Configuration["AppConfig:ConnectionString"]);

//if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
//{
//    // Load configuration from Azure App Configuration
//    // https://learn.microsoft.com/fr-fr/azure/azure-app-configuration/howto-integrate-azure-managed-service-identity?pivots=framework-dotnet
//    builder.Configuration.AddAzureAppConfiguration(options =>
//    options.Connect(
//        new Uri(builder.Configuration["AppConfig:Endpoint"]),
//        new ManagedIdentityCredential("d7ef3c85-2d71-41bd-afe0-2c715ff53583")));
//}

builder.Services.AddAPIServices();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddHttpClient();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Initialise and seed database
    using (var scope = app.Services.CreateScope())
    {
        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();
        await initialiser.InitialiseAsync();
        // await initialiser.SeedAsync();
    }
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHealthChecks("/health");

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseRouting();

app.MapControllers();

app.Run();

