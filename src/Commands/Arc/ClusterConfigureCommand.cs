// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Models.Option;
using AzureMcp.Options.Arc;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Commands.Arc;

public sealed class ClusterConfigureCommand(ILogger<ClusterConfigureCommand> logger)
    : BaseClusterCommand<ClusterConfigureOptions>
{
    private const string _commandTitle = "Configure Azure Arc Kubernetes Cluster";
    private readonly ILogger<ClusterConfigureCommand> _logger = logger;

    private readonly Option<string> _configurationPathOption = OptionDefinitions.Arc.ConfigPath;

    public override string Name => "configure";

    public override string Description =>
        """
        Configure an Azure Arc-enabled Kubernetes cluster with specified settings and configurations.
        This command applies configuration changes and verifies cluster connectivity with Azure Arc.
        Returns configuration status and any applied changes.
          Required options:
        - cluster-name: The name of the Azure Arc-enabled Kubernetes cluster
        - resource-group: The resource group containing the cluster
        - subscription: The Azure subscription ID
          Optional options:
        - configuration-path: Path to the Kubernetes configuration file
        """;

    public override string Title => _commandTitle;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_configurationPathOption);
    }

    protected override ClusterConfigureOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ConfigurationPath = parseResult.GetValueForOption(_configurationPathOption);
        return options;
    }

    [McpServerTool(
        Destructive = true,     // Configuration changes can modify the cluster
        ReadOnly = false,       // This command modifies cluster configuration
        Title = _commandTitle)]
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);

        try
        {
            // Required validation step
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            // Get the Arc service from DI
            var arcService = context.GetService<IArcService>();

            // Configure the cluster
            var result = await arcService.ConfigureClusterAsync(
                options.ClusterName!,
                options.ResourceGroup!,
                options.Subscription!,
                options.ConfigurationPath,
                options.Tenant,
                options.RetryPolicy);

            // Set results based on configuration outcome
            if (result.Success)
            {
                context.Response.Results = ResponseResult.Create(
                    new ClusterConfigureCommandResult(result),
                    ArcJsonContext.Default.ClusterConfigureCommandResult);
            }
            else
            {
                context.Response.Status = 400;
                context.Response.Message = result.Message ?? "Configuration failed";
                context.Response.Results = ResponseResult.Create(
                    new ClusterConfigureCommandResult(result),
                    ArcJsonContext.Default.ClusterConfigureCommandResult);
            }
        }
        catch (Exception ex)
        {
            // Log error with all relevant context
            _logger.LogError(ex,
                "Error configuring Arc cluster. Cluster: {ClusterName}, ResourceGroup: {ResourceGroup}, Options: {@Options}",
                options.ClusterName, options.ResourceGroup, options);
            HandleException(context.Response, ex);
        }

        return context.Response;
    }

    // Implementation-specific error handling
    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException => "Invalid configuration parameters. Please check your cluster name and resource group.",
        UnauthorizedAccessException => "Insufficient permissions to configure the Arc cluster. Verify your Azure RBAC permissions.",
        FileNotFoundException => "Configuration file not found. Please check the configuration path.",
        InvalidOperationException => "Cluster is not in a valid state for configuration. Please check cluster status.",
        _ => base.GetErrorMessage(ex)
    };

    protected override int GetStatusCode(Exception ex) => ex switch
    {
        ArgumentException => 400,
        UnauthorizedAccessException => 403,
        FileNotFoundException => 404,
        InvalidOperationException => 409,
        _ => base.GetStatusCode(ex)
    };

    // Strongly-typed result record
    internal record ClusterConfigureCommandResult(ConfigurationResult Result);
}
