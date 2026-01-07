using Avalonia.Controls;
using ProjectManager.UI.ViewModels;

namespace ProjectManager.UI.Views;

public partial class NewProjectDialog : Window
{
    public NewProjectDialog()
    {
        InitializeComponent();
    }

    public NewProjectDialog(NewProjectViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.SetWindow(this);
    }
}
