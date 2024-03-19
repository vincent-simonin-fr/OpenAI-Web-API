using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MagellanGPT.Infrastructure.Persistence;

public class ApplicationDbContextInitialiser
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _config;
    private static bool _ensureCreated { get; set; } = false;

    public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context, IConfiguration config)
    {
        _logger = logger;
        _context = context;
        _config = config;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            if (!_ensureCreated)
            {
                _context.Database.EnsureCreated();
                _ensureCreated = true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        // Container
        //using CosmosClient client = new CosmosClient(
        //    accountEndpoint: _config.GetSection("CosmosDb:Endpoint").Value!,
        //    authKeyOrResourceToken: _config.GetSection("CosmosDb:Key").Value!);

        //var db = client.GetDatabase(CosmosDbConst.DbName);

        //// https://learn.microsoft.com/en-us/dotnet/api/microsoft.azure.cosmos.containerproperties?view=azure-dotnet
        //ContainerProperties containerProperties = new ContainerProperties()
        //{
        //    Id = CosmosDbConst.ConversationContainer,
        //    PartitionKeyPaths = new List<string>{ "/ConversationID", "/Dialogs", "LlmDeploymentName", "Token" },
        //    IndexingPolicy = new IndexingPolicy()
        //    {
        //        Automatic = false,
        //        IndexingMode = IndexingMode.Lazy,
        //    }
        //};

        // TODO: CosmosException The expression cannot be evaluated. A common cause of this error is attempting to pass a lambda into a delegate.
        //await db.CreateContainerIfNotExistsAsync(
        //     containerProperties,
        //     ThroughputProperties.CreateAutoscaleThroughput(1000));


        // Sample Data
        // var container = db.GetContainer(CosmosDbConst.ConversationContainer);

        //await container.CreateItemAsync(new Conversation
        //{
        //    Question = "Give a json representing a list of two fake users",
        //    Answer = "{\r\n        \"users\": [\r\n            {\r\n                \"id\": 1,\r\n                \"username\": \"blueSky123\",\r\n                \"name\": \"Alex Johnson\",\r\n                \"email\": \"alex.johnson@example.com\",\r\n                \"age\": 29,\r\n                \"gender\": \"Non-Binary\",\r\n                \"location\": {\r\n                    \"city\": \"San Francisco\",\r\n                    \"state\": \"California\",\r\n                    \"country\": \"USA\"\r\n                },\r\n                \"preferences\": {\r\n                    \"hobbies\": [\r\n                        \"hiking\",\r\n                        \"photography\",\r\n                        \"reading\"\r\n                    ],\r\n                    \"music\": [\r\n                        \"indie\",\r\n                        \"electronic\",\r\n                        \"jazz\"\r\n                    ],\r\n                    \"movies\": [\r\n                        \"drama\",\r\n                        \"thriller\",\r\n                        \"documentary\"\r\n                    ]\r\n                }\r\n            },\r\n            {\r\n                \"id\": 2,\r\n                \"username\": \"nightOwl88\",\r\n                \"name\": \"Jamie Rivera\",\r\n                \"email\": \"jamie.rivera@example.com\",\r\n                \"age\": 35,\r\n                \"gender\": \"Female\",\r\n                \"location\": {\r\n                    \"city\": \"New York\",\r\n                    \"state\": \"New York\",\r\n                    \"country\": \"USA\"\r\n                },\r\n                \"preferences\": {\r\n                    \"hobbies\": [\r\n                        \"cooking\",\r\n                        \"gaming\",\r\n                        \"traveling\"\r\n                    ],\r\n                    \"music\": [\r\n                        \"rock\",\r\n                        \"pop\",\r\n                        \"classical\"\r\n                    ],\r\n                    \"movies\": [\r\n                        \"comedy\",\r\n                        \"action\",\r\n                        \"sci-fi\"\r\n                    ]\r\n                }\r\n            }\r\n        ]\r\n    }",
        //    Token = 100
        //});
    } 
}

