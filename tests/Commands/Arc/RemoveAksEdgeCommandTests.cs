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

namespace YourNamespace.Tests
{
    public class RemoveAksEdgeCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_ShouldRemoveAksEdgeSuccessfully()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<RemoveAksEdgeCommand>>();
            var mockService = new Mock<IArcService>();
            mockService
                    .Setup(service => service.RemoveAksEdgeAsync())
                    .ReturnsAsync(true);

            var serviceProvider = new ServiceCollection()
                .AddSingleton(mockService.Object)
                .BuildServiceProvider();

            var command = new RemoveAksEdgeCommand(mockLogger.Object, mockService.Object);
            var rootCommand = new RootCommand { new Command("remove-cluster-installation") };
            var parseResult = rootCommand.Parse("remove-cluster-installation");
            var context = new CommandContext(serviceProvider);

            // Act
            var response = await command.ExecuteAsync(context, parseResult);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(200, response.Status);
            Assert.Equal("AKS Edge Essentials removed successfully.", response.Message);
        }
    }
}