using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ProjectManager.Core;

/// <summary>统一处理所有文件操作和异常</summary>
public static class FileManager
{
    private static readonly JsonSerializerSettings JsonSettings = new()
    {
        Formatting = Formatting.Indented,
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        NullValueHandling = NullValueHandling.Ignore
    };

    public static T LoadJson<T>(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        var json = File.ReadAllText(filePath);
        return JsonConvert.DeserializeObject<T>(json, JsonSettings) 
            ?? throw new InvalidOperationException($"Failed to deserialize {filePath}");
    }

    public static void SaveJson<T>(string filePath, T data)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var json = JsonConvert.SerializeObject(data, JsonSettings);
        File.WriteAllText(filePath, json);
    }

    public static void EnsureDirectory(string path) => Directory.CreateDirectory(path);
}
