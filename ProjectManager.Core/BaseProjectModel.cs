namespace ProjectManager.Core;

/// <summary>
/// 模型基类
/// 所有具体模型类的抽象基类，定义了模型的基本结构和通用功能
/// </summary>
public abstract class BaseProjectModel(ModelProjectConfig projectConfig)
{
    /// <summary>
    /// 模型配置信息
    /// 包含模型名称、类型、描述和参数等配置信息
    /// </summary>
    public ModelProjectConfig ProjectConfig { get; set; } = projectConfig;

    /// <summary>
    /// 执行模型功能的抽象方法
    /// 子类必须实现此方法以定义具体的模型行为
    /// </summary>
    /// <param name="args">执行所需的参数数组</param>
    public abstract void Execute(params object[] args);

    /// <summary>
    /// 加载模型配置
    /// 从配置文件中读取模型配置信息并更新当前实例
    /// </summary>
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

    /// <summary>
    /// 保存模型配置
    /// 将当前模型配置信息保存到配置文件
    /// </summary>
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

/// <summary>
/// 模型配置数据类
/// 用于序列化和反序列化模型配置信息
/// </summary>
public class ModelConfigData
{
    /// <summary>
    /// 模型名称
    /// </summary>
    public string? ModelName { get; set; }
    
    /// <summary>
    /// 模型类型
    /// </summary>
    public string? ModelType { get; set; }
    
    /// <summary>
    /// 模型描述
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// 模型参数字典
    /// </summary>
    public Dictionary<string, object>? Parameters { get; set; }
}
