# ProjectManager - 项目管理系统

一个基于 .NET 9 的项目管理框架，提供灵活的模型化项目管理功能，支持多种项目模型类型的动态创建和管理。

> **注意**: 本文档中的代码行号引用基于当前代码库版本。随着代码演进，行号可能会发生变化。建议使用文件路径和类/方法名称作为主要导航方式。

## 📋 目录

- [项目概述](#项目概述)
- [项目架构](#项目架构)
- [核心功能](#核心功能)
- [代码结构](#代码结构)
- [核心类索引](#核心类索引)
- [快速开始](#快速开始)
- [使用示例](#使用示例)
- [模型类型](#模型类型)
- [项目操作](#项目操作)
- [测试](#测试)
- [技术栈](#技术栈)

---

## 项目概述

ProjectManager 是一个模块化的项目管理系统，采用模型工厂模式设计，支持通过不同的模型类型（如日志记录器、计数器等）来管理项目的各种功能。系统提供了完整的项目生命周期管理，包括创建、打开、保存、另存为以及从模板创建等功能。

### 主要特性

- 🏗️ **模块化架构**：基于抽象基类的可扩展模型系统
- 📦 **多项目支持**：支持同时管理多个项目实例
- 🔧 **灵活配置**：基于 JSON 的配置文件管理
- 🎨 **多种界面**：提供控制台和 Avalonia UI 两种界面
- ✅ **完整测试**：包含单元测试覆盖核心功能
- 🔄 **模板系统**：支持从现有项目创建新项目

---

## 项目架构

```
ProjectManager/
├── ProjectManager.Core/          # 核心业务逻辑层
│   ├── BaseProjectModel.cs      # 模型基类
│   ├── ProjectManager.cs         # 项目管理核心类
│   ├── ModelFactory.cs           # 模型工厂
│   ├── FileManager.cs            # 文件管理器
│   ├── ValidationRules.cs        # 验证规则
│   ├── CounterProjectModel.cs    # 计数器模型
│   ├── LoggerProjectModel.cs     # 日志记录器模型
│   ├── ProjectConfig.cs          # 项目配置
│   └── ModelProjectConfig.cs     # 模型配置
│
├── ProjectManager.Console/        # 控制台应用程序
│   └── Program.cs                # 控制台入口
│
├── ProjectManager.UI/             # Avalonia UI 应用程序
│   ├── ViewModels/               # 视图模型
│   │   ├── MainViewModel.cs      # 主窗口视图模型
│   │   ├── NewProjectViewModel.cs
│   │   ├── SaveAsViewModel.cs
│   │   └── TemplateSelectionViewModel.cs
│   ├── Views/                    # 视图
│   │   ├── MainWindow.axaml      # 主窗口
│   │   └── ...
│   └── Program.cs                # UI 入口
│
└── ProjectManager.Tests/          # 单元测试
    ├── ProjectManagerTests.cs
    ├── ModelsTests.cs
    ├── FileManagerTests.cs
    ├── ValidationRulesTests.cs
    └── MainViewModelTests.cs
```

---

## 核心功能

### 1. 项目管理
- **创建新项目**：通过配置创建全新的项目结构
- **打开项目**：加载现有项目及其所有模型
- **保存项目**：持久化项目配置和模型数据
- **另存为**：将项目复制到新位置
- **从模板创建**：基于现有项目创建新项目

### 2. 模型系统
- **模型工厂**：通过注册机制动态创建模型实例
- **可扩展性**：轻松添加新的模型类型
- **配置管理**：每个模型独立的 JSON 配置文件
- **参数化**：支持灵活的参数配置

### 3. 内置模型类型
- **Logger（日志记录器）**：支持不同级别的日志记录
- **Counter（计数器）**：支持步长和最大值限制的计数功能

---

## 代码结构

### 核心层次关系

```
BaseProjectModel (抽象基类)
    ├── LoggerProjectModel (日志记录器)
    └── CounterProjectModel (计数器)

ProjectManager (项目管理器)
    ├── ProjectConfig (项目配置)
    └── List<BaseProjectModel> (模型集合)

ModelFactory (工厂)
    └── 注册的模型创建函数
```

---

## 核心类索引

### 1. BaseProjectModel - 模型基类
**文件位置**: [`ProjectManager.Core/BaseProjectModel.cs`](https://github.com/Delay-n-days/projectdemo/blob/main/ProjectManager.Core/BaseProjectModel.cs)

所有具体模型类的抽象基类，定义了模型的基本结构和通用功能。

**核心成员**:
```csharp
// 第 13 行：模型配置属性
public ModelProjectConfig ProjectConfig { get; set; }

// 第 20 行：抽象执行方法
public abstract void Execute(params object[] args);

// 第 26 行：加载配置
public virtual void LoadConfig()

// 第 42 行：保存配置  
public virtual void SaveConfig()
```

**关键代码片段**:
```csharp
// 第 7-21 行：基类定义
public abstract class BaseProjectModel(ModelProjectConfig projectConfig)
{
    public ModelProjectConfig ProjectConfig { get; set; } = projectConfig;
    
    public abstract void Execute(params object[] args);
}
```

---

### 2. ProjectManager - 项目管理核心类
**文件位置**: [`ProjectManager.Core/ProjectManager.cs`](https://github.com/Delay-n-days/projectdemo/blob/main/ProjectManager.Core/ProjectManager.cs)

负责项目的完整生命周期管理，是系统的核心控制器。

**核心成员**:
```csharp
// 第 19 行：项目配置
public ProjectConfig? ProjectConfig { get; private set; }

// 第 25 行：模型集合
public List<BaseProjectModel> Models { get; private set; } = [];

// 第 43 行：打开项目
public void OpenProject(string projectJsonPath)

// 第 94 行：创建新项目
public void NewProject(string projectPath, string projectName, string version, 
                       List<ModelConfigDto> modelsConfig)

// 第 132 行：保存项目
public void SaveProject()

// 第 154 行：另存为
public void SaveAsProject(string newProjectPath)

// 第 185 行：从模板创建
public void CreateFromTemplate(string templateJsonPath, string newProjectPath)

// 第 211 行：获取单个模型
public BaseProjectModel? GetModel(string modelName)

// 第 219 行：按类型获取模型
public List<BaseProjectModel> GetModelsByType(string modelType)
```

**关键实现**:
```csharp
// 第 43-82 行：打开项目的完整流程
public void OpenProject(string projectJsonPath)
{
    // 1. 加载项目配置
    var data = FileManager.LoadJson<ProjectConfigData>(projectJsonPath);
    ProjectConfig = new() { ... };
    
    // 2. 遍历项目目录，发现并加载所有模型
    Models.Clear();
    foreach (var modelDir in Directory.GetDirectories(projectPath))
    {
        var model = ModelFactory.Create(modelConfig.ModelType, modelConfig);
        model.LoadConfig();
        Models.Add(model);
    }
}
```

---

### 3. LoggerProjectModel - 日志记录器模型
**文件位置**: [`ProjectManager.Core/LoggerProjectModel.cs`](https://github.com/Delay-n-days/projectdemo/blob/main/ProjectManager.Core/LoggerProjectModel.cs)

负责记录和管理项目中的日志信息，支持不同日志级别和最大条目限制。

**核心成员**:
```csharp
// 第 17 行：构造函数，支持日志级别和最大条目配置
public LoggerProjectModel(ModelProjectConfig projectConfig, 
                         string logLevel = "INFO", int maxEntries = 1000)

// 第 39 行：执行日志记录
public override void Execute(params object[] args)

// 第 93 行：获取日志列表
public List<string> GetLogs()

// 第 102 行：清空日志
public void ClearLogs()

// 第 109 行：设置日志级别
public void SetLogLevel(string level)
```

**使用示例**:
```csharp
// 第 39-59 行：日志记录实现
public override void Execute(params object[] args)
{
    var message = args[0].ToString();
    var level = args[1].ToString() ?? "INFO";
    var logEntry = $"[{level}] {message}";
    
    var logs = GetLogsInternal();
    logs.Add(logEntry);
    
    // 自动管理日志数量
    if (logs.Count > maxEntries)
        logs = logs.TakeLast(maxEntries).ToList();
}
```

**支持的日志级别**: DEBUG, INFO, WARNING, ERROR

---

### 4. CounterProjectModel - 计数器模型
**文件位置**: [`ProjectManager.Core/CounterProjectModel.cs`](https://github.com/Delay-n-days/projectdemo/blob/main/ProjectManager.Core/CounterProjectModel.cs)

提供计数功能，支持自定义步长和最大值限制。

**核心成员**:
```csharp
// 第 6 行：构造函数，支持步长和最大值配置
public CounterProjectModel(ModelProjectConfig projectConfig, 
                          int step = 1, int? maxCount = null)

// 第 14 行：执行计数操作
public override void Execute(params object[] args)

// 第 27 行：递增
public int Increment()

// 第 47 行：递减
public int Decrement()

// 第 56 行：重置
public int Reset()

// 第 62 行：获取当前计数
public int GetCount()

// 第 64 行：设置步长
public void SetStep(int step)

// 第 70 行：设置最大值
public void SetMaxCount(int? maxCount)
```

**关键实现**:
```csharp
// 第 27-45 行：递增逻辑，支持最大值自动重置
public int Increment()
{
    var step = Convert.ToInt32(ProjectConfig.Parameters["step"]);
    var maxCount = ProjectConfig.Parameters.GetValueOrDefault("maxCount");
    var count = Convert.ToInt32(ProjectConfig.Parameters["count"]);
    
    var newCount = count + step;
    
    // 达到最大值时自动重置
    if (maxCount.HasValue && newCount > maxCount.Value)
    {
        ProjectConfig.Parameters["count"] = 0;
        return 0;
    }
    
    ProjectConfig.Parameters["count"] = newCount;
    return newCount;
}
```

---

### 5. ModelFactory - 模型工厂
**文件位置**: [`ProjectManager.Core/ModelFactory.cs`](https://github.com/Delay-n-days/projectdemo/blob/main/ProjectManager.Core/ModelFactory.cs)

通过注册字典模式创建模型实例，支持动态扩展。

**核心成员**:
```csharp
// 第 6 行：注册字典
private static readonly Dictionary<string, Func<ModelProjectConfig, BaseProjectModel>> Registry

// 第 12 行：注册新模型类型
public static void Register(string modelType, Func<ModelProjectConfig, BaseProjectModel> factory)

// 第 15 行：创建模型实例
public static BaseProjectModel Create(string modelType, ModelProjectConfig projectConfig)

// 第 20 行：获取可用模型类型
public static IEnumerable<string> GetAvailableTypes()
```

**默认注册的模型**:
```csharp
// 第 6-10 行：内置模型注册
private static readonly Dictionary<string, Func<ModelProjectConfig, BaseProjectModel>> Registry = new()
{
    ["logger"] = config => new LoggerProjectModel(config),
    ["counter"] = config => new CounterProjectModel(config)
};
```

**扩展新模型**:
```csharp
// 注册自定义模型
ModelFactory.Register("mymodel", config => new MyCustomModel(config));
```

---

### 6. FileManager - 文件管理器
**文件位置**: [`ProjectManager.Core/FileManager.cs`](https://github.com/Delay-n-days/projectdemo/blob/main/ProjectManager.Core/FileManager.cs)

统一处理所有文件操作和 JSON 序列化/反序列化。

**核心成员**:
```csharp
// 第 9 行：JSON 序列化配置
private static readonly JsonSerializerSettings JsonSettings

// 第 16 行：加载 JSON 文件
public static T LoadJson<T>(string filePath)

// 第 26 行：保存 JSON 文件
public static void SaveJson<T>(string filePath, T data)

// 第 36 行：确保目录存在
public static void EnsureDirectory(string path)
```

**配置特性**:
```csharp
// 第 9-14 行：JSON 配置
private static readonly JsonSerializerSettings JsonSettings = new()
{
    Formatting = Formatting.Indented,              // 格式化输出
    ContractResolver = new CamelCasePropertyNamesContractResolver(),  // 驼峰命名
    NullValueHandling = NullValueHandling.Ignore   // 忽略 null 值
};
```

---

### 7. ValidationRules - 验证规则
**文件位置**: [`ProjectManager.Core/ValidationRules.cs`](https://github.com/Delay-n-days/projectdemo/blob/main/ProjectManager.Core/ValidationRules.cs)

集中管理所有验证规则，确保数据的有效性。

**核心成员**:
```csharp
// 第 8 行：最大版本限制
private const string MaxVersion = "1.0.0";

// 第 10 行：版本格式正则表达式
[GeneratedRegex(@"^\d+\.\d+\.\d+$")]
private static partial Regex VersionPattern();

// 第 13 行：验证版本号
public static void ValidateVersion(string version)
```

**验证逻辑**:
```csharp
// 第 13-20 行：版本验证
public static void ValidateVersion(string version)
{
    // 1. 检查格式 (X.Y.Z)
    if (!VersionPattern().IsMatch(version))
        throw new ArgumentException("Invalid version format");
    
    // 2. 检查版本上限
    if (ParseVersion(version).CompareTo(ParseVersion(MaxVersion)) > 0)
        throw new ArgumentException("Version exceeds maximum");
}
```

---

### 8. MainViewModel - UI 视图模型
**文件位置**: [`ProjectManager.UI/ViewModels/MainViewModel.cs`](https://github.com/Delay-n-days/projectdemo/blob/main/ProjectManager.UI/ViewModels/MainViewModel.cs)

Avalonia UI 应用的主视图模型，连接 UI 和核心业务逻辑。

**核心成员**:
```csharp
// 第 16 行：项目管理器实例
private readonly Core.ProjectManager _projectManager = new();

// 第 31 行：打开项目命令
[RelayCommand] private async Task OpenProject()

// 第 48 行：添加日志命令
[RelayCommand] private void AddLog()

// 第 64 行：递增计数器命令
[RelayCommand] private void IncrementCounter()

// 第 77 行：递减计数器命令
[RelayCommand] private void DecrementCounter()

// 第 90 行：重置计数器命令
[RelayCommand] private void ResetCounter()

// 第 103 行：另存为命令
[RelayCommand] private async Task SaveAsProject()

// 第 134 行：从模板创建命令
[RelayCommand] private async Task CreateFromTemplate()
```

---

## 快速开始

### 前置要求

- .NET 9 SDK
- 支持的操作系统：Windows, Linux, macOS

### 构建项目

```bash
# 克隆仓库
git clone https://github.com/Delay-n-days/projectdemo.git
cd projectdemo

# 还原依赖
dotnet restore

# 构建解决方案
dotnet build

# 运行测试
dotnet test
```

### 运行控制台应用

```bash
cd ProjectManager.Console
dotnet run
```

### 运行 UI 应用

```bash
cd ProjectManager.UI
dotnet run
```

---

## 使用示例

### 示例 1: 创建新项目

**代码位置**: [`ProjectManager.Console/Program.cs`](https://github.com/Delay-n-days/projectdemo/blob/main/ProjectManager.Console/Program.cs) (第 12-31 行，参考)

```csharp
var app = new ProjectManager.Core.ProjectManager();

// 定义项目模型
var models = new List<ModelConfigDto>
{
    new("logger", "main_logger", "主日志记录器", 
        new() { ["logLevel"] = "INFO" }),
    new("counter", "operation_counter", "操作计数器", 
        new() { ["step"] = 1, ["maxCount"] = 100 })
};

// 创建新项目
app.NewProject("/path/to/project", "MyProject", "1.0.0", models);

// 使用模型
var logger = app.GetModel("main_logger") as LoggerProjectModel;
logger!.Execute("Project created", "INFO");

var counter = app.GetModel("operation_counter") as CounterProjectModel;
counter!.Increment();

// 保存项目
app.SaveProject();
```

### 示例 2: 打开现有项目

**代码位置**: [`ProjectManager.Console/Program.cs`](https://github.com/Delay-n-days/projectdemo/blob/main/ProjectManager.Console/Program.cs) (第 34-40 行，参考)

```csharp
var app = new ProjectManager.Core.ProjectManager();
app.OpenProject("/path/to/project/MyProject.json");

// 访问项目信息
Console.WriteLine($"项目: {app.ProjectConfig?.ProjectName}");
Console.WriteLine($"版本: {app.ProjectConfig?.Version}");

// 访问模型
var logger = app.GetModel("main_logger") as LoggerProjectModel;
var logs = logger?.GetLogs();
```

### 示例 3: 从模板创建项目

**代码位置**: [`ProjectManager.Console/Program.cs`](https://github.com/Delay-n-days/projectdemo/blob/main/ProjectManager.Console/Program.cs) (第 43-54 行，参考)

```csharp
var app = new ProjectManager.Core.ProjectManager();

// 从现有项目创建新项目
app.CreateFromTemplate(
    "/path/to/template/MyProject.json",
    "/path/to/new/project"
);

// 新项目已包含模板的所有模型
var logger = app.GetModel("main_logger") as LoggerProjectModel;
logger!.Execute("Created from template", "INFO");
app.SaveProject();
```

### 示例 4: 计数器最大值测试

**代码位置**: [`ProjectManager.Console/Program.cs`](https://github.com/Delay-n-days/projectdemo/blob/main/ProjectManager.Console/Program.cs) (第 57-73 行，参考)

```csharp
var app = new ProjectManager.Core.ProjectManager();
app.NewProject(
    "/path/to/counter_test",
    "CounterTest",
    "0.8.0",
    [new("counter", "limited_counter", 
         Parameters: new() { ["step"] = 1, ["maxCount"] = 5 })]
);

var counter = app.GetModel("limited_counter") as CounterProjectModel;

// 测试最大值限制 - 到达 5 后自动重置为 0
for (int i = 0; i < 7; i++)
{
    var count = counter!.Increment();
    Console.WriteLine($"Count = {count}");
}
// 输出: 1, 2, 3, 4, 5, 0, 1
```

---

## 模型类型

### Logger（日志记录器）

**参数配置**:
```json
{
  "logLevel": "INFO",
  "maxEntries": 1000,
  "logs": []
}
```

**操作**:
- `Execute(message, level)` - 记录日志
- `GetLogs()` - 获取所有日志
- `ClearLogs()` - 清空日志
- `SetLogLevel(level)` - 设置日志级别

**支持的日志级别**: DEBUG, INFO, WARNING, ERROR

### Counter（计数器）

**参数配置**:
```json
{
  "count": 0,
  "step": 1,
  "maxCount": 100
}
```

**操作**:
- `Execute("increment")` - 递增
- `Execute("decrement")` - 递减  
- `Execute("reset")` - 重置
- `GetCount()` - 获取当前值
- `SetStep(step)` - 设置步长
- `SetMaxCount(maxCount)` - 设置最大值

---

## 项目操作

### 项目配置文件结构

项目根目录下的主配置文件（`{ProjectName}.json`）:
```json
{
  "name": "MyProject",
  "version": "1.0.0"
}
```

### 模型配置文件结构

每个模型在独立目录下的配置文件（`{ModelName}/{ModelName}.json`）:
```json
{
  "modelName": "main_logger",
  "modelType": "logger",
  "description": "主日志记录器",
  "parameters": {
    "logLevel": "INFO",
    "maxEntries": 1000
  }
}
```

### 项目目录结构示例

```
MyProject/
├── MyProject.json              # 项目主配置
├── main_logger/                # 日志记录器模型
│   └── main_logger.json        # 模型配置
└── operation_counter/          # 计数器模型
    └── operation_counter.json  # 模型配置
```

---

## 测试

项目包含完整的单元测试套件，覆盖所有核心功能。

**测试文件位置**: [`ProjectManager.Tests/`](https://github.com/Delay-n-days/projectdemo/tree/main/ProjectManager.Tests)

### 运行所有测试

```bash
dotnet test
```

### 测试覆盖范围

- **ProjectManagerTests.cs** - 项目管理器核心功能测试
- **ModelsTests.cs** - 模型功能测试（Logger, Counter）
- **FileManagerTests.cs** - 文件操作测试
- **ValidationRulesTests.cs** - 验证规则测试
- **MainViewModelTests.cs** - UI 视图模型测试

### 关键测试用例

```csharp
// 创建和保存项目
[Fact] public void NewProject_CreatesValidProject()

// 打开项目
[Fact] public void OpenProject_LoadsConfiguration()

// 模型操作
[Fact] public void Logger_AddsAndRetrievesLogs()
[Fact] public void Counter_IncrementsCorrectly()

// 版本验证
[Fact] public void ValidateVersion_ThrowsOnInvalidFormat()
```

---

## 技术栈

### 核心框架
- **.NET 9** - 最新的 .NET 平台
- **C# 13** - 使用最新语言特性

### 依赖库
- **Newtonsoft.Json** - JSON 序列化/反序列化
- **Avalonia UI** - 跨平台桌面 UI 框架
- **CommunityToolkit.Mvvm** - MVVM 模式支持
- **xUnit** - 单元测试框架

### 设计模式
- **工厂模式** - ModelFactory 用于创建模型实例
- **抽象基类模式** - BaseProjectModel 提供统一接口
- **MVVM 模式** - UI 层采用 Model-View-ViewModel 架构
- **依赖注入** - 通过构造函数注入配置

### 代码特性
- **Records** - 使用 C# 记录类型简化配置类
- **Nullable Reference Types** - 启用可空引用类型检查
- **Primary Constructors** - 使用主构造函数简化代码
- **Pattern Matching** - 使用模式匹配简化逻辑

---

## 扩展开发

### 添加新的模型类型

1. **创建模型类**，继承 `BaseProjectModel`:

```csharp
namespace ProjectManager.Core;

public class MyCustomModel(ModelProjectConfig projectConfig) 
    : BaseProjectModel(projectConfig)
{
    public override void Execute(params object[] args)
    {
        // 实现自定义逻辑
    }
}
```

2. **注册到工厂**:

```csharp
ModelFactory.Register("mycustom", config => new MyCustomModel(config));
```

3. **使用新模型**:

```csharp
var models = new List<ModelConfigDto>
{
    new("mycustom", "my_instance", "我的自定义模型")
};
app.NewProject("/path", "Project", "1.0.0", models);
```

---

## 许可证

本项目为演示项目，用于学习和研究目的。

---

## 贡献

欢迎提交问题和拉取请求！

---

**最后更新**: 2026-01-07
