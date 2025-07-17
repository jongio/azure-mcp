using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using AzureMcp.Commands.Arc;
using AzureMcp.Models;
using AzureMcp.Models.Command;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using AzureMcp.Services.Azure.Arc;
using System.Diagnostics;
using AzureMcp.Options.Arc;
using Microsoft.Azure.Amqp.Framing;

namespace AzureMcp.Tests.Commands.Arc
{
    public class QuickDeployAksEdgeEssentialsCommandTests
    {
        private readonly IServiceProvider _serviceProvider;

        public QuickDeployAksEdgeEssentialsCommandTests()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton(Substitute.For<IArcService>());
            services.AddSingleton(new ArcConnectOptions
            {
                ClusterName = "TestCluster",
                ResourceGroupName = "TestResourceGroup",
                SubscriptionId = "TestSubscriptionId",
                TenantId = "TestTenantId",
                Location = "TestLocation"
            });
            _serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturnSuccessResponse()
        {
            // Arrange
            var arcServiceMock = Substitute.For<IArcService>();
            arcServiceMock.QuickDeployAksEdgeEssentialsAsync(
                Arg.Is<string>(x => x == "TestCluster"),
                Arg.Is<string>(x => x == "TestResourceGroup"),
                Arg.Is<string>(x => x == "TestSubscriptionId"),
                Arg.Is<string>(x => x == "TestTenantId"),
                Arg.Is<string>(x => x == "TestLocation"),
                Arg.Is<string>(x => x == "testUserProvidedPath"))
                .Returns(Task.FromResult(new DeploymentResult
                {
                    Success = true,
                    Steps = "Quick deployment of AKS Edge Essentials completed successfully."
                }));

            var command = new QuickDeployAksEdgeEssentialsCommand(
                _serviceProvider.GetRequiredService<ILogger<QuickDeployAksEdgeEssentialsCommand>>(),
                arcServiceMock);

            var parseResult = command.GetCommand().Parse("--cluster-name TestCluster --resource-group-name TestResourceGroup --subscription-id TestSubscriptionId --tenant-id TestTenantId --location TestLocation --user-provided-path testUserProvidedPath");
            var context = new CommandContext(_serviceProvider);

            // Act
            var response = await command.ExecuteAsync(context, parseResult);

            // Assert
            Assert.Equal(200, response.Status);
            Assert.Equal("Quick deployment of AKS Edge Essentials completed successfully.", response.Message);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldHandleErrorResponse()
        {
            // Arrange
            var loggerMock = Substitute.For<ILogger<QuickDeployAksEdgeEssentialsCommand>>();
            var arcServiceMock = Substitute.For<IArcService>();
            arcServiceMock.QuickDeployAksEdgeEssentialsAsync(
                "errorCluster",
                "errorResourceGroup",
                "errorSubscription",
                "errorTenant",
                "errorLocation",
                "errorPath")
                .Returns(Task.FromException<DeploymentResult>(new InvalidOperationException("Error during quick deployment.")));

            var command = new QuickDeployAksEdgeEssentialsCommand(
                loggerMock,
                arcServiceMock);

            var parseResult = command.GetCommand().Parse("");
            var context = new CommandContext(_serviceProvider);

            // Act
            var response = await command.ExecuteAsync(context, parseResult);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(500, response.Status);
            Assert.NotNull(response.Message);
        }
    }
}
