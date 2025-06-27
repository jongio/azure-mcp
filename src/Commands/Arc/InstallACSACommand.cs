using System.CommandLine;
using System.CommandLine.Parsing;
using AzureMcp.Models.Option;
using AzureMcp.Options.Arc;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Commands.Arc;

public sealed class InstallACSACommand(ILogger<InstallACSACommand> logger)
    : BaseClusterCommand<ClusterConnectOptions>
{
    private const string _commandTitle = "Install Azure Container Storage for Arc (ACSA) Extension";
    private readonly ILogger<InstallACSACommand> _logger = logger;

    public override string Name => "install-acsa";
    public override string Description =>
        """
        Install the Azure Container Storage for Arc (ACSA) extension on a connected AKS/Arc cluster.
        Required options:
        - cluster-name: The name of the AKS/Arc cluster
        - resource-group: The resource group containing the cluster
        """;
    public override string Title => _commandTitle;

    [McpServerTool(
        Destructive = false,
        ReadOnly = false,
        Title = _commandTitle)]
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
                return context.Response;
            var arcService = context.GetService<IArcService>();
            var result = await arcService.InstallAcsaExtensionAsync(
                options.ResourceGroup!,
                options.ClusterName!,
                options.Subscription!,
                msg => _logger.LogInformation(msg)
            );

            context.Response.Results = ResponseResult.Create(
                new InstallACSACommandResult(result),
                ArcJsonContext.Default.InstallACSACommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error installing ACSA extension. Cluster: {ClusterName}, ResourceGroup: {ResourceGroup}, Options: {@Options}",
                options.ClusterName, options.ResourceGroup, options);
            HandleException(context.Response, ex);
        }

        return context.Response;
    }

    internal record InstallACSACommandResult(bool Success);
}