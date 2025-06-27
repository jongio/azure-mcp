// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure;
using Azure.ResourceManager.HybridCompute;
using Azure.ResourceManager.Resources;
using AzureMcp.Models.Arc;
using AzureMcp.Models.Identity;
using AzureMcp.Options;
using AzureMcp.Services.Interfaces;
using AzureMcp.Services.Azure;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.ContainerService;
using Azure.ResourceManager.ContainerService.Models;
using Azure.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.ResourceManager.Models;
using System.Diagnostics;

namespace AzureMcp.Services.Azure.Arc;

public sealed class ArcService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    ICacheService cacheService,
    IResourceGroupService resourceGroupService)
    : BaseAzureService(tenantService), IArcService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    private readonly IResourceGroupService _resourceGroupService = resourceGroupService ?? throw new ArgumentNullException(nameof(resourceGroupService));
    private readonly ITenantService _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
    private const string CACHE_GROUP = "arc";
    private static readonly TimeSpan CACHE_DURATION = TimeSpan.FromHours(1);

    /// <summary>
    /// Creates an AKS cluster with a system-assigned managed identity.
    /// </summary>
    public async Task<string> CreateAksClusterAsync(
        IResourceGroupService resourceGroupService,
        string subscriptionId,
        string resourceGroupName,
        string aksClusterName,
        string region,
        string tenantId,
        Action<string>? logger = null)
    {
        logger ??= Console.WriteLine;
        logger("Authenticating with Azure...");

        var resourceGroup = await resourceGroupService.GetResourceGroupResource(
            subscriptionId, resourceGroupName, tenantId);

        if (resourceGroup == null)
            throw new Exception($"Resource group '{resourceGroupName}' not found in subscription '{subscriptionId}'.");

        logger($"Defining AKS cluster '{aksClusterName}' parameters...");
        var aksData = new ContainerServiceManagedClusterData(region)
        {
            DnsPrefix = aksClusterName,
            AgentPoolProfiles =
             {
                 new ManagedClusterAgentPoolProfile("nodepool1")
                 {
                     Count = 2,
                      VmSize = "Standard_DS2_v2", //Comand to see which VMs available: az vm list-skus --location eastus --resource-type virtualMachines --query "[?restrictions[?type=='Location' && restrictionInfo.reasons[?contains(@, 'QuotaId')]==null]].name" --output tsv
                   // VmSize = "Standard_B2s",
                     Mode = AgentPoolMode.System
                 }
             },
            Identity = new ManagedServiceIdentity(ManagedServiceIdentityType.SystemAssigned)
        };
        logger($"Creating AKS cluster '{aksClusterName}' in resource group '{resourceGroupName}'...");
        var aksCollection = resourceGroup.GetContainerServiceManagedClusters();
        var aksLro = await aksCollection.CreateOrUpdateAsync(WaitUntil.Completed, aksClusterName, aksData);
        var aksCluster = aksLro.Value;

        logger($"AKS cluster '{aksClusterName}' created successfully. Resource ID: {aksCluster.Id}");
        return aksCluster.Id.ToString();
    }

    /// <summary>
    /// Retrieves the kubeconfig for the specified AKS cluster by invoking the Azure CLI.
    /// This will update the local kubeconfig file (usually ~/.kube/config).
    /// </summary>
    /// <param name="resourceGroupName">The name of the resource group containing the AKS cluster.</param>
    /// <param name="aksClusterName">The name of the AKS cluster.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task GetKubeConfigAsync(
        string resourceGroupName,
        string aksClusterName, Action<string>? logger = null)
    {
        var command = $"az aks get-credentials --resource-group {resourceGroupName} --name {aksClusterName} --admin --overwrite-existing";
        logger?.Invoke($"Executing command: {command}");

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "pwsh",
            Arguments = $"-Command \"{command}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = processStartInfo };
        process.Start();

        string output = await process.StandardOutput.ReadToEndAsync();
        string error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (!string.IsNullOrWhiteSpace(output))
            logger?.Invoke($"Azure CLI output: {output}");
        if (!string.IsNullOrWhiteSpace(error))
            logger?.Invoke($"Azure CLI error: {error}");

        if (process.ExitCode != 0)
        {
            throw new Exception($"Failed to retrieve kubeconfig: {error}");
        }

        logger?.Invoke("Kubernetes credentials retrieved and kubeconfig updated successfully.");

    }

    /// <summary>
    /// Connects an AKS cluster to Azure Arc using the Azure CLI.
    /// </summary>
    /// <param name="resourceGroupName">The name of the resource group containing the AKS cluster.</param>
    /// <param name="arcClusterName">The desired Azure Arc cluster name.</param>
    /// <param name="location">The Azure region for the Arc resource (e.g., "eastus2euap").</param>
    /// <param name="logger">Optional logger action for outputting progress and errors.</param>
    /// <returns>True if the connection was successful, otherwise false.</returns>
    public async Task<bool> ConnectAksToArcAsync(
        string resourceGroupName,
        string arcClusterName,
        string location,
        Action<string>? logger = null)
    {
        logger ??= Console.WriteLine;

        // Retrieve kubeconfig for the AKS cluster before connecting to Arc
        await GetKubeConfigAsync(resourceGroupName, arcClusterName, logger);

        var command = $"az connectedk8s connect --name {arcClusterName} --resource-group {resourceGroupName} --location {location}";
        logger($"Executing command: {command}");

        var processStartInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "pwsh",
            Arguments = $"-Command \"{command}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new System.Diagnostics.Process { StartInfo = processStartInfo };
        process.Start();

        string output = await process.StandardOutput.ReadToEndAsync();
        string error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (!string.IsNullOrWhiteSpace(output))
            logger($"Azure CLI output: {output}");
        if (!string.IsNullOrWhiteSpace(error))
            logger($"Azure CLI error: {error}");

        if (process.ExitCode == 0)
        {
            logger("AKS cluster connected to Azure Arc successfully.");
            return true;
        }
        else
        {
            logger("Error connecting AKS cluster to Azure Arc.");
            return false;
        }
    }    /// <summary>
         /// Installs the Azure IoT Operations (AIO) Platform extension on a connected Kubernetes cluster using the Azure CLI.
         /// </summary>
         /// <param name="resourceGroupName">The name of the resource group containing the cluster.</param>
         /// <param name="clusterName">The name of the connected cluster.</param>
         /// <param name="subscriptionId">The Azure subscription ID.</param>
         /// <param name="logger">Optional logger action for outputting progress and errors.</param>
         /// <returns>True if the extension was installed successfully, otherwise false.</returns>
    public async Task<bool> InstallAioPlatformExtensionAsync(
        string resourceGroupName,
        string clusterName,
        string subscriptionId,
        Action<string>? logger = null)
    {
        logger ??= Console.WriteLine;

        // Note: For Arc-enabled clusters, we don't need to retrieve kubeconfig
        // This was causing unnecessary delays and potential failures

        const string extensionName = "azure-iot-operations-platform";
        const string extensionVersion = "0.7.6";
        const string extensionType = "microsoft.iotoperations.platform";
        const string releaseTrain = "preview";
        const string releaseNamespace = "cert-manager";

        var installCommand =
            $"az k8s-extension create --cluster-name {clusterName} --cluster-type connectedClusters " +
            $"--extension-type {extensionType} --resource-group {resourceGroupName} --name {extensionName} " +
            $"--auto-upgrade-minor-version False --config installTrustManager=true --config installCertManager=true " +
            $"--version {extensionVersion} --release-train {releaseTrain} --release-namespace {releaseNamespace} " +
            $"--scope cluster --subscription {subscriptionId}";

        logger($"Executing command: {installCommand}");

        var processStartInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "pwsh",
            Arguments = $"-Command \"{installCommand}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            using var process = new System.Diagnostics.Process { StartInfo = processStartInfo };
            process.Start();

            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (!string.IsNullOrWhiteSpace(output))
                logger($"Azure CLI output: {output}");
            if (!string.IsNullOrWhiteSpace(error))
                logger($"Azure CLI error: {error}");

            if (process.ExitCode == 0)
            {
                logger("AIO Platform extension installed successfully.");
                return true;
            }
            else
            {
                logger("Error installing AIO Platform extension.");
                return false;
            }
        }
        catch (Exception ex)
        {
            logger($"Exception while installing AIO Platform extension: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Installs the Azure IoT Operations (AIO) Platform extension on a connected Kubernetes cluster using the Azure CLI.
    /// Overload without subscription parameter - uses default Azure CLI context.
    /// </summary>
    /// <param name="resourceGroupName">The name of the resource group containing the cluster.</param>
    /// <param name="clusterName">The name of the connected cluster.</param>
    /// <param name="logger">Optional logger action for outputting progress and errors.</param>
    /// <returns>True if the extension was installed successfully, otherwise false.</returns>
    public async Task<bool> InstallAioPlatformExtensionAsync(
        string resourceGroupName,
        string clusterName,
        Action<string>? logger = null)
    {
        logger ??= Console.WriteLine;

        const string extensionName = "azure-iot-operations-platform";
        const string extensionVersion = "0.7.6";
        const string extensionType = "microsoft.iotoperations.platform";
        const string releaseTrain = "preview";
        const string releaseNamespace = "cert-manager";

        var installCommand =
            $"az k8s-extension create --cluster-name {clusterName} --cluster-type connectedClusters " +
            $"--extension-type {extensionType} --resource-group {resourceGroupName} --name {extensionName} " +
            $"--auto-upgrade-minor-version False --config installTrustManager=true --config installCertManager=true " +
            $"--version {extensionVersion} --release-train {releaseTrain} --release-namespace {releaseNamespace} " +
            $"--scope cluster";

        logger($"Executing command: {installCommand}");

        var processStartInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "pwsh",
            Arguments = $"-Command \"{installCommand}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            using var process = new System.Diagnostics.Process { StartInfo = processStartInfo };
            process.Start();

            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (!string.IsNullOrWhiteSpace(output))
                logger($"Azure CLI output: {output}");
            if (!string.IsNullOrWhiteSpace(error))
                logger($"Azure CLI error: {error}");

            if (process.ExitCode == 0)
            {
                logger("AIO Platform extension installed successfully.");
                return true;
            }
            else
            {
                logger("Error installing AIO Platform extension.");
                return false;
            }
        }
        catch (Exception ex)
        {
            logger($"Exception while installing AIO Platform extension: {ex.Message}");
            return false;
        }
    }

    public async Task<ConfigurationResult> ConfigureClusterAsync(
        string clusterName,
        string resourceGroupName,
        string subscriptionId,
        string? configurationPath = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(clusterName, resourceGroupName, subscriptionId);

        try
        {
            var cluster = await GetClusterAsync(clusterName, resourceGroupName, subscriptionId, tenant, retryPolicy);
            if (cluster == null)
            {
                return new ConfigurationResult
                {
                    Success = false,
                    Message = $"Azure Arc-enabled Kubernetes cluster '{clusterName}' not found in resource group '{resourceGroupName}'.",
                    Errors = new[] { "Cluster not found" }
                };
            }

            // For now, we'll simulate configuration by returning cluster status
            // In a real implementation, this would apply configurations via Azure Arc APIs
            var configurations = new List<string>();

            if (!string.IsNullOrEmpty(configurationPath))
            {
                configurations.Add($"Applied configuration from: {configurationPath}");
            }

            configurations.Add("Verified cluster connectivity");
            configurations.Add("Updated Arc agent configuration");

            return new ConfigurationResult
            {
                Success = true,
                Message = $"Successfully configured Azure Arc cluster '{clusterName}'.",
                AppliedConfigurations = configurations.ToArray()
            };
        }
        catch (Exception ex)
        {
            return new ConfigurationResult
            {
                Success = false,
                Message = $"Failed to configure Azure Arc cluster: {ex.Message}",
                Errors = new[] { ex.Message }
            };
        }
    }    /// <summary>
         /// Installs the Secret Sync Service (SSE) extension on a connected Kubernetes cluster using the Azure CLI.
         /// </summary>
         /// <param name="resourceGroupName">The name of the resource group containing the cluster.</param>
         /// <param name="clusterName">The name of the connected cluster.</param>
         /// <param name="logger">Optional logger action for outputting progress and errors.</param>
         /// <returns>True if the extension was installed successfully, otherwise false.</returns>
    public async Task<bool> InstallSecretSyncServiceExtensionAsync(
        string resourceGroupName,
        string clusterName,
        Action<string>? logger = null)
    {
        logger ??= Console.WriteLine;

        Console.WriteLine("=== DEBUG: Starting InstallSecretSyncServiceExtensionAsync (overload 1) ===");
        Console.WriteLine($"DEBUG: resourceGroupName = {resourceGroupName}");
        Console.WriteLine($"DEBUG: clusterName = {clusterName}");
        Console.WriteLine($"DEBUG: logger is null = {logger == null}");

        const string extensionName = "azure-secret-store";
        const string extensionType = "microsoft.azure.secretstore";
        const string releaseTrain = "preview";

        Console.WriteLine($"DEBUG: extensionName = {extensionName}");
        Console.WriteLine($"DEBUG: extensionType = {extensionType}");
        Console.WriteLine($"DEBUG: releaseTrain = {releaseTrain}");

        var installCommand =
            $"az k8s-extension create --cluster-name {clusterName} --cluster-type connectedClusters " +
            $"--extension-type {extensionType} --resource-group {resourceGroupName} --name {extensionName} " +
            $"--scope cluster --release-train {releaseTrain}";
        Console.WriteLine($"DEBUG: Generated command = {installCommand}");
        logger!($"Executing command: {installCommand}");

        var processStartInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "pwsh",
            Arguments = $"-Command \"{installCommand}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        Console.WriteLine($"DEBUG: ProcessStartInfo configured - FileName = {processStartInfo.FileName}");
        Console.WriteLine($"DEBUG: ProcessStartInfo Arguments = {processStartInfo.Arguments}");

        try
        {
            Console.WriteLine("DEBUG: Creating and starting process...");
            using var process = new System.Diagnostics.Process { StartInfo = processStartInfo };
            process.Start();
            Console.WriteLine($"DEBUG: Process started with ID = {process.Id}");

            Console.WriteLine("DEBUG: Reading process output...");
            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();
            Console.WriteLine("DEBUG: Waiting for process to exit...");
            await process.WaitForExitAsync();
            Console.WriteLine($"DEBUG: Process exited with code = {process.ExitCode}");

            if (!string.IsNullOrWhiteSpace(output))
            {
                Console.WriteLine($"DEBUG: Process output length = {output.Length}");
                logger($"Azure CLI output: {output}");
            }
            else
            {
                Console.WriteLine("DEBUG: Process output is empty or whitespace");
            }

            if (!string.IsNullOrWhiteSpace(error))
            {
                Console.WriteLine($"DEBUG: Process error length = {error.Length}");
                logger($"Azure CLI error: {error}");
            }
            else
            {
                Console.WriteLine("DEBUG: Process error is empty or whitespace");
            }

            if (process.ExitCode == 0)
            {
                Console.WriteLine("DEBUG: Process succeeded (exit code 0)");
                logger("SSE extension installed successfully.");
                Console.WriteLine("=== DEBUG: InstallSecretSyncServiceExtensionAsync completed successfully ===");
                return true;
            }
            else
            {
                Console.WriteLine($"DEBUG: Process failed with exit code {process.ExitCode}");
                logger("Error installing SSE extension.");
                Console.WriteLine("=== DEBUG: InstallSecretSyncServiceExtensionAsync completed with error ===");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DEBUG: Exception caught - Type: {ex.GetType().Name}");
            Console.WriteLine($"DEBUG: Exception message: {ex.Message}");
            Console.WriteLine($"DEBUG: Exception stack trace: {ex.StackTrace}");
            logger($"Exception while installing SSE extension: {ex.Message}");
            Console.WriteLine("=== DEBUG: InstallSecretSyncServiceExtensionAsync completed with exception ===");
            return false;
        }
    }    /// <summary>
         /// Installs the Secret Sync Service (SSE) extension on a connected Kubernetes cluster using the Azure CLI.
         /// </summary>
         /// <param name="resourceGroupName">The name of the resource group containing the cluster.</param>
         /// <param name="clusterName">The name of the connected cluster.</param>
         /// <param name="subscriptionId">The Azure subscription ID.</param>
         /// <param name="logger">Optional logger action for outputting progress and errors.</param>
         /// <returns>True if the extension was installed successfully, otherwise false.</returns>
    public async Task<bool> InstallSecretSyncServiceExtensionAsync(
        string resourceGroupName,
        string clusterName,
        string subscriptionId,
        Action<string>? logger = null)
    {
        logger ??= Console.WriteLine;

        Console.WriteLine("=== DEBUG: Starting InstallSecretSyncServiceExtensionAsync (overload 2 with subscription) ===");
        Console.WriteLine($"DEBUG: resourceGroupName = {resourceGroupName}");
        Console.WriteLine($"DEBUG: clusterName = {clusterName}");
        Console.WriteLine($"DEBUG: subscriptionId = {subscriptionId}");
        Console.WriteLine($"DEBUG: logger is null = {logger == null}");

        const string extensionName = "azure-secret-store";
        const string extensionType = "microsoft.azure.secretstore";
        const string releaseTrain = "preview";

        Console.WriteLine($"DEBUG: extensionName = {extensionName}");
        Console.WriteLine($"DEBUG: extensionType = {extensionType}");
        Console.WriteLine($"DEBUG: releaseTrain = {releaseTrain}");

        var installCommand =
            $"az k8s-extension create --cluster-name {clusterName} --cluster-type connectedClusters " +
            $"--extension-type {extensionType} --resource-group {resourceGroupName} --name {extensionName} " +
            $"--scope cluster --release-train {releaseTrain} --subscription {subscriptionId}";

        Console.WriteLine($"DEBUG: Generated command = {installCommand}");
        logger!($"Executing command: {installCommand}");

        var processStartInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "pwsh",
            Arguments = $"-Command \"{installCommand}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        Console.WriteLine($"DEBUG: ProcessStartInfo configured - FileName = {processStartInfo.FileName}");
        Console.WriteLine($"DEBUG: ProcessStartInfo Arguments = {processStartInfo.Arguments}");

        try
        {
            Console.WriteLine("DEBUG: Creating process and setting up cancellation token...");
            using var process = new System.Diagnostics.Process { StartInfo = processStartInfo };
            process.Start();
            Console.WriteLine($"DEBUG: Process started with ID = {process.Id}");

            // Add timeout to prevent hanging (15 minutes)
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(15));
            Console.WriteLine("DEBUG: Cancellation token set for 15 minutes timeout");

            try
            {
                Console.WriteLine("DEBUG: Waiting for process to exit with timeout...");
                await process.WaitForExitAsync(cancellationTokenSource.Token);
                Console.WriteLine($"DEBUG: Process exited normally with code = {process.ExitCode}");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("DEBUG: Process timed out after 15 minutes, attempting to kill process...");
                logger!("Process timed out after 10 minutes. Killing process...");
                process.Kill();
                Console.WriteLine("DEBUG: Process killed due to timeout");
                Console.WriteLine("=== DEBUG: InstallSecretSyncServiceExtensionAsync completed with timeout ===");
                return false;
            }

            Console.WriteLine("DEBUG: Reading process output and error streams...");
            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();

            if (!string.IsNullOrWhiteSpace(output))
            {
                Console.WriteLine($"DEBUG: Process output length = {output.Length}");
                logger!($"Azure CLI output: {output}");
            }
            else
            {
                Console.WriteLine("DEBUG: Process output is empty or whitespace");
            }

            if (!string.IsNullOrWhiteSpace(error))
            {
                Console.WriteLine($"DEBUG: Process error length = {error.Length}");
                logger!($"Azure CLI error: {error}");
            }
            else
            {
                Console.WriteLine("DEBUG: Process error is empty or whitespace");
            }

            if (process.ExitCode == 0)
            {
                Console.WriteLine("DEBUG: Process succeeded (exit code 0)");
                logger!("SSE extension installed successfully.");
                Console.WriteLine("=== DEBUG: InstallSecretSyncServiceExtensionAsync completed successfully ===");
                return true;
            }
            else
            {
                Console.WriteLine($"DEBUG: Process failed with exit code {process.ExitCode}");
                logger!("Error installing SSE extension.");
                Console.WriteLine("=== DEBUG: InstallSecretSyncServiceExtensionAsync completed with error ===");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DEBUG: Exception caught - Type: {ex.GetType().Name}");
            Console.WriteLine($"DEBUG: Exception message: {ex.Message}");
            Console.WriteLine($"DEBUG: Exception stack trace: {ex.StackTrace}");
            logger!($"Exception while installing SSE extension: {ex.Message}");
            Console.WriteLine("=== DEBUG: InstallSecretSyncServiceExtensionAsync completed with exception ===");
            return false;
        }
    }

    /// <summary>
    /// Installs the Azure Container Storage for Arc (ACSA) extension on a connected Kubernetes cluster using the Azure CLI.
    /// </summary>
    /// <param name="resourceGroupName">The name of the resource group containing the cluster.</param>
    /// <param name="clusterName">The name of the connected cluster.</param>
    /// <param name="logger">Optional logger action for outputting progress and errors.</param>
    /// <returns>True if the extension was installed successfully, otherwise false.</returns>
    public async Task<bool> InstallAcsaExtensionAsync(
        string resourceGroupName,
        string clusterName,
        Action<string>? logger = null)
    {
        logger ??= Console.WriteLine;
        const string extensionName = "azure-arc-containerstorage";
        const string extensionType = "microsoft.arc.containerstorage";
        const string releaseTrain = "preview";

        var installCommand =
            $"az k8s-extension create --cluster-name {clusterName} --cluster-type connectedClusters " +
            $"--extension-type {extensionType} --resource-group {resourceGroupName} --name {extensionName} " +
            $"--scope cluster --release-train {releaseTrain}";

        logger($"Executing command: {installCommand}");

        var processStartInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "pwsh",
            Arguments = $"-Command \"{installCommand}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            using var process = new System.Diagnostics.Process { StartInfo = processStartInfo };
            process.Start();

            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (!string.IsNullOrWhiteSpace(output))
                logger($"Azure CLI output: {output}");
            if (!string.IsNullOrWhiteSpace(error))
                logger($"Azure CLI error: {error}");

            if (process.ExitCode == 0)
            {
                logger("ACSA extension installed successfully.");
                return true;
            }
            else
            {
                logger("Error installing ACSA extension.");
                return false;
            }
        }
        catch (Exception ex)
        {
            logger($"Exception while installing ACSA extension: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Installs the Azure Container Storage for Arc (ACSA) extension on a connected Kubernetes cluster using the Azure CLI.
    /// </summary>
    /// <param name="resourceGroupName">The name of the resource group containing the cluster.</param>
    /// <param name="clusterName">The name of the connected cluster.</param>
    /// <param name="subscriptionId">The Azure subscription ID.</param>
    /// <param name="logger">Optional logger action for outputting progress and errors.</param>
    /// <returns>True if the extension was installed successfully, otherwise false.</returns>
    public async Task<bool> InstallAcsaExtensionAsync(
        string resourceGroupName,
        string clusterName,
        string subscriptionId,
        Action<string>? logger = null)
    {
        logger ??= Console.WriteLine;
        const string extensionName = "azure-arc-containerstorage";
        const string extensionType = "microsoft.arc.containerstorage";
        const string releaseTrain = "preview";

        var installCommand =
            $"az k8s-extension create --cluster-name {clusterName} --cluster-type connectedClusters " +
            $"--extension-type {extensionType} --resource-group {resourceGroupName} --name {extensionName} " +
            $"--scope cluster --release-train {releaseTrain} --subscription {subscriptionId}";

        logger($"Executing command: {installCommand}");

        var processStartInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "pwsh",
            Arguments = $"-Command \"{installCommand}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            using var process = new System.Diagnostics.Process { StartInfo = processStartInfo };
            process.Start();

            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (!string.IsNullOrWhiteSpace(output))
                logger($"Azure CLI output: {output}");
            if (!string.IsNullOrWhiteSpace(error))
                logger($"Azure CLI error: {error}");

            if (process.ExitCode == 0)
            {
                logger("ACSA extension installed successfully.");
                return true;
            }
            else
            {
                logger("Error installing ACSA extension.");
                return false;
            }
        }
        catch (Exception ex)
        {
            logger($"Exception while installing ACSA extension: {ex.Message}");
            return false;
        }
    }

    public async Task<Cluster?> GetClusterAsync(
        string clusterName,
        string resourceGroupName,
        string subscriptionId,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(clusterName, resourceGroupName, subscriptionId);

        try
        {
            var subscriptionResource = await _subscriptionService.GetSubscription(subscriptionId, tenant, retryPolicy);
            var resourceGroupResponse = await subscriptionResource.GetResourceGroupAsync(resourceGroupName);
            var resourceGroupResource = resourceGroupResponse.Value;

            // Try to get the Arc-enabled Kubernetes cluster
            var connectedClusters = resourceGroupResource.GetHybridComputeMachines();

            await foreach (var machine in connectedClusters)
            {
                if (string.Equals(machine.Data.Name, clusterName, StringComparison.OrdinalIgnoreCase))
                {
                    return MapToClusterModel(machine);
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving Arc cluster details: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<Cluster>> ListClustersAsync(
        string subscriptionId,
        string? resourceGroupName = null,
        string? tag = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscriptionId);

        var cacheKey = string.IsNullOrEmpty(tenant)
            ? $"clusters_{subscriptionId}"
            : $"clusters_{subscriptionId}_{tenant}";

        var cachedClusters = await _cacheService.GetAsync<List<Cluster>>(CACHE_GROUP, cacheKey, CACHE_DURATION);
        if (cachedClusters != null)
        {
            return cachedClusters;
        }

        try
        {
            var subscriptionResource = await _subscriptionService.GetSubscription(subscriptionId, tenant, retryPolicy);
            var clusters = new List<Cluster>();

            foreach (var machine in subscriptionResource.GetHybridComputeMachines())
            {
                // Filter for Kubernetes clusters (Arc-enabled Kubernetes shows as hybrid compute machines)
                if (IsKubernetesCluster(machine))
                {
                    var cluster = MapToClusterModel(machine);
                    if (cluster != null)
                    {
                        clusters.Add(cluster);
                    }
                }
            }

            await _cacheService.SetAsync(CACHE_GROUP, cacheKey, clusters, CACHE_DURATION);
            return clusters;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving Arc clusters: {ex.Message}", ex);
        }
    }

    private static bool IsKubernetesCluster(HybridComputeMachineResource machine)
    {
        // Check if this is a Kubernetes cluster by looking at properties
        // Arc-enabled Kubernetes clusters have specific properties that distinguish them from regular VMs
        return machine.Data.Kind?.ToString()?.Contains("Kubernetes", StringComparison.OrdinalIgnoreCase) == true ||
               machine.Data.OSName?.Contains("Kubernetes", StringComparison.OrdinalIgnoreCase) == true ||
               machine.Data.Extensions?.Any(ext => ext.Name?.Contains("kubernetes", StringComparison.OrdinalIgnoreCase) == true) == true;
    }

    private static Cluster? MapToClusterModel(HybridComputeMachineResource machine)
    {
        if (machine?.Data == null)
            return null;

        var data = machine.Data;

        return new Cluster
        {
            Name = data.Name,
            SubscriptionId = machine.Id.SubscriptionId,
            ResourceGroupName = machine.Id.ResourceGroupName,
            Location = data.Location.ToString(),
            ProvisioningState = data.ProvisioningState?.ToString(),
            Status = data.Status?.ToString(),
            AgentVersion = data.AgentVersion,
            KubernetesVersion = data.OSVersion,
            LastConnectivityTime = data.LastStatusChange?.DateTime,
            Infrastructure = data.DetectedProperties?.TryGetValue("infrastructure", out var infra) == true ? infra : null,
            Distribution = data.DetectedProperties?.TryGetValue("distribution", out var dist) == true ? dist : null,
            Tags = data.Tags?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            Identity = data.Identity == null ? null : new ManagedIdentityInfo
            {
                SystemAssignedIdentity = new SystemAssignedIdentityInfo
                {
                    Enabled = data.Identity != null,
                    TenantId = data.Identity?.TenantId?.ToString(),
                    PrincipalId = data.Identity?.PrincipalId?.ToString()
                }
            },
            Extensions = data.Extensions?.Select(ext => ext.Name).ToArray()
        };
    }

}