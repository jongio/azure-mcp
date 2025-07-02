using AzureMcp.Commands.Arc;
using AzureMcp.Models.Command;
using AzureMcp.Options.Arc;
using AzureMcp.Services.Azure.Arc;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Parsing;
using Xunit;

namespace AzureMcp.Tests.Commands.Arc;

public class TestWithRealData
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnRealDeploymentSteps()
    {
        // Arrange
        var logger = new LoggerFactory().CreateLogger<DeployAksEdgeEssentialClusterCommand>();
        var arcService = new ArcService();

        var serviceProvider = new ServiceCollection()
            .AddSingleton<IArcService>(_ => arcService)
            .BuildServiceProvider();

        var command = new DeployAksEdgeEssentialClusterCommand(logger, arcService);
        var rootCommand = new RootCommand { new Command("install-aksee-cluster") };
        var parseResult = rootCommand.Parse("install-aksee-cluster");
        var context = new CommandContext(serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);
        Assert.NotEmpty(response.Results?.ToString() ?? string.Empty);
    }
}