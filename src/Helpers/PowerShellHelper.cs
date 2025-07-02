using System;
using System.Diagnostics;
using System.Threading.Tasks;

public static class PowerShellHelper
{
    public static void ImportModule(string modulePath)
    {
        if (string.IsNullOrEmpty(modulePath))
        {
            throw new ArgumentException("Module path cannot be null or empty.", nameof(modulePath));
        }

        ExecuteCommand($"Import-Module {modulePath}");
    }

    public static void ExecuteCommand(string command, object parameters = null)
    {
        if (string.IsNullOrEmpty(command))
        {
            throw new ArgumentException("Command cannot be null or empty.", nameof(command));
        }

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "pwsh.exe",
                Arguments = $"-Command \"{command}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Command failed: {process.StandardError.ReadToEnd()}\nCommand: {command}");
        }
    }

    public static async Task<string> ExecuteCommandAsync(string command, object parameters = null)
    {
        if (string.IsNullOrEmpty(command))
        {
            throw new ArgumentException("Command cannot be null or empty.", nameof(command));
        }

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "pwsh.exe",
                Arguments = $"-Command \"{command}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        string output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Command failed: {process.StandardError.ReadToEnd()}\nCommand: {command}");
        }

        return output ?? string.Empty;
    }
}
