using AzureMcp.Commands.Subscription;
using AzureMcp.Options.Arc;
using AzureMcp.Services.Azure.Arc;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;

namespace AzureMcp.Commands.Arc;

public sealed class ValidatePrerequisitesForAksEdgeClusterCommand : BaseSubscriptionCommand<AkseeDeploymentOptions>
{
    private const string _commandTitle = "Validate prerequisites for AKS Edge Essentials Cluster";
    private readonly ILogger<ValidatePrerequisitesForAksEdgeClusterCommand> _logger;
    private readonly IArcService _arcService;

    public ValidatePrerequisitesForAksEdgeClusterCommand(
        ILogger<ValidatePrerequisitesForAksEdgeClusterCommand> logger,
        IArcService arcService)
    {
        _logger = logger;
        _arcService = arcService;
    }

    public override string Name => "validate-prerequisites-aksee-cluster";

    public override string Description =>
        "Validates prerequisites for installing AKS Edge Essentials cluster by loading steps from the resource file and suggesting next steps";

    public override string Title => _commandTitle;

    [McpServerTool(
        Destructive = false,
        ReadOnly = true,
        Title = _commandTitle)]
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        try
        {
            var result = await _arcService.ValidatePrerequisitesForAksEdgeClusterAsync();
            context.Response.Status = 200;
            context.Response.Message = "Prerequisites validated successfully";
            context.Response.Results = ResponseResult.Create(result, JsonSourceGenerationContext.Default.DeploymentResult);

            return context.Response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate prerequisites for AKS Edge Essentials cluster");
            context.Response.Status = 500;
            context.Response.Message = ex.Message;
            return context.Response;
        }
    }
}
