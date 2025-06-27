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

public class ClusterConfigureCommandTest
{
    private readonly IArcService _arcService;
    private readonly ILogger<ClusterConfigureCommand> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ClusterConfigureCommand _command;

    public ClusterConfigureCommandTest()
    {
        _arcService = Substitute.For<IArcService>();
        _logger = Substitute.For<ILogger<ClusterConfigureCommand>>();

        var services = new ServiceCollection();
        services.AddSingleton(_arcService);
        services.AddSingleton(_logger);
        _serviceProvider = services.BuildServiceProvider();

        _command = new ClusterConfigureCommand(_logger);
    }

    [Fact]
    public async Task ExecuteAsync_SuccessfulConfiguration_ReturnsSuccess()
    {
        // Arrange
        var configResult = new ConfigurationResult
        {
            Success = true,
            Message = "Configuration applied successfully",
            AppliedConfigurations = new[] { "test-config" }
        };

        _arcService.ConfigureClusterAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(configResult);

        var parseResult = CreateParseResult(
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--cluster-name", "test-cluster",
            "--configuration-path", "/path/to/config.yaml");

        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.Contains("Successfully configured", response.Message);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_ConfigurationFails_ReturnsError()
    {
        // Arrange
        var configResult = new ConfigurationResult
        {
            Success = false,
            Message = "Configuration failed",
            Errors = new[] { "Connection timeout" }
        };

        _arcService.ConfigureClusterAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(configResult);

        var parseResult = CreateParseResult(
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--cluster-name", "test-cluster");

        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Contains("Configuration failed", response.Message);
    }

    /* [Fact]
    public async Task ExecuteAsync_ServiceException_ReturnsServerError()
     {
         // Arrange
         _arcService.ConfigureClusterAsync(
             Arg.Any<string>(),
             Arg.Any<string>(),
             Arg.Any<string>(),
             Arg.Any<string?>(),
             Arg.Any<string?>(),
             Arg.Any<RetryPolicyOptions?>())
             .Throws(new Exception("Service unavailable"));

         var parseResult = CreateParseResult(
             "--subscription", "test-sub",
             "--resource-group", "test-rg",
             "--cluster-name", "test-cluster");

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
