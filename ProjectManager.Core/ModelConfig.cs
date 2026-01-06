namespace ProjectManager.Core;

/// <summary>模型配置数据类</summary>
public record ModelConfig(
    string ModelName,
    string ModelType,
    string Description,
    Dictionary<string, object> Parameters,
    string ProjectPath)
{
    public string ModelPath => Path.Combine(ProjectPath, ModelName);
    public string ConfigPath => Path.Combine(ModelPath, $"{ModelName}.json");
}
