namespace ProjectManager.Core;

/// <summary>项目配置数据类</summary>
public record ProjectConfig
{
    public required string ProjectName { get; init; }
    public required string Version { get; init; }
    public required string ProjectPath { get; init; }
    public required string ConfigPath { get; init; }

    public ProjectConfig()
    {
        // 构造后验证
        if (!string.IsNullOrEmpty(Version))
            ValidationRules.ValidateVersion(Version);
    }
}
