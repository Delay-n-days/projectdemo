namespace ProjectManager.Core;

/// <summary>模型基类</summary>
public abstract class BaseModel(ModelConfig config)
{
    public ModelConfig Config { get; set; } = config;

    public abstract void Execute(params object[] args);

    public virtual void LoadConfig()
    {
        var data = FileManager.LoadJson<ModelConfigData>(Config.ConfigPath);
        Config = Config with
        {
            ModelName = data.ModelName ?? Config.ModelName,
            ModelType = data.ModelType ?? Config.ModelType,
            Description = data.Description ?? Config.Description,
            Parameters = data.Parameters ?? []
        };
    }

    public virtual void SaveConfig()
    {
        var data = new ModelConfigData
        {
            ModelName = Config.ModelName,
            ModelType = Config.ModelType,
            Description = Config.Description,
            Parameters = Config.Parameters
        };
        FileManager.SaveJson(Config.ConfigPath, data);
    }
}

public class ModelConfigData
{
    public string? ModelName { get; set; }
    public string? ModelType { get; set; }
    public string? Description { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
}
