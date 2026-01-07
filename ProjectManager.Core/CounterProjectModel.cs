namespace ProjectManager.Core;

/// <summary>计数器模型</summary>
public class CounterProjectModel(ModelProjectConfig projectConfig) : BaseProjectModel(projectConfig)
{
    public CounterProjectModel(ModelProjectConfig projectConfig, int step = 1, int? maxCount = null) : this(projectConfig)
    {
        ProjectConfig.Parameters.TryAdd("count", 0);
        ProjectConfig.Parameters.TryAdd("step", step);
        if (maxCount.HasValue)
            ProjectConfig.Parameters.TryAdd("maxCount", maxCount.Value);
    }

    public override void Execute(params object[] args)
    {
        var operation = args.Length > 0 ? args[0].ToString()?.ToLower() : "increment";
        
        _ = operation switch
        {
            "increment" => Increment(),
            "decrement" => Decrement(),
            "reset" => Reset(),
            _ => throw new ArgumentException($"Unknown operation: {operation}")
        };
    }

    public int Increment()
    {
        var step = Convert.ToInt32(ProjectConfig.Parameters.GetValueOrDefault("step", 1));
        var maxCountValue = ProjectConfig.Parameters.GetValueOrDefault("maxCount");
        var maxCount = maxCountValue != null ? Convert.ToInt32(maxCountValue) : (int?)null;
        var count = Convert.ToInt32(ProjectConfig.Parameters.GetValueOrDefault("count", 0));

        var newCount = count + step;

        if (maxCount.HasValue && newCount > maxCount.Value)
        {
            Console.WriteLine($"[CounterModel] Max count {maxCount} reached. Resetting to 0.");
            ProjectConfig.Parameters["count"] = 0;
            return 0;
        }

        ProjectConfig.Parameters["count"] = newCount;
        return newCount;
    }

    public int Decrement()
    {
        var step = Convert.ToInt32(ProjectConfig.Parameters.GetValueOrDefault("step", 1));
        var count = Convert.ToInt32(ProjectConfig.Parameters.GetValueOrDefault("count", 0));
        
        ProjectConfig.Parameters["count"] = Math.Max(0, count - step);
        return Convert.ToInt32(ProjectConfig.Parameters["count"]);
    }

    public int Reset()
    {
        ProjectConfig.Parameters["count"] = 0;
        return 0;
    }

    public int GetCount() => Convert.ToInt32(ProjectConfig.Parameters.GetValueOrDefault("count", 0));

    public void SetStep(int step)
    {
        if (step <= 0) throw new ArgumentException("Step must be positive");
        ProjectConfig.Parameters["step"] = step;
    }

    public void SetMaxCount(int? maxCount)
    {
        if (maxCount.HasValue && maxCount.Value <= 0)
            throw new ArgumentException("Max count must be positive");
        
        if (maxCount.HasValue)
            ProjectConfig.Parameters["maxCount"] = maxCount.Value;
        else
            ProjectConfig.Parameters.Remove("maxCount");
    }
}
