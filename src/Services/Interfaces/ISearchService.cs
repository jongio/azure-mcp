// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Arguments;

namespace AzureMcp.Services.Interfaces;

public interface ISearchService
{
    Task<List<string>> ListServices(
        string subscription,
        string? tenantId = null,
        RetryPolicyArguments? retryPolicy = null);
}
