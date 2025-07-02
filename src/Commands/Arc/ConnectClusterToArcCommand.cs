using AzureMcp.Commands.Subscription;
using AzureMcp.Options;
using AzureMcp.Options.Arc;
using AzureMcp.Options.Subscription;
using AzureMcp.Services.Azure.Arc;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;

namespace AzureMcp.Commands.Arc;

public sealed class ConnectClusterToArcCommand(ILogger<ConnectClusterToArcCommand> logger)
    : BaseSubscriptionCommand<ArcConnectOptions>
{
    private const string _commandTitle = "Connect AKS Edge Essentials Cluster to Azure Arc";
    private readonly ILogger<ConnectClusterToArcCommand> _logger = logger;

    public override string Name => "connect-aksee-cluster";

    public override string Description =>
        "This command facilitates the connection of an AKS Edge Essentials cluster to Azure Arc. It requires the user to provide essential parameters such as subscription ID, tenant ID, location, resource group name, and cluster name. The command validates these inputs and invokes the Azure Arc service to establish the connection, logging the process and handling errors appropriately.";

    public override string Title => _commandTitle;

    private readonly Option<string> _subscriptionIdOption = new Option<string>("--subscription-id", "The subscription ID.");
    private readonly Option<string> _tenantIdOption = new Option<string>("--tenant-id", "The tenant ID.");
    private readonly Option<string> _locationOption = new Option<string>("--location", "The location.");
    private readonly Option<string> _resourceGroupNameOption = new Option<string>("--resource-group-name", "The resource group name.");
    private readonly Option<string> _clusterNameOption = new Option<string>("--cluster-name", "The cluster name.");

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_subscriptionIdOption);
        command.AddOption(_tenantIdOption);
        command.AddOption(_locationOption);
        command.AddOption(_resourceGroupNameOption);
        command.AddOption(_clusterNameOption);
    }

    protected override ArcConnectOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.SubscriptionId = parseResult.GetValueForOption(_subscriptionIdOption) ?? PromptUserForInput("Enter Subscription ID:");
        options.TenantId = parseResult.GetValueForOption(_tenantIdOption) ?? PromptUserForInput("Enter Tenant ID:");
        options.Location = parseResult.GetValueForOption(_locationOption) ?? PromptUserForInput("Enter Location:");
        options.ResourceGroupName = parseResult.GetValueForOption(_resourceGroupNameOption) ?? PromptUserForInput("Enter Resource Group Name:");
        options.ClusterName = parseResult.GetValueForOption(_clusterNameOption) ?? PromptUserForInput("Enter Cluster Name:");
        return options;
    }

    private string PromptUserForInput(string message)
    {
        Console.WriteLine(message);
        return Console.ReadLine() ?? string.Empty;
    }

    [McpServerTool(
        Destructive = false,
        ReadOnly = true,
        Title = _commandTitle)]
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var arcService = context.GetService<IArcService>();
        var options = BindOptions(parseResult);

        _logger.LogInformation("Connecting AKS Edge Essentials cluster '{ClusterName}' to Azure Arc in resource group '{ResourceGroup}'.", options.ClusterName, options.ResourceGroupName);

        try
        {
            options.SubscriptionId ??= PromptUserForInput("Enter Subscription ID:");
            options.TenantId ??= PromptUserForInput("Enter Tenant ID:");
            options.Location ??= PromptUserForInput("Enter Location:");
            options.ResourceGroupName ??= PromptUserForInput("Enter Resource Group Name:");
            options.ClusterName ??= PromptUserForInput("Enter Cluster Name:");

            bool success = await arcService.ConnectClusterToArcAsync(options.ClusterName, options.ResourceGroupName, options.Location, options.SubscriptionId, options.TenantId);

            if (success)
            {
                context.Response.Status = 200;
                context.Response.Message = "Success";
            }
            else
            {
                context.Response.Status = 500;
                context.Response.Message = "Failed to connect cluster to Azure Arc.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect AKS Edge Essentials cluster '{ClusterName}' to Azure Arc.", options.ClusterName);
            context.Response.Status = 500;
            context.Response.Message = ex.Message;
        }

        return context.Response;
    }
}
