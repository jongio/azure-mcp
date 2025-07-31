// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Core.Areas;
using AzureMcp.Core.Commands;
using AzureMcp.Deploy.Commands;
using AzureMcp.Deploy.Commands.InfraCodeRules;
using AzureMcp.Deploy.Commands.Plan;
using AzureMcp.Deploy.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Deploy;

public sealed class DeploySetup : IAreaSetup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IDeployService, DeployService>();
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        var deploy = new CommandGroup("deploy", "Deploy commands for deploying applications to Azure");
        rootGroup.AddSubGroup(deploy);

        deploy.AddCommand("plan-get", new PlanGetCommand(loggerFactory.CreateLogger<PlanGetCommand>()));

        deploy.AddCommand("iac-rules-get", new IaCRulesGetCommand(loggerFactory.CreateLogger<IaCRulesGetCommand>()));

        deploy.AddCommand("azd-app-log-get", new AzdAppLogGetCommand(loggerFactory.CreateLogger<AzdAppLogGetCommand>()));

        deploy.AddCommand("cicd-pipeline-guidance-get", new PipelineGenerateCommand(loggerFactory.CreateLogger<PipelineGenerateCommand>()));

        deploy.AddCommand("architecture-diagram-generate", new GenerateArchitectureDiagramCommand(loggerFactory.CreateLogger<GenerateArchitectureDiagramCommand>()));
    }
}
