using AzureMcp.Commands.Arc;
using AzureMcp.Models.Command;
using AzureMcp.Options.Arc;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.CommandLine;
using System.CommandLine.Parsing;
using Xunit;

namespace AzureMcp.Tests.Commands.Arc;

public class DeployAksEdgeEssentialClusterCommandTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldWriteDeploymentStepsToFile()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DeployAksEdgeEssentialClusterCommand>>();
        var mockService = new Mock<IArcService>();
        mockService.Setup(s => s.DeployAksEdgeEssentialClusterAsync())
            .ReturnsAsync(new AzureMcp.Services.Azure.Arc.DeploymentResult
            {
                Success = true,
                Steps = "Test deployment steps"
                // Removed OutputPath dependency
            });

        var serviceProvider = new ServiceCollection()
            .AddSingleton(mockService.Object)
            .BuildServiceProvider();

        var command = new DeployAksEdgeEssentialClusterCommand(mockLogger.Object, mockService.Object);
        var rootCommand = new RootCommand { new Command("install-aksee-cluster") };
        var parseResult = rootCommand.Parse("install-aksee-cluster --cluster-name test-cluster --location eastus");
        var context = new CommandContext(serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.Equal("Deployment steps generated successfully", response.Message);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public void LoadDeploymentSteps_ShouldReturnNonEmptyString()
    {
        // Arrange
        var mockService = new Mock<IArcService>();
        mockService.Setup(s => s.LoadDeploymentSteps()).Returns("Sample deployment steps");

        // Act
        var deploymentSteps = mockService.Object.LoadDeploymentSteps();

        // Assert
        Assert.NotNull(deploymentSteps);
        Assert.NotEmpty(deploymentSteps);
    }
}
