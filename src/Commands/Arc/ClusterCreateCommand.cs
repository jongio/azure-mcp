using System.CommandLine;
using System.CommandLine.Parsing;
using AzureMcp.Models.Option;
using AzureMcp.Options.Arc;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Commands.Arc;

public sealed class ClusterCreateCommand(ILogger<ClusterCreateCommand> logger)
    : BaseClusterCommand<ClusterCreateOptions>
{
    private const string _commandTitle = "Create Azure Kubernetes Service (AKS) Cluster";
    private readonly ILogger<ClusterCreateCommand> _logger = logger;

    // Only define new options here if not present in base class
    private readonly Option<string> _regionOption = new("--region", "The Azure region (e.g., eastus2)") { IsRequired = true };

    public override string Name => "create";

    public override string Description =>
        """
        Create a new AKS cluster with a system-assigned managed identity.
        Required options:
        - cluster-name: The name of the AKS cluster
        - resource-group: The resource group to create the cluster in
        - subscription: The Azure subscription ID
        - region: The Azure region (e.g., eastus2)
        - tenant: The Azure AD tenant ID
        """;

    public override string Title => _commandTitle;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_regionOption);
    }

    protected override ClusterCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Region = parseResult.GetValueForOption(_regionOption);
        return options;
    }

    [McpServerTool(
        Destructive = true,
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

            var result = await arcService.CreateAksClusterAsync(
                context.GetService<IResourceGroupService>(),
                options.Subscription!,
                options.ResourceGroup!,
                options.ClusterName!,
                options.Region!,
                options.Tenant!,
                msg => _logger.LogInformation(msg)
            );

            // Explicitly specify the result type and serialization context
            context.Response.Results = ResponseResult.Create(
                new ClusterCreateCommandResult(result),
                ArcJsonContext.Default.ClusterCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating AKS cluster. Cluster: {ClusterName}, ResourceGroup: {ResourceGroup}, Options: {@Options}",
                options.ClusterName, options.ResourceGroup, options);
            HandleException(context.Response, ex);
        }

        return context.Response;
    }

    // Result record for serialization
    internal record ClusterCreateCommandResult(string ResourceId);
}