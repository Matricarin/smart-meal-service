using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

using SmartMealService.GuiClient.ViewModels;

namespace SmartMealService.GuiClient.Views;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;

    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = _viewModel;

        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        if (_viewModel.LoadVariablesCommand.CanExecute(null))
        {
            await _viewModel.LoadVariablesCommand.ExecuteAsync(null);
        }
    }
}