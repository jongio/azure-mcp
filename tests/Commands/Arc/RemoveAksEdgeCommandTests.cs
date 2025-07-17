using AzureMcp.Commands.Arc;
using AzureMcp.Models.Command;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.CommandLine;
using System.CommandLine.Parsing;
using Xunit;

namespace AzureMcp.Tests.Commands.Arc;

public class RemoveAksEdgeCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IArcService _arcService;
    private readonly ILogger<RemoveAksEdgeCommand> _logger;
    private readonly RemoveAksEdgeCommand _command;
    private readonly CommandContext _context;
    private readonly Parser _parser;
    private readonly string UserProvidedPath = "testPath";

    public RemoveAksEdgeCommandTests()
    {
        _arcService = Substitute.For<IArcService>();
        _logger = Substitute.For<ILogger<RemoveAksEdgeCommand>>();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(_arcService);
        _serviceProvider = serviceCollection.BuildServiceProvider();

        _command = new(_logger, _arcService);
        _context = new(_serviceProvider);
        _parser = new(_command.GetCommand());
    }

    private void MockArcServiceSuccess()
    {
        _arcService.RemoveAksEdgeAsync(Arg.Any<string>()).Returns(true);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRemoveAksEdgeSuccessfully()
    {
        // Arrange
        MockArcServiceSuccess();

        var args = new[]
        {
            "--path", UserProvidedPath
        };

        var parseResult = _parser.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.Equal("AKS Edge Essentials removed successfully.", response.Message);
        await _arcService.Received(1).RemoveAksEdgeAsync(Arg.Is(UserProvidedPath));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldHandleException()
    {
        // Arrange
        var args = new[]
        {
            "--path", UserProvidedPath
        };

        var expectedError = "Test error";
        _arcService.When(x => x.RemoveAksEdgeAsync(Arg.Is(UserProvidedPath))).Throws(new Exception(expectedError));

        var parseResult = _parser.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.Contains(expectedError, response.Message, StringComparison.OrdinalIgnoreCase);
    }
}