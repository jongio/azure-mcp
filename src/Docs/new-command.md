# Implementing a New Command in Azure MCP

This document provides a guide for implementing commands in Azure MCP following established patterns.

## Command Structure 

Commands follow this exact pattern:
```
azmcp <service> <resource> <operation>
```

Example: `azmcp storage container list`

Where:
- `service` - Azure service name (lowercase)
- `resource` - Resource type (singular noun, lowercase)
- `operation` - Action to perform (verb, lowercase)

## Required Files

A complete command requires:

1. Options class: `src/Options/{Service}/{Resource}/{Operation}Options.cs`
2. Command class: `src/Commands/{Service}/{Resource}/{Resource}{Operation}Command.cs`
3. Service interface: `src/Services/Interfaces/I{Service}Service.cs`
4. Service implementation: `src/Services/Azure/{Service}/{Service}Service.cs`
5. Unit test: `tests/Commands/{Service}/{Resource}/{Resource}{Operation}CommandTests.cs`
6. Integration test: `tests/Client/{Service}CommandTests.cs`
7. Registration in `src/Commands/CommandFactory.cs`

## Implementation Guidelines

### 1. Options Class

```csharp
public class {Resource}{Operation}Options : Base{Service}Options 
{
    // Only add properties not in base class
    public string? NewOption { get; set; }
}
```

IMPORTANT:
- Inherit from appropriate base class (BaseServiceOptions, GlobalOptions, etc.)
- Never redefine properties from base classes 
- Make properties nullable if not required

### 2. Command Class

```csharp
public sealed class {Resource}{Operation}Command : Base{Service}Command<{Resource}{Operation}Options>
{
    private const string _commandTitle = "Human Readable Title";
    private readonly ILogger<{Resource}{Operation}Command> _logger;
    
    // Define options from OptionDefinitions
    private readonly Option<string> _newOption = OptionDefinitions.Service.NewOption;

    public {Resource}{Operation}Command(ILogger<{Resource}{Operation}Command> logger)
    {
        _logger = logger;
    }

    public override string Name => "operation";

    public override string Description =>
        """
        Detailed description of what the command does.
        Returns description of return format.
          Required options:
        - list required options
        """;

    public override string Title => _commandTitle;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_newOption);
    }    protected override {Resource}{Operation}Options BindOptions(ParseResult parseResult)
    {
        var args = base.BindOptions(parseResult);
        args.NewOption = parseResult.GetValueForOption(_newOption);
        return args;
    }

    [McpServerTool(
        Destructive = false,
        ReadOnly = true,
        Title = _commandTitle,
        OpenWorld = false,
        Idempotent = true)]
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var args = BindOptions(parseResult);

        try
        {
            var validationResult = Validate(parseResult.CommandResult); 
            if (!validationResult.IsValid)
            {
                context.Response.Status = 400;
                context.Response.Message = validationResult.ErrorMessage!;
                return context.Response;
            }

            var service = context.GetService<I{Service}Service>();
            var results = await service.{Operation}(args);

            context.Response.Results = results?.Count > 0 ?
                ResponseResult.Create(results, {Service}JsonContext.Default.{Resource}{Operation}CommandResult) :
                null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Operation}. Options: {Options}", Name, args);
            HandleException(context.Response, ex);
        }

        return context.Response;
    }

    internal record {Resource}{Operation}CommandResult(List<ResultType> Results);
}
```

### 3. Base Service Command 

```csharp
public abstract class Base{Service}Command<TOptions> : GlobalCommand<TOptions> 
    where TOptions : Base{Service}Options, new()
{
    protected readonly Option<string> _commonOption = OptionDefinitions.Service.CommonOption;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_commonOption);
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var args = base.BindOptions(parseResult);
        args.CommonOption = parseResult.GetValueForOption(_commonOption);
        return args;
    }
}
```

### 4. Unit Tests

```csharp
public class {Resource}{Operation}CommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly I{Service}Service _service;
    private readonly ILogger<{Resource}{Operation}Command> _logger;
    private readonly {Resource}{Operation}Command _command;

    public {Resource}{Operation}CommandTests()
    {
        _service = Substitute.For<I{Service}Service>();
        _logger = Substitute.For<ILogger<{Resource}{Operation}Command>>();
        
        var collection = new ServiceCollection();
        collection.AddSingleton(_service);
        _serviceProvider = collection.BuildServiceProvider();
        
        _command = new(_logger);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        // Arrange & Act
        var command = _command.GetCommand();

        // Assert
        Assert.Equal("operation", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact] 
    public async Task ExecuteAsync_WithValidInput_ReturnsSuccess()
    {
        // Arrange
        _service.{Operation}(Arg.Any<{Resource}{Operation}Options>())
            .Returns(new List<ResultType>());

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--required value");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingRequiredOption_ReturnsBadRequest()
    {
        // Arrange
        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }
}
```

### 5. Integration Tests

```csharp
public class {Service}CommandTests : CommandTestsBase, 
    IClassFixture<LiveTestFixture>
{
    private readonly {Service}Service _{service}Service;

    public {Service}CommandTests(LiveTestFixture fixture, ITestOutputHelper output)
        : base(fixture, output)
    {
        _{service}Service = new {Service}Service();
    }

    [Fact]
    [Trait("Category", "Live")]
    public async Task Should_{Operation}_{Resource}()
    {
        var result = await CallToolAsync(
            "azmcp-{service}-{resource}-{operation}",
            new() 
            {
                { "required-option", "value" }
            });

        // Assert expected result format
        var resultArray = result.AssertProperty("propertyName");
        Assert.Equal(JsonValueKind.Array, resultArray.ValueKind);
        Assert.NotEmpty(resultArray.EnumerateArray());
    }
}
```

### 6. CommandFactory Registration

```csharp
private void Register{Service}Commands()
{
    var service = new CommandGroup(
        "{service}",
        "{Service} operations");
    _rootGroup.AddSubGroup(service);

    var resource = new CommandGroup(
        "{resource}", 
        "{Resource} operations");
    service.AddSubGroup(resource);

    resource.AddCommand("operation", new {Service}.{Resource}{Operation}Command(
        GetLogger<{Resource}{Operation}Command>()));
```

## Testing Requirements

1. Unit Tests:
   - Constructor initialization
   - Option validation
   - Success path
   - Error paths
   - Service error handling
   - Required option validation

2. Integration Tests:
   - Live command execution
   - Response format validation
   - Error scenarios
   - Authentication handling

## Best Practices

1. Command Structure:
   - Make command classes sealed
   - Use primary constructors
   - Follow exact namespace hierarchy
   - Register all options in RegisterOptions
   - Handle all exceptions

2. Error Handling:
   - Return 400 for validation errors
   - Return 500 for unexpected errors
   - Use ValidateOptions for input validation
   - Handle service-specific exceptions

3. Response Format:
   - Always set Results property for success
   - Set Status and Message for errors
   - Use consistent JSON property names
   - Follow existing response patterns

4. Documentation:
   - Clear command description
   - List all required options
   - Describe return format
   - Include examples in description

## Common Pitfalls to Avoid

1. Do not:
   - Redefine base class properties in Options classes
   - Skip base.RegisterOptions() call
   - Use hardcoded option strings
   - Return different response formats
   - Leave command unregistered 
   - Skip error handling
   - Miss required tests

2. Always:
   - Use OptionDefinitions for options
   - Follow exact file structure
   - Implement all base members
   - Add both unit and integration tests
   - Register in CommandFactory
   - Handle all error cases
   - Use primary constructors
   - Make command classes sealed

## Checklist

Before submitting:

- [ ] Options class follows inheritance pattern
- [ ] Command class implements all required members 
- [ ] Command uses proper OptionDefinitions
- [ ] Service interface and implementation complete
- [ ] Unit tests cover all paths
- [ ] Integration tests added
- [ ] Registered in CommandFactory 
- [ ] Follows file structure exactly
- [ ] Error handling implemented
- [ ] Documentation complete
- [ ] No compiler warnings
- [ ] Tests pass
