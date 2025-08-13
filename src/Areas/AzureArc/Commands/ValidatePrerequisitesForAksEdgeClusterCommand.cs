// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Areas.AzureArc.Services;
using AzureMcp.Commands.Subscription;
using AzureMcp.Options.Arc;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Commands.Arc;

public sealed class ValidatePrerequisitesForAksEdgeClusterCommand(ILogger<ValidatePrerequisitesForAksEdgeClusterCommand> logger) : SubscriptionCommand<ArcConnectOptions>
{
    private const string _commandTitle = "Describes prerequisites needed for AKS Edge Essentials Cluster and available tools";

    public override string Name => "describe-prereqs-edge-cluster";

    public override string Description =>
        "Validates prerequisites for installing AKS Edge Essentials cluster by loading steps from the resource file and suggesting next steps";

    public override string Title => _commandTitle;

    [McpServerTool(
        Destructive = false,
        ReadOnly = true,
        Title = _commandTitle)]
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        try
        {
            var arcService = context.GetService<IArcServices>();
            var result = await arcService.ValidatePrerequisitesForAksEdgeClusterAsync();

            context.Response.Status = result.Success ? 200 : 500;
            context.Response.Message = result.Steps;

            return context.Response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to validate prerequisites for AKS Edge Essentials cluster");
            context.Response.Status = 500;
            context.Response.Message = ex.Message;
            return context.Response;
        }
    }
}
