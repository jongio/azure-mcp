using AzureMcp.Commands.Arc;
using AzureMcp.Models.Command;
using AzureMcp.Services.Azure.Arc; // Added namespace for DeploymentResult
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.CommandLine;
using System.CommandLine.Parsing;
using Xunit;

namespace AzureMcp.Tests.Commands.Arc;

public class ValidatePrerequisitesForAksEdgeClusterCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IArcService _arcService;
    private readonly ILogger<ValidatePrerequisitesForAksEdgeClusterCommand> _logger;
    private readonly ValidatePrerequisitesForAksEdgeClusterCommand _command;
    private readonly CommandContext _context;
    private readonly Parser _parser;

    public ValidatePrerequisitesForAksEdgeClusterCommandTests()
    {
        _arcService = Substitute.For<IArcService>();
        _logger = Substitute.For<ILogger<ValidatePrerequisitesForAksEdgeClusterCommand>>();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(_arcService);
        _serviceProvider = serviceCollection.BuildServiceProvider();

        _command = new(_logger, _arcService);
        _context = new(_serviceProvider);
        _parser = new(_command.GetCommand());
    }

    private void MockArcServiceSuccess()
    {
        _arcService.ValidatePrerequisitesForAksEdgeClusterAsync()
            .Returns(Task.FromResult(new DeploymentResult
            {
                Success = true,
                Steps = "Test Prerequisites steps"
            }));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldValidatePrerequisitesSuccessfully()
    {
        // Arrange
        MockArcServiceSuccess();

        var args = new[] { "validate-prerequisites-aksee-cluster" };
        var parseResult = _parser.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.Equal("Prerequisites validated successfully", response.Message);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public void LoadPrerequisitesSteps_ShouldReturnNonEmptyString()
    {
        // Arrange
        _arcService.LoadResourceFiles(Arg.Any<string>()).Returns("Sample prerequisites steps");

        // Act
        var prerequisitesSteps = _arcService.LoadResourceFiles("AzureMcp.Resources.Prerequisites_aksee_installation.txt");

        // Assert
        Assert.NotNull(prerequisitesSteps);
        Assert.NotEmpty(prerequisitesSteps);
    }
}
