namespace ProjectManager.Core;

/// <summary>模型工厂:通过注册字典创建模型</summary>
public static class ModelFactory
{
    private static readonly Dictionary<string, Func<ModelProjectConfig, BaseProjectModel>> Registry = new()
    {
        ["logger"] = config => new LoggerProjectModel(config),
        ["counter"] = config => new CounterProjectModel(config)
    };

    public static void Register(string modelType, Func<ModelProjectConfig, BaseProjectModel> factory) =>
        Registry[modelType] = factory;

    public static BaseProjectModel Create(string modelType, ModelProjectConfig projectConfig) =>
        Registry.TryGetValue(modelType, out var factory)
            ? factory(projectConfig)
            : throw new ArgumentException($"Unknown model type: {modelType}. Available: {string.Join(", ", Registry.Keys)}");

    public static IEnumerable<string> GetAvailableTypes() => Registry.Keys;
}
