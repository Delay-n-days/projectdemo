namespace ProjectManager.Core;

/// <summary>模型工厂:通过注册字典创建模型</summary>
public static class ModelFactory
{
    private static readonly Dictionary<string, Func<ModelConfig, BaseModel>> Registry = new()
    {
        ["logger"] = config => new LoggerModel(config),
        ["counter"] = config => new CounterModel(config)
    };

    public static void Register(string modelType, Func<ModelConfig, BaseModel> factory) =>
        Registry[modelType] = factory;

    public static BaseModel Create(string modelType, ModelConfig config) =>
        Registry.TryGetValue(modelType, out var factory)
            ? factory(config)
            : throw new ArgumentException($"Unknown model type: {modelType}. Available: {string.Join(", ", Registry.Keys)}");

    public static IEnumerable<string> GetAvailableTypes() => Registry.Keys;
}
