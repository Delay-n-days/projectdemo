using Avalonia.Controls;
using ProjectManager.UI.ViewModels;

namespace ProjectManager.UI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var viewModel = new MainViewModel();
        viewModel.SetWindow(this);
        DataContext = viewModel;
    }
}