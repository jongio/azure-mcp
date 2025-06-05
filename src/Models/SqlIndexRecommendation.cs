// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace AzureMcp.Services.Interfaces;

/// <summary>
/// Represents a Sql index recommendation.
/// </summary>
public record SqlIndexRecommendation(
    [property: JsonPropertyName("name")]
    string Name,
    
    [property: JsonPropertyName("tableName")]
    string TableName,
    
    [property: JsonPropertyName("impact")]
    int Impact,
    
    [property: JsonPropertyName("description")]
    string Description
);
