using MagellanGPT.Application.ChatbotUseCasesCommands;
using MagellanGPT.Application.Common.Interfaces;
using MagellanGPT.Infrastructure.OpenAI;
using MagellanGPT.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace MagelanGPT.Application.UnitTests;

[TestFixture]
public class ChatBotUseCasesTests
{
    private Mock<IApplicationDbContext> _dbContext;
    private Mock<IOpenAIService> _openAiService;
    private CreateAICompletionSynchronouslyHandler _createAICompletionSynchronouslyHandler;
    private CreateAICompletionSynchronously _createAICompletionSynchronously;
    private Mock<IMediator> _mediator;

    [SetUp]
    public void Setup()
    {
        _dbContext = new();
        _openAiService = new();

        _mediator = new Mock<IMediator>();

        _createAICompletionSynchronouslyHandler = new(_openAiService.Object, _dbContext.Object);

    }

    [Test]
    public void ShouldReturnResponseContainingTextAndTokenCost()
    {
        // Arrange
        _createAICompletionSynchronously = new() { Demand = "Répond moi cette phrase à l'envers : Engage le jeu que je le gagne" };
        //var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        //    .UseInMemoryDatabase("Conversation") // Utilisez un nom de base de données unique pour chaque test
        //    .Options;

        //using var context = new ApplicationDbContext(options);
        //var handler = new CreateAICompletionSynchronouslyHandler(_openAiService.Object, context);
        // Act
        var response = _createAICompletionSynchronouslyHandler.Handle(_createAICompletionSynchronously, new System.Threading.CancellationToken());

        
        Assert.IsNotNull(response.Result);
        Assert.IsInstanceOf<string>(response.Result);
    }
}
