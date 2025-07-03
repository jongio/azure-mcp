// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using System.Text.Json;
using AzureMcp.Tests.Client;
using AzureMcp.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace AzureMcp.Tests.Areas.Sql.LiveTests;

public class SqlCommandTests : CommandTestsBase, IClassFixture<LiveTestFixture>
{
    protected const string TenantNameReason = "Service principals cannot use TenantName for lookup";
    protected LiveTestSettings Settings { get; }
    protected StringBuilder FailureOutput { get; } = new();
    protected ITestOutputHelper Output { get; }
    protected IMcpClient Client { get; }

    public SqlCommandTests(LiveTestFixture fixture, ITestOutputHelper output)
        : base(fixture, output)
    {
        Client = fixture.Client;
        Settings = fixture.Settings;
        Output = output;
    }

    [Theory]
    [InlineData(AuthMethod.Credential)]
    [Trait("Category", "Live")]
    public async Task Should_ShowDatabase_WithAuth(AuthMethod authMethod)
    {
        // Note: This test would require a real SQL database to be deployed
        // For now, we'll test parameter validation
        var result = await CallToolAsync(
            "azmcp-sql-db-show",
            new()
            {
                { "subscription", Settings.Subscription },
                { "resource-group", Settings.ResourceGroup },
                { "server", "nonexistent-server" },
                { "database", "nonexistent-db" },
                { "auth-method", authMethod.ToString().ToLowerInvariant() }
            });

        // Should get a 404 or similar error for nonexistent resources
        var status = result.GetProperty("status").GetInt32();
        Assert.True(status >= 400, $"Expected error status code but got {status}");
    }

    [Theory]
    [InlineData("--invalid-param")]
    [InlineData("--subscription invalidSub")]
    [InlineData("--subscription sub --resource-group rg")]  // Missing server and database
    [Trait("Category", "Live")]
    public async Task Should_Return400_WithInvalidInput(string args)
    {
        var result = await CallToolAsync($"azmcp-sql-db-show {args}");

        Assert.Equal(400, result.GetProperty("status").GetInt32());
        Assert.Contains("required",
            result.GetProperty("message").GetString()!.ToLower());
    }

    [Fact]
    [Trait("Category", "Live")]
    public async Task Should_ValidateRequiredParameters()
    {
        var result = await CallToolAsync("azmcp-sql-db-show");

        Assert.Equal(400, result.GetProperty("status").GetInt32());
        var message = result.GetProperty("message").GetString()!.ToLower();
        Assert.True(
            message.Contains("subscription") ||
            message.Contains("resource-group") ||
            message.Contains("server") ||
            message.Contains("database"),
            "Error message should mention missing required parameters");
    }
}
