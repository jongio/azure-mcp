using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Threading.Tasks;
using AzureMcp.Commands.Arc;
using AzureMcp.Models.Command;
using AzureMcp.Services.Azure.Arc; // Added namespace for DeploymentResult
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace AzureMcp.Tests.Commands.Arc;

public class ValidateSystemRequirementsAndSetupHyperVCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IArcService _arcService;
    private readonly ILogger<ValidateSystemRequirementsAndSetupHyperVCommand> _logger;
    private readonly ValidateSystemRequirementsAndSetupHyperVCommand _command;
    private readonly CommandContext _context;
    private readonly Parser _parser;

    public ValidateSystemRequirementsAndSetupHyperVCommandTests()
    {
        _arcService = Substitute.For<IArcService>();
        _logger = Substitute.For<ILogger<ValidateSystemRequirementsAndSetupHyperVCommand>>();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(_arcService);
        _serviceProvider = serviceCollection.BuildServiceProvider();

        _command = new(_logger, _arcService);
        _context = new(_serviceProvider);
        _parser = new(_command.GetCommand());
    }

    private void MockArcServiceSuccess()
    {
        _arcService.ValidateSystemRequirementsAndSetupHyperVAsync(Arg.Any<string>())
            .Returns(Task.FromResult(new DeploymentResult
            {
                Success = true,
                Steps = "Validation steps"
            }));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccessResponse()
    {
        // Arrange
        MockArcServiceSuccess();

        var args = new[] { "--path", "C:\\TestPath" };
        var parseResult = _parser.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.Contains("Note: The system may restart after Hyper-V installation.", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldHandleErrorResponse()
    {
        // Arrange
        var args = new[] { "--path", "C:\\InvalidPath" };

        _arcService.When(x => x.StartProcess(Arg.Any<string>(), Arg.Any<ProcessStartInfo>()))
            .Throws(new InvalidOperationException());

        var parseResult = _parser.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.NotNull(response.Message);
    }
}
