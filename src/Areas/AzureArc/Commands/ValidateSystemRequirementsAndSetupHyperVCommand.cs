// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Areas.AzureArc.Services;
using AzureMcp.Options.Arc;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Commands.Arc;

public sealed class ValidateSystemRequirementsAndSetupHyperVCommand(ILogger<ValidateSystemRequirementsAndSetupHyperVCommand> logger) : GlobalCommand<ArcConnectOptions>
{
    private const string _commandTitle = "Validate System Requirements and Setup Hyper-V";

    private readonly Option<string> _pathOption = new Option<string>("--path", "The path to validate system requirements and setup Hyper-V") { IsRequired = true };

    public override string Name => "validate-system-requirements-hyperv";

    public override string Description =>
        "Validates system requirements and sets up Hyper-V for AKS Edge Essentials or Aksee cluster installation.";

    public override string Title => _commandTitle;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_pathOption);
    }

    protected override ArcConnectOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.UserProvidedPath = parseResult.GetValueForOption(_pathOption);
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
            var options = BindOptions(parseResult);
            if (string.IsNullOrEmpty(options.UserProvidedPath))
            {
                throw new ArgumentException("The --path option is required.");
            }

            // Informational warning about system restart
            string warningMessage = "Note: The system may restart after Hyper-V installation.";
            logger.LogInformation(warningMessage);

            var arcService = context.GetService<IArcServices>();
            var result = await arcService.ValidateSystemRequirementsAndSetupHyperVAsync(options.UserProvidedPath);

            context.Response.Status = result.Success ? 200 : 500;
            context.Response.Message = $"{warningMessage}\n{result.Steps}";

            return context.Response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to validate system requirements and setup Hyper-V");
            context.Response.Status = 500;
            context.Response.Message = ex.Message;
            return context.Response;
        }
    }
}
