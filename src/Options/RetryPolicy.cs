// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace AzureMcp.Options;

/// <summary>
/// The retry policy for Azure operations
/// </summary>
public class RetryPolicy
{
    public string? RetryType { get; set; }
    public int? MaxRetries { get; set; }
    public int? RetryAfterSeconds { get; set; }
    public bool? ExponentialBackoff { get; set; }
}
