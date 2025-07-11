// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Options;

namespace AzureMcp.Areas.Aks.Services;

public interface IAksService
{
    Task<List<string>> ListClusters(
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);
}
