// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Arguments.Monitor;
using AzureMcp.Models.Argument;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Commands.Monitor.Log;

public sealed class LogQueryCommand(ILogger<LogQueryCommand> logger) : BaseMonitorCommand<LogQueryArguments>()
{
    private const string _commandTitle = "Query Log Analytics Workspace";
    private readonly ILogger<LogQueryCommand> _logger = logger;
    private readonly Option<string> _tableNameOption = ArgumentDefinitions.Monitor.TableName;
    private readonly Option<string> _queryOption = ArgumentDefinitions.Monitor.Query;
    private readonly Option<int> _hoursOption = ArgumentDefinitions.Monitor.Hours;
    private readonly Option<int> _limitOption = ArgumentDefinitions.Monitor.Limit;

    public override string Name => "query";

    public override string Description =>
        $"""
        Execute a KQL query against a Log Analytics workspace. Requires {ArgumentDefinitions.Monitor.WorkspaceIdOrName}
        and resource group. Optional {ArgumentDefinitions.Monitor.HoursName}
        (default: {ArgumentDefinitions.Monitor.Hours.GetDefaultValue()}) and {ArgumentDefinitions.Monitor.LimitName}
        (default: {ArgumentDefinitions.Monitor.Limit.GetDefaultValue()}) parameters.
        The {ArgumentDefinitions.Monitor.QueryTextName} parameter accepts KQL syntax.
        """;

    public override string Title => _commandTitle;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_tableNameOption);
        command.AddOption(_queryOption);
        command.AddOption(_hoursOption);
        command.AddOption(_limitOption);
        command.AddOption(_resourceGroupOption);
    }

    protected override void RegisterArguments()
    {
        base.RegisterArguments();
        AddArgument(CreateTableNameArgument());
        AddArgument(CreateQueryArgument());
        AddArgument(CreateHoursArgument());
        AddArgument(CreateLimitArgument());
        AddArgument(CreateResourceGroupArgument());
    }

    [McpServerTool(Destructive = false, ReadOnly = true, Title = _commandTitle)]
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var args = BindArguments(parseResult);

        try
        {
            if (!context.Validate(parseResult))

            {
                return context.Response;
            }

            var monitorService = context.GetService<IMonitorService>();
            var results = await monitorService.QueryLogs(
                args.Subscription!,
                args.Workspace!,
                args.Query!,
                args.TableName!,
                args.Hours,
                args.Limit,
                args.Tenant,
                args.RetryPolicy);

            context.Response.Results = ResponseResult.Create(results, JsonSourceGenerationContext.Default.ListJsonNode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing log query command.");
            HandleException(context.Response, ex);
        }

        return context.Response;
    }

    private static ArgumentBuilder<LogQueryArguments> CreateTableNameArgument()
    {
        return ArgumentBuilder<LogQueryArguments>
            .Create(ArgumentDefinitions.Monitor.TableName.Name, ArgumentDefinitions.Monitor.TableName.Description!)
            .WithValueAccessor(args =>
            {
                try
                {
                    return args.TableName ?? string.Empty;
                }
                catch
                {
                    return string.Empty;
                }
            })
            .WithIsRequired(ArgumentDefinitions.Monitor.TableName.IsRequired);
    }

    private static ArgumentBuilder<LogQueryArguments> CreateQueryArgument() =>
        ArgumentBuilder<LogQueryArguments>
            .Create(ArgumentDefinitions.Monitor.Query.Name, ArgumentDefinitions.Monitor.Query.Description!)
            .WithValueAccessor(args => args.Query ?? string.Empty)
            .WithIsRequired(true);

    private static ArgumentBuilder<LogQueryArguments> CreateHoursArgument() =>
        ArgumentBuilder<LogQueryArguments>
            .Create(ArgumentDefinitions.Monitor.Hours.Name, ArgumentDefinitions.Monitor.Hours.Description!)
            .WithValueAccessor(args => args.Hours?.ToString() ?? ArgumentDefinitions.Monitor.Hours.GetDefaultValue().ToString())
            .WithIsRequired(false);

    private static ArgumentBuilder<LogQueryArguments> CreateLimitArgument() =>
        ArgumentBuilder<LogQueryArguments>
            .Create(ArgumentDefinitions.Monitor.Limit.Name, ArgumentDefinitions.Monitor.Limit.Description!)
            .WithValueAccessor(args => args.Limit?.ToString() ?? ArgumentDefinitions.Monitor.Limit.GetDefaultValue().ToString())
            .WithIsRequired(false);

    protected override LogQueryArguments BindArguments(ParseResult parseResult)
    {
        var args = base.BindArguments(parseResult);
        args.TableName = parseResult.GetValueForOption(_tableNameOption);
        args.Query = parseResult.GetValueForOption(_queryOption);
        args.Hours = parseResult.GetValueForOption(_hoursOption);
        args.Limit = parseResult.GetValueForOption(_limitOption);
        args.ResourceGroup = parseResult.GetValueForOption(_resourceGroupOption);
        return args;
    }
}
