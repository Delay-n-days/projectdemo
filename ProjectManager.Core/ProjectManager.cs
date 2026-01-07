namespace ProjectManager.Core;

/// <summary>项目管理核心类</summary>
public class ProjectManager
{
    public ProjectConfig? ProjectConfig { get; private set; }
    public List<BaseProjectModel> Models { get; private set; } = [];
    public ProjectManager(string? projectJsonPath = null)
    {
        if (!string.IsNullOrEmpty(projectJsonPath))
            OpenProject(projectJsonPath);
    }
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

    public BaseProjectModel? GetModel(string modelName) =>
        Models.FirstOrDefault(m => m.ProjectConfig.ModelName == modelName);

    public List<BaseProjectModel> GetModelsByType(string modelType) =>
        Models.Where(m => m.ProjectConfig.ModelType == modelType).ToList();
}

public record ModelConfigDto(string Type, string Name, string? Description = null, Dictionary<string, object>? Parameters = null);

public class ProjectConfigData
{
    public string? Name { get; set; }
    public string? Version { get; set; }
}
