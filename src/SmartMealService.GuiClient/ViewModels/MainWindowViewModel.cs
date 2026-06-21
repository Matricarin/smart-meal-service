using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Serilog;

using SmartMealService.GuiClient.Application;
using SmartMealService.GuiClient.Models;

#pragma warning disable MVVMTK0034

namespace SmartMealService.GuiClient.ViewModels;

public sealed partial class MainWindowViewModel : ObservableObject
{
    private readonly ILogger _logger;
    private readonly IVariablesRepository _repository;

    [ObservableProperty] private ObservableCollection<VariableViewModel> _environmentVariables = [];

    public MainWindowViewModel(IVariablesRepository repository, ILogger logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [RelayCommand]
    private async Task LoadVariablesAsync(CancellationToken token)
    {
        _logger.Information("Запуск загрузки переменных среды в UI...");

        try
        {
            _environmentVariables.Clear();

            var variables = await _repository.GetAllVariablesAsync(token);

            foreach (var variable in variables)
            {
                var itemViewModel = new VariableViewModel(variable, _repository, _logger);

                _environmentVariables.Add(itemViewModel);
            }

            _logger.Information("Успешно загружено {Count} переменных в таблицу", _environmentVariables.Count);
        }
        catch (OperationCanceledException)
        {
            _logger.Warning("Операция загрузки переменных была отменена.");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Критическая ошибка при заполнении таблицы DataGrid данными");
        }
    }

    [RelayCommand]
    private async Task CommitNewVariableAsync(VariableViewModel? variableVm)
    {
        if (variableVm == null || string.IsNullOrWhiteSpace(variableVm.Key) || !variableVm.IsNew)
        {
            return;
        }

        await AddNewVariableAsync(variableVm);
    }

    public async Task AddNewVariableAsync(VariableViewModel variableVm)
    {
        if (string.IsNullOrWhiteSpace(variableVm.Key))
        {
            _logger.Warning("Попытка добавить переменную с пустым ключом.");
            return;
        }

        try
        {
            var customVar = new CustomEnvironmentVariable
            {
                Key = variableVm.Key, Value = variableVm.Value, Comment = variableVm.Comment
            };

            await _repository.AddVariableAsync(customVar, CancellationToken.None);

            variableVm.InitializeDependencies(_repository, _logger);

            _logger.Information("Добавлена новая переменная среды: {Key}", variableVm.Key);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Ошибка при добавлении переменной среды {Key}", variableVm.Key);
        }
    }

    [RelayCommand]
    private async Task DeleteVariableAsync(VariableViewModel? variable)
    {
        if (variable == null)
        {
            return;
        }

        try
        {
            var customVar = new CustomEnvironmentVariable
            {
                Key = variable.Key, Value = variable.Value, Comment = variable.Comment
            };
            await _repository.DeleteVariableAsync(customVar, CancellationToken.None);

            _environmentVariables.Remove(variable);

            _logger.Information("Удалена переменная среды: {Key}", variable.Key);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Ошибка при удалении переменной среды {Key}", variable.Key);
        }
    }
}