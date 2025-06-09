// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace AzureMcp.Models.Sql;

internal interface IDatabaseAdviseCommandResult
{
    List<SqlIndexRecommendation> Recommendations { get; }
    SqlIndexAnalysisResult Analysis { get; }
}
