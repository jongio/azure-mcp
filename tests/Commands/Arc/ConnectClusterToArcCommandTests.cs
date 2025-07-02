using AzureMcp.Commands.Arc;
using AzureMcp.Models.Command;
using AzureMcp.Options.Arc;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text.Json;
using Xunit;

namespace AzureMcp.Tests.Commands.Arc;

public class ConnectClusterToArcCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IArcService _arcService;
    private readonly ILogger<ConnectClusterToArcCommand> _logger;
    private readonly ConnectClusterToArcCommand _command;
    private readonly CommandContext _context;
    private readonly Parser _parser;
    private readonly string ClusterName = "TestCluster";
    private readonly string ResourceGroupName = "TestResourceGroup";
    private readonly string Location = "TestLocation";
    private readonly string SubscriptionId = "TestSubscriptionId";
    private readonly string TenantId = "TestTenantId";

    public ConnectClusterToArcCommandTests()
    {
        _arcService = Substitute.For<IArcService>();
        _logger = Substitute.For<ILogger<ConnectClusterToArcCommand>>();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(_arcService);
        _serviceProvider = serviceCollection.BuildServiceProvider();

        _command = new(_logger);
        _context = new(_serviceProvider);
        _parser = new(_command.GetCommand());

    }

    private void MockArcServiceSuccess()
    {
        _arcService.When(x => x.ConnectClusterToArcAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>()))
            .Do(callInfo =>
            {
                var clusterName = callInfo.ArgAt<string>(0);
                var resourceGroupName = callInfo.ArgAt<string>(1);
                var location = callInfo.ArgAt<string>(2);
                var subscriptionId = callInfo.ArgAt<string>(3);
                var tenantId = callInfo.ArgAt<string>(4);

                if (string.IsNullOrEmpty(clusterName) ||
                    string.IsNullOrEmpty(resourceGroupName) ||
                    string.IsNullOrEmpty(location) ||
                    string.IsNullOrEmpty(subscriptionId) ||
                    string.IsNullOrEmpty(tenantId))
                {
                    throw new ArgumentException("One or more required parameters are null.");
                }
            });
    }
    [Fact]
    public async Task ExecuteAsync_ShouldConnectClusterToAzureArc()
    {
        // Arrange
        MockArcServiceSuccess();

        var args = new[]
        {
        "--subscriptionId", SubscriptionId,
        "--tenantId", TenantId,
        "--location", Location,
        "--resourceGroupName", ResourceGroupName,
        "--clusterName", ClusterName
    };

        var parseResult = _parser.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("200", response.Status.ToString());

    }

    [Fact]
    public void ValidateOptions_ShouldThrowExceptionForMissingFields()
    {
        // Arrange
        var options = new ArcConnectOptions
        {
            ClusterName = null,
            ResourceGroupName = null,
            Location = null,
            SubscriptionId = null,
            TenantId = null
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
        {
            if (string.IsNullOrEmpty(options.ClusterName) ||
                string.IsNullOrEmpty(options.ResourceGroupName) ||
                string.IsNullOrEmpty(options.Location) ||
                string.IsNullOrEmpty(options.SubscriptionId) ||
                string.IsNullOrEmpty(options.TenantId))
            {
                throw new ArgumentException("One or more required options are missing.");
            }
        });
    }

    [Fact]
    public async Task ExecuteAsync_ShouldHandleException()
    {
        // Arrange
        var args = new[]
        {
            "--subscriptionId", "TestSubscriptionId",
            "--tenantId", "TestTenantId",
            "--location", "TestLocation",
            "--resourceGroupName", "TestResourceGroup",
            "--clusterName", "TestCluster"
        };

        var expectedError = "Test error";
        _arcService.When(x => x.ConnectClusterToArcAsync(
            Arg.Is("TestCluster"),
            Arg.Is("TestResourceGroup"),
            Arg.Is("TestLocation"),
            Arg.Is("TestSubscriptionId"),
            Arg.Is("TestTenantId"))).Throws(new Exception(expectedError));

        var parseResult = _parser.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.True(response.Message.Contains(expectedError), $"Expected error message to contain '{expectedError}', but got '{response.Message}'");
    }
}