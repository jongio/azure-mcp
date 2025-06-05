public class SqlIndexRecommendCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISqlService _service;
    private readonly ILogger<SqlIndexRecommendCommand> _logger;
    private readonly SqlIndexRecommendCommand _command;

    public SqlIndexRecommendCommandTests()
    {
        _service = Substitute.For<ISqlService>();
        _logger = Substitute.For<ILogger<SqlIndexRecommendCommand>>();
        
        var collection = new ServiceCollection();
        collection.AddSingleton(_service);
        _serviceProvider = collection.BuildServiceProvider();
        
        _command = new(_logger);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("recommend", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--database mydb --subscription sub1", true)]
    [InlineData("--database mydb --table-name table1 --subscription sub1", true)]
    [InlineData("--table-name table1", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            _service.GetIndexRecommendations(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<string>())
                .Returns(new List<SqlIndexRecommendCommand.IndexRecommendation>());
        }

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse(args);

        var response = await _command.ExecuteAsync(context, parseResult);

        Assert.Equal(shouldSucceed ? 200 : 400, response.Status);
        if (!shouldSucceed)
        {
            Assert.Contains("database", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        _service.GetIndexRecommendations(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int>(),
            Arg.Any<string>())
            .Returns(Task.FromException<List<SqlIndexRecommendCommand.IndexRecommendation>>(
                new SqlException("Test error")));

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--database mydb --subscription sub1");

        var response = await _command.ExecuteAsync(context, parseResult);

        Assert.Equal(500, response.Status);
        Assert.Contains("SQL error", response.Message);
    }
}
