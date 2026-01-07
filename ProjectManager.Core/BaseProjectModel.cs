namespace ProjectManager.Core;

/// <summary>模型基类</summary>
public abstract class BaseProjectModel(ModelProjectConfig projectConfig)
{
    public ModelProjectConfig ProjectConfig { get; set; } = projectConfig;

    public abstract void Execute(params object[] args);

    public virtual void LoadConfig()
    {
        var data = FileManager.LoadJson<ModelConfigData>(ProjectConfig.ConfigPath);
        ProjectConfig = ProjectConfig with
        {
            ModelName = data.ModelName ?? ProjectConfig.ModelName,
            ModelType = data.ModelType ?? ProjectConfig.ModelType,
            Description = data.Description ?? ProjectConfig.Description,
            Parameters = data.Parameters ?? []
        };
    }

    public virtual void SaveConfig()
    {
        var data = new ModelConfigData
        {
            ModelName = ProjectConfig.ModelName,
            ModelType = ProjectConfig.ModelType,
            Description = ProjectConfig.Description,
            Parameters = ProjectConfig.Parameters
        };
        FileManager.SaveJson(ProjectConfig.ConfigPath, data);
    }
}

public class ModelConfigData
{
    public string? ModelName { get; set; }
    public string? ModelType { get; set; }
    public string? Description { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
}
