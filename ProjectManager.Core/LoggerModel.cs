namespace ProjectManager.Core;

/// <summary>日志记录器模型</summary>
public class LoggerModel(ModelConfig config) : BaseModel(config)
{
    public LoggerModel(ModelConfig config, string logLevel = "INFO", int maxEntries = 1000) : this(config)
    {
        EnsureParameters();
        Config.Parameters.TryAdd("logLevel", logLevel);
        Config.Parameters.TryAdd("maxEntries", maxEntries);
    }

    private void EnsureParameters()
    {
        Config.Parameters.TryAdd("logs", new List<object>());
        Config.Parameters.TryAdd("logLevel", "INFO");
        Config.Parameters.TryAdd("maxEntries", 1000);
    }

    public override void Execute(params object[] args)
    {
        EnsureParameters();
        
        var message = args.Length > 0 ? args[0].ToString() : "";
        var level = args.Length > 1 ? args[1].ToString() : "INFO";
        
        var logEntry = $"[{level}] {message}";
        Console.WriteLine($"[LoggerModel] {logEntry}");

        var logs = Config.Parameters["logs"] as List<object> ?? [];
        Config.Parameters["logs"] = logs;
        logs.Add(logEntry);

        var maxEntries = Convert.ToInt32(Config.Parameters.GetValueOrDefault("maxEntries", 1000));
        if (logs.Count > maxEntries)
            Config.Parameters["logs"] = logs.TakeLast(maxEntries).ToList();
    }

    public List<string> GetLogs()
    {
        var logs = Config.Parameters.GetValueOrDefault("logs") as List<object>;
        return logs?.Select(x => x.ToString() ?? "").ToList() ?? [];
    }

    public void ClearLogs() => Config.Parameters["logs"] = new List<object>();

    public void SetLogLevel(string level)
    {
        string[] validLevels = ["DEBUG", "INFO", "WARNING", "ERROR"];
        if (!validLevels.Contains(level))
            throw new ArgumentException($"Invalid log level: {level}. Must be one of {string.Join(", ", validLevels)}");
        
        Config.Parameters["logLevel"] = level;
    }
}
