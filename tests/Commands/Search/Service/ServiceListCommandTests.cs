// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Arguments;
using AzureMcp.Commands.Search.Service;
using AzureMcp.Models.Command;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace AzureMcp.Tests.Commands.Search.Service;

public class ServiceListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISearchService _searchService;
    private readonly ILogger<ServiceListCommand> _logger;

    public ServiceListCommandTests()
    {
        _searchService = Substitute.For<ISearchService>();
        _logger = Substitute.For<ILogger<ServiceListCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_searchService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsServices_WhenServicesExist()
    {
        // Arrange
        var expectedServices = new List<string> { "service1", "service2" };
        _searchService.ListServices(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyArguments>())
            .Returns(expectedServices);

        var command = new ServiceListCommand(_logger);
        var parser = new Parser(command.GetCommand());
        var args = parser.Parse("--subscription sub123");
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<ServiceListResult>(json);

        Assert.NotNull(result);
        Assert.Equal(expectedServices, result.Services);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNull_WhenNoServices()
    {
        // Arrange
        _searchService.ListServices(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyArguments>())
            .Returns(new List<string>());

        var command = new ServiceListCommand(_logger);
        var parser = new Parser(command.GetCommand());
        var args = parser.Parse("--subscription sub123");
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Null(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";
        var subscriptionId = "sub123";

        _searchService.ListServices(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyArguments>())
            .ThrowsAsync(new Exception(expectedError));

        var command = new ServiceListCommand(_logger);
        var parser = new Parser(command.GetCommand());
        var args = parser.Parse($"--subscription {subscriptionId}");
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.Equal(expectedError, response.Message);
    }

    private class ServiceListResult
    {
        [JsonPropertyName("services")]
        public List<string> Services { get; set; } = [];
    }
}