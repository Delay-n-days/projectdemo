using ProjectManager.Core;

namespace ProjectManager.Tests;

public class ProjectManagerTests : IDisposable
{
    private readonly string _testDir;

    public ProjectManagerTests()
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
    public void NewProject_ShouldCreateProjectAndModels()
    {
        var projectPath = Path.Combine(_testDir, "test_project");
        var app = new Application();

        var models = new List<ModelConfigDto>
        {
            new("logger", "logger1"),
            new("counter", "counter1", Parameters: new() { ["step"] = 2 })
        };

        app.NewProject(projectPath, "TestProject", "1.0.0", models);

        Assert.True(File.Exists(Path.Combine(projectPath, "TestProject.json")));
        
        var logger = app.GetModel("logger1");
        Assert.NotNull(logger);
        Assert.IsType<LoggerModel>(logger);

        var counter = app.GetModel("counter1") as CounterModel;
        Assert.NotNull(counter);
        Assert.Equal(2, counter.Config.Parameters["step"]);
    }

    [Fact]
    public void OpenProject_ShouldLoadProjectAndModels()
    {
        var projectPath = Path.Combine(_testDir, "test_project");
        var app = new Application();

        app.NewProject(projectPath, "TestProject", "0.9.0", [new("counter", "counter1")]);
        
        var counter = app.GetModel("counter1") as CounterModel;
        counter!.Increment();
        counter.Increment();
        app.SaveProject();

        var app2 = new Application(Path.Combine(projectPath, "TestProject.json"));
        var counter2 = app2.GetModel("counter1") as CounterModel;
        
        Assert.Equal(2, counter2!.GetCount());
    }

    [Fact]
    public void SaveAsProject_ShouldCopyProjectToNewLocation()
    {
        var projectPath = Path.Combine(_testDir, "test_project");
        var newPath = Path.Combine(_testDir, "test_project_copy");

        var app = new Application();
        app.NewProject(projectPath, "TestProject", "1.0.0", [new("counter", "counter1")]);

        var counter = app.GetModel("counter1") as CounterModel;
        counter!.Increment();

        app.SaveAs(newPath);

        Assert.True(File.Exists(Path.Combine(newPath, "TestProject.json")));

        var app2 = new Application(Path.Combine(newPath, "TestProject.json"));
        var counter2 = app2.GetModel("counter1") as CounterModel;
        
        Assert.Equal(1, counter2!.GetCount());
    }

    [Fact]
    public void CreateFromTemplate_ShouldCreateNewProjectFromTemplate()
    {
        var templatePath = Path.Combine(_testDir, "template_project");
        var newPath = Path.Combine(_testDir, "new_from_template");

        var app = new Application();
        var models = new List<ModelConfigDto>
        {
            new("logger", "logger1", Parameters: new() { ["logLevel"] = "DEBUG" }),
            new("counter", "counter1", Parameters: new() { ["step"] = 5 })
        };

        app.NewProject(templatePath, "TemplateProject", "1.0.0", models);

        var app2 = new Application();
        app2.CreateFromTemplate(Path.Combine(templatePath, "TemplateProject.json"), newPath);

        Assert.True(File.Exists(Path.Combine(newPath, "new_from_template.json")));

        var logger = app2.GetModel("logger1") as LoggerModel;
        Assert.Equal("DEBUG", logger!.Config.Parameters["logLevel"]?.ToString());

        var counter = app2.GetModel("counter1") as CounterModel;
        Assert.Equal(5, Convert.ToInt32(counter!.Config.Parameters["step"]));
    }

    [Fact]
    public void NewProject_InvalidVersion_ShouldThrowException()
    {
        var projectPath = Path.Combine(_testDir, "test_project");
        var app = new Application();

        Assert.Throws<ArgumentException>(() =>
            app.NewProject(projectPath, "TestProject", "1.0.1", []));
    }
}
