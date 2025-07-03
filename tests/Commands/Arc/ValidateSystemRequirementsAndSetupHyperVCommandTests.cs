using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using AzureMcp.Commands.Arc;
using AzureMcp.Models;
using AzureMcp.Models.Command;
using AzureMcp.Services.Interfaces; // Added namespace for IArcService
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json; // Added namespace for JSON serialization
using System.Diagnostics; // Added namespace for ProcessStartInfo
using Xunit;
using AzureMcp.Services.Azure.Arc; // Added namespace for DeploymentResult

namespace AzureMcp.Tests.Commands.Arc
{
    public class ValidateSystemRequirementsAndSetupHyperVCommandTests
    {
        private readonly IServiceProvider _serviceProvider;

        public ValidateSystemRequirementsAndSetupHyperVCommandTests()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IArcService, MockArcService>();
            _serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturnSuccessResponse()
        {
            // Arrange
            var command = new ValidateSystemRequirementsAndSetupHyperVCommand(
                _serviceProvider.GetRequiredService<ILogger<ValidateSystemRequirementsAndSetupHyperVCommand>>(),
                _serviceProvider.GetRequiredService<IArcService>());

            var parseResult = command.GetCommand().Parse("");
            var context = new CommandContext(_serviceProvider);

            // Act
            var response = await command.ExecuteAsync(context, parseResult);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(200, response.Status);
            Assert.Contains("Note: The system may restart after Hyper-V installation.", response.Message);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldHandleErrorResponse()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ValidateSystemRequirementsAndSetupHyperVCommand>>();
            var arcServiceMock = new Mock<IArcService>();
            arcServiceMock.Setup(x => x.StartProcess(It.IsAny<string>(), It.IsAny<ProcessStartInfo>()))
                .Throws(new InvalidOperationException());

            var command = new ValidateSystemRequirementsAndSetupHyperVCommand(
                loggerMock.Object,
                arcServiceMock.Object);

            var parseResult = command.GetCommand().Parse("");
            var context = new CommandContext(_serviceProvider);

            // Act
            var response = await command.ExecuteAsync(context, parseResult);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(500, response.Status);
        }
    }

    public class MockArcService : IArcService
    {
        public Task<DeploymentResult> DeployAksEdgeEssentialClusterAsync()
        {
            return Task.FromResult(new DeploymentResult
            {
                Success = true,
                Steps = "Deployment steps"
            });
        }

        public Task<bool> DeployAksClusterToArcAsync(string resourceGroup, string clusterName, string location)
        {
            return Task.FromResult(true);
        }

        public string LoadResourceFiles(string resourceName)
        {
            return "Resource content";
        }

        public Process StartProcess(string scriptPath, ProcessStartInfo processInfo)
        {
            return new Process();
        }

        public Task<bool> RemoveAksEdgeAsync()
        {
            return Task.FromResult(true);
        }

        public Task<DeploymentResult> ValidatePrerequisitesForAksEdgeClusterAsync()
        {
            return Task.FromResult(new DeploymentResult
            {
                Success = true,
                Steps = "Prerequisite steps"
            });
        }

        public Task<bool> ConnectClusterToArcAsync(string clusterName, string resourceGroupName, string location, string subscriptionId, string tenantId)
        {
            return Task.FromResult(true);
        }

        public Task<DeploymentResult> ValidateSystemRequirementsAndSetupHyperVAsync()
        {
            return Task.FromResult(new DeploymentResult
            {
                Success = true,
                Steps = "Validation steps"
            });
        }
    }
}
