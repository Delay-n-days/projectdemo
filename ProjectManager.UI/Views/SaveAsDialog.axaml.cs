using Avalonia.Controls;
using ProjectManager.UI.ViewModels;

namespace ProjectManager.UI.Views;

public partial class SaveAsDialog : Window
{
    public SaveAsDialog()
    {
        InitializeComponent();
    }

    public SaveAsDialog(SaveAsViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.SetWindow(this);
    }
}
