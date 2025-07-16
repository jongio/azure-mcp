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
        private const string PrerequisitesAksEdgeInstallation = "AzureMcp.Resources.Arc.PrerequisitesAksEdgeInstallation.txt";
        private const string RemoveAksEdgeCompletely = "AzureMcp.Resources.Arc.RemoveAksEdgeCompletely.ps1";
        private const string ValidateAndSetupSystemRequirements = "AzureMcp.Resources.Arc.ValidateAndSetupSystemRequirements.ps1";
        private const string ValidateAndSetupSwRequirementScript = "AzureMcp.Resources.Arc.ValidateAndSetupSoftwarerequirements.ps1";
        private const string AksEdgeQuickDeploy = "AzureMcp.Resources.Arc.AksEdgeQuickDeploy.ps1";
        private const string DisconnectAzureArc = "AzureMcp.Resources.Arc.DisconnectAzureArc.ps1";
        private const string ConfirmAksEdgeDeletion = "AzureMcp.Resources.Arc.ConfirmAksEdgeDeletion.ps1";
        private readonly Assembly _assembly;

        public ArcService(ITenantService? tenantService = null) : base(tenantService)
        {
            _assembly = typeof(ArcService).Assembly;
        }

        public async Task<bool> OnboardClusterToArcAsync(string clusterName, string resourceGroupName, string location, string subscriptionId, string tenantId, string kubeConfigPath)
        {
            try
            {

                Console.WriteLine("Connecting AKS Edge Essentials cluster to Azure Arc...");

                // Step 1: Check kubeconfig and context
                kubeConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".kube", "config");
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
        public async Task<bool> RemoveAksEdgeAsync(string userProvidedPath)
        {
            try
            {
                // Extract the embedded script to a temporary file
                string tempScriptPath = Path.Combine(userProvidedPath, "RemoveAksEdgeCompletely.ps1");
                File.WriteAllText(tempScriptPath, LoadResourceFiles(RemoveAksEdgeCompletely));

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
                string tempConfirmScriptPath = Path.Combine(userProvidedPath, "ConfirmAksEdgeDeletion.ps1");
                File.WriteAllText(tempConfirmScriptPath, LoadResourceFiles(ConfirmAksEdgeDeletion));

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
        public string LoadResourceFiles(string resourceName)
        {
            return EmbeddedResourceHelper.ReadEmbeddedResource(_assembly, resourceName);
        }

        public async Task<DeploymentResult> ValidatePrerequisitesForAksEdgeClusterAsync()
        {
            string prerequisiteSteps = await Task.Run(() => LoadResourceFiles(PrerequisitesAksEdgeInstallation));
            return new DeploymentResult
            {
                Success = true,
                Steps = prerequisiteSteps
            };
        }

        public async Task<DeploymentResult> ValidateSystemRequirementsAndSetupHyperVAsync(string userProvidedPath)
        {
            // Ensure the directory exists
            if (!Directory.Exists(userProvidedPath))
            {
                Directory.CreateDirectory(userProvidedPath);
            }

            string tempScriptPath = Path.Combine(userProvidedPath, "ValidateAndSetupSystemRequirements.ps1");
            File.WriteAllText(tempScriptPath, LoadResourceFiles(ValidateAndSetupSystemRequirements));

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-ExecutionPolicy Bypass -File \"{tempScriptPath}\" -Force",
                UseShellExecute = true,
                Verb = "runas"
            };

            using var process = StartProcess(tempScriptPath, processStartInfo);
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

        public async Task<DeploymentResult> ValidateAndInstallSwRequirementAsync(string userProvidedPath)
        {
            // Ensure the directory exists
            if (!Directory.Exists(userProvidedPath))
            {
                Directory.CreateDirectory(userProvidedPath);
            }

            string tempScriptPath = Path.Combine(userProvidedPath, "ValidateAndSetupSoftwarerequirements.ps1");
            File.WriteAllText(tempScriptPath, LoadResourceFiles(ValidateAndSetupSwRequirementScript));

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-ExecutionPolicy Bypass -File \"{tempScriptPath}\" -Force",
                UseShellExecute = true,
                Verb = "runas"
            };

            using var process = StartProcess(tempScriptPath, processStartInfo);
            if (process == null)
            {
                throw new InvalidOperationException("Failed to start the software requirements validation and installation script.");
            }

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"Script execution failed with exit code {process.ExitCode}.");
            }

            return new DeploymentResult
            {
                Success = true,
                Steps = "Software requirements validated and installed successfully."
            };
        }

        public async Task<DeploymentResult> QuickDeployAksEdgeEssentialsAsync(string clusterName, string resourceGroupName, string subscriptionId, string tenantId, string location, string userProvidedPath)
        {
            // Ensure the directory exists
            if (!Directory.Exists(userProvidedPath))
            {
                Directory.CreateDirectory(userProvidedPath);
            }

            string tempScriptPath = Path.Combine(userProvidedPath, "AksEdgeQuickDeploy.ps1");
            File.WriteAllText(tempScriptPath, LoadResourceFiles(AksEdgeQuickDeploy));

            var arguments = $"-ExecutionPolicy Bypass -File \"{tempScriptPath}\" -Force";

            if (!string.IsNullOrEmpty(clusterName))
                arguments += $" -ClusterName {clusterName}";
            if (!string.IsNullOrEmpty(resourceGroupName))
                arguments += $" -ResourceGroupName {resourceGroupName}";
            if (!string.IsNullOrEmpty(subscriptionId))
                arguments += $" -SubscriptionId {subscriptionId}";
            if (!string.IsNullOrEmpty(tenantId))
                arguments += $" -TenantId {tenantId}";
            if (!string.IsNullOrEmpty(location))
                arguments += $" -Location {location}";

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = arguments,
                UseShellExecute = true,
                Verb = "runas"
            };

            Process process = StartProcess(tempScriptPath, processStartInfo);
            await Task.Run(() => process.WaitForExit());

            return new DeploymentResult
            {
                Success = process.ExitCode == 0,
                Steps = process.ExitCode == 0 ? "Deployment completed successfully." : "Deployment failed."
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
