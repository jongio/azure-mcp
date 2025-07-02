using AzureMcp.Commands.Subscription;
using AzureMcp.Options.Arc;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;

namespace AzureMcp.Commands.Arc;

public sealed class DeployAksEdgeEssentialClusterCommand : BaseSubscriptionCommand<AkseeDeploymentOptions>
{
    private const string _commandTitle = "Deploy or installs AKS Edge Essentials Cluster";
    private readonly ILogger<DeployAksEdgeEssentialClusterCommand> _logger;
    private readonly IArcService _arcService;

    public DeployAksEdgeEssentialClusterCommand(
        ILogger<DeployAksEdgeEssentialClusterCommand> logger,
        IArcService arcService)
    {
        _logger = logger;
        _arcService = arcService;
    }

    public override string Name => "install-aksee-cluster";

    public override string Description =>
        "Deploys an AKS Edge Essentials cluster by providing deployment steps and configuring the cluster.";

    public override string Title => _commandTitle;

    private readonly Option<string> _clusterNameOption = new("--cluster-name", "Name of the AKS Edge Essentials cluster.");
    private readonly Option<string> _locationOption = new("--location", "Azure region where the cluster will be registered.");

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_clusterNameOption);
        command.AddOption(_locationOption);
    }

    protected override AkseeDeploymentOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ClusterName = parseResult.GetValueForOption(_clusterNameOption) ?? string.Empty;
        options.Location = parseResult.GetValueForOption(_locationOption) ?? string.Empty;
        return options;
    }

    [McpServerTool(
        Destructive = false,
        ReadOnly = false,
        Title = _commandTitle)]
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        try
        {
            var options = BindOptions(parseResult);
            _logger.LogInformation("Starting AKS Edge Essentials cluster deployment for cluster '{ClusterName}'", options.ClusterName);

            var result = await _arcService.DeployAksEdgeEssentialClusterAsync(); // Removed OutputPath dependency
            if (result.Success)
            {
                context.Response.Status = 200;
                context.Response.Message = "Deployment steps generated successfully";
                context.Response.Results = ResponseResult.Create(result, JsonSourceGenerationContext.Default.DeploymentResult);
            }
            else
            {
                context.Response.Status = 500;
                context.Response.Message = result.ErrorMessage;
            }

            return context.Response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deploy AKS Edge Essentials cluster");
            context.Response.Status = 500;
            context.Response.Message = ex.Message;
            return context.Response;
        }
    }
}
