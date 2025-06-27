using System.CommandLine;
using System.CommandLine.Parsing;
using AzureMcp.Models.Option;
using AzureMcp.Options.Arc;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Commands.Arc;

public sealed class ConnectToArcCommand(ILogger<ConnectToArcCommand> logger)
    : BaseClusterCommand<ClusterConnectOptions>
{
    private const string _commandTitle = "Connect AKS Cluster to Azure Arc";
    private readonly ILogger<ConnectToArcCommand> _logger = logger;

    // Only define new options here if not present in base class
    private readonly Option<string> _locationOption = new("--location", "The Azure region for the Arc resource (e.g., eastus2euap)") { IsRequired = true };
    public override string Name => "connect";
    public override string Description =>
        """
        Connect an existing AKS cluster to Azure Arc.
        Required options:
        - cluster-name: The name of the AKS cluster
        - resource-group: The resource group containing the cluster
        Optional:
        - location: The Azure region for the Arc resource (default: eastus2euap)
        """;
    public override string Title => _commandTitle;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_locationOption);
    }

    protected override ClusterConnectOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Location = parseResult.GetValueForOption(_locationOption);
        return options;
    }

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
            var result = await arcService.ConnectAksToArcAsync(
                options.ResourceGroup!,
                options.ClusterName!,
                options.Location!,
                msg => _logger.LogInformation(msg)
            );

            context.Response.Results = ResponseResult.Create(
                new ConnectToArcCommandResult(result),
                ArcJsonContext.Default.ConnectToArcCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting AKS cluster to Azure Arc. Cluster: {ClusterName}, ResourceGroup: {ResourceGroup}, Options: {@Options}",
                options.ClusterName, options.ResourceGroup, options);
            HandleException(context.Response, ex);
        }

        return context.Response;
    }

    // Result record for serialization
    internal record ConnectToArcCommandResult(bool Success);
}