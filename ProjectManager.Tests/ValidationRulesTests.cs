using ProjectManager.Core;

namespace ProjectManager.Tests;

public class ValidationRulesTests
{
    [Theory]
    [InlineData("0.1.0")]
    [InlineData("1.0.0")]
    [InlineData("0.9.9")]
    public void ValidateVersion_ValidVersions_ShouldNotThrow(string version)
    {
        var exception = Record.Exception(() => ValidationRules.ValidateVersion(version));
        Assert.Null(exception);
    }

    [Theory]
    [InlineData("1.0")]
    [InlineData("v1.0.0")]
    [InlineData("invalid")]
    public void ValidateVersion_InvalidFormat_ShouldThrowArgumentException(string version)
    {
        Assert.Throws<ArgumentException>(() => ValidationRules.ValidateVersion(version));
    }

    [Theory]
    [InlineData("1.0.1")]
    [InlineData("2.0.0")]
    public void ValidateVersion_ExceedsMax_ShouldThrowArgumentException(string version)
    {
        Assert.Throws<ArgumentException>(() => ValidationRules.ValidateVersion(version));
    }
}
