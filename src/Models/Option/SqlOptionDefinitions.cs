// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;

namespace AzureMcp.Models.Option;

public static partial class OptionDefinitions
{
    public static class Sql
    {
        public const string TableName = "table-name";
        public const string MinimumImpactName = "minimum-impact";
        public const string DatabaseName = "database";
        public const string ServerName = "server-name";

        public static readonly Option<string> Table = new(
            $"--{TableName}",
            "The name of the SQL table to analyze for index recommendations."
        )
        {
            IsRequired = false
        };

        public static readonly Option<int> MinimumImpact = new(
            $"--{MinimumImpactName}",
            () => 0,
            "The minimum impact threshold for index recommendations."
        )
        {
            IsRequired = false
        };

        public static readonly Option<string> Database = new(
            $"--{DatabaseName}",
            "The SQL database to analyze."
        )
        {
            IsRequired = true
        };

        public static readonly Option<string> Server = new(
            $"--{ServerName}",
            "The Sql server containing the database."
        )
        {
            IsRequired = true
        };
    }
}
