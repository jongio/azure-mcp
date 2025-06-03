// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using AzureMcp.Services.Azure.Monitor;
using AzureMcp.Services.Azure.ResourceGroup;
using AzureMcp.Services.Azure.Subscription;
using AzureMcp.Services.Azure.Tenant;
using AzureMcp.Services.Caching;
using AzureMcp.Services.Interfaces;
using AzureMcp.Tests.Client.Helpers;
using AzureMcp.Tests.Helpers;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace AzureMcp.Tests.Client;

public class MonitorCommandTests(LiveTestFixture fixture, ITestOutputHelper output) : CommandTestsBase(fixture, output), IClassFixture<LiveTestFixture>, IAsyncLifetime
{    private OtelLogHelper? _logHelper;
    private const string ServiceName = "MonitorCommandTests";
    private IMonitorService? _monitorService; 

    ValueTask IAsyncLifetime.InitializeAsync()
    {
        _monitorService = GetMonitorService();
        // If APPLICATIONINSIGHTS_CONNECTION_STRING is set, it will be used automatically
        _logHelper = new OtelLogHelper();
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private static IMonitorService GetMonitorService()
    {
        var memoryCache = new MemoryCache(Microsoft.Extensions.Options.Options.Create(new MemoryCacheOptions()));
        var cacheService = new CacheService(memoryCache);
        var tenantService = new TenantService(cacheService);
        var subscriptionService = new SubscriptionService(cacheService, tenantService);
        var resourceGroupService = new ResourceGroupService(cacheService, subscriptionService);
        return new MonitorService(subscriptionService, tenantService, resourceGroupService);
    }

    [Fact()]
    [Trait("Category", "Live")]
    public async Task Should_list_monitor_tables()
    {
        var result = await CallToolAsync(
            "azmcp-monitor-table-list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "workspace", Settings.ResourceBaseName },
                { "resource-group", Settings.ResourceGroupName }
            });

        var tablesArray = result.AssertProperty("tables");
        Assert.Equal(JsonValueKind.Array, tablesArray.ValueKind);
        var array = tablesArray.EnumerateArray();
        Assert.NotEmpty(array);
    }

    [Fact]
    [Trait("Category", "Live")]
    public async Task Should_list_monitor_workspaces()
    {
        var result = await CallToolAsync(
            "azmcp-monitor-workspace-list",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var workspacesArray = result.AssertProperty("workspaces");
        Assert.Equal(JsonValueKind.Array, workspacesArray.ValueKind);
        var array = workspacesArray.EnumerateArray();
        Assert.NotEmpty(array);
    }

    [Fact()]
    [Trait("Category", "Live")]    public async Task Should_query_monitor_logs()
    {
        // Send test logs via OpenTelemetry first to ensure we have data
        var queryStartTime = DateTime.UtcNow;
        
        // Try sending test logs
        Output.WriteLine("Sending test logs via OpenTelemetry...");
        var (infoStatus, errorStatus) = await _logHelper!.SendTestLogsAsync("test_query", TestContext.Current.CancellationToken);
        Output.WriteLine($"Test logs sent with status codes: Info={infoStatus}, Error={errorStatus}");

        // Query logs using Azure Monitor's Logs API
        var elapsed = (DateTime.UtcNow - queryStartTime).TotalSeconds;
        Output.WriteLine($"Querying logs after {elapsed:F1}s wait...");

        // Perform a broad query that should catch our test messages
        var result = await CallToolAsync("azmcp-monitor-log-query",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "workspace", Settings.ResourceBaseName },
                { "query", "traces | where TimeGenerated > ago(5m) | where Message startswith 'Test' | order by TimeGenerated desc | limit 10" },
                { "table-name", "traces" },
                { "resource-group", Settings.ResourceGroupName },
                { "hours", "1" }
            });

        Assert.NotNull(result);
        Assert.Equal(JsonValueKind.Array, result.Value.ValueKind);
        
        var logs = result.Value.EnumerateArray();
        Assert.NotEmpty(logs); // Should find the logs we just sent

        var queryDuration = (DateTime.UtcNow - queryStartTime).TotalSeconds;
        Output.WriteLine($"Found {logs.Count()} logs in {queryDuration:F1}s");
    }

    [Fact]
    [Trait("Category", "Live")]
    public async Task Should_list_monitor_table_types()
    {
        var result = await CallToolAsync(
            "azmcp-monitor-table-type-list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "workspace", Settings.ResourceBaseName },
                { "resource-group", Settings.ResourceGroupName }
            });

        var tableTypesArray = result.AssertProperty("tableTypes");
        Assert.Equal(JsonValueKind.Array, tableTypesArray.ValueKind);
        var array = tableTypesArray.EnumerateArray();
        Assert.NotEmpty(array);
    }

    [Fact]
    [Trait("Category", "Live")]
    public async Task Should_send_log_directly()
    {
        Assert.NotNull(_logHelper);

        var status = await _logHelper!.SendInfoLogAsync(TestContext.Current.CancellationToken);
        Output.WriteLine($"Info log sent with status code: {status}");

        if ((int)status < 200 || (int)status >= 300)
        {
            Output.WriteLine($"Failed to send log. Status code: {status} ({(int)status})");
            // The exception info will be bubbled up by the test runner
        }

        Assert.True((int)status >= 200 && (int)status < 300, $"Expected successful status code, got {status}");
    }
}
