// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Arguments.Search.Service;
using AzureMcp.Models.Command;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.CommandLine.Parsing;

namespace AzureMcp.Commands.Search.Service;

public sealed class ServiceListCommand(ILogger<ServiceListCommand> logger) : SubscriptionCommand<ServiceListArguments>()
{
    private readonly ILogger<ServiceListCommand> _logger = logger;

    protected override string GetCommandName() => "list";

    protected override string GetCommandDescription() => 
        "List all Azure AI Search services in a subscription. Returns an array of search service names.\n\n" +
        "Required arguments:\n- subscription";

    [McpServerTool(Destructive = false, ReadOnly = true)]
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var args = BindArguments(parseResult);

        try
        {
            if (!await ProcessArguments(context, args))
            {
                return context.Response;
            }

            var searchService = context.GetService<ISearchService>();
            
            var services = await searchService.ListServices(
                args.Subscription!,
                args.Tenant,
                args.RetryPolicy);

            context.Response.Results = services?.Count > 0 ? new { services } : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing search services");
            HandleException(context.Response, ex);
        }

        return context.Response;
    }
}
