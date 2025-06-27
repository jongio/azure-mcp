// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using AzureMcp.Commands.Arc;
using AzureMcp.Models.Arc;
using AzureMcp.Models.Command;
using AzureMcp.Options;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace AzureMcp.Tests.Commands.Arc;

public class ClusterListCommandTest
{
    private readonly IArcService _arcService;
    private readonly ILogger<ClusterListCommand> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ClusterListCommand _command;

    public ClusterListCommandTest()
    {
        _arcService = Substitute.For<IArcService>();
        _logger = Substitute.For<ILogger<ClusterListCommand>>();

        var services = new ServiceCollection();
        services.AddSingleton(_arcService);
        services.AddSingleton(_logger);
        _serviceProvider = services.BuildServiceProvider();

        _command = new ClusterListCommand(_logger);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsClusterList_Success()
    {
        // Arrange
        var clusters = new List<Cluster>
        {
            new()
            {
                Name = "test-cluster-1",
                SubscriptionId = "test-sub",
                ResourceGroupName = "test-rg",
                Location = "eastus",
                Status = "Connected"
            },
            new()
            {
                Name = "test-cluster-2",
                SubscriptionId = "test-sub",
                ResourceGroupName = "test-rg",
                Location = "westus",
                Status = "Disconnected"
            }
        };

        _arcService.ListClustersAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(clusters);

        var parseResult = CreateParseResult(
            "--subscription", "test-sub",
            "--resource-group", "test-rg");

        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.Contains("Successfully retrieved Arc clusters", response.Message);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_EmptyClusterList_Success()
    {
        // Arrange
        var clusters = new List<Cluster>();

        _arcService.ListClustersAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(clusters);

        var parseResult = CreateParseResult(
            "--subscription", "test-sub");

        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.Contains("Successfully retrieved Arc clusters", response.Message);
    }

    /*[Fact]
    public async Task ExecuteAsync_ServiceException_ReturnsServerError()
    {
        // Arrange
        _arcService.ListClustersAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Throws(new Exception("Service unavailable"));

        var parseResult = CreateParseResult(
            "--subscription", "test-sub");

        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(500, response.Status);
        Assert.Contains("Service unavailable", response.Message);
    }*/

    private ParseResult CreateParseResult(params string[] args)
    {
        var command = _command.GetCommand();
        var parser = new Parser(command);
        return parser.Parse(args);
    }
}
