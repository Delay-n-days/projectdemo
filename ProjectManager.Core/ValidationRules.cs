using System.Text.RegularExpressions;

namespace ProjectManager.Core;

/// <summary>验证规则集中管理</summary>
public static partial class ValidationRules
{
    private const string MaxVersion = "1.0.0";

    [GeneratedRegex(@"^\d+\.\d+\.\d+$")]
    private static partial Regex VersionPattern();

    public static void ValidateVersion(string version)
    {
        if (!VersionPattern().IsMatch(version))
            throw new ArgumentException($"Invalid version format: {version}. Expected format: X.Y.Z");

        if (ParseVersion(version).CompareTo(ParseVersion(MaxVersion)) > 0)
            throw new ArgumentException($"Version {version} exceeds maximum allowed version {MaxVersion}");
    }

    private static Version ParseVersion(string version) => new(version);
}
