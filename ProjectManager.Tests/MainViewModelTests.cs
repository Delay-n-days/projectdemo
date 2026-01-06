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

    [Fact]
    public void CreateProject_ShouldCreateProjectAndUpdateStatus()
    {
        var viewModel = new MainViewModel
        {
            ProjectPath = Path.Combine(_testDir, "test_project"),
            ProjectName = "TestProject",
            Version = "1.0.0"
        };

        viewModel.CreateProjectCommand.Execute(null);

        Assert.Contains("创建成功", viewModel.StatusMessage);
        Assert.True(File.Exists(Path.Combine(viewModel.ProjectPath, "TestProject.json")));
    }

    [Fact]
    public void OpenProject_ShouldLoadProjectAndUpdateStatus()
    {
        var projectPath = Path.Combine(_testDir, "test_project");
        var app = new Application();
        app.NewProject(projectPath, "TestProject", "1.0.0", [
            new("logger", "AppLogger"),
            new("counter", "OperationCounter", Parameters: new() { ["step"] = 1 })
        ]);

        var viewModel = new MainViewModel
        {
            ProjectPath = projectPath,
            ProjectName = "TestProject"
        };

        viewModel.OpenProjectCommand.Execute(null);

        Assert.Contains("已打开", viewModel.StatusMessage);
        Assert.Contains("TestProject", viewModel.StatusMessage);
    }

    [Fact]
    public void AddLog_ShouldAddLogEntryAndUpdateDisplay()
    {
        var projectPath = Path.Combine(_testDir, "test_project");
        var viewModel = new MainViewModel
        {
            ProjectPath = projectPath,
            ProjectName = "TestProject",
            Version = "1.0.0"
        };

        viewModel.CreateProjectCommand.Execute(null);
        viewModel.LogMessage = "Test log entry";
        viewModel.AddLogCommand.Execute(null);

        Assert.Contains("日志已添加", viewModel.StatusMessage);
        Assert.Single(viewModel.Logs, log => log.Contains("Test log entry"));
        Assert.Empty(viewModel.LogMessage);
    }

    [Fact]
    public void IncrementCounter_ShouldIncreaseCounterValue()
    {
        var projectPath = Path.Combine(_testDir, "test_project");
        var viewModel = new MainViewModel
        {
            ProjectPath = projectPath,
            ProjectName = "TestProject",
            Version = "1.0.0"
        };

        viewModel.CreateProjectCommand.Execute(null);
        var initialValue = viewModel.CounterValue;

        viewModel.IncrementCounterCommand.Execute(null);

        Assert.Equal(initialValue + 1, viewModel.CounterValue);
        Assert.Contains("计数器", viewModel.StatusMessage);
    }

    [Fact]
    public void DecrementCounter_ShouldDecreaseCounterValue()
    {
        var projectPath = Path.Combine(_testDir, "test_project");
        var viewModel = new MainViewModel
        {
            ProjectPath = projectPath,
            ProjectName = "TestProject",
            Version = "1.0.0"
        };

        viewModel.CreateProjectCommand.Execute(null);
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
        var viewModel = new MainViewModel
        {
            ProjectPath = projectPath,
            ProjectName = "TestProject",
            Version = "1.0.0"
        };

        viewModel.CreateProjectCommand.Execute(null);
        viewModel.IncrementCounterCommand.Execute(null);
        viewModel.IncrementCounterCommand.Execute(null);

        viewModel.ResetCounterCommand.Execute(null);

        Assert.Equal(0, viewModel.CounterValue);
        Assert.Contains("已重置", viewModel.StatusMessage);
    }

    [Fact]
    public void SaveAsProject_ShouldCopyProjectToNewLocation()
    {
        var projectPath = Path.Combine(_testDir, "test_project");
        var saveAsPath = Path.Combine(_testDir, "test_project_copy");
        
        var viewModel = new MainViewModel
        {
            ProjectPath = projectPath,
            ProjectName = "TestProject",
            Version = "1.0.0",
            SaveAsPath = saveAsPath
        };

        viewModel.CreateProjectCommand.Execute(null);
        viewModel.IncrementCounterCommand.Execute(null);

        viewModel.SaveAsProjectCommand.Execute(null);

        Assert.Contains("另存为", viewModel.StatusMessage);
        Assert.True(File.Exists(Path.Combine(saveAsPath, "TestProject.json")));
    }

    [Fact]
    public void SaveAsProject_WithoutOpenProject_ShouldShowError()
    {
        var viewModel = new MainViewModel();
        viewModel.SaveAsProjectCommand.Execute(null);

        Assert.Contains("错误", viewModel.StatusMessage);
        Assert.Contains("没有打开的项目", viewModel.StatusMessage);
    }

    [Fact]
    public void CreateFromTemplate_ShouldCreateNewProjectFromTemplate()
    {
        var templatePath = Path.Combine(_testDir, "template_project");
        var newProjectPath = Path.Combine(_testDir, "new_from_template");

        // Create template
        var app = new Application();
        var models = new List<ModelConfigDto>
        {
            new("logger", "AppLogger"),
            new("counter", "OperationCounter", Parameters: new() { ["step"] = 2 })
        };
        app.NewProject(templatePath, "TemplateProject", "1.0.0", models);

        var viewModel = new MainViewModel
        {
            TemplatePath = Path.Combine(templatePath, "TemplateProject.json"),
            NewProjectPath = newProjectPath
        };

        viewModel.CreateFromTemplateCommand.Execute(null);

        Assert.Contains("从模板创建项目成功", viewModel.StatusMessage);
        Assert.True(File.Exists(Path.Combine(newProjectPath, "new_from_template.json")));
    }

    [Fact]
    public void CreateFromTemplate_WithEmptyPaths_ShouldShowError()
    {
        var viewModel = new MainViewModel
        {
            TemplatePath = "",
            NewProjectPath = ""
        };

        viewModel.CreateFromTemplateCommand.Execute(null);

        Assert.Contains("错误", viewModel.StatusMessage);
        Assert.Contains("请输入模板路径和新项目路径", viewModel.StatusMessage);
    }

    [Fact]
    public void CreateProject_WithInvalidVersion_ShouldShowError()
    {
        var viewModel = new MainViewModel
        {
            ProjectPath = Path.Combine(_testDir, "test_project"),
            ProjectName = "TestProject",
            Version = "2.0.0" // Invalid version (exceeds max)
        };

        viewModel.CreateProjectCommand.Execute(null);

        Assert.Contains("错误", viewModel.StatusMessage);
    }

    [Fact]
    public void AddLog_WithEmptyMessage_ShouldNotAddLog()
    {
        var projectPath = Path.Combine(_testDir, "test_project");
        var viewModel = new MainViewModel
        {
            ProjectPath = projectPath,
            ProjectName = "TestProject",
            Version = "1.0.0"
        };

        viewModel.CreateProjectCommand.Execute(null);
        viewModel.LogMessage = "";
        var initialLogCount = viewModel.Logs.Count;

        viewModel.AddLogCommand.Execute(null);

        Assert.Equal(initialLogCount, viewModel.Logs.Count);
    }

    [Fact]
    public void ViewModel_Properties_ShouldHaveDefaultValues()
    {
        var viewModel = new MainViewModel();

        Assert.NotNull(viewModel.ProjectPath);
        Assert.NotNull(viewModel.ProjectName);
        Assert.NotNull(viewModel.Version);
        Assert.Equal("Ready", viewModel.StatusMessage);
        Assert.Equal(0, viewModel.CounterValue);
        Assert.NotNull(viewModel.SaveAsPath);
        Assert.NotNull(viewModel.TemplatePath);
        Assert.NotNull(viewModel.NewProjectPath);
    }
}
