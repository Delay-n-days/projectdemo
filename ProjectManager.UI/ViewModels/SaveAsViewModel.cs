using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ProjectManager.UI.ViewModels;

public partial class SaveAsViewModel : ObservableObject
{
    private Window? _window;

    [ObservableProperty] private string _projectName = "";
    [ObservableProperty] private string _projectPath = "";
    [ObservableProperty] private string _fullPath = "";

    public string? Result { get; private set; }

    public SaveAsViewModel(string currentProjectName)
    {
        // 默认使用当前项目名称
        ProjectName = currentProjectName;
        // 默认路径为桌面
        ProjectPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        UpdateFullPath();
    }

    public void SetWindow(Window window)
    {
        _window = window;
    }

    partial void OnProjectNameChanged(string value)
    {
        UpdateFullPath();
    }

    partial void OnProjectPathChanged(string value)
    {
        UpdateFullPath();
    }

    private void UpdateFullPath()
    {
        if (!string.IsNullOrWhiteSpace(ProjectPath) && !string.IsNullOrWhiteSpace(ProjectName))
        {
            FullPath = Path.Combine(ProjectPath, ProjectName);
        }
        else
        {
            FullPath = "";
        }
    }

    [RelayCommand]
    private async Task Browse()
    {
        if (_window == null) return;

        var folder = await _window.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "选择保存位置",
            AllowMultiple = false
        });

        if (folder.Count > 0)
        {
            ProjectPath = folder[0].Path.LocalPath;
        }
    }

    [RelayCommand]
    private void Save()
    {
        if (string.IsNullOrWhiteSpace(ProjectName))
        {
            // TODO: 显示错误提示
            return;
        }

        if (string.IsNullOrWhiteSpace(ProjectPath))
        {
            // TODO: 显示错误提示
            return;
        }

        Result = FullPath;
        _window?.Close(true);
    }

    [RelayCommand]
    private void Cancel()
    {
        Result = null;
        _window?.Close(false);
    }
}
