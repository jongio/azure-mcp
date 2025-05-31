// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Core;
using Azure.Monitor.Ingestion;

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
    private LogsIngestionClient? _logsIngestionClient;

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

    private LogsIngestionClient GetLogsIngestionClient(string customerId)
    {
        if (_logsIngestionClient == null)
        {
            if (string.IsNullOrEmpty(customerId))
            {
                throw new ArgumentNullException(nameof(customerId), "Customer ID cannot be null or empty");
            }

            try
            {
                var endpoint = new Uri($"https://{customerId}.ods.opinsights.azure.com");
                var options = new LogsIngestionClientOptions
                {
                    Retry =
                    {
                        MaxRetries = 3,
                        Delay = TimeSpan.FromSeconds(2),
                        MaxDelay = TimeSpan.FromSeconds(10)
                    }
                };

                _logsIngestionClient = new LogsIngestionClient(endpoint, _credential, options);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create LogsIngestionClient: {ex.Message}", ex);
            }
        }
        return _logsIngestionClient;
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
        var client = GetLogsIngestionClient(customerId);
        var jsonContent = JsonSerializer.Serialize(logs);

        try
        {
            using var content = RequestContent.Create(logs);
            var response = await client.UploadAsync(
                customerId,      // DCR rule ID
                _logType,       // Stream name (table name)
                content,        // Log data as request content
                null,           // No content type (defaults to application/json)
                default         // No request context
            );
            
            return (HttpStatusCode)response.Status;
        }
        catch (Exception ex)
        {
            // Log the error and return a 500 status code
            Console.Error.WriteLine($"Error sending logs: {ex.Message}");
            return HttpStatusCode.InternalServerError;
        }
    }
}
