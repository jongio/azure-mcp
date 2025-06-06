// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using AzureMcp.Commands.Sql.Database;
using AzureMcp.Commands.Sql.Database.Index;
using AzureMcp.Commands.Sql.Server;

namespace AzureMcp.Commands.Sql;

[JsonSerializable(typeof(SqlIndexRecommendCommand.IndexRecommendCommandResult))]
[JsonSerializable(typeof(SqlDatabaseListCommand.DatabaseListCommandResult))]
[JsonSerializable(typeof(SqlServerListCommand.ServerListCommandResult))]
[JsonSerializable(typeof(Database.SqlDatabaseListCommand.DatabaseListCommandResult))]
[JsonSerializable(typeof(Server.SqlServerListCommand.ServerListCommandResult))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class SqlJsonContext : JsonSerializerContext
{
    // This class is generated at runtime by the source generator.
}
