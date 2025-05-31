// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
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
    private string? _primaryKey;

    private async Task<(string workspaceId, string primaryKey)> GetWorkspaceInfoAsync()
    {
        if (!string.IsNullOrEmpty(_workspaceId) && !string.IsNullOrEmpty(_primaryKey))
        {
            return (_workspaceId, _primaryKey);
        }

        // Get workspace info using the monitor service
        var workspaces = await _monitorService.ListWorkspaces(_subscription, _tenantId);
        var workspace = workspaces.FirstOrDefault(w => w.Name.Equals(_workspaceName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Could not find workspace {_workspaceName}");

        // Get the workspace keys using monitor service
        _workspaceId = workspace.CustomerId;
        var keys = await _monitorService.GetWorkspaceKeys(_subscription, _workspaceName, _tenantId);
        _primaryKey = keys.PrimarySharedKey ?? throw new InvalidOperationException("Workspace primary key not found");

        return (_workspaceId, _primaryKey);
    }

    private string BuildSignature(string customerId, string sharedKey, string date, string contentLength, string method, string contentType, string resource)
    {
        var stringToHash = string.Join("\n",
            method,
            contentLength,
            contentType,
            $"x-ms-date:{date}",
            resource);

        byte[] bytes = Encoding.UTF8.GetBytes(stringToHash);
        using var hmacsha256 = new HMACSHA256(Convert.FromBase64String(sharedKey));
        return $"SharedKey {customerId}:{Convert.ToBase64String(hmacsha256.ComputeHash(bytes))}";
    }
    public async Task<(HttpStatusCode infoStatus, HttpStatusCode errorStatus)> SendTestLogsAsync(string testId)
    {
        var (workspaceId, primaryKey) = await GetWorkspaceInfoAsync();

        // Create test logs
        var infoLog = new LogRecord
        {
            TimeGenerated = DateTimeOffset.UtcNow,
            Level = "Information",
            Message = $"Test info message. TestId: {testId}",
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
        var infoStatus = await SendLogAsync(workspaceId, primaryKey, [infoLog]);
        var errorStatus = await SendLogAsync(workspaceId, primaryKey, [errorLog]);

        return (infoStatus, errorStatus);
    }

    public async Task<HttpStatusCode> SendInfoLogAsync()
    {
        var (workspaceId, primaryKey) = await GetWorkspaceInfoAsync();

        // Create info log
        var infoLog = new LogRecord
        {
            TimeGenerated = DateTimeOffset.UtcNow,
            Level = "Information",
            Message = $"Test info message: {DateTimeOffset.UtcNow:O}",
            Application = "MonitorCommandTests"
        };

        // Send only the info log
        return await SendLogAsync(workspaceId, primaryKey, [infoLog]);
    }

    private async Task<HttpStatusCode> SendLogAsync(string customerId, string sharedKey, LogRecord[] logs)
    {
        var jsonContent = JsonSerializer.Serialize(logs);
        var dateString = DateTime.UtcNow.ToString("r");
        var contentType = "application/json";
        var method = "POST";
        var resource = $"/api/logs";
        var contentLength = Encoding.UTF8.GetBytes(jsonContent).Length.ToString();

        var uri = $"https://{customerId}.ods.opinsights.azure.com/api/logs?api-version=2016-04-01";

        using var request = new HttpRequestMessage(HttpMethod.Post, uri);
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("Log-Type", _logType);
        request.Headers.Add("x-ms-date", dateString);

        // Build the signature
        var signature = BuildSignature(customerId, sharedKey, dateString, contentLength, method, contentType, resource);
        request.Headers.Add("Authorization", signature);

        request.Content = new StringContent(jsonContent, Encoding.UTF8);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        using var client = new HttpClient();
        var response = await client.SendAsync(request);
        return response.StatusCode;
    }
}
