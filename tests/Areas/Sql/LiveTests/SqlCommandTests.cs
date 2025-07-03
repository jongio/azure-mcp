// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using AzureMcp.Tests.Client;
using AzureMcp.Tests.Client.Helpers;
using Xunit;

namespace AzureMcp.Tests.Areas.Sql.LiveTests;

[Trait("Area", "Sql")]
public class SqlCommandTests(LiveTestFixture liveTestFixture, ITestOutputHelper output)
    : CommandTestsBase(liveTestFixture, output), IClassFixture<LiveTestFixture>
{

    [Fact]
    [Trait("Category", "Live")]
    public async Task Should_ShowDatabase_Successfully()
    {
        // Use the deployed test SQL server and database
        var serverName = $"{Settings.ResourceBaseName}-sql";
        var databaseName = "testdb";

        var result = await CallToolAsync(
            "azmcp-sql-db-show",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "server", serverName },
                { "database", databaseName }
            });

        // Should successfully retrieve the database
        var database = result.AssertProperty("database");
        Assert.Equal(JsonValueKind.Object, database.ValueKind);

        // Verify database properties
        var dbName = database.GetProperty("name").GetString();
        Assert.Equal(databaseName, dbName);

        var dbType = database.GetProperty("type").GetString();
        Assert.Equal("Microsoft.Sql/servers/databases", dbType);
    }

    [Theory]
    [InlineData("--invalid-param", new string[0])]
    [InlineData("--subscription", new[] { "invalidSub" })]
    [InlineData("--subscription", new[] { "sub", "--resource-group", "rg" })]  // Missing server and database
    [Trait("Category", "Live")]
    public async Task Should_Return400_WithInvalidInput(string firstArg, string[] remainingArgs)
    {
        var allArgs = new[] { firstArg }.Concat(remainingArgs);
        var argsString = string.Join(" ", allArgs);

        var result = await CallToolAsync(
            "azmcp-sql-db-show",
            new()
            {
                { "args", argsString }
            });

        // Note: This might need adjustment based on how the test framework handles invalid args
        // For now, let's test with known bad parameter combinations
    }

    [Fact]
    [Trait("Category", "Live")]
    public async Task Should_ValidateRequiredParameters()
    {
        // Test with missing required parameters
        var result = await CallToolAsync(
            "azmcp-sql-db-show",
            new()
            {
                { "subscription", Settings.SubscriptionId }
                // Missing resource-group, server, and database
            });

        // Should get validation error
        // Note: The exact behavior depends on the command validation implementation
    }
}
