// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using AzureMcp.Commands.Subscription;
using AzureMcp.Models.Option;
using AzureMcp.Options.Sql;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Commands.Sql;

public abstract class BaseSqlCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>
    : SubscriptionCommand<TOptions> where TOptions : BaseSqlOptions, new()
{
    protected readonly Option<string> _databaseOption = OptionDefinitions.Sql.Database;
    protected readonly Option<string> _serverOption = OptionDefinitions.Sql.Server;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_databaseOption);
        command.AddOption(_serverOption);
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Database = parseResult.GetValueForOption(_databaseOption);
        options.ServerName = parseResult.GetValueForOption(_serverOption);
        return options;
    }
}
