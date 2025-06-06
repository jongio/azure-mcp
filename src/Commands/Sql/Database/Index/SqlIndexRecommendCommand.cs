// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Models.Option;
using AzureMcp.Models.Sql;
using AzureMcp.Options.Sql.Database.Index;
using AzureMcp.Services.Azure.Sql.Exceptions;
using AzureMcp.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Commands.Sql.Database.Index;

public sealed class SqlIndexRecommendCommand(ILogger<SqlIndexRecommendCommand> logger)
    : BaseDatabaseCommand<IndexRecommendOptions>
{
    private const string _commandTitle = "Get Sql Index Recommendations";
    private readonly ILogger<SqlIndexRecommendCommand> _logger = logger;

    private readonly Option<string> _tableName = OptionDefinitions.Sql.Table;
    private readonly Option<int> _minImpact = new(
        "--min-impact",
        description: "Minimum performance impact percentage to include (0-100)",
        getDefaultValue: () => 20);

    public override string Name => "recommend";
    public override string Title => _commandTitle; public override string Description =>
        """
        Gets index recommendations for a Sql database.
        Returns a list of recommended indexes with their estimated performance impact.
        Required options:
        - database: The name of the database to analyze
        - server-name: The name of the Sql server containing the database
        Optional:
        - table-name: Filter recommendations to a specific table
        - min-impact: Minimum performance impact percentage (default: 20)
        """;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_tableName);
        command.AddOption(_minImpact);
    }

    protected override IndexRecommendOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.TableName = parseResult.GetValueForOption(_tableName);
        options.MinimumImpact = parseResult.GetValueForOption(_minImpact);
        return options;
    }

    [McpServerTool(
        Destructive = false,
        ReadOnly = true,
        Title = _commandTitle)]
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }            var service = context.GetService<ISqlService>();
            var serviceRecommendations = await service.GetIndexRecommendationsAsync(
                options.Database!,
                options.ServerName!,
                options.ResourceGroup!,
                options.TableName,
                options.MinimumImpact ?? 20,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy);

            var recommendations = serviceRecommendations.Select(r => new Models.Sql.SqlIndexRecommendation
            {
                Name = r.Name,
                Description = r.Description ?? string.Empty,
                Impact = r.Impact,
                TableName = r.TableName ?? string.Empty
            }).ToList();

            context.Response.Results = recommendations?.Count > 0 ?
                ResponseResult.Create<SqlIndexRecommendCommand.IndexRecommendCommandResult>(
                    new IndexRecommendCommandResult(recommendations),
                    SqlJsonContext.Default.IndexRecommendCommandResult) :
                null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting index recommendations. Database: {Database}, Server: {Server}, Options: {@Options}",
                options.Database, options.ServerName, options);
            HandleException(context.Response, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        SqlException sqlEx => $"Sql error occurred: {sqlEx.Message}",
        DatabaseNotFoundException => "Database not found. Verify the database exists and you have access.",
        _ => base.GetErrorMessage(ex)
    }; internal record IndexRecommendCommandResult(List<Models.Sql.SqlIndexRecommendation> Recommendations) : IIndexRecommendCommandResult;
}
