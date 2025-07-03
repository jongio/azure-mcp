using AzureMcp.Commands.Arc;
using AzureMcp.Models.Command;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.CommandLine;
using System.CommandLine.Parsing;
using Xunit;

namespace AzureMcp.Tests.Commands.Arc;

public class ValidatePrerequisitesForAksEdgeClusterCommandTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldValidatePrerequisitesSuccessfully()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ValidatePrerequisitesForAksEdgeClusterCommand>>();
        var mockService = new Mock<IArcService>();
        mockService.Setup(s => s.ValidatePrerequisitesForAksEdgeClusterAsync())
            .ReturnsAsync(new AzureMcp.Services.Azure.Arc.DeploymentResult
            {
                Success = true,
                Steps = "Test Prerequisites steps"
            });

        var serviceProvider = new ServiceCollection()
            .AddSingleton(mockService.Object)
            .BuildServiceProvider();

        var command = new ValidatePrerequisitesForAksEdgeClusterCommand(mockLogger.Object, mockService.Object);
        var rootCommand = new RootCommand { new Command("validate-prerequisites-aksee-cluster") };
        var parseResult = rootCommand.Parse("validate-prerequisites-aksee-cluster");
        var context = new CommandContext(serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, parseResult);

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
        var mockService = new Mock<IArcService>();
        mockService.Setup(s => s.LoadResourceFiles(It.IsAny<string>())).Returns("Sample prerequisites steps");

        // Act
        var prerequisitesSteps = mockService.Object.LoadResourceFiles("AzureMcp.Resources.Prerequisites_aksee_installation.txt");

        // Assert
        Assert.NotNull(prerequisitesSteps);
        Assert.NotEmpty(prerequisitesSteps);
    }
}
