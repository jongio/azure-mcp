// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Core.Commands;
using AzureMcp.Core.Commands.Subscription;
using AzureMcp.Core.Services.Telemetry;
using AzureMcp.Deploy.Options;
using AzureMcp.Deploy.Services;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Deploy.Commands;

public sealed class AzdAppLogGetCommand(ILogger<AzdAppLogGetCommand> logger) : SubscriptionCommand<AzdAppLogOptions>()
{
    private const string CommandTitle = "Get AZD deployed App Logs";
    private readonly ILogger<AzdAppLogGetCommand> _logger = logger;

    private readonly Option<string> _workspaceFolderOption = DeployOptionDefinitions.AzdAppLogOptions.WorkspaceFolder;
    private readonly Option<string> _azdEnvNameOption = DeployOptionDefinitions.AzdAppLogOptions.AzdEnvName;
    private readonly Option<int> _limitOption = DeployOptionDefinitions.AzdAppLogOptions.Limit;

    public override string Name => "azd-app-log-get";
    public override string Title => CommandTitle;
    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    public override string Description =>
        """
        This tool helps fetch logs from log analytics workspace for Container Apps, App Services, function apps that were deployed through azd. Invoke this tool directly after a successful `azd up` or when user prompts to check the app's status or provide errors in the deployed apps.
        """;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_workspaceFolderOption);
        command.AddOption(_azdEnvNameOption);
        command.AddOption(_limitOption);
    }

    protected override AzdAppLogOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WorkspaceFolder = parseResult.GetValueForOption(_workspaceFolderOption)!;
        options.AzdEnvName = parseResult.GetValueForOption(_azdEnvNameOption)!;
        options.Limit = parseResult.GetValueForOption(_limitOption);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            context.Activity?.WithSubscriptionTag(options);

            var deployService = context.GetService<IDeployService>();
            string result = await deployService.GetAzdResourceLogsAsync(
                options.WorkspaceFolder!,
                options.AzdEnvName!,
                options.Subscription!,
                options.Limit);

            context.Response.Message = result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred getting azd app logs.");
            HandleException(context, ex);
        }

        return context.Response;
    }

}
