namespace ProjectManager.Core;

/// <summary>应用程序入口</summary>
public class Application
{
    private readonly ProjectManager _manager = new();

    public Application(string? projectJsonPath = null)
    {
        if (!string.IsNullOrEmpty(projectJsonPath))
            OpenProject(projectJsonPath);
    }

    public void OpenProject(string projectJsonPath) => _manager.OpenProject(projectJsonPath);

    public void NewProject(string projectPath, string projectName, string version, List<ModelConfigDto> modelsConfig) =>
        _manager.NewProject(projectPath, projectName, version, modelsConfig);

    public void SaveProject() => _manager.SaveProject();

    public void SaveAs(string newProjectPath) => _manager.SaveAsProject(newProjectPath);

    public void CreateFromTemplate(string templatePath, string newProjectPath) =>
        _manager.CreateFromTemplate(templatePath, newProjectPath);

    public BaseModel? GetModel(string modelName) => _manager.GetModel(modelName);

    public ProjectConfig? ProjectConfig => _manager.ProjectConfig;
    public List<BaseModel> Models => _manager.Models;
}
