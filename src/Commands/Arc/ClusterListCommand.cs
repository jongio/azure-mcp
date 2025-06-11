// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Models.Arc;
using AzureMcp.Options.Arc;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Commands.Arc;

public sealed class ClusterListCommand(ILogger<ClusterListCommand> logger)
    : BaseClusterCommand<ClusterListOptions>
{
    private const string _commandTitle = "List Azure Arc-enabled Kubernetes clusters";
    private readonly ILogger<ClusterListCommand> _logger = logger;

    public override string Name => "list";
    public override string Description => "Lists Azure Arc-enabled Kubernetes clusters in the specified subscription and resource group.";
    public override string Title => _commandTitle;

    [McpServerTool(Destructive = false, ReadOnly = true, Title = _commandTitle)]
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);
        _logger.LogInformation("Listing Arc clusters. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, Tag: {Tag}",
            options.Subscription, options.ResourceGroup, options.Tag);
        try
        {
            var arcService = context.GetService<IArcService>();
            var clusters = await arcService.ListClustersAsync(options.Subscription!, options.ResourceGroup, options.Tag);

            var result = new ClusterListCommandResult(clusters.ToList());
            context.Response.Status = 200;
            context.Response.Message = "Successfully retrieved Arc clusters.";
            context.Response.Results = ResponseResult.Create(result, ArcJsonContext.Default.ClusterListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError("An exception occurred processing command. Exception: {Exception}", ex);
            HandleException(context.Response, ex);
        }

        return context.Response;
    }

    internal record ClusterListCommandResult(List<Cluster> Clusters);
}
