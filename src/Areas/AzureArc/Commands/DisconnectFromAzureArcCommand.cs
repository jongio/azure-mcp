// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Areas.AzureArc.Services;
using AzureMcp.Options.Arc;
using AzureMcp.Models.Option;
using AzureMcp.Commands.Subscription;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Commands.Arc;

public sealed class DisconnectFromAzureArcCommand(ILogger<DisconnectFromAzureArcCommand> logger) : SubscriptionCommand<ArcConnectOptions>
{
    private const string _commandTitle = "Disconnects connected cluster from Azure Arc";

    // Define options as fields
    private readonly Option<string> _clusterNameOption = new("--cluster-name", "Name of the cluster to disconnect from Azure Arc");
    private readonly Option<string> _userProvidedPathOption = new("--user-provided-path", "Path to the user-provided files");

    public override string Name => "disconnect-arc";

    public override string Description =>
        "Disconnects an arc connected cluster from Azure Arc. It deletes all extensions and removes the cluster from Arc. The --user-provided-path option is required.";

    public override string Title => _commandTitle;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);

        // Add options to the command
        command.AddOption(_resourceGroupOption);
        command.AddOption(_clusterNameOption);
        command.AddOption(_userProvidedPathOption);
    }

    protected override ArcConnectOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);

        // Use the defined Option instances
        options.ClusterName = parseResult.GetValueForOption(_clusterNameOption);
        options.UserProvidedPath = parseResult.GetValueForOption(_userProvidedPathOption);

        // Manually bind ResourceGroup since the base class doesn't do it
        options.ResourceGroup = parseResult.GetValueForOption(_resourceGroupOption);

        return options;
    }

    [McpServerTool(
        Destructive = true,
        ReadOnly = false,
        Title = _commandTitle)]
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        try
        {
            // Retrieve options using model binding
            var options = BindOptions(parseResult);

            var arcService = context.GetService<IArcServices>();
            var result = await arcService.DisconnectFromAzureArcAsync(
                options.ResourceGroup!,
                options.ClusterName!,
                options.UserProvidedPath!);

            context.Response.Status = result.Success ? 200 : 500;
            context.Response.Message = result.Success
                ? "Cluster successfully disconnected from Azure Arc."
                : "Failed to disconnect cluster from Azure Arc.";

            return context.Response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to disconnect cluster from Azure Arc");
            context.Response.Status = 500;
            context.Response.Message = $"Failed to disconnect cluster from Azure Arc: {ex.Message}";
            return context.Response;
        }
    }
}
