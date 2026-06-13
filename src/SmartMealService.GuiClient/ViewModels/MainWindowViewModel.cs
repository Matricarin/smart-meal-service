using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Serilog;

using SmartMealService.GuiClient.Application;

namespace SmartMealService.GuiClient.ViewModels;

public sealed partial class MainWindowViewModel : ObservableObject
{
    private readonly ILogger _logger;
    private readonly IVariablesRepository _repository;

    [ObservableProperty] private string? _errorMessage;

    [ObservableProperty] private bool _isLoading;

    public ObservableCollection<VariableViewModel> EnvironmentVariables { get; } = new();

    public MainWindowViewModel(IVariablesRepository repository, ILogger logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [RelayCommand]
    private async Task LoadVariablesAsync(CancellationToken token)
    {
        _isLoading = true;
        _errorMessage = null;
        _logger.Information("Запуск загрузки переменных среды в UI...");

        try
        {
            EnvironmentVariables.Clear();

            var variables = await _repository.GetAllVariablesAsync(token);

            foreach (var variable in variables)
            {
                var itemViewModel = new VariableViewModel(variable, _repository, _logger);
                EnvironmentVariables.Add(itemViewModel);
            }

            _logger.Information("Успешно загружено {Count} переменных в таблицу", EnvironmentVariables.Count);
        }
        catch (OperationCanceledException)
        {
            _logger.Warning("Операция загрузки переменных была отменена.");
        }
        catch (Exception ex)
        {
            _errorMessage = "Не удалось загрузить переменные среды.";
            _logger.Error(ex, "Критическая ошибка при заполнении таблицы DataGrid данными");
        }
        finally
        {
            _isLoading = false;
        }
    }
}