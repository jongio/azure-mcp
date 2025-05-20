// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using AzureMcp.Arguments;
using AzureMcp.Arguments.Monitor;
using AzureMcp.Models.Argument;

namespace AzureMcp.Commands.Monitor;

public abstract class BaseMonitorCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TArgs>
    : SubscriptionCommand<TArgs>
    where TArgs : SubscriptionArguments, IWorkspaceArguments, new()
{
    protected readonly Option<string> _workspaceOption = ArgumentDefinitions.Monitor.Workspace;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_workspaceOption);
    }

    protected override void RegisterArguments()
    {
        base.RegisterArguments();
        AddArgument(CreateWorkspaceArgument());
    }

    protected virtual ArgumentBuilder<TArgs> CreateWorkspaceArgument() =>
        ArgumentBuilder<TArgs>
            .Create(ArgumentDefinitions.Monitor.Workspace.Name, ArgumentDefinitions.Monitor.Workspace.Description!)
            .WithValueAccessor(args => args.Workspace ?? string.Empty)
            .WithIsRequired(ArgumentDefinitions.Monitor.Workspace.IsRequired);

    protected override TArgs BindArguments(ParseResult parseResult)
    {
        var args = base.BindArguments(parseResult);
        args.Workspace = parseResult.GetValueForOption(_workspaceOption);
        return args;
    }
}
