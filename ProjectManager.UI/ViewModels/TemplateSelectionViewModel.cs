using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace ProjectManager.UI.ViewModels;

public partial class TemplateSelectionViewModel : ObservableObject
{
    private Window? _window;
    private const string TemplateFolder = @"C:\Temp";

    [ObservableProperty] private TemplateInfo? _selectedTemplate;
    [ObservableProperty] private string _previewJson = "";

    public ObservableCollection<TemplateInfo> Templates { get; } = [];
    public TemplateInfo? Result { get; private set; }

    public TemplateSelectionViewModel()
    {
        LoadTemplates();
    }

    public void SetWindow(Window window)
    {
        _window = window;
    }

    private void LoadTemplates()
    {
        try
        {
            if (!Directory.Exists(TemplateFolder))
            {
                Directory.CreateDirectory(TemplateFolder);
                return;
            }

            // 只扫描第二级：C:\Temp\项目文件夹\项目.json
            var projectFolders = Directory.GetDirectories(TemplateFolder);
            
            foreach (var folder in projectFolders)
            {
                try
                {
                    var folderName = Path.GetFileName(folder);
                    var projectJsonPath = Path.Combine(folder, $"{folderName}.json");
                    
                    // 只添加符合命名规范的项目JSON文件
                    if (File.Exists(projectJsonPath))
                    {
                        Templates.Add(new TemplateInfo
                        {
                            Name = folderName,
                            Path = projectJsonPath
                        });
                    }
                }
                catch
                {
                    // 跳过无效的文件夹
                }
            }
        }
        catch (Exception ex)
        {
            PreviewJson = $"加载模版失败: {ex.Message}";
        }
    }

    partial void OnSelectedTemplateChanged(TemplateInfo? value)
    {
        if (value == null)
        {
            PreviewJson = "";
            return;
        }

        try
        {
            var templateDir = Path.GetDirectoryName(value.Path);
            if (templateDir == null) return;

            var preview = new
            {
                TemplateName = value.Name,
                Models = Directory.GetDirectories(templateDir)
                    .Select(modelDir =>
                    {
                        var modelName = Path.GetFileName(modelDir);
                        var modelJsonPath = Path.Combine(modelDir, $"{modelName}.json");
                        
                        if (File.Exists(modelJsonPath))
                        {
                            try
                            {
                                var content = File.ReadAllText(modelJsonPath);
                                return JsonSerializer.Deserialize<object>(content);
                            }
                            catch
                            {
                                return new { ModelName = modelName, Error = "无法解析" };
                            }
                        }
                        return new { ModelName = modelName, Status = "无配置文件" };
                    })
                    .ToList()
            };

            PreviewJson = JsonSerializer.Serialize(preview, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
        }
        catch (Exception ex)
        {
            PreviewJson = $"预览失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private void Next()
    {
        if (SelectedTemplate != null)
        {
            Result = SelectedTemplate;
            _window?.Close(true);
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        Result = null;
        _window?.Close(false);
    }
}

public class TemplateInfo
{
    public string Name { get; set; } = "";
    public string Path { get; set; } = "";
}
