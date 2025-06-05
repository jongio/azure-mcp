// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using AzureMcp.Models.Option;

namespace AzureMcp.Options.Sql;

public class BaseSqlOptions : SubscriptionOptions
{
    [JsonPropertyName(OptionDefinitions.Sql.DatabaseName)]
    public string? Database { get; set; }

    [JsonPropertyName(OptionDefinitions.Sql.ServerName)]
    public string? ServerName { get; set; }
}
