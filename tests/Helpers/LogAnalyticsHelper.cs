// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;

namespace AzureMcp.Tests.Helpers;

/// <summary>
/// Helper class for sending logs using OpenTelemetry for testing purposes.
/// </summary>
public class LogAnalyticsHelper : IDisposable
{
    private readonly string _logType;
    private readonly ILogger _logger;
    private readonly ActivitySource _activitySource;
    private readonly ILoggerFactory _loggerFactory;

    public LogAnalyticsHelper(
        string endpoint = "http://localhost:4317",
        string logType = "TestLogs_CL",
        ILogger? logger = null)
    {
        _logType = logType;
        _logger = logger ?? NullLogger.Instance;
        _activitySource = new ActivitySource("AzureMcp.Tests");

        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService("AzureMcp.Tests")
            .AddAttributes(new Dictionary<string, object>
            {
                ["log_type"] = logType
            });

        _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddOpenTelemetry(options =>
            {
                options.SetResourceBuilder(resourceBuilder);
                options.AddOtlpExporter(otlpOptions =>
                {
                    otlpOptions.Endpoint = new Uri(endpoint);
                });
            });
        });
    }

    /// <summary>
    /// Sends an information level log message.
    /// </summary>
    public async Task<bool> SendInfoLogAsync(CancellationToken cancellationToken = default)
    {
        return await CreateAndSendLogAsync(
            LogLevel.Information,
            $"Test info message: {DateTimeOffset.UtcNow:O}",
            cancellationToken);
    }

    /// <summary>
    /// Sends both information and error test logs.
    /// </summary>
    public async Task<(bool infoStatus, bool errorStatus)> SendTestLogsAsync(
        string testId,
        CancellationToken cancellationToken = default)
    {
        var infoStatus = await CreateAndSendLogAsync(
            LogLevel.Information,
            $"Test info message: {testId}",
            cancellationToken);

        var errorStatus = await CreateAndSendLogAsync(
            LogLevel.Error,
            $"Test error message {Guid.NewGuid()}",
            cancellationToken);

        return (infoStatus, errorStatus);
    }

    /// <summary>
    /// Creates and sends a log with the specified level and message.
    /// </summary>
    private Task<bool> CreateAndSendLogAsync(
        LogLevel level,
        string message,
        CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("SendLog");
        var logger = _loggerFactory.CreateLogger(_logType);

        try
        {
            logger.Log(level, message);
            _logger.LogInformation("Sent log message with level {Level}: {Message}", level, message);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send log message");
            return Task.FromResult(false);
        }
    }

    public void Dispose()
    {
        (_loggerFactory as IDisposable)?.Dispose();
        _activitySource.Dispose();
    }
}
