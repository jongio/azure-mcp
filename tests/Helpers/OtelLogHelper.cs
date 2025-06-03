// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Net;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;

namespace AzureMcp.Tests.Helpers;

/// <summary>
/// Helper class for sending logs using OpenTelemetry.
/// </summary>
public class OtelLogHelper : IDisposable
{
    private const string DefaultCategory = "OtelLogHelper";
    private readonly string _serviceName;
    private readonly ILogger _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ActivitySource _activitySource;

    public OtelLogHelper(
        string serviceName = DefaultCategory,
        string endpoint = "http://localhost:18889",
        ILogger? logger = null)
    {
        _serviceName = serviceName;
        _logger = logger ?? NullLogger.Instance;
        _activitySource = new ActivitySource(_serviceName);

        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(_serviceName);

        _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddOpenTelemetry(options =>
            {
                options.SetResourceBuilder(resourceBuilder);
                options.AddOtlpExporter(otlpOptions =>
                {
                    otlpOptions.Endpoint = new Uri(endpoint);
                });
                options.IncludeFormattedMessage = true;
                options.IncludeScopes = true;
            });
        });
    }

    /// <summary>
    /// Sends an information level log message.
    /// </summary>
    public Task<HttpStatusCode> SendInfoLogAsync(CancellationToken cancellationToken = default)
    {
        return SendLogAsync(
            LogLevel.Information,
            $"Test info message: {DateTimeOffset.UtcNow:O}",
            cancellationToken);
    }

    /// <summary>
    /// Sends both information and error test logs.
    /// </summary>
    public async Task<(HttpStatusCode infoStatus, HttpStatusCode errorStatus)> SendTestLogsAsync(
        string testId,
        CancellationToken cancellationToken = default)
    {
        var infoStatus = await SendLogAsync(
            LogLevel.Information,
            $"Test info message: {testId}",
            cancellationToken).ConfigureAwait(false);

        var errorStatus = await SendLogAsync(
            LogLevel.Error,
            $"Test error message {Guid.NewGuid()}",
            cancellationToken).ConfigureAwait(false);

        return (infoStatus, errorStatus);
    }

    /// <summary>
    /// Sends a log with the specified level and message.
    /// </summary>
    private Task<HttpStatusCode> SendLogAsync(
        LogLevel level,
        string message,
        CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("SendLog");
        var logger = _loggerFactory.CreateLogger(_serviceName);

        try
        {
            logger.Log(level, message);
            _logger.LogInformation("Sent log message with level {Level}: {Message}", level, message);
            return Task.FromResult(HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send log message");
            return Task.FromResult(HttpStatusCode.InternalServerError);
        }
    }

    public void Dispose()
    {
        (_loggerFactory as IDisposable)?.Dispose();
        _activitySource.Dispose();
    }
}
