using ProjectManager.Core;
using ProjectManager.UI.ViewModels;

namespace ProjectManager.Tests;

public class MainViewModelTests : IDisposable
{
    private readonly string _testDir;

    public MainViewModelTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), $"test_vm_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
            Directory.Delete(_testDir, true);
    }

    // 注意：OpenProject、SaveAsProject和CreateFromTemplate现在需要对话框交互，无法在单元测试中直接测试
    // 这些测试已移至集成测试或手动测试

    [Fact]
    public void AddLog_ShouldAddLogEntryAndUpdateDisplay()
    {
        // 直接创建项目而不通过ViewModel
        var projectPath = Path.Combine(_testDir, "test_project");
        var app = new Core.ProjectManager();
        app.NewProject(projectPath, "TestProject", "1.0.0", [
            new("logger", "AppLogger"),
            new("counter", "OperationCounter", Parameters: new() { ["step"] = 1 })
        ]);

        // 通过反射访问_projectManager
        var viewModel = new MainViewModel();
        var projectFile = Path.Combine(projectPath, "TestProject.json");
        
        var pmField = typeof(MainViewModel).GetField("_projectManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var vmPm = pmField?.GetValue(viewModel) as Core.ProjectManager;
        vmPm?.OpenProject(projectFile);
        
        // 需要手动调用UpdateDisplay来更新UI
        var updateMethod = typeof(MainViewModel).GetMethod("UpdateDisplay", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        updateMethod?.Invoke(viewModel, null);

        // 测试添加日志
        viewModel.LogMessage = "Test log entry";
        viewModel.AddLogCommand.Execute(null);

        Assert.Contains("日志已添加", viewModel.StatusMessage);
        Assert.Contains(viewModel.Logs, log => log.Contains("Test log entry"));
        Assert.Empty(viewModel.LogMessage);
    }

    [Fact]
    public void IncrementCounter_ShouldIncreaseCounterValue()
    {
        var projectPath = Path.Combine(_testDir, "test_project");
        var app = new Core.ProjectManager();
        app.NewProject(projectPath, "TestProject", "1.0.0", [
            new("logger", "AppLogger"),
            new("counter", "OperationCounter", Parameters: new() { ["step"] = 1 })
        ]);

        var viewModel = new MainViewModel();
        var projectFile = Path.Combine(projectPath, "TestProject.json");
        
        var pmField = typeof(MainViewModel).GetField("_projectManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var vmPm = pmField?.GetValue(viewModel) as Core.ProjectManager;
        vmPm?.OpenProject(projectFile);
        
        var updateMethod = typeof(MainViewModel).GetMethod("UpdateDisplay", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        updateMethod?.Invoke(viewModel, null);

        var initialValue = viewModel.CounterValue;
        viewModel.IncrementCounterCommand.Execute(null);

        Assert.Equal(initialValue + 1, viewModel.CounterValue);
        Assert.Contains("计数器", viewModel.StatusMessage);
    }

    [Fact]
    public void DecrementCounter_ShouldDecreaseCounterValue()
    {
        var projectPath = Path.Combine(_testDir, "test_project");
        var app = new Core.ProjectManager();
        app.NewProject(projectPath, "TestProject", "1.0.0", [
            new("logger", "AppLogger"),
            new("counter", "OperationCounter", Parameters: new() { ["step"] = 1 })
        ]);

        var viewModel = new MainViewModel();
        var projectFile = Path.Combine(projectPath, "TestProject.json");
        
        var pmField = typeof(MainViewModel).GetField("_projectManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var vmPm = pmField?.GetValue(viewModel) as Core.ProjectManager;
        vmPm?.OpenProject(projectFile);
        
        var updateMethod = typeof(MainViewModel).GetMethod("UpdateDisplay", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        updateMethod?.Invoke(viewModel, null);

        viewModel.IncrementCounterCommand.Execute(null);
        viewModel.IncrementCounterCommand.Execute(null);
        var currentValue = viewModel.CounterValue;

        viewModel.DecrementCounterCommand.Execute(null);

        Assert.Equal(currentValue - 1, viewModel.CounterValue);
    }

    [Fact]
    public void ResetCounter_ShouldSetCounterToZero()
    {
        var projectPath = Path.Combine(_testDir, "test_project");
        var app = new Core.ProjectManager();
        app.NewProject(projectPath, "TestProject", "1.0.0", [
            new("logger", "AppLogger"),
            new("counter", "OperationCounter", Parameters: new() { ["step"] = 1 })
        ]);

        var viewModel = new MainViewModel();
        var projectFile = Path.Combine(projectPath, "TestProject.json");
        
        var pmField = typeof(MainViewModel).GetField("_projectManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var vmPm = pmField?.GetValue(viewModel) as Core.ProjectManager;
        vmPm?.OpenProject(projectFile);
        
        var updateMethod = typeof(MainViewModel).GetMethod("UpdateDisplay", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        updateMethod?.Invoke(viewModel, null);

        viewModel.IncrementCounterCommand.Execute(null);
        viewModel.IncrementCounterCommand.Execute(null);

        viewModel.ResetCounterCommand.Execute(null);

        Assert.Equal(0, viewModel.CounterValue);
        Assert.Contains("已重置", viewModel.StatusMessage);
    }

    [Fact]
    public void AddLog_WithEmptyMessage_ShouldNotAddLog()
    {
        var projectPath = Path.Combine(_testDir, "test_project");
        var app = new Core.ProjectManager();
        app.NewProject(projectPath, "TestProject", "1.0.0", [
            new("logger", "AppLogger"),
            new("counter", "OperationCounter", Parameters: new() { ["step"] = 1 })
        ]);

        var viewModel = new MainViewModel();
        var projectFile = Path.Combine(projectPath, "TestProject.json");
        
        var pmField = typeof(MainViewModel).GetField("_projectManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var vmPm = pmField?.GetValue(viewModel) as Core.ProjectManager;
        vmPm?.OpenProject(projectFile);
        
        var updateMethod = typeof(MainViewModel).GetMethod("UpdateDisplay", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        updateMethod?.Invoke(viewModel, null);

        viewModel.LogMessage = "";
        var initialLogCount = viewModel.Logs.Count;

        viewModel.AddLogCommand.Execute(null);

        Assert.Equal(initialLogCount, viewModel.Logs.Count);
    }

    [Fact]
    public void ViewModel_Properties_ShouldHaveDefaultValues()
    {
        var viewModel = new MainViewModel();

        Assert.Equal("Ready", viewModel.StatusMessage);
        Assert.Equal(0, viewModel.CounterValue);
        Assert.Empty(viewModel.LogMessage);
        Assert.NotNull(viewModel.Logs);
    }
}
