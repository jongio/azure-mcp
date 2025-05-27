// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using AzureMcp.Options.Subscription;

namespace AzureMcp.Commands.Subscription;

public abstract class BaseSubscriptionCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TArgs> : GlobalCommand<TArgs>
    where TArgs : BaseSubscriptionOptions, new()
{
    protected BaseSubscriptionCommand()
    {
    }
}
