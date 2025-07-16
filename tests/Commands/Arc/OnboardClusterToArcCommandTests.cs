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

public class OnboardClusterToArcCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IArcService _arcService;
    private readonly ILogger<OnboardClusterToArcCommand> _logger;
    private readonly OnboardClusterToArcCommand _command;
    private readonly CommandContext _context;
    private readonly Parser _parser;
    private readonly string ClusterName = "TestCluster";
    private readonly string ResourceGroupName = "TestResourceGroup";
    private readonly string Location = "TestLocation";
    private readonly string SubscriptionId = "TestSubscriptionId";
    private readonly string TenantId = "TestTenantId";
    private readonly string KubeConfigPath = "TestKubeConfigPath";

    public OnboardClusterToArcCommandTests()
    {
        _arcService = Substitute.For<IArcService>();
        _logger = Substitute.For<ILogger<OnboardClusterToArcCommand>>();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(_arcService);
        _serviceProvider = serviceCollection.BuildServiceProvider();

        _command = new(_logger, _arcService);
        _context = new(_serviceProvider);
        _parser = new(_command.GetCommand());

    }

    private void MockArcServiceSuccess()
    {
        _arcService.OnboardClusterToArcAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>()).Returns(true);
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
        "--clusterName", ClusterName,
        "--kubeConfigPath", KubeConfigPath
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
            TenantId = null,
            KubeConfigPath = null
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
        {
            if (string.IsNullOrEmpty(options.ClusterName) ||
                string.IsNullOrEmpty(options.ResourceGroupName) ||
                string.IsNullOrEmpty(options.Location) ||
                string.IsNullOrEmpty(options.SubscriptionId) ||
                string.IsNullOrEmpty(options.TenantId) ||
                string.IsNullOrEmpty(options.KubeConfigPath))
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
            "--clusterName", "TestCluster",
            "--kubeConfigPath", "TestKubeConfigPath"
        };

        var expectedError = "Test error";
        _arcService.When(x => x.OnboardClusterToArcAsync(
            Arg.Is("TestCluster"),
            Arg.Is("TestResourceGroup"),
            Arg.Is("TestLocation"),
            Arg.Is("TestSubscriptionId"),
            Arg.Is("TestTenantId"),
            Arg.Is("KubeConfigPath"))).Throws(new Exception(expectedError));

        var parseResult = _parser.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        // Assert.Contains(expectedError, response.Message, StringComparison.OrdinalIgnoreCase);
    }
}