using MagellanGPT.Application.ChatbotUseCasesCommands;
using MagellanGPT.Application.Common.Behaviours;
using MagellanGPT.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace MagelanGPT.Application.UnitTests.Common.Behaviours;

public class RequestLoggerTests
{
    private Mock<ILogger<CreateAICompletionSynchronously>> _logger = null!;
    private Mock<ICurrentUserService> _currentUserService = null!;
    private Mock<IIdentityService> _identityService = null!;

    [SetUp]
    public void Setup()
    {
        _logger = new Mock<ILogger<CreateAICompletionSynchronously>>();
        _currentUserService = new Mock<ICurrentUserService>();
        _identityService = new Mock<IIdentityService>();
    }

    [Test]
    public async Task ShouldCallGetUserNameAsyncOnceIfAuthenticated()
    {
        _currentUserService.Setup(x => x.UserId).Returns(Guid.NewGuid().ToString());

        // var requestLogger = new LoggingBehaviour<CreateAICompletionSynchronously>(_logger.Object, _currentUserService.Object, _identityService.Object);
        var requestLogger = new LoggingBehaviour<CreateAICompletionSynchronously>(_logger.Object);

        await requestLogger.Process(new CreateAICompletionSynchronously { Demand = "Est ce que la méthode GetUserNameAsync est appelé lorsqu'un utilisateur authentifié requête l'API" }, new CancellationToken());

        _identityService.Verify(i => i.GetUserNameAsync(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task ShouldNotCallGetUserNameAsyncOnceIfUnauthenticated()
    {
        var requestLogger = new LoggingBehaviour<CreateAICompletionSynchronously>(_logger.Object);

        await requestLogger.Process(new CreateAICompletionSynchronously { Demand = "Est ce que la méthode GetUserNameAsync n'est pas appelé lorsqu'un utilisateur non authentifié requête l'API" }, new CancellationToken());

        _identityService.Verify(i => i.GetUserNameAsync(It.IsAny<string>()), Times.Never);
    }
}



