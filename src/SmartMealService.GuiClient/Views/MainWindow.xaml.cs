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

    private void VariablesDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
    {
        if (e.EditAction == DataGridEditAction.Commit)
        {
            if (e.Row.Item is VariableViewModel newVariableVm)
            {
                Dispatcher.BeginInvoke(new Action(async void () =>
                {
                    if (!string.IsNullOrWhiteSpace(newVariableVm.Key) && newVariableVm.IsNew)
                    {
                        await _viewModel.AddNewVariableAsync(newVariableVm);
                    }
                }));
            }
        }
    }

    private void VariablesDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Delete)
        {
            DataGrid? dataGrid = sender as DataGrid;
            if (dataGrid?.SelectedItem is VariableViewModel selectedVariable)
            {
                // Проверяем, что это не строка-placeholder для добавления нового элемента
                if (selectedVariable != CollectionView.NewItemPlaceholder)
                {
                    // Вызываем команду удаления из ViewModel
                    if (_viewModel.DeleteVariableCommand.CanExecute(selectedVariable))
                    {
                        _viewModel.DeleteVariableCommand.Execute(selectedVariable);
                        e.Handled = true; // Перехватываем событие, чтобы DataGrid не ругался на изменение коллекции
                    }
                }
            }
        }
    }
}