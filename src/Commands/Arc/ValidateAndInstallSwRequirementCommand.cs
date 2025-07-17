using AzureMcp.Commands.Subscription;
using AzureMcp.Options.Arc;
using AzureMcp.Services.Azure.Arc;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;

namespace AzureMcp.Commands.Arc;

public sealed class ValidateAndInstallSwRequirementCommand : BaseSubscriptionCommand<ArcConnectOptions>
{
    private const string _commandTitle = "Validate and Install Software Requirements";
    private readonly ILogger<ValidateAndInstallSwRequirementCommand> _logger;
    private readonly IArcService _arcService;

    private readonly Option<string> _pathOption = new Option<string>("--path", "The path to use for script execution") { IsRequired = true };

    public ValidateAndInstallSwRequirementCommand(
        ILogger<ValidateAndInstallSwRequirementCommand> logger,
        IArcService arcService)
    {
        _logger = logger;
        _arcService = arcService;
    }

    public override string Name => "validate-and-install-sw-requirements";

    public override string Description =>
        "Validates and installs software requirements for AKS Edge Essentials or Aksee cluster installation.";

    public override string Title => _commandTitle;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_pathOption);
    }

    [McpServerTool(
        Destructive = false,
        ReadOnly = true,
        Title = _commandTitle)]
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        try
        {
            // Retrieve the user-provided path from the command options
            var userProvidedPath = parseResult.GetValueForOption(_pathOption);
            if (string.IsNullOrEmpty(userProvidedPath))
            {
                throw new ArgumentException("The path parameter is required.");
            }

            var result = await _arcService.ValidateAndInstallSwRequirementAsync(userProvidedPath);
            context.Response.Status = 200;
            context.Response.Message = result.Steps;
            context.Response.Results = ResponseResult.Create(result, JsonSourceGenerationContext.Default.DeploymentResult);

            return context.Response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate and install software requirements");
            context.Response.Status = 500;
            context.Response.Message = ex.Message;
            return context.Response;
        }
    }
}
