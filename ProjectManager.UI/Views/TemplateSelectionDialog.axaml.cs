using Avalonia.Controls;
using ProjectManager.UI.ViewModels;

namespace ProjectManager.UI.Views;

public partial class TemplateSelectionDialog : Window
{
    public TemplateSelectionDialog()
    {
        InitializeComponent();
    }

    public TemplateSelectionDialog(TemplateSelectionViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.SetWindow(this);
    }
}
