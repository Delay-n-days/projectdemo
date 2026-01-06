using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectManager.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace ProjectManager.UI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly Application _app = new();

    [ObservableProperty] private string _projectPath = @"C:\Temp\MyProject";
    [ObservableProperty] private string _projectName = "TestProject";
    [ObservableProperty] private string _version = "1.0.0";
    [ObservableProperty] private string _logMessage = "";
    [ObservableProperty] private int _counterValue;
    [ObservableProperty] private string _statusMessage = "Ready";
    
    public ObservableCollection<string> Logs { get; } = [];

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
