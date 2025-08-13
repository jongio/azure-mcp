// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Areas.AzureArc.Services;
using AzureMcp.Commands.Subscription;
using AzureMcp.Options.Arc;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Commands.Arc;

public sealed class ValidateAndInstallSwRequirementCommand(ILogger<ValidateAndInstallSwRequirementCommand> logger) : SubscriptionCommand<ArcConnectOptions>
{
    private const string _commandTitle = "Validate and Install Software Requirements";

    private readonly Option<string> _userProvidedPathOption = new("--user-provided-path", "The path to validate and install software requirements") { IsRequired = true };

    public override string Name => "setup-software-requirement";

    public override string Description =>
        "Validates and installs software requirements for AKS Edge. This command checks for required software components and installs them if missing.";

    public override string Title => _commandTitle;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_userProvidedPathOption);
    }

    protected override ArcConnectOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.UserProvidedPath = parseResult.GetValueForOption(_userProvidedPathOption);
        return options;
    }

    [McpServerTool(
        Destructive = false,
        ReadOnly = false,
        Title = _commandTitle)]
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        try
        {
            var options = BindOptions(parseResult);

            if (string.IsNullOrEmpty(options.UserProvidedPath))
            {
                throw new ArgumentException("The --user-provided-path option is required.");
            }

            var arcService = context.GetService<IArcServices>();
            var result = await arcService.ValidateAndInstallSwRequirementAsync(options.UserProvidedPath);

            context.Response.Status = result.Success ? 200 : 500;
            context.Response.Message = result.Steps;

            return context.Response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to validate and install software requirements");
            context.Response.Status = 500;
            context.Response.Message = ex.Message;
            return context.Response;
        }
    }
}
