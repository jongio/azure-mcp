// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using AzureMcp.Arguments;
using AzureMcp.Models.Argument;

namespace AzureMcp.Commands;

public abstract class SubscriptionCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TArgs> : GlobalCommand<TArgs>
    where TArgs : SubscriptionArguments, new()
{
    protected readonly Option<string> _subscriptionOption = ArgumentDefinitions.Common.Subscription;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_subscriptionOption);
    }

    protected override void RegisterArguments()
    {
        base.RegisterArguments();
        AddArgument(CreateSubscriptionArgument());
    }

    protected ArgumentBuilder<TArgs> CreateSubscriptionArgument()
    {
        return ArgumentBuilder<TArgs>
            .Create(ArgumentDefinitions.Common.Subscription.Name, ArgumentDefinitions.Common.Subscription.Description!)
            .WithValueAccessor(args => args.Subscription ?? string.Empty)
            .WithIsRequired(ArgumentDefinitions.Common.Subscription.IsRequired);
    }

    protected override TArgs BindArguments(ParseResult parseResult)
    {
        var args = base.BindArguments(parseResult);
        args.Subscription = parseResult.GetValueForOption(_subscriptionOption);
        return args;
    }
}
