using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectManager.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManager.UI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly Application _app = new();
    private Window? _window;

    [ObservableProperty] private string _projectPath = @"C:\Temp\MyProject";
    [ObservableProperty] private string _projectName = "TestProject";
    [ObservableProperty] private string _version = "1.0.0";
    [ObservableProperty] private string _logMessage = "";
    [ObservableProperty] private int _counterValue;
    [ObservableProperty] private string _statusMessage = "Ready";
    [ObservableProperty] private string _saveAsPath = @"C:\Temp\MyProjectCopy";
    [ObservableProperty] private string _templatePath = @"C:\Temp\TemplateProject\TemplateProject.json";
    [ObservableProperty] private string _newProjectPath = @"C:\Temp\NewProject";
    
    public ObservableCollection<string> Logs { get; } = [];

    public void SetWindow(Window window)
    {
        _window = window;
    }

    [RelayCommand]
    private void CreateProject()
    {
        try
        {
            var models = new List<ModelConfigDto>
            {
                new("logger", "AppLogger"),
                new("counter", "OperationCounter", Parameters: new() { ["step"] = 1 })
            };

            _app.NewProject(ProjectPath, ProjectName, Version, models);
            StatusMessage = $"✓ 项目 '{ProjectName}' 创建成功！";
            UpdateDisplay();
        }
        catch (Exception ex)
        {
            StatusMessage = $"✗ 错误: {ex.Message}";
        }
    }

    [RelayCommand]
    private void OpenProject()
    {
        try
        {
            var projectFile = Path.Combine(ProjectPath, $"{ProjectName}.json");
            _app.OpenProject(projectFile);
            StatusMessage = $"✓ 项目已打开: {_app.ProjectConfig?.ProjectName} v{_app.ProjectConfig?.Version}";
            UpdateDisplay();
        }
        catch (Exception ex)
        {
            StatusMessage = $"✗ 错误: {ex.Message}";
        }
    }

    [RelayCommand]
    private void AddLog()
    {
        if (string.IsNullOrWhiteSpace(LogMessage)) return;

        var logger = _app.GetModel("AppLogger") as LoggerModel;
        if (logger != null)
        {
            logger.Execute(LogMessage, "INFO");
            _app.SaveProject();
            UpdateDisplay();
            LogMessage = "";
            StatusMessage = "✓ 日志已添加";
        }
    }

    [RelayCommand]
    private void IncrementCounter()
    {
        var counter = _app.GetModel("OperationCounter") as CounterModel;
        if (counter != null)
        {
            counter.Increment();
            _app.SaveProject();
            UpdateDisplay();
            StatusMessage = $"✓ 计数器: {counter.GetCount()}";
        }
    }

    [RelayCommand]
    private void DecrementCounter()
    {
        var counter = _app.GetModel("OperationCounter") as CounterModel;
        if (counter != null)
        {
            counter.Decrement();
            _app.SaveProject();
            UpdateDisplay();
            StatusMessage = $"✓ 计数器: {counter.GetCount()}";
        }
    }

    [RelayCommand]
    private void ResetCounter()
    {
        var counter = _app.GetModel("OperationCounter") as CounterModel;
        if (counter != null)
        {
            counter.Reset();
            _app.SaveProject();
            UpdateDisplay();
            StatusMessage = "✓ 计数器已重置";
        }
    }

    [RelayCommand]
    private void SaveAsProject()
    {
        try
        {
            if (_app.ProjectConfig == null)
            {
                StatusMessage = "✗ 错误: 没有打开的项目";
                return;
            }

            _app.SaveAs(SaveAsPath);
            StatusMessage = $"✓ 项目已另存为: {SaveAsPath}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"✗ 错误: {ex.Message}";
        }
    }

    [RelayCommand]
    private void CreateFromTemplate()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(TemplatePath) || string.IsNullOrWhiteSpace(NewProjectPath))
            {
                StatusMessage = "✗ 错误: 请输入模板路径和新项目路径";
                return;
            }

            _app.CreateFromTemplate(TemplatePath, NewProjectPath);
            StatusMessage = $"✓ 从模板创建项目成功: {NewProjectPath}";
            UpdateDisplay();
        }
        catch (Exception ex)
        {
            StatusMessage = $"✗ 错误: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task BrowseProjectPath()
    {
        var path = await OpenFolderPickerAsync("选择项目路径");
        if (path != null) ProjectPath = path;
    }

    [RelayCommand]
    private async Task BrowseSaveAsPath()
    {
        var path = await OpenFolderPickerAsync("选择另存为路径");
        if (path != null) SaveAsPath = path;
    }

    [RelayCommand]
    private async Task BrowseTemplatePath()
    {
        var path = await OpenFilePickerAsync("选择模板文件", new[] { "*.json" }, "JSON文件");
        if (path != null) TemplatePath = path;
    }

    [RelayCommand]
    private async Task BrowseNewProjectPath()
    {
        var path = await OpenFolderPickerAsync("选择新项目路径");
        if (path != null) NewProjectPath = path;
    }

    private async Task<string?> OpenFolderPickerAsync(string title)
    {
        if (_window == null) return null;

        var folder = await _window.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = title,
            AllowMultiple = false
        });

        return folder.Count > 0 ? folder[0].Path.LocalPath : null;
    }

    private async Task<string?> OpenFilePickerAsync(string title, string[] patterns, string description)
    {
        if (_window == null) return null;

        var files = await _window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter = new[] { new FilePickerFileType(description) { Patterns = patterns } }
        });

        return files.Count > 0 ? files[0].Path.LocalPath : null;
    }

    private void UpdateDisplay()
    {
        var logger = _app.GetModel("AppLogger") as LoggerModel;
        if (logger != null)
        {
            Logs.Clear();
            foreach (var log in logger.GetLogs())
                Logs.Add(log);
        }

        var counter = _app.GetModel("OperationCounter") as CounterModel;
        CounterValue = counter?.GetCount() ?? 0;
    }
}
