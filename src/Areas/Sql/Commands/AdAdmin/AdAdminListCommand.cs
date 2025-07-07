// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Areas.Sql.Models;
using AzureMcp.Areas.Sql.Options.AdAdmin;
using AzureMcp.Areas.Sql.Services;
using AzureMcp.Services.Telemetry;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Areas.Sql.Commands.AdAdmin;

public sealed class AdAdminListCommand(ILogger<AdAdminListCommand> logger)
    : BaseSqlCommand<AdAdminListOptions>(logger)
{
    private const string CommandTitle = "List SQL Server AD Administrators";

    public override string Name => "list";

    public override string Description =>
        """
        Gets a list of Azure Active Directory administrators for a SQL server. This command retrieves all 
        AD administrators configured for the specified SQL server, including their display names, object IDs, 
        and tenant information. Returns an array of AD administrator objects with their properties.
        """;

    public override string Title => CommandTitle;

    [McpServerTool(
        Destructive = false,
        ReadOnly = true,
        Title = CommandTitle)]
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            context.Activity?.WithSubscriptionTag(options);

            var sqlService = context.GetService<ISqlService>();

            var administrators = await sqlService.GetAdAdministratorsAsync(
                options.Server!,
                options.ResourceGroup!,
                options.Subscription!,
                options.RetryPolicy);

            context.Response.Results = administrators?.Count > 0
                ? ResponseResult.Create(
                    new AdAdminListResult(administrators),
                    SqlJsonContext.Default.AdAdminListResult)
                : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing SQL server AD administrators. Server: {Server}, ResourceGroup: {ResourceGroup}, Options: {@Options}",
                options.Server, options.ResourceGroup, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        Azure.RequestFailedException reqEx when reqEx.Status == 404 =>
            "SQL server not found. Verify the server name, resource group, and that you have access.",
        Azure.RequestFailedException reqEx when reqEx.Status == 403 =>
            $"Authorization failed accessing the SQL server. Verify you have appropriate permissions. Details: {reqEx.Message}",
        Azure.RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override int GetStatusCode(Exception ex) => ex switch
    {
        Azure.RequestFailedException reqEx => reqEx.Status,
        _ => base.GetStatusCode(ex)
    };

    internal record AdAdminListResult(List<SqlServerAdAdministrator> Administrators);
}
