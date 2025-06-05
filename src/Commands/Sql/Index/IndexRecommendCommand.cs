// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using AzureMcp.Models.Option;
using AzureMcp.Options.Sql.Index;
using AzureMcp.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Commands.Sql.Index;

public sealed class IndexRecommendCommand(ILogger<IndexRecommendCommand> logger) 
    : BaseSqlCommand<IndexRecommendOptions>
{
    private const string _commandTitle = "SQL Index Recommendations";
    private readonly ILogger<IndexRecommendCommand> _logger = logger;
    
    private readonly Option<string> _tableOption = OptionDefinitions.Sql.Table;
    private readonly Option<int> _minimumImpactOption = OptionDefinitions.Sql.MinimumImpact;

    public override string Name => "recommend";

    public override string Description =>
        """
        Analyzes database usage patterns and provides index recommendations.
        Returns a list of recommended indexes with their estimated impact.
          Required options:
        - --database
        Optional options:
        - --table-name: Analyze specific table only
        - --minimum-impact: Filter recommendations by minimum impact
        """;

    public override string Title => _commandTitle;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_tableOption);
        command.AddOption(_minimumImpactOption);
    }    
    
    protected override IndexRecommendOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.TableName = parseResult.GetValueForOption(_tableOption);
        options.MinimumImpact = parseResult.GetValueForOption(_minimumImpactOption);
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
            }

            var service = context.GetService<ISqlService>();
              var results = await service.GetIndexRecommendationsAsync(
                options.Database!,
                options.ServerName!,
                options.TableName,
                options.MinimumImpact,
                options.Subscription!,
                options.RetryPolicy);

            context.Response.Results = results?.Count > 0 ? 
                ResponseResult.Create(
                    new IndexRecommendCommandResult(results),
                    SqlJsonContext.Default.IndexRecommendCommandResult) : 
                null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error in recommend. Database: {Database}, Table: {Table}, Impact: {Impact}, Options: {@Options}", 
                options.Database, options.TableName, options.MinimumImpact, options);
            HandleException(context.Response, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        SqlResourceNotFoundException => "SQL resource not found. Verify the resource exists and you have access.",
        SqlAuthorizationException authEx => 
            $"SQL authorization failed. Details: {authEx.Message}",
        SqlException sqlEx => sqlEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override int GetStatusCode(Exception ex) => ex switch  
    {
        SqlResourceNotFoundException => 404,
        SqlAuthorizationException => 403,
        SqlException sqlEx => sqlEx.ErrorCode switch
        {
            40615 => 429, // Rate limiting
            _ => 500
        },
        _ => base.GetStatusCode(ex)
    };

    internal record IndexRecommendCommandResult(List<SqlIndexRecommendation> Results);
}
