// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Areas.AzureArc.Services;
using AzureMcp.Options.Arc;
using AzureMcp.Models.Option;
using AzureMcp.Commands.Subscription;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Commands.Arc;

public sealed class OnboardClusterToArcCommand(ILogger<OnboardClusterToArcCommand> logger) : SubscriptionCommand<ArcConnectOptions>
{
    private const string _commandTitle = "Connects existing cluster to Azure Arc";

    // Define options as fields
    private readonly Option<string> _clusterNameOption = new("--cluster-name", "Name of the cluster to connect to Azure Arc");
    private readonly Option<string> _locationOption = new("--location", "Azure region");
    private readonly Option<string> _kubeConfigPathOption = new("--kube-config-path", "Path to the kubeconfig file");
    private readonly Option<string> _userProvidedPathOption = new("--user-provided-path", "Path to the user-provided files");

    public override string Name => "connect-arc";

    public override string Description =>
        "Onboards or connects an existing cluster to Azure Arc by validating the cluster and executing the necessary commands.The --user-provided-path option is required.";

    public override string Title => _commandTitle;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);

        // Add options to the command
        command.AddOption(_resourceGroupOption);
        command.AddOption(_clusterNameOption);
        command.AddOption(_locationOption);
        command.AddOption(_kubeConfigPathOption);
        command.AddOption(_userProvidedPathOption);
    }

    protected override ArcConnectOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);

        // Use the defined Option instances
        options.ClusterName = parseResult.GetValueForOption(_clusterNameOption);
        options.Location = parseResult.GetValueForOption(_locationOption);
        options.KubeConfigPath = parseResult.GetValueForOption(_kubeConfigPathOption);
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
            var result = await arcService.OnboardClusterToArcAsync(
                options.ClusterName!,
                options.ResourceGroup!,
                options.Location!,
                options.Subscription!,
                options.Tenant!,
                options.KubeConfigPath!,
                options.UserProvidedPath!);

            context.Response.Status = result.Success ? 200 : 500;
            context.Response.Message = result.Steps;

            return context.Response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to onboard cluster to Azure Arc");
            context.Response.Status = 500;
            context.Response.Message = $"Failed to onboard cluster to Azure Arc: {ex.Message}";
            return context.Response;
        }
    }
}
