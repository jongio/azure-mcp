using AzureMcp.Commands.Subscription;
using AzureMcp.Options.Arc;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;

namespace AzureMcp.Commands.Arc;

public sealed class RemoveAksEdgeCommand : BaseCommand
{
    private const string _commandTitle = "Remove installation for existing cluster";
    private readonly ILogger<RemoveAksEdgeCommand> _logger;
    private readonly IArcService _arcService;

    public RemoveAksEdgeCommand(
        ILogger<RemoveAksEdgeCommand> logger,
        IArcService arcService)
    {
        _logger = logger;
        _arcService = arcService;
    }

    public override string Name => "remove-cluster-installation";

    public override string Description =>
        "Completely removes AKS Edge Essentials from the machine. Ensures that any existing installation is removed and settings are preset for new installation.";

    public override string Title => _commandTitle;


    [McpServerTool(
        Destructive = true,
        ReadOnly = false,
        Title = _commandTitle)]
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        try
        {

            _logger.LogInformation("Starting AKS Edge Essentials removal.");

            var removalSuccess = await _arcService.RemoveAksEdgeAsync();

            if (removalSuccess)
            {
                context.Response.Status = 200;
                context.Response.Message = "AKS Edge Essentials removed successfully.";
            }
            else
            {
                context.Response.Status = 500;
                context.Response.Message = "Failed to remove AKS Edge Essentials.";
            }

            return context.Response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove AKS Edge Essentials.");
            context.Response.Status = 500;
            context.Response.Message = ex.Message;
            return context.Response;
        }
    }
}
