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
    private async Task Save()
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

        // 检查目标路径是否已存在
        if (Directory.Exists(FullPath))
        {
            var result = await ShowOverwriteConfirmation();
            if (!result)
            {
                return; // 用户选择不覆盖，取消操作
            }
        }

        Result = FullPath;
        _window?.Close(true);
    }

    private async Task<bool> ShowOverwriteConfirmation()
    {
        if (_window == null) return false;

        var dialog = new Window
        {
            Title = "确认覆盖",
            Width = 400,
            Height = 180,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false
        };

        var result = false;
        var panel = new StackPanel
        {
            Margin = new Avalonia.Thickness(20),
            Spacing = 20
        };

        panel.Children.Add(new Avalonia.Controls.TextBlock
        {
            Text = $"目标位置已存在项目:\n{FullPath}\n\n是否覆盖现有项目？",
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            FontSize = 14
        });

        var buttonPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Spacing = 10
        };

        var yesButton = new Button
        {
            Content = "覆盖",
            Padding = new Avalonia.Thickness(20, 8),
            IsDefault = false
        };
        yesButton.Click += (s, e) =>
        {
            result = true;
            dialog.Close();
        };

        var noButton = new Button
        {
            Content = "取消",
            Padding = new Avalonia.Thickness(20, 8),
            IsDefault = true
        };
        noButton.Click += (s, e) =>
        {
            result = false;
            dialog.Close();
        };

        buttonPanel.Children.Add(yesButton);
        buttonPanel.Children.Add(noButton);
        panel.Children.Add(buttonPanel);

        dialog.Content = panel;

        await dialog.ShowDialog(_window);
        return result;
    }

    [RelayCommand]
    private void Cancel()
    {
        Result = null;
        _window?.Close(false);
    }
}
