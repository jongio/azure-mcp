// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Options;

namespace AzureMcp.Areas.ContainerApps.Options.App;

public class AppListOptions : SubscriptionOptions
{
    public string? Environment { get; set; }
}
