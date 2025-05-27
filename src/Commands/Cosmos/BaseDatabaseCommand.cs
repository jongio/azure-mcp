// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using AzureMcp.Models.Option;
using AzureMcp.Options.Cosmos;

namespace AzureMcp.Commands.Cosmos;

public abstract class BaseDatabaseCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TArgs>
    : BaseCosmosCommand<TArgs> where TArgs : BaseDatabaseOptions, new()
{
    protected readonly Option<string> _databaseOption = OptionDefinitions.Cosmos.Database;

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
