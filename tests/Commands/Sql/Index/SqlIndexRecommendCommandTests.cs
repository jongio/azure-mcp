// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using AzureMcp.Commands.Sql.Database.Index;
using AzureMcp.Models.Command;
using AzureMcp.Models.Sql;
using AzureMcp.Options;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace AzureMcp.Tests.Commands.Sql.Index;

public class SqlIndexRecommendCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISqlService _service;
    private readonly ILogger<SqlIndexRecommendCommand> _logger;
    private readonly SqlIndexRecommendCommand _command;

    public SqlIndexRecommendCommandTests()
    {
        _service = Substitute.For<ISqlService>();
        _logger = Substitute.For<ILogger<SqlIndexRecommendCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_service);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("recommend", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }    [Theory]
    [InlineData("--database mydb --server myserver --resource-group myrg --subscription sub1", true)]
    [InlineData("--database mydb --table myTable --server myserver --resource-group myrg --subscription sub1", true)]
    [InlineData("--table myTable", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {        if (shouldSucceed)
        {
            _service.GetIndexRecommendationsAsync(
                Arg.Any<string>(), // database
                Arg.Any<string>(), // server
                Arg.Any<string>(), // resourceGroup
                Arg.Any<string>(), // tableName
                Arg.Any<int?>(),   // minImpact
                Arg.Any<string>(), // subscription
                Arg.Any<string?>(), // tenant
                Arg.Any<RetryPolicyOptions>()) // retryPolicy
                .Returns(new List<SqlIndexRecommendation>());
        }

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse(args);

        var response = await _command.ExecuteAsync(context, parseResult);

        Assert.Equal(shouldSucceed ? 200 : 400, response.Status);
        if (!shouldSucceed)
        {
            Assert.Contains("database", response.Message.ToLower());
        }
    }    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        _service.GetIndexRecommendationsAsync(
            Arg.Any<string>(), // database
            Arg.Any<string>(), // server
            Arg.Any<string>(), // resourceGroup
            Arg.Any<string>(), // tableName
            Arg.Any<int?>(),   // minImpact
            Arg.Any<string>(), // subscription
            Arg.Any<string?>(), // tenant
            Arg.Any<RetryPolicyOptions>()) // retryPolicy
            .Returns(Task.FromException<List<SqlIndexRecommendation>>(
                new Exception("Test SQL error")));        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--database mydb --server-name myserver --resource-group myrg --subscription sub1");

        var response = await _command.ExecuteAsync(context, parseResult);

        Assert.Equal(500, response.Status);
        Assert.Contains("SQL error", response.Message);
    }
}
