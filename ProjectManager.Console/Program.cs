using ProjectManager.Core;

Console.WriteLine("=== ProjectManager .NET 9 Demo ===\n");

var demoDir = Path.Combine(Path.GetTempPath(), $"ProjectManager_Demo_{Guid.NewGuid():N}");
Directory.CreateDirectory(demoDir);
Console.WriteLine($"Demo Directory: {demoDir}\n");

try
{
    // 示例1: 创建新项目
    Console.WriteLine("=== Example 1: Create New Project ===");
    var app1 = new Application();
    var models1 = new List<ModelConfigDto>
    {
        new("logger", "main_logger", "主日志记录器", new() { ["logLevel"] = "INFO" }),
        new("counter", "operation_counter", "操作计数器", new() { ["step"] = 1, ["maxCount"] = 100 })
    };

    app1.NewProject(Path.Combine(demoDir, "project1"), "MyProject", "1.0.0", models1);

    var logger1 = app1.GetModel("main_logger") as LoggerModel;
    var counter1 = app1.GetModel("operation_counter") as CounterModel;

    logger1!.Execute("Project created", "INFO");
    counter1!.Increment();
    counter1.Increment();
    logger1.Execute($"Operations: {counter1.GetCount()}", "INFO");

    app1.SaveProject();
    Console.WriteLine($"✓ Project created with {logger1.GetLogs().Count} logs and count: {counter1.GetCount()}\n");

    // 示例2: 打开现有项目
    Console.WriteLine("=== Example 2: Open Existing Project ===");
    var app2 = new Application(Path.Combine(demoDir, "project1", "MyProject.json"));
    var logger2 = app2.GetModel("main_logger") as LoggerModel;
    var counter2 = app2.GetModel("operation_counter") as CounterModel;

    Console.WriteLine($"✓ Project: {app2.ProjectConfig?.ProjectName}, Version: {app2.ProjectConfig?.Version}");
    Console.WriteLine($"✓ Current count: {counter2!.GetCount()}\n");

    // 示例3: 从模板创建
    Console.WriteLine("=== Example 3: Create From Template ===");
    var app3 = new Application();
    app3.CreateFromTemplate(
        Path.Combine(demoDir, "project1", "MyProject.json"),
        Path.Combine(demoDir, "project_from_template")
    );
    
    var logger3 = app3.GetModel("main_logger") as LoggerModel;
    logger3!.Execute("Created from template", "INFO");
    app3.SaveProject();
    
    Console.WriteLine($"✓ Template project created successfully\n");

    // 示例4: 测试计数器最大值
    Console.WriteLine("=== Example 4: Counter Max Count Test ===");
    var app4 = new Application();
    app4.NewProject(
        Path.Combine(demoDir, "counter_test"),
        "CounterTest",
        "0.8.0",
        [new("counter", "limited_counter", Parameters: new() { ["step"] = 1, ["maxCount"] = 5 })]
    );

    var counter4 = app4.GetModel("limited_counter") as CounterModel;
    Console.WriteLine("Max count set to 5:");
    
    for (int i = 0; i < 7; i++)
    {
        var count = counter4!.Increment();
        Console.WriteLine($"  Increment #{i + 1}: count = {count}");
    }

    Console.WriteLine("\n=== All Examples Completed! ===");
}
finally
{
    Console.WriteLine($"\nCleaning up: {demoDir}");
    Directory.Delete(demoDir, true);
}
