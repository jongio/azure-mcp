using AzureMcp.Commands.Arc;
using AzureMcp.Models.Command;
using AzureMcp.Options.Arc;
using AzureMcp.Services.Azure.Arc;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.CommandLine;
using System.CommandLine.Parsing;
using Xunit;

namespace AzureMcp.Tests.Commands.Arc;

public class DisconnectFromAzureArcCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IArcService _arcService;
    private readonly ILogger<DisconnectFromAzureArcCommand> _logger;
    private readonly DisconnectFromAzureArcCommand _command;
    private readonly CommandContext _context;
    private readonly Parser _parser;
    private readonly string ClusterName = "TestCluster";
    private readonly string ResourceGroupName = "TestResourceGroup";
    private readonly string UserProvidedPath = "TestUserProvidedPath";

    public DisconnectFromAzureArcCommandTests()
    {
        _arcService = Substitute.For<IArcService>();
        _logger = Substitute.For<ILogger<DisconnectFromAzureArcCommand>>();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(_arcService);
        _serviceProvider = serviceCollection.BuildServiceProvider();

        _command = new(_logger, _arcService);
        _context = new(_serviceProvider);
        _parser = new(_command.GetCommand());
    }

    private void MockArcServiceSuccess()
    {
        _arcService.DisconnectFromAzureArcAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>()).Returns(Task.FromResult(new DeploymentResult { Success = true }));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldDisconnectClusterFromAzureArc()
    {
        // Arrange
        MockArcServiceSuccess();

        var args = new[]
        {
            "--resource-group-name", ResourceGroupName,
            "--cluster-name", ClusterName,
            "--user-provided-path", UserProvidedPath
        };

        var parseResult = _parser.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldHandleException()
    {
        // Arrange
        var args = new[]
        {
            "--resource-group-name", ResourceGroupName,
            "--cluster-name", ClusterName,
            "--user-provided-path", UserProvidedPath
        };

        var expectedError = "Test error";
        _arcService.When(x => x.DisconnectFromAzureArcAsync(
            Arg.Is(ResourceGroupName),
            Arg.Is(ClusterName),
            Arg.Is(UserProvidedPath))).Throws(new Exception(expectedError));

        var parseResult = _parser.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.Contains(expectedError, response.Message, StringComparison.OrdinalIgnoreCase);
    }
}
