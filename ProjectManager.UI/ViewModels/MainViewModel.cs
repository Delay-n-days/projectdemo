using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectManager.Core;
using ProjectManager.UI.Views;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace ProjectManager.UI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly Core.ProjectManager _projectManager = new();
    private Window? _window;

    [ObservableProperty] private string _logMessage = "";
    [ObservableProperty] private int _counterValue;
    [ObservableProperty] private string _statusMessage = "Ready";
    
    public ObservableCollection<string> Logs { get; } = [];

    public void SetWindow(Window window)
    {
        _window = window;
    }

    [RelayCommand]
    private async Task OpenProject()
    {
        try
        {
            var file = await OpenFilePickerAsync("选择项目文件", new[] { "*.json" }, "JSON文件");
            if (file == null) return;

            _projectManager.OpenProject(file);
            StatusMessage = $"✓ 项目已打开: {_projectManager.ProjectConfig?.ProjectName} v{_projectManager.ProjectConfig?.Version}";
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

        var logger = _projectManager.GetModel("AppLogger") as LoggerProjectModel;
        if (logger != null)
        {
            logger.Execute(LogMessage, "INFO");
            _projectManager.SaveProject();
            UpdateDisplay();
            LogMessage = "";
            StatusMessage = "✓ 日志已添加";
        }
    }

    [RelayCommand]
    private void IncrementCounter()
    {
        var counter = _projectManager.GetModel("OperationCounter") as CounterProjectModel;
        if (counter != null)
        {
            counter.Increment();
            _projectManager.SaveProject();
            UpdateDisplay();
            StatusMessage = $"✓ 计数器: {counter.GetCount()}";
        }
    }

    [RelayCommand]
    private void DecrementCounter()
    {
        var counter = _projectManager.GetModel("OperationCounter") as CounterProjectModel;
        if (counter != null)
        {
            counter.Decrement();
            _projectManager.SaveProject();
            UpdateDisplay();
            StatusMessage = $"✓ 计数器: {counter.GetCount()}";
        }
    }

    [RelayCommand]
    private void ResetCounter()
    {
        if (_projectManager.GetModel("OperationCounter") is CounterProjectModel counter)
        {
            counter.Reset();
            _projectManager.SaveProject();
            UpdateDisplay();
            StatusMessage = "✓ 计数器已重置";
        }
    }

    [RelayCommand]
    private async Task SaveAsProject()
    {
        try
        {
            if (_projectManager.ProjectConfig == null)
            {
                StatusMessage = "✗ 错误: 没有打开的项目";
                return;
            }

            // 弹出另存为对话框
            var saveAsVm = new SaveAsViewModel(_projectManager.ProjectConfig.ProjectName);
            var saveAsDialog = new SaveAsDialog(saveAsVm) { DataContext = saveAsVm };
            
            var result = await saveAsDialog.ShowDialog<bool>(_window!);
            if (!result || saveAsVm.Result == null)
            {
                StatusMessage = "✗ 已取消";
                return;
            }

            _projectManager.SaveAsProject(saveAsVm.Result);
            StatusMessage = $"✓ 项目已另存为: {saveAsVm.Result}";
            UpdateDisplay();
        }
        catch (Exception ex)
        {
            StatusMessage = $"✗ 错误: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task CreateFromTemplate()
    {
        try
        {
            // 第一步：选择模版
            var templateVm = new TemplateSelectionViewModel();
            var templateDialog = new TemplateSelectionDialog(templateVm) { DataContext = templateVm };
            
            var templateResult = await templateDialog.ShowDialog<bool>(_window!);
            if (!templateResult || templateVm.Result == null)
            {
                StatusMessage = "✗ 已取消";
                return;
            }

            // 第二步：输入新项目信息
            var newProjectVm = new NewProjectViewModel();
            var newProjectDialog = new NewProjectDialog(newProjectVm) { DataContext = newProjectVm };
            
            var createResult = await newProjectDialog.ShowDialog<bool>(_window!);
            if (!createResult || newProjectVm.Result == null)
            {
                StatusMessage = "✗ 已取消";
                return;
            }

            // 创建项目
            _projectManager.CreateFromTemplate(templateVm.Result.Path, newProjectVm.Result);
            StatusMessage = $"✓ 从模板创建项目成功: {newProjectVm.Result}";
            UpdateDisplay();
        }
        catch (Exception ex)
        {
            StatusMessage = $"✗ 错误: {ex.Message}";
        }
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
        var logger = _projectManager.GetModel("AppLogger") as LoggerProjectModel;
        if (logger != null)
        {
            Logs.Clear();
            foreach (var log in logger.GetLogs())
                Logs.Add(log);
        }

        var counter = _projectManager.GetModel("OperationCounter") as CounterProjectModel;
        CounterValue = counter?.GetCount() ?? 0;
    }
}
