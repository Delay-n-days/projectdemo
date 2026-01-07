namespace ProjectManager.Core;

using Newtonsoft.Json.Linq;

/// <summary>日志记录器模型</summary>
public class LoggerProjectModel(ModelProjectConfig projectConfig) : BaseProjectModel(projectConfig)
{
    public LoggerProjectModel(ModelProjectConfig projectConfig, string logLevel = "INFO", int maxEntries = 1000) : this(projectConfig)
    {
        EnsureParameters();
        ProjectConfig.Parameters.TryAdd("logLevel", logLevel);
        ProjectConfig.Parameters.TryAdd("maxEntries", maxEntries);
    }

    private void EnsureParameters()
    {
        ProjectConfig.Parameters.TryAdd("logs", new List<object>());
        ProjectConfig.Parameters.TryAdd("logLevel", "INFO");
        ProjectConfig.Parameters.TryAdd("maxEntries", 1000);
    }

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

    public List<string> GetLogs()
    {
        var logs = GetLogsInternal();
        return logs.Select(x => x.ToString() ?? "").ToList();
    }

    public void ClearLogs() => ProjectConfig.Parameters["logs"] = new List<object>();

    public void SetLogLevel(string level)
    {
        string[] validLevels = ["DEBUG", "INFO", "WARNING", "ERROR"];
        if (!validLevels.Contains(level))
            throw new ArgumentException($"Invalid log level: {level}. Must be one of {string.Join(", ", validLevels)}");
        
        ProjectConfig.Parameters["logLevel"] = level;
    }
}
