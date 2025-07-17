using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using AzureMcp.Commands.Arc;
using AzureMcp.Models;
using AzureMcp.Models.Command;
using AzureMcp.Services.Azure.Arc; // Added namespace for DeploymentResult
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace AzureMcp.Tests.Commands.Arc
{
    public class ValidateAndInstallSwRequirementCommandTests
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IArcService _arcService;
        private readonly ILogger<ValidateAndInstallSwRequirementCommand> _logger;
        private readonly ValidateAndInstallSwRequirementCommand _command;
        private readonly CommandContext _context;
        private readonly Parser _parser;
        private readonly string UserProvidedPath = "C:\\TestPath";

        public ValidateAndInstallSwRequirementCommandTests()
        {
            _arcService = Substitute.For<IArcService>();
            _logger = Substitute.For<ILogger<ValidateAndInstallSwRequirementCommand>>();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(_arcService);
            _serviceProvider = serviceCollection.BuildServiceProvider();

            _command = new(_logger, _arcService);
            _context = new(_serviceProvider);
            _parser = new(_command.GetCommand());
        }

        private void MockArcServiceSuccess()
        {
            _arcService.ValidateAndInstallSwRequirementAsync(Arg.Any<string>())
                .Returns(Task.FromResult(new DeploymentResult
                {
                    Success = true,
                    Steps = "Software requirements validated and installed successfully."
                }));
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturnSuccessResponse()
        {
            // Arrange
            MockArcServiceSuccess();

            var args = new[] { "--path", UserProvidedPath };
            var parseResult = _parser.Parse(args);

            // Act
            var response = await _command.ExecuteAsync(_context, parseResult);

            // Assert
            Assert.Equal(200, response.Status);
            Assert.NotNull(response.Message);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldHandleErrorResponse()
        {
            // Arrange
            var args = new[] { "--path", "C:\\InvalidPath" };

            var expectedError = "Error during validation and installation.";
            _arcService.When(x => x.ValidateAndInstallSwRequirementAsync(Arg.Any<string>()))
                .Throws(new InvalidOperationException(expectedError));

            var parseResult = _parser.Parse(args);

            // Act
            var response = await _command.ExecuteAsync(_context, parseResult);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(500, response.Status);
            Assert.Contains(expectedError, response.Message, StringComparison.OrdinalIgnoreCase);
        }
    }
}
