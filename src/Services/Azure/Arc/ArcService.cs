using System.IO;
using System.Threading.Tasks;
using AzureMcp.Services.Interfaces;
using System.Reflection;
using AzureMcp.Helpers;
using AzureMcp.Services.Azure.Tenant;
using AzureMcp.Options;
using System.Diagnostics;

namespace AzureMcp.Services.Azure.Arc
{
    public class ArcService : BaseAzureService, IArcService
    {
        private const string EdgeEssentialsDeploymentSteps = "AzureMcp.Resources.aks_edge_essentials_steps.txt";
        private const string PrerequisitesAksEdgeInstallationSteps = "AzureMcp.Resources.prerequisites_aksee_installation.txt";
        private const string RemoveAksEdgeClusterScript = "AzureMcp.Resources.RemoveAksEdgeCompletely.ps1";
        private const string ValidateSystemRequirements = "AzureMcp.Resources.ValidateSystemRequirements.ps1";

        private const string ConfirmAksEdgeClusterRemovalScript = "AzureMcp.Resources.ConfirmAksEdgeDeletion.ps1";
        private readonly Assembly _assembly;

        public ArcService(ITenantService? tenantService = null) : base(tenantService)
        {
            _assembly = typeof(ArcService).Assembly;
        }

        public async Task<DeploymentResult> DeployAksEdgeEssentialClusterAsync()
        {
            string deploymentSteps = await Task.Run(() => LoadResourceFiles(EdgeEssentialsDeploymentSteps));
            return new DeploymentResult
            {
                Success = true,
                Steps = deploymentSteps
            };
        }

        public async Task<bool> DeployAksClusterToArcAsync(string resourceGroup, string clusterName, string location)
        {
            await Task.Delay(0); // Placeholder for async operation
            return false;
        }


        public async Task<bool> ConnectClusterToArcAsync(string clusterName, string resourceGroupName, string location, string subscriptionId, string tenantId)
        {
            try
            {
                // Validate inputs
                ArgumentException.ThrowIfNullOrEmpty(clusterName);
                ArgumentException.ThrowIfNullOrEmpty(resourceGroupName);
                ArgumentException.ThrowIfNullOrEmpty(location);
                ArgumentException.ThrowIfNullOrEmpty(subscriptionId);
                ArgumentException.ThrowIfNullOrEmpty(tenantId);

                Console.WriteLine("Connecting AKS Edge Essentials cluster to Azure Arc...");

                // Step 1: Check kubeconfig and context
                string kubeConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".kube", "config");
                if (string.IsNullOrEmpty(kubeConfigPath))
                {
                    throw new InvalidOperationException("Kubeconfig path is invalid.");
                }

                if (!File.Exists(kubeConfigPath))
                {
                    Console.WriteLine("Kubeconfig not found. Attempting to retrieve...");
                    string aksEdgeModulePath = "C:\\Program Files\\WindowsPowerShell\\Modules\\AksEdge";
                    string aksEdgeVersion = Directory.GetDirectories(aksEdgeModulePath)?.OrderByDescending(d => d).FirstOrDefault() ?? string.Empty;
                    if (string.IsNullOrEmpty(aksEdgeVersion))
                    {
                        throw new InvalidOperationException("AksEdge module not found. Please ensure it is installed.");
                    }

                    string aksEdgePsModulePath = Path.Combine(aksEdgeModulePath, aksEdgeVersion, "AksEdge.psd1");
                    PowerShellHelper.ImportModule(aksEdgePsModulePath);

                    Directory.CreateDirectory(Path.GetDirectoryName(kubeConfigPath) ?? throw new InvalidOperationException("Failed to create kubeconfig directory."));
                    PowerShellHelper.ExecuteCommand("Get-AksEdgeKubeConfig", new { KubeConfigPath = kubeConfigPath });
                }

                string currentContext = await PowerShellHelper.ExecuteCommandAsync("kubectl config current-context", new { kubeconfig = kubeConfigPath });
                if (string.IsNullOrEmpty(currentContext))
                {
                    Console.WriteLine("No current context set in kubeconfig.");
                }
                else
                {
                    Console.WriteLine($"Current context: {currentContext}");
                }

                // Step 2: Verify cluster access
                string nodes = await PowerShellHelper.ExecuteCommandAsync("kubectl get nodes", new { kubeconfig = kubeConfigPath });
                if (string.IsNullOrEmpty(nodes))
                {
                    throw new InvalidOperationException("Cannot access cluster. Please check your AKS Edge deployment.");
                }

                Console.WriteLine("Cluster access verified.");

                // Step 3: Azure CLI setup
                var azAccount = await PowerShellHelper.ExecuteCommandAsync("az account show");
                if (string.IsNullOrEmpty(azAccount))
                {
                    await PowerShellHelper.ExecuteCommandAsync("az login");
                    azAccount = await PowerShellHelper.ExecuteCommandAsync("az account show");
                }

                Console.WriteLine("Azure CLI logged in.");
                await PowerShellHelper.ExecuteCommandAsync("az account set", new { subscription = subscriptionId });

                // Step 4: Clean up existing Arc connections
                var existingClusters = await PowerShellHelper.ExecuteCommandAsync("az connectedk8s list", new { resourceGroup = resourceGroupName });
                if (!string.IsNullOrEmpty(existingClusters))
                {
                    Console.WriteLine("Found existing Arc clusters.");
                    await PowerShellHelper.ExecuteCommandAsync("az connectedk8s delete", new { name = clusterName, resourceGroup = resourceGroupName, yes = true });
                }

                // Step 5: Connect to Azure Arc
                Console.WriteLine("Connecting to Azure Arc...");
                await PowerShellHelper.ExecuteCommandAsync("az connectedk8s connect", new
                {
                    name = clusterName,
                    resourceGroup = resourceGroupName,
                    location = location,
                    kubeConfig = kubeConfigPath,
                    tags = "CreatedBy=AKSEdge,Environment=Development"
                });

                Console.WriteLine("Azure Arc connection successful.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during Azure Arc connection: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> RemoveAksEdgeAsync()
        {
            try
            {
                // Extract the embedded script to a temporary file
                string tempScriptPath = Path.Combine(Path.GetTempPath(), "RemoveAksEdgeCompletely.ps1");
                File.WriteAllText(tempScriptPath, LoadResourceFiles(RemoveAksEdgeClusterScript));

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-ExecutionPolicy Bypass -File \"{tempScriptPath}\" -Force",
                    UseShellExecute = true,
                    Verb = "runas"
                };

                using var process = StartProcess(tempScriptPath, processStartInfo);
                await process.WaitForExitAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during AKS Edge removal: {ex.Message}");
            }

            try
            {
                // Extract the confirmation script to a temporary file
                string tempConfirmScriptPath = Path.Combine(Path.GetTempPath(), "ConfirmAksEdgeDeletion.ps1");
                File.WriteAllText(tempConfirmScriptPath, LoadResourceFiles(ConfirmAksEdgeClusterRemovalScript));

                var confirmProcessStartInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-ExecutionPolicy Bypass -File \"{tempConfirmScriptPath}\" -Force",
                    UseShellExecute = true,
                    Verb = "runas"
                };

                using var confirmProcess = StartProcess(tempConfirmScriptPath, confirmProcessStartInfo);
                await confirmProcess.WaitForExitAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during confirmation script execution: {ex.Message}");
            }

            return true;
        }

        /** public string LoadDeploymentSteps()
         {
             return EmbeddedResourceHelper.ReadEmbeddedResource(_assembly, ResourceName);
         }**/
        public string LoadResourceFiles(string resourceName)
        {
            return EmbeddedResourceHelper.ReadEmbeddedResource(_assembly, resourceName);
        }

        public async Task<DeploymentResult> ValidatePrerequisitesForAksEdgeClusterAsync()
        {
            string prerequisiteSteps = await Task.Run(() => LoadResourceFiles(PrerequisitesAksEdgeInstallationSteps));
            return new DeploymentResult
            {
                Success = true,
                Steps = prerequisiteSteps
            };
        }

        public async Task<DeploymentResult> ValidateSystemRequirementsAndSetupHyperVAsync()
        {
            // Extract the confirmation script to a temporary file
            string tempConfirmScriptPath = Path.Combine(Path.GetTempPath(), "ValidateSystemRequirements.ps1");
            File.WriteAllText(tempConfirmScriptPath, LoadResourceFiles(ValidateSystemRequirements));

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-ExecutionPolicy Bypass -File \"{tempConfirmScriptPath}\" -Force",
                UseShellExecute = true,
                Verb = "runas"
            };

            using var process = StartProcess(tempConfirmScriptPath, processStartInfo);
            if (process == null)
            {
                throw new InvalidOperationException("Failed to start the system requirements validation script.");
            }

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"Script execution failed with exit code {process.ExitCode}.");
            }

            return new DeploymentResult
            {
                Success = true,
                Steps = "System requirements validated and Hyper-V setup successfully."
            };
        }

        public Process StartProcess(string scriptPath, ProcessStartInfo processInfo)
        {
            if (!File.Exists(scriptPath))
            {
                throw new FileNotFoundException("The specified script file was not found.", scriptPath);
            }

            var process = Process.Start(processInfo);
            if (process == null)
            {
                throw new InvalidOperationException("Failed to start the process.");
            }

            return process;
        }
    }

    public class DeploymentResult
    {
        public bool Success { get; set; }
        public string Steps { get; set; } = string.Empty;
        public string OutputPath { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
