// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace AzureMcp.Models.Sql;

internal interface IIndexRecommendCommandResult
{
    List<SqlIndexRecommendation> Recommendations { get; }
    SqlIndexAnalysisResult Analysis { get; }
}
