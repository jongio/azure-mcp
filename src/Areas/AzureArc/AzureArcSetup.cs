// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Areas.AzureArc.Services;
using AzureMcp.Commands;
using AzureMcp.Commands.Arc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Areas.AzureArc;

public class AzureArcSetup : IAreaSetup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IArcServices, ArcServices>();
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        // Create Azure Arc command group
        var arc = new CommandGroup("arc", "Azure Arc operations - Commands for managing Azure Arc-enabled Kubernetes clusters and AKS Edge Essentials.");
        rootGroup.AddSubGroup(arc);

        // Register Azure Arc commands
        arc.AddCommand("connect-arc", new OnboardClusterToArcCommand(loggerFactory.CreateLogger<OnboardClusterToArcCommand>()));
        arc.AddCommand("disconnect-arc", new DisconnectFromAzureArcCommand(loggerFactory.CreateLogger<DisconnectFromAzureArcCommand>()));
        arc.AddCommand("remove-edge-essentials", new RemoveAksEdgeCommand(loggerFactory.CreateLogger<RemoveAksEdgeCommand>()));
        arc.AddCommand("describe-prereqs-edge-cluster", new ValidatePrerequisitesForAksEdgeClusterCommand(loggerFactory.CreateLogger<ValidatePrerequisitesForAksEdgeClusterCommand>()));
        arc.AddCommand("setup-system-requirement", new ValidateSystemRequirementsAndSetupHyperVCommand(loggerFactory.CreateLogger<ValidateSystemRequirementsAndSetupHyperVCommand>()));
        arc.AddCommand("setup-software-requirement", new ValidateAndInstallSwRequirementCommand(loggerFactory.CreateLogger<ValidateAndInstallSwRequirementCommand>()));
        arc.AddCommand("deploy-edge-essentials", new QuickDeployAksEdgeEssentialsCommand(loggerFactory.CreateLogger<QuickDeployAksEdgeEssentialsCommand>()));
    }
}
