// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Models.Arc;
using AzureMcp.Options;

namespace AzureMcp.Services.Interfaces;

public interface IArcService
{    /// <summary>
     /// Installs the Azure Container Storage for Arc (ACSA) extension on a connected Kubernetes cluster using the Azure CLI.
     /// </summary>
     /// <param name="resourceGroupName">The name of the resource group containing the cluster.</param>
     /// <param name="clusterName">The name of the connected cluster.</param>
     /// <param name="logger">Optional logger action for outputting progress and errors.</param>
     /// <returns>True if the extension was installed successfully, otherwise false.</returns>
    Task<bool> InstallAcsaExtensionAsync(
        string resourceGroupName,
        string clusterName,
        Action<string>? logger = null);

    /// <summary>
    /// Installs the Azure Container Storage for Arc (ACSA) extension on a connected Kubernetes cluster using the Azure CLI.
    /// </summary>
    /// <param name="resourceGroupName">The name of the resource group containing the cluster.</param>
    /// <param name="clusterName">The name of the connected cluster.</param>
    /// <param name="subscriptionId">The Azure subscription ID.</param>
    /// <param name="logger">Optional logger action for outputting progress and errors.</param>
    /// <returns>True if the extension was installed successfully, otherwise false.</returns>
    Task<bool> InstallAcsaExtensionAsync(
        string resourceGroupName,
        string clusterName,
        string subscriptionId,
        Action<string>? logger = null);

    /// <summary>
    /// Installs the Secret Sync Service (SSE) extension on a connected Kubernetes cluster using the Azure CLI.
    /// </summary>
    /// <param name="resourceGroupName">The name of the resource group containing the cluster.</param>
    /// <param name="clusterName">The name of the connected cluster.</param>
    /// <param name="logger">Optional logger action for outputting progress and errors.</param>
    /// <returns>True if the extension was installed successfully, otherwise false.</returns>
    Task<bool> InstallSecretSyncServiceExtensionAsync(
        string resourceGroupName,
        string clusterName,
        Action<string>? logger = null);

    /// <summary>
    /// Installs the Secret Sync Service (SSE) extension on a connected Kubernetes cluster using the Azure CLI.
    /// </summary>
    /// <param name="resourceGroupName">The name of the resource group containing the cluster.</param>
    /// <param name="clusterName">The name of the connected cluster.</param>
    /// <param name="subscriptionId">The Azure subscription ID.</param>
    /// <param name="logger">Optional logger action for outputting progress and errors.</param>
    /// <returns>True if the extension was installed successfully, otherwise false.</returns>
    Task<bool> InstallSecretSyncServiceExtensionAsync(
        string resourceGroupName,
        string clusterName,
        string subscriptionId,
        Action<string>? logger = null);/// <summary>
                                       /// Installs the Azure IoT Operations (AIO) Platform extension on a connected Kubernetes cluster using the Azure CLI.
                                       /// </summary>
                                       /// <param name="resourceGroupName">The name of the resource group containing the cluster.</param>
                                       /// <param name="clusterName">The name of the connected cluster.</param>
                                       /// <param name="logger">Optional logger action for outputting progress and errors.</param>
                                       /// <returns>True if the extension was installed successfully, otherwise false.</returns>
    Task<bool> InstallAioPlatformExtensionAsync(
        string resourceGroupName,
        string clusterName,
        Action<string>? logger = null);

    /// <summary>
    /// Installs the Azure IoT Operations (AIO) Platform extension on a connected Kubernetes cluster using the Azure CLI.
    /// </summary>
    /// <param name="resourceGroupName">The name of the resource group containing the cluster.</param>
    /// <param name="clusterName">The name of the connected cluster.</param>
    /// <param name="subscriptionId">The Azure subscription ID.</param>
    /// <param name="logger">Optional logger action for outputting progress and errors.</param>
    /// <returns>True if the extension was installed successfully, otherwise false.</returns>
    Task<bool> InstallAioPlatformExtensionAsync(
        string resourceGroupName,
        string clusterName,
        string subscriptionId,
        Action<string>? logger = null);

    /// <summary>
    /// Connects an AKS cluster to Azure Arc using the Azure CLI.
    /// </summary>
    /// <param name="resourceGroupName">The name of the resource group containing the AKS cluster.</param>
    /// <param name="arcClusterName">The desired Azure Arc cluster name.</param>
    /// <param name="location">The Azure region for the Arc resource (e.g., "eastus2euap").</param>
    /// <param name="logger">Optional logger action for outputting progress and errors.</param>
    /// <returns>True if the connection was successful, otherwise false.</returns>

    Task<bool> ConnectAksToArcAsync(
       string resourceGroupName,
       string arcClusterName,
       string location,
       Action<string>? logger = null);

    /// <summary>
    /// Creates an AKS cluster with a system-assigned managed identity.
    /// </summary>
    /// <param name="resourceGroupService">The resource group service.</param>
    /// <param name="subscriptionId">The subscription ID.</param>
    /// <param name="resourceGroupName">The resource group name.</param>
    /// <param name="aksClusterName">The AKS cluster name.</param>
    /// <param name="region">The region.</param>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="logger">Optional logger action for outputting progress and errors.</param>
    /// <returns>The resource ID of the created AKS cluster.</returns>
    Task<string> CreateAksClusterAsync(
        IResourceGroupService resourceGroupService,
        string subscriptionId,
        string resourceGroupName,
        string aksClusterName,
        string region,
        string tenantId,
        Action<string>? logger = null);


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
