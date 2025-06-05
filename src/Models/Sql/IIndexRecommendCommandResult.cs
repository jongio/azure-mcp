// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace AzureMcp.Models.Sql;

internal interface IIndexRecommendCommandResult
{
    List<SqlIndexRecommendation> Recommendations { get; }
}
