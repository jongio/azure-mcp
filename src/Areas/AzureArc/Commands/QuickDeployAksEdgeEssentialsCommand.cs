// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Areas.AzureArc.Services;
using AzureMcp.Options.Arc;
using AzureMcp.Models.Option;
using AzureMcp.Commands.Subscription;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Commands.Arc;

public sealed class QuickDeployAksEdgeEssentialsCommand(ILogger<QuickDeployAksEdgeEssentialsCommand> logger) : SubscriptionCommand<ArcConnectOptions>
{
    private const string _commandTitle = "Quickly Deploy AKS Edge Essentials in local environment";

    private readonly Option<string> _clusterNameOption = new("--cluster-name", "Name of the AKS Edge Essentials cluster.") { IsRequired = true };
    private readonly Option<string> _locationOption = new("--location", "Azure region where the cluster will be registered.") { IsRequired = true };
    private readonly Option<string> _userProvidedPathOption = new("--user-provided-path", "User-provided path for temporary files.") { IsRequired = true };

    public override string Name => "deploy-edge-essentials";

    public override string Description =>
        "Performs a quick deployment of AKS Edge Essentials by executing the necessary scripts and configurations to set up the environment efficiently. After the Edge Essentials deployment, this also connects the cluster to Azure Arc";

    public override string Title => _commandTitle;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);

        // Add options specific to ArcConnectOptions
        command.AddOption(_resourceGroupOption);
        command.AddOption(_clusterNameOption);
        command.AddOption(_locationOption);
        command.AddOption(_userProvidedPathOption);
    }

    protected override ArcConnectOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);

        // Bind additional options
        options.ClusterName = parseResult.GetValueForOption(_clusterNameOption);
        options.Location = parseResult.GetValueForOption(_locationOption);
        options.UserProvidedPath = parseResult.GetValueForOption(_userProvidedPathOption);

        // Manually bind ResourceGroup since the base class doesn't do it
        options.ResourceGroup = parseResult.GetValueForOption(_resourceGroupOption);

        return options;
    }

    [McpServerTool(
        Destructive = false,
        ReadOnly = true,
        Title = _commandTitle)]
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        try
        {

            // Retrieve options using model binding
            var options = BindOptions(parseResult);

            var arcService = context.GetService<IArcServices>();
            var result = await arcService.QuickDeployAksEdgeEssentialsAsync(
                options.ClusterName!,
                options.ResourceGroup!,
                options.Subscription!,
                options.Tenant!,
                options.Location!,
                options.UserProvidedPath!);

            context.Response.Status = result.Success ? 200 : 500;
            context.Response.Message = result.Steps;

            return context.Response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to perform quick deployment of AKS Edge Essentials");
            context.Response.Status = 500;
            context.Response.Message = ex.Message;
            return context.Response;
        }
    }
}
