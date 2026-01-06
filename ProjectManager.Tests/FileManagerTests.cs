using ProjectManager.Core;

namespace ProjectManager.Tests;

public class FileManagerTests : IDisposable
{
    private readonly string _testDir;

    public FileManagerTests()
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
    public void SaveAndLoadJson_ShouldWorkCorrectly()
    {
        var testFile = Path.Combine(_testDir, "test.json");
        var testData = new TestData { Key = "value", Number = 123 };

        FileManager.SaveJson(testFile, testData);
        var loaded = FileManager.LoadJson<TestData>(testFile);

        Assert.Equal("value", loaded.Key);
        Assert.Equal(123, loaded.Number);
    }

    private class TestData
    {
        public string Key { get; set; } = "";
        public int Number { get; set; }
    }

    [Fact]
    public void LoadJson_NonexistentFile_ShouldThrowFileNotFoundException()
    {
        Assert.Throws<FileNotFoundException>(() =>
            FileManager.LoadJson<Dictionary<string, object>>(Path.Combine(_testDir, "nonexistent.json")));
    }
}
