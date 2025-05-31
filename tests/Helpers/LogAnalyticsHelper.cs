// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Core;
using AzureMcp.Services.Azure.Authentication;
using AzureMcp.Services.Interfaces;

namespace AzureMcp.Tests.Helpers;

public class LogRecord
{
    [JsonPropertyName("TimeGenerated")]
    public DateTimeOffset TimeGenerated { get; set; } = DateTimeOffset.UtcNow;

    [JsonPropertyName("Level")]
    public string Level { get; set; } = "";

    [JsonPropertyName("Message")]
    public string Message { get; set; } = "";

    [JsonPropertyName("Application")]
    public string Application { get; set; } = "";

    [JsonExtensionData]
    public Dictionary<string, JsonElement> AdditionalData { get; set; } = new();
}

public class LogAnalyticsHelper(string workspaceName, string subscription, IMonitorService monitorService, string? tenantId = null, string logType = "TestLogs_CL")
{
    private readonly string _workspaceName = workspaceName;
    private readonly string _subscription = subscription;
    private readonly string _logType = logType;
    private readonly string? _tenantId = tenantId;
    private readonly TokenCredential _credential = new CustomChainedCredential(tenantId);
    private readonly IMonitorService _monitorService = monitorService ?? throw new ArgumentNullException(nameof(monitorService));
    private string? _workspaceId;

    private async Task<string> GetWorkspaceIdAsync()
    {
        if (!string.IsNullOrEmpty(_workspaceId))
        {
            return _workspaceId;
        }

        // Get workspace info using the monitor service
        var workspaces = await _monitorService.ListWorkspaces(_subscription, _tenantId);
        var workspace = workspaces.FirstOrDefault(w => w.Name.Equals(_workspaceName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Could not find workspace {_workspaceName}");

        _workspaceId = workspace.CustomerId;
        return _workspaceId;
    }

    public async Task<HttpStatusCode> SendInfoLogAsync()
    {
        var workspaceId = await GetWorkspaceIdAsync();

        // Create info log
        var infoLog = new LogRecord
        {
            TimeGenerated = DateTimeOffset.UtcNow,
            Level = "Information",
            Message = $"Test info message: {DateTimeOffset.UtcNow:O}",
            Application = "MonitorCommandTests"
        };

        // Send only the info log
        return await SendLogAsync(workspaceId, [infoLog]);
    }

    public async Task<(HttpStatusCode infoStatus, HttpStatusCode errorStatus)> SendTestLogsAsync(string testId)
    {
        var workspaceId = await GetWorkspaceIdAsync();

        // Create test logs
        var infoLog = new LogRecord
        {
            TimeGenerated = DateTimeOffset.UtcNow,
            Level = "Information",
            Message = $"Test info message: {testId}",
            Application = "MonitorCommandTests"
        };

        var errorLog = new LogRecord
        {
            TimeGenerated = DateTimeOffset.UtcNow,
            Level = "Error",
            Message = $"Test error message {Guid.NewGuid()}",
            Application = "MonitorCommandTests"
        };

        // Send logs
        var infoStatus = await SendLogAsync(workspaceId, [infoLog]);
        var errorStatus = await SendLogAsync(workspaceId, [errorLog]);

        return (infoStatus, errorStatus);
    }

    private async Task<HttpStatusCode> SendLogAsync(string customerId, LogRecord[] logs)
    {
        var jsonContent = JsonSerializer.Serialize(logs);
        var contentType = "application/json";
        var dateString = DateTime.UtcNow.ToString("r");

        // Use the Data Collection API endpoint
        var uri = $"https://{customerId}.ods.opinsights.azure.com/api/logs?api-version=2016-04-01";

        using var request = new HttpRequestMessage(HttpMethod.Post, uri);
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("Log-Type", _logType);
        request.Headers.Add("x-ms-date", dateString);

        // Get a token for the Data Collection API
        var token = await _credential.GetTokenAsync(
            new TokenRequestContext(
                new[] { "https://monitor.azure.com/.default" },
                tenantId: _tenantId), 
            default);
            
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

        request.Content = new StringContent(jsonContent, Encoding.UTF8);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        using var client = new HttpClient();
        var response = await client.SendAsync(request);
        return response.StatusCode;
    }
}
