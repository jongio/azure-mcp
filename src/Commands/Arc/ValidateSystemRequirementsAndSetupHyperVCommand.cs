using AzureMcp.Commands.Subscription;
using AzureMcp.Options.Arc;
using AzureMcp.Services.Azure.Arc;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;

namespace AzureMcp.Commands.Arc;

public sealed class ValidateSystemRequirementsAndSetupHyperVCommand : BaseSubscriptionCommand<AkseeDeploymentOptions>
{
    private const string _commandTitle = "Validate System Requirements and Setup Hyper-V";
    private readonly ILogger<ValidateSystemRequirementsAndSetupHyperVCommand> _logger;
    private readonly IArcService _arcService;

    private readonly Option<string> _pathOption = new Option<string>("--path", "The path to validate system requirements and setup Hyper-V") { IsRequired = true };

    public ValidateSystemRequirementsAndSetupHyperVCommand(
        ILogger<ValidateSystemRequirementsAndSetupHyperVCommand> logger,
        IArcService arcService)
    {
        _logger = logger;
        _arcService = arcService;
    }

    public override string Name => "validate-system-requirements-hyperv";

    public override string Description =>
        "Validates system requirements and sets up Hyper-V for AKS Edge Essentials or Aksee cluster installation.";

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
            var userProvidedPath = parseResult.GetValueForOption(_pathOption);
            if (string.IsNullOrEmpty(userProvidedPath))
            {
                throw new ArgumentException("The --path option is required.");
            }

            // Informational warning about system restart
            string warningMessage = "Note: The system may restart after Hyper-V installation.";
            _logger.LogInformation(warningMessage);
            var result = await _arcService.ValidateSystemRequirementsAndSetupHyperVAsync(userProvidedPath);
            context.Response.Status = 200;
            context.Response.Message = $"{warningMessage}\n{result.Steps}";
            context.Response.Results = ResponseResult.Create(result, JsonSourceGenerationContext.Default.DeploymentResult);


            return context.Response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate system requirements and setup Hyper-V");
            context.Response.Status = 500;
            context.Response.Message = ex.Message;
            return context.Response;
        }
    }
}
