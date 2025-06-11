// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Models.Arc;
using AzureMcp.Options.Arc;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Commands.Arc;

public sealed class ClusterGetCommand(ILogger<ClusterGetCommand> logger)
    : BaseClusterCommand<ClusterGetOptions>
{
    private const string _commandTitle = "Get Azure Arc-enabled Kubernetes cluster details";
    private readonly ILogger<ClusterGetCommand> _logger = logger;

    public override string Name => "get";
    public override string Description => "Gets detailed information about a specific Azure Arc-enabled Kubernetes cluster.";
    public override string Title => _commandTitle;

    [McpServerTool(Destructive = false, ReadOnly = true, Title = _commandTitle)]
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);
        _logger.LogInformation("Getting Arc cluster details. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ClusterName: {ClusterName}",
            options.Subscription, options.ResourceGroup, options.ClusterName);
        try
        {
            var arcService = context.GetService<IArcService>();
            var cluster = await arcService.GetClusterAsync(options.ClusterName!, options.ResourceGroup!, options.Subscription!);

            if (cluster == null)
            {
                context.Response.Status = 404;
                context.Response.Message = $"Arc cluster '{options.ClusterName}' not found.";
                return context.Response;
            }

            var result = new ClusterGetCommandResult(cluster);
            context.Response.Status = 200;
            context.Response.Message = "Successfully retrieved Arc cluster details.";
            context.Response.Results = ResponseResult.Create(result, ArcJsonContext.Default.ClusterGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError("An exception occurred processing command. Exception: {Exception}", ex);
            HandleException(context.Response, ex);
        }

        return context.Response;
    }

    internal record ClusterGetCommandResult(Cluster Cluster);
}
