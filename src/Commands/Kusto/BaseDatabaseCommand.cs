// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using AzureMcp.Models.Option;
using AzureMcp.Options.Kusto;

namespace AzureMcp.Commands.Kusto;

public abstract class BaseDatabaseCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TArgs>
    : BaseClusterCommand<TArgs> where TArgs : BaseDatabaseOptions, new()
{
    protected readonly Option<string> _databaseOption = OptionDefinitions.Kusto.Database;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_databaseOption);
    }

    protected override TArgs BindOptions(ParseResult parseResult)
    {
        var args = base.BindOptions(parseResult);
        args.Database = parseResult.GetValueForOption(_databaseOption);
        return args;
    }
}
