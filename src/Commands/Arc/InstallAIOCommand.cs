using System.CommandLine;
using System.CommandLine.Parsing;
using AzureMcp.Models.Option;
using AzureMcp.Options.Arc;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Commands.Arc;

public sealed class InstallAIOCommand(ILogger<InstallAIOCommand> logger)
    : BaseClusterCommand<ClusterConnectOptions>
{
    private const string _commandTitle = "Install Azure IoT Operations (AIO) Platform Extension";
    private readonly ILogger<InstallAIOCommand> _logger = logger;

    public override string Name => "install-aio";
    public override string Description =>
        """
        Install the Azure IoT Operations (AIO) Platform extension on a connected AKS/Arc cluster.
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
            var result = await arcService.InstallAioPlatformExtensionAsync(
                options.ResourceGroup!,
                options.ClusterName!,
                options.Subscription!,
                msg => _logger.LogInformation(msg)
            );

            context.Response.Results = ResponseResult.Create(
                new InstallAIOCommandResult(result),
                ArcJsonContext.Default.InstallAIOCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error installing AIO extension. Cluster: {ClusterName}, ResourceGroup: {ResourceGroup}, Options: {@Options}",
                options.ClusterName, options.ResourceGroup, options);
            HandleException(context.Response, ex);
        }

        return context.Response;
    }

    internal record InstallAIOCommandResult(bool Success);
}