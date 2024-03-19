using MagellanGPT.Application.ChatbotUseCasesCommands;
using MagellanGPT.Application.Common.Interfaces;
using MagellanGPT.Application.Common.Models;
using MagellanGPT.Domain.Entities;
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
    private Mock<ICurrentUserService> _currentUserService;

    [SetUp]
    public void Setup()
    {
        _dbContext = new();
        _openAiService = new();
        _currentUserService = new();
        _createAICompletionSynchronouslyHandler = new(_openAiService.Object, _dbContext.Object, _currentUserService.Object);
    }

    [Test]
    public void ShouldReturnResponseContainingTextAndTokenCost()
    {
        // Arrange
        List<string> demands = new ()
        {
            // Basiques
            "Répond moi cette phrase à l'envers : Engage le jeu que je le gagne",
            "",
            "texte simple",
            "123456",
    
            // Caractères spéciaux
            "!@#$%^&*()",
            "<>[]{}|\\",
            "‘’“”",
    
            // SQL Injection - Tâches simples
            "'; DROP TABLE users;",
            "' OR '1'='1",
            "' OR 1=1--",
            "' UNION SELECT * FROM users",
    
            // Tentatives d'échappement
            "'; EXEC xp_cmdshell('dir'); --",
            "\"; DROP TABLE users; --",
    
            // Encodage
            "Robert'); DROP TABLE Students;--",
            "זה טקסט בעברית",
            "这是一段中文文本",
            "これは日本語のテキストです",
    
            // Données longues et complexes
            new string('A', 2048), // Chaîne très longue
            "';WAITFOR DELAY '00:00:05'--", // Délai pour tester les performances et le timing des requêtes
    
             // Encapsulations et formatages
             "John Doe <john.doe@example.com>",
             "\" OR \"\"=\"",

             // Contenus malveillants potentiels
             "<script>alert('XSS')</script>",
             "%27%20OR%20%271%27%3D%271",
        };

        demands.ForEach(demand =>
        {
            // Act
            _createAICompletionSynchronously = new() { Demand = demand, UserId = "fd285508-8ba1-4064-be24-30dfdea0b376" };

            var mockDbSetConversation = GetDbSetMockedOf<Conversation>();
            var mockDbSetUser = GetDbSetMockedOf<User>();

            _dbContext.Setup(c => c.Conversation).Returns(mockDbSetConversation);
            _dbContext.Setup(c => c.User).Returns(mockDbSetUser);

            // Arrange
            var response = _createAICompletionSynchronouslyHandler.Handle(_createAICompletionSynchronously, CancellationToken.None).Result;

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response, Is.InstanceOf<ResponseDto>());
            Assert.That(response.Id, Is.Not.Null);
            Assert.That(response.ConversationId, Is.Not.Null);
            Assert.That(response.Tokens, Is.Zero);

        });
    }

    private DbSet<T> GetDbSetMockedOf<T>() where T: class, new()
    {
        var entites = new List<T>().AsQueryable();

        var mockSet = new Mock<DbSet<T>>();
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(entites.Provider);
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(entites.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(entites.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(entites.GetEnumerator());

        return mockSet.Object;
    }
}
