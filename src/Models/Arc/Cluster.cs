// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Models.Identity;

namespace AzureMcp.Models.Arc;

public class Cluster
{
    /// <summary> Name of the Azure Arc-enabled Kubernetes cluster. </summary>
    public string? Name { get; set; }

    /// <summary> ID of the Azure subscription containing the Arc cluster. </summary>
    public string? SubscriptionId { get; set; }

    /// <summary> Name of the resource group containing the Arc cluster. </summary>
    public string? ResourceGroupName { get; set; }

    /// <summary> Azure geo-location where the Arc cluster resource lives. </summary>
    public string? Location { get; set; }

    /// <summary> Provisioning status of the Arc cluster. </summary>
    public string? ProvisioningState { get; set; }

    /// <summary> Current status of the Arc cluster. </summary>
    public string? Status { get; set; }

    /// <summary> Version of Kubernetes running on the cluster. </summary>
    public string? KubernetesVersion { get; set; }

    /// <summary> Agent version for Azure Arc agents. </summary>
    public string? AgentVersion { get; set; }

    /// <summary> Infrastructure type (e.g., AWS_EKS, GCP_GKE, or generic). </summary>
    public string? Infrastructure { get; set; }

    /// <summary> Total number of nodes in the cluster. </summary>
    public int? TotalNodeCount { get; set; }

    /// <summary> Total number of CPU cores in the cluster. </summary>
    public int? TotalCoreCount { get; set; }

    /// <summary> System-assigned managed identity of the Arc cluster. </summary>
    public ManagedIdentityInfo? Identity { get; set; }

    /// <summary> Tags on the Arc cluster resource. </summary>
    public IDictionary<string, string>? Tags { get; set; }

    /// <summary> Date/time when the cluster was connected to Azure Arc. </summary>
    public DateTime? ConnectivityStatus { get; set; }

    /// <summary> Last activity time for the cluster. </summary>
    public DateTime? LastConnectivityTime { get; set; }

    /// <summary> Offering type (e.g., Public Cloud, Azure Stack Edge). </summary>
    public string? Offering { get; set; }

    /// <summary> Distribution of Kubernetes (e.g., AKS, OpenShift, Rancher). </summary>
    public string? Distribution { get; set; }

    /// <summary> Azure Arc extensions installed on the cluster. </summary>
    public string[]? Extensions { get; set; }
}
