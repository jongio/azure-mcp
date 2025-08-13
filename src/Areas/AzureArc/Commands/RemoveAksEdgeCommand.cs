// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Areas.AzureArc.Services;
using AzureMcp.Options.Arc;
using AzureMcp.Models.Option;
using AzureMcp.Commands.Subscription;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Commands.Arc;

public sealed class RemoveAksEdgeCommand(ILogger<RemoveAksEdgeCommand> logger) : SubscriptionCommand<ArcConnectOptions>
{
    private const string _commandTitle = "Remove installations for existing edge cluster from local";

    public override string Name => "remove-edge-essentials";

    public override string Description =>
        "Completely removes AKS Edge Essentials from the machine. Ensures that any existing installation is removed and settings are preset for new installation.";

    public override string Title => _commandTitle;

    private readonly Option<string> _userProvidedPathOption = new("--user-provided-path", "The path to use for script execution") { IsRequired = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_resourceGroupOption);
        command.AddOption(_userProvidedPathOption);
    }

    protected override ArcConnectOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup = parseResult.GetValueForOption(_resourceGroupOption);
        options.UserProvidedPath = parseResult.GetValueForOption(_userProvidedPathOption);
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
            var options = BindOptions(parseResult);
            if (string.IsNullOrEmpty(options.UserProvidedPath))
            {
                throw new ArgumentException("The path parameter is required.");
            }

            logger.LogInformation("Starting AKS Edge Essentials removal.");

            var arcService = context.GetService<IArcServices>();
            var removalSuccess = await arcService.RemoveAksEdgeAsync(options.UserProvidedPath);

            if (removalSuccess)
            {
                context.Response.Status = 200;
                context.Response.Message = "AKS Edge Essentials removed successfully.";
            }
            else
            {
                context.Response.Status = 500;
                context.Response.Message = "Failed to remove AKS Edge Essentials.";
            }

            return context.Response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to remove AKS Edge Essentials.");
            context.Response.Status = 500;
            context.Response.Message = ex.Message;
            return context.Response;
        }
    }
}
