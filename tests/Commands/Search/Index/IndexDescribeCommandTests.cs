// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Arguments;
using AzureMcp.Commands.Search.Index;
using AzureMcp.Models.Argument;
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

namespace AzureMcp.Tests.Commands.Search.Index;

public class IndexDescribeCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISearchService _searchService = Substitute.For<ISearchService>();
    private readonly ILogger<IndexDescribeCommand> _logger = Substitute.For<ILogger<IndexDescribeCommand>>();

    public IndexDescribeCommandTests()
    {
        var collection = new ServiceCollection();
        collection.AddSingleton(_searchService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsIndexDefinition_WhenIndexExists()
    {
        // Arrange
        var serviceName = "service123";
        var indexName = "index1";
        var expectedDefinition = CreateMockIndexDefinition();

        // When using ThrowsAsync or Returns with NSubstitute, we need to match the exact parameter signature
        _searchService
            .DescribeIndex(
                Arg.Is<string>(s => s == serviceName),
                Arg.Is<string>(i => i == indexName),
                Arg.Any<RetryPolicyArguments?>())
            .Returns(expectedDefinition);

        var command = new IndexDescribeCommand(_logger);
        var parser = new Parser(command.GetCommand());
        var args = parser.Parse($"--service-name {serviceName} --index-name {indexName}");
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(200, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<IndexDescribeResult>(json);

        Assert.NotNull(result);
        Assert.NotNull(result?.Index);
        Assert.Equal(expectedDefinition.Name, result?.Index?.Name);
        Assert.Equal(expectedDefinition.Fields.Count, result?.Index?.Fields?.Count ?? 0);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNull_WhenDefinitionIsNull()
    {
        // Arrange
        var serviceName = "service123";
        var indexName = "index1";

        _searchService
            .DescribeIndex(
                Arg.Is<string>(s => s == serviceName),
                Arg.Is<string>(i => i == indexName),
                Arg.Any<RetryPolicyArguments?>())
            .Returns((object)null!);

        var command = new IndexDescribeCommand(_logger);
        var parser = new Parser(command.GetCommand());
        var args = parser.Parse($"--service-name {serviceName} --index-name {indexName}");
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Null(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceException()
    {
        // Arrange
        var expectedError = "Test error";
        var serviceName = "service123";
        var indexName = "index1";

        _searchService
            .DescribeIndex(
                Arg.Is<string>(s => s == serviceName),
                Arg.Is<string>(i => i == indexName),
                Arg.Any<RetryPolicyArguments?>())
            .ThrowsAsync(new Exception(expectedError));

        var command = new IndexDescribeCommand(_logger);
        var parser = new Parser(command.GetCommand());
        var args = parser.Parse($"--service-name {serviceName} --index-name {indexName}");
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.Contains(expectedError, response.Message ?? string.Empty);
    }

    [Fact]
    public async Task ExecuteAsync_ValidatesRequiredArguments()
    {
        // Arrange
        var command = new IndexDescribeCommand(_logger);
        var parser = new Parser(command.GetCommand());
        var args = parser.Parse(""); // Missing required arguments
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(400, response.Status);
        Assert.NotNull(response.Arguments);
        Assert.Contains(response.Arguments ?? [], a => a.Name == "service-name" && a.Required);
        Assert.Contains(response.Arguments ?? [], a => a.Name == "index-name" && a.Required);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        // Arrange & Act
        var command = new IndexDescribeCommand(_logger);
        var cmd = command.GetCommand();

        // Assert
        Assert.Equal("describe", cmd.Name);
        Assert.NotNull(cmd.Description);
        Assert.NotEmpty(cmd.Description);

        // Verify options
        var serviceOption = cmd.Options.FirstOrDefault(o => o.Name == "service-name");
        var indexOption = cmd.Options.FirstOrDefault(o => o.Name == "index-name");

        Assert.NotNull(serviceOption);
        Assert.NotNull(indexOption);
    }

    private static MockIndexDefinition CreateMockIndexDefinition()
    {
        return new MockIndexDefinition
        {
            Name = "sampleIndex",
            Fields = [
                new MockField { Name = "id", Type = "Edm.String", Key = true },
                new MockField { Name = "title", Type = "Edm.String", Searchable = true },
                new MockField { Name = "content", Type = "Edm.String", Searchable = true, Filterable = true }
            ]
        };
    }

    private class IndexDescribeResult
    {
        [JsonPropertyName("index")]
        public MockIndexDefinition? Index { get; set; }
    }

    private class MockIndexDefinition
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("fields")]
        public List<MockField> Fields { get; set; } = [];
    }

    private class MockField
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("key")]
        public bool Key { get; set; }

        [JsonPropertyName("searchable")]
        public bool Searchable { get; set; }

        [JsonPropertyName("filterable")]
        public bool Filterable { get; set; }
    }
}