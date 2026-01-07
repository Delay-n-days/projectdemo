namespace ProjectManager.Core;

/// <summary>
/// 项目管理核心类
/// 负责项目的生命周期管理，包括：
/// - 打开现有项目
/// - 创建新项目
/// - 保存项目配置
/// - 另存为新项目
/// - 从模板创建项目
/// - 管理项目中的各种模型实例
/// </summary>
public class ProjectManager
{
    /// <summary>
    /// 当前项目的配置信息
    /// 包含项目名称、版本号、项目路径、配置文件路径等基本信息
    /// </summary>
    public ProjectConfig? ProjectConfig { get; private set; }

    /// <summary>
    /// 项目中所有模型实例的集合
    /// 每个模型对应项目中的一个功能模块（如日志模型、计数器模型等）
    /// </summary>
    public List<BaseProjectModel> Models { get; private set; } = [];

    /// <summary>
    /// 构造函数：初始化项目管理器
    /// </summary>
    /// <param name="projectJsonPath">可选的项目配置文件路径，如果提供则自动打开该项目</param>
    public ProjectManager(string? projectJsonPath = null)
    {
        if (!string.IsNullOrEmpty(projectJsonPath))
            OpenProject(projectJsonPath);
    }
    /// <summary>
    /// 打开现有项目
    /// 从指定的JSON配置文件加载项目信息，并自动发现和初始化项目目录下的所有模型
    /// </summary>
    /// <param name="projectJsonPath">项目主配置文件的完整路径（.json文件）</param>
    /// <exception cref="ArgumentException">当项目路径无效时抛出</exception>
    /// <exception cref="InvalidOperationException">当无法加载配置文件时抛出</exception>
    public void OpenProject(string projectJsonPath)
    {
        var projectPath = Path.GetDirectoryName(projectJsonPath) 
            ?? throw new ArgumentException("Invalid project path");

        var data = FileManager.LoadJson<ProjectConfigData>(projectJsonPath);
        ProjectConfig = new()
        {
            ProjectName = data.Name ?? "",
            Version = data.Version ?? "1.0.0",
            ProjectPath = projectPath,
            ConfigPath = projectJsonPath
        };

        Models.Clear();
        if (Directory.Exists(projectPath))
        {
            foreach (var modelDir in Directory.GetDirectories(projectPath))
            {
                var modelName = Path.GetFileName(modelDir);
                var modelConfigPath = Path.Combine(modelDir, $"{modelName}.json");
                
                if (File.Exists(modelConfigPath))
                {
                    var modelData = FileManager.LoadJson<ModelConfigData>(modelConfigPath);
                    var modelConfig = new ModelProjectConfig(
                        modelName,
                        modelData.ModelType ?? "",
                        modelData.Description ?? "",
                        modelData.Parameters ?? [],
                        projectPath
                    );
                    
                    var model = ModelFactory.Create(modelConfig.ModelType, modelConfig);
                    model.LoadConfig();
                    Models.Add(model);
                }
            }
        }
    }

    /// <summary>
    /// 创建新项目
    /// 根据提供的参数创建全新的项目结构，包括项目配置和指定的模型配置
    /// </summary>
    /// <param name="projectPath">新项目的根目录路径</param>
    /// <param name="projectName">项目名称，将用于生成项目配置文件名</param>
    /// <param name="version">项目版本号，格式如 "1.0.0"</param>
    /// <param name="modelsConfig">要包含在项目中的模型配置列表</param>
    /// <exception cref="ArgumentException">当版本号格式不正确时抛出</exception>
    /// <exception cref="InvalidOperationException">当无法创建目录时抛出</exception>
    public void NewProject(string projectPath, string projectName, string version, List<ModelConfigDto> modelsConfig)
    {
        ValidationRules.ValidateVersion(version);
        FileManager.EnsureDirectory(projectPath);

        var projectJsonPath = Path.Combine(projectPath, $"{projectName}.json");
        ProjectConfig = new()
        {
            ProjectName = projectName,
            Version = version,
            ProjectPath = projectPath,
            ConfigPath = projectJsonPath
        };

        Models.Clear();
        foreach (var dto in modelsConfig)
        {
            var config = new ModelProjectConfig(
                dto.Name,
                dto.Type,
                dto.Description ?? "",
                dto.Parameters ?? [],
                projectPath
            );

            var model = ModelFactory.Create(dto.Type, config);
            Models.Add(model);
        }

        SaveProject();
    }

    /// <summary>
    /// 保存当前项目
    /// 将项目配置信息保存到配置文件，并保存所有模型的配置信息
    /// </summary>
    /// <exception cref="InvalidOperationException">当没有项目被加载时抛出</exception>
    /// <exception cref="IOException">当文件写入失败时抛出</exception>
    public void SaveProject()
    {
        if (ProjectConfig is null)
            throw new InvalidOperationException("No project loaded");

        var data = new ProjectConfigData
        {
            Name = ProjectConfig.ProjectName,
            Version = ProjectConfig.Version
        };

        FileManager.SaveJson(ProjectConfig.ConfigPath, data);
        Models.ForEach(m => m.SaveConfig());
    }

    /// <summary>
    /// 另存为新项目
    /// 将当前项目保存到新的目录路径，更新所有相关路径引用
    /// </summary>
    /// <param name="newProjectPath">新项目的根目录路径</param>
    /// <exception cref="InvalidOperationException">当没有项目被加载时抛出</exception>
    /// <exception cref="IOException">当文件操作失败时抛出</exception>
    public void SaveAsProject(string newProjectPath)
    {
        if (ProjectConfig is null)
            throw new InvalidOperationException("No project loaded");

        FileManager.EnsureDirectory(newProjectPath);
        
        // JSON文件名使用文件夹名
        var folderName = Path.GetFileName(newProjectPath);
        var newProjectJsonPath = Path.Combine(newProjectPath, $"{folderName}.json");

        ProjectConfig = ProjectConfig with
        {
            ProjectPath = newProjectPath,
            ConfigPath = newProjectJsonPath
        };

        foreach (var model in Models)
            model.ProjectConfig = model.ProjectConfig with { ProjectPath = newProjectPath };

        SaveProject();
    }

    /// <summary>
    /// 从模板创建新项目
    /// 基于现有项目的配置作为模板，创建一个全新的项目副本
    /// </summary>
    /// <param name="templateJsonPath">模板项目的配置文件路径</param>
    /// <param name="newProjectPath">新项目的根目录路径</param>
    /// <exception cref="InvalidOperationException">当模板项目无法加载时抛出</exception>
    /// <exception cref="IOException">当文件操作失败时抛出</exception>
    public void CreateFromTemplate(string templateJsonPath, string newProjectPath)
    {
        FileManager.EnsureDirectory(newProjectPath);
        OpenProject(templateJsonPath);

        var folderName = Path.GetFileName(newProjectPath);
        var newProjectJsonPath = Path.Combine(newProjectPath, $"{folderName}.json");

        ProjectConfig = ProjectConfig! with
        {
            ProjectPath = newProjectPath,
            ConfigPath = newProjectJsonPath
        };

        foreach (var model in Models)
            model.ProjectConfig = model.ProjectConfig with { ProjectPath = newProjectPath };

        SaveProject();
        OpenProject(newProjectJsonPath);
    }

    /// <summary>
    /// 根据模型名称获取单个模型实例
    /// </summary>
    /// <param name="modelName">模型的名称（通常是文件夹名）</param>
    /// <returns>匹配的模型实例，如果未找到则返回 null</returns>
    public BaseProjectModel? GetModel(string modelName) =>
        Models.FirstOrDefault(m => m.ProjectConfig.ModelName == modelName);

    /// <summary>
    /// 根据模型类型获取所有匹配的模型实例
    /// </summary>
    /// <param name="modelType">模型类型（如 "logger", "counter"）</param>
    /// <returns>所有匹配类型模型的列表</returns>
    public List<BaseProjectModel> GetModelsByType(string modelType) =>
        Models.Where(m => m.ProjectConfig.ModelType == modelType).ToList();
}

/// <summary>
/// 模型配置数据传输对象
/// 用于在创建新项目时传递模型的配置信息
/// </summary>
public record ModelConfigDto(string Type, string Name, string? Description = null, Dictionary<string, object>? Parameters = null);

/// <summary>
/// 项目主配置数据类
/// 对应项目根目录下的 .json 配置文件结构
/// </summary>
public class ProjectConfigData
{
    public string? Name { get; set; }
    public string? Version { get; set; }
}
