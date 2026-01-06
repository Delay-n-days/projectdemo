using ProjectManager.Core;

namespace ProjectManager.Tests;

public class ModelsTests : IDisposable
{
    private readonly string _testDir;

    public ModelsTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
            Directory.Delete(_testDir, true);
    }

    [Fact]
    public void LoggerModel_Execute_ShouldAddLog()
    {
        var config = new ModelConfig("test_logger", "logger", "Test logger", [], _testDir);
        var logger = new LoggerModel(config);

        logger.Execute("Test message", "INFO");
        var logs = logger.GetLogs();

        Assert.Single(logs);
        Assert.Contains("Test message", logs[0]);
    }

    [Fact]
    public void LoggerModel_ClearLogs_ShouldRemoveAllLogs()
    {
        var config = new ModelConfig("test_logger", "logger", "Test logger", [], _testDir);
        var logger = new LoggerModel(config);

        logger.Execute("Test 1");
        logger.Execute("Test 2");
        logger.ClearLogs();

        Assert.Empty(logger.GetLogs());
    }

    [Fact]
    public void CounterModel_Increment_ShouldIncreaseCount()
    {
        var config = new ModelConfig("test_counter", "counter", "Test counter", [], _testDir);
        var counter = new CounterModel(config);

        var count = counter.Increment();
        Assert.Equal(1, count);

        count = counter.Increment();
        Assert.Equal(2, count);
    }

    [Fact]
    public void CounterModel_MaxCount_ShouldResetWhenExceeded()
    {
        var config = new ModelConfig("test_counter", "counter", "Test counter", [], _testDir);
        var counter = new CounterModel(config, maxCount: 3);

        counter.Increment(); // 1
        counter.Increment(); // 2
        counter.Increment(); // 3
        var count = counter.Increment(); // should reset to 0

        Assert.Equal(0, count);
    }

    [Fact]
    public void CounterModel_Reset_ShouldSetCountToZero()
    {
        var config = new ModelConfig("test_counter", "counter", "Test counter", [], _testDir);
        var counter = new CounterModel(config);

        counter.Increment();
        counter.Increment();
        var count = counter.Reset();

        Assert.Equal(0, count);
    }
}
