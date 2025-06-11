// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Models.Arc;
using AzureMcp.Options;

namespace AzureMcp.Services.Interfaces;

public interface IArcService
{
    /// <summary>
    /// Configures an Azure Arc-enabled Kubernetes cluster by applying configuration and settings.
    /// </summary>
    /// <param name="clusterName">The name of the Azure Arc-enabled Kubernetes cluster</param>
    /// <param name="resourceGroupName">The resource group containing the cluster</param>
    /// <param name="subscriptionId">The subscription ID</param>
    /// <param name="configurationPath">Optional path to Kubernetes configuration file</param>
    /// <param name="tenant">Optional tenant ID for cross-tenant operations</param>
    /// <param name="retryPolicy">Optional retry policy configuration</param>
    /// <returns>Configuration operation result</returns>
    /// <exception cref="System.Exception">When the service request fails</exception>
    Task<ConfigurationResult> ConfigureClusterAsync(
        string clusterName,
        string resourceGroupName,
        string subscriptionId,
        string? configurationPath = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    /// <summary>
    /// Gets details for a specific Azure Arc-enabled Kubernetes cluster.
    /// </summary>
    /// <param name="clusterName">The name of the Arc cluster</param>
    /// <param name="resourceGroupName">The resource group containing the cluster</param>
    /// <param name="subscriptionId">The subscription ID</param>
    /// <param name="tenant">Optional tenant ID for cross-tenant operations</param>
    /// <param name="retryPolicy">Optional retry policy configuration</param>
    /// <returns>Arc cluster details or null if not found</returns>
    /// <exception cref="System.Exception">When the service request fails</exception>
    Task<Cluster?> GetClusterAsync(
        string clusterName,
        string resourceGroupName,
        string subscriptionId,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);    /// <summary>
                                                    /// Lists all Azure Arc-enabled Kubernetes clusters in the specified subscription.
                                                    /// </summary>
                                                    /// <param name="subscriptionId">The subscription ID</param>
                                                    /// <param name="resourceGroupName">Optional resource group name to filter clusters</param>
                                                    /// <param name="tag">Optional tag filter to match clusters</param>
                                                    /// <param name="tenant">Optional tenant ID for cross-tenant operations</param>
                                                    /// <param name="retryPolicy">Optional retry policy configuration</param>
                                                    /// <returns>List of Arc clusters</returns>
                                                    /// <exception cref="System.Exception">When the service request fails</exception>
    Task<IEnumerable<Cluster>> ListClustersAsync(
        string subscriptionId,
        string? resourceGroupName = null,
        string? tag = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);
}

public class ConfigurationResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string[]? AppliedConfigurations { get; set; }
    public string[]? Errors { get; set; }
}
