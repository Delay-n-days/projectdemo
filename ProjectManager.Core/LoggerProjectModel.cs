namespace ProjectManager.Core;

using Newtonsoft.Json.Linq;

/// <summary>
/// 日志记录器模型
/// 负责记录和管理项目中的日志信息，支持不同日志级别和最大条目限制
/// </summary>
public class LoggerProjectModel(ModelProjectConfig projectConfig) : BaseProjectModel(projectConfig)
{
    /// <summary>
    /// 构造函数：创建日志记录器模型实例
    /// </summary>
    /// <param name="projectConfig">项目配置信息</param>
    /// <param name="logLevel">默认日志级别，默认为"INFO"</param>
    /// <param name="maxEntries">最大日志条目数，默认为1000</param>
    public LoggerProjectModel(ModelProjectConfig projectConfig, string logLevel = "INFO", int maxEntries = 1000) : this(projectConfig)
    {
        EnsureParameters();
        ProjectConfig.Parameters.TryAdd("logLevel", logLevel);
        ProjectConfig.Parameters.TryAdd("maxEntries", maxEntries);
    }

    /// <summary>
    /// 确保必要的参数存在，包括日志列表、日志级别和最大条目数
    /// </summary>
    private void EnsureParameters()
    {
        ProjectConfig.Parameters.TryAdd("logs", new List<object>());
        ProjectConfig.Parameters.TryAdd("logLevel", "INFO");
        ProjectConfig.Parameters.TryAdd("maxEntries", 1000);
    }

    /// <summary>
    /// 执行日志记录操作
    /// 根据传入的参数记录日志信息，并自动管理日志数量
    /// </summary>
    /// <param name="args">日志参数，第一个参数为消息内容，第二个参数为日志级别</param>
    public override void Execute(params object[] args)
    {
        EnsureParameters();
        
        var message = args.Length > 0 ? args[0].ToString() : "";
        var level = args.Length > 1 ? args[1].ToString() : "INFO";
        
        var logEntry = $"[{level}] {message}";
        Console.WriteLine($"[LoggerModel] {logEntry}");

        var logs = GetLogsInternal();
        logs.Add(logEntry);
        ProjectConfig.Parameters["logs"] = logs;

        var maxEntries = Convert.ToInt32(ProjectConfig.Parameters.GetValueOrDefault("maxEntries", 1000));
        if (logs.Count > maxEntries)
        {
            var trimmedLogs = logs.TakeLast(maxEntries).ToList();
            ProjectConfig.Parameters["logs"] = trimmedLogs;
        }
    }

    /// <summary>
    /// 内部方法：获取日志列表，处理不同数据类型
    /// </summary>
    /// <returns>日志列表</returns>
    private List<object> GetLogsInternal()
    {
        var logsParam = ProjectConfig.Parameters.GetValueOrDefault("logs");
        
        // 处理从JSON反序列化的JArray
        if (logsParam is JArray jArray)
        {
            var logs = jArray.Select(x => (object)(x.ToString())).ToList();
            ProjectConfig.Parameters["logs"] = logs;
            return logs;
        }
        
        // 处理List<object>
        if (logsParam is List<object> list)
        {
            return list;
        }
        
        // 默认返回空列表
        var newList = new List<object>();
        ProjectConfig.Parameters["logs"] = newList;
        return newList;
    }

    /// <summary>
    /// 获取格式化后的日志列表
    /// </summary>
    /// <returns>格式化后的日志字符串列表</returns>
    public List<string> GetLogs()
    {
        var logs = GetLogsInternal();
        return logs.Select(x => x.ToString() ?? "").ToList();
    }

    /// <summary>
    /// 清空所有日志记录
    /// </summary>
    public void ClearLogs() => ProjectConfig.Parameters["logs"] = new List<object>();

    /// <summary>
    /// 设置日志级别
    /// </summary>
    /// <param name="level">日志级别，必须是 DEBUG, INFO, WARNING, ERROR 中的一个</param>
    /// <exception cref="ArgumentException">当设置无效的日志级别时抛出</exception>
    public void SetLogLevel(string level)
    {
        string[] validLevels = ["DEBUG", "INFO", "WARNING", "ERROR"];
        if (!validLevels.Contains(level))
            throw new ArgumentException($"Invalid log level: {level}. Must be one of {string.Join(", ", validLevels)}");
        
        ProjectConfig.Parameters["logLevel"] = level;
    }
}
