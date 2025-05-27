// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Runtime.InteropServices;
using AzureMcp.Arguments.Extension;
using AzureMcp.Models.Argument;
using AzureMcp.Models.Command;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace AzureMcp.Commands.Extension;

public sealed class BicepCommand(ILogger<BicepCommand> logger, int processTimeoutSeconds = 300) : GlobalCommand<BicepArguments>()
{
    private readonly ILogger<BicepCommand> _logger = logger;
    private readonly int _processTimeoutSeconds = processTimeoutSeconds;
    private readonly Option<string> _commandOption = ArgumentDefinitions.Extension.Bicep.Command.ToOption();
    private static string? _cachedBicepPath;

    protected override string GetCommandName() => "bicep";

    protected override string GetCommandDescription() =>
        """
        Use the Bicep CLI to manage Azure infrastructure as code. You have the following capabilities:

        - Build Bicep files into ARM templates using 'build'
        - Decompile ARM templates into Bicep using 'decompile'
        - Format Bicep files using 'format'
        - Validate Bicep files for errors using 'build --verify'
        - Preview changes using 'what-if'
        - Generate parameters files using '--generate-parameters'

        Before modifying resources, always:
        - Validate Bicep files for errors
        - Use what-if to preview changes
        - Request user confirmation for destructive operations

        Provide clear error messages and suggestions when builds or deployments fail.
        Focus on infrastructure changes that match the user's intent and Azure best practices.
        """;

    private static string? FindBicepCliPath()
    {
        // Return cached path if available and still exists
        if (!string.IsNullOrEmpty(_cachedBicepPath) && File.Exists(_cachedBicepPath))
        {
            return _cachedBicepPath;
        }

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        var executableName = isWindows ? "bicep.exe" : "bicep";

        // First check if bicep is in PATH
        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (!string.IsNullOrEmpty(pathEnv))
        {
            foreach (var path in pathEnv.Split(Path.PathSeparator))
            {
                var fullPath = Path.Combine(path.Trim(), executableName);
                if (File.Exists(fullPath))
                {
                    _cachedBicepPath = fullPath;
                    return _cachedBicepPath;
                }
            }
        }

        // Check common installation locations
        var commonPaths = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Microsoft", "Bicep CLI"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".azure", "bin"),
            "/usr/local/bin",
            "/usr/bin"
        };

        foreach (var path in commonPaths)
        {
            var fullPath = Path.Combine(path, executableName);
            if (File.Exists(fullPath))
            {
                _cachedBicepPath = fullPath;
                return _cachedBicepPath;
            }
        }

        return null;
    }

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_commandOption);
    }

    protected override BicepArguments BindArguments(ParseResult parseResult)
    {
        var args = base.BindArguments(parseResult);
        args.Command = parseResult.GetValueForOption(_commandOption);
        return args;
    }

    [McpServerTool(Destructive = true, ReadOnly = false)]
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var args = BindArguments(parseResult);

        try
        {
            if (!await ProcessArguments(context, args))
            {
                return context.Response;
            }

            ArgumentNullException.ThrowIfNull(args.Command);
            var command = args.Command;

            var processService = context.GetService<IExternalProcessService>();
            var bicepPath = FindBicepCliPath() ?? throw new FileNotFoundException("Bicep CLI executable not found in PATH or common installation locations. Please ensure Bicep CLI is installed.");

            var result = await processService.ExecuteAsync(bicepPath, command, _processTimeoutSeconds);

            if (string.IsNullOrWhiteSpace(result.Error) && result.ExitCode == 0)
            {
                context.Response.Results = ResponseResult.Create(new List<string> { result.Output }, JsonSourceGenerationContext.Default.ListString);
            }
            else
            {
                context.Response.Status = 500;
                context.Response.Message = result.Error;

                // Add helpful suggestions for common errors
                if (result.Error.Contains("not found"))
                {
                    context.Response.Message += "\nPlease ensure the Bicep file exists and try again.";
                }
                else if (result.Error.Contains("syntax error"))
                {
                    context.Response.Message += "\nThere appears to be a syntax error in your Bicep file. Please validate the file and try again.";
                }
                else if (result.Error.Contains("access denied"))
                {
                    context.Response.Message += "\nAccess denied. Please ensure you have appropriate permissions.";
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred executing Bicep command: {Command}", args.Command);
            HandleException(context.Response, ex);
        }

        return context.Response;
    }
}
