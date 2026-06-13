using CommunityToolkit.Mvvm.ComponentModel;

using Serilog;

using SmartMealService.GuiClient.Application;
using SmartMealService.GuiClient.Models;

namespace SmartMealService.GuiClient.ViewModels;

public partial class VariableViewModel : ObservableObject
{
    private readonly CustomEnvironmentVariable _model;
    private readonly IVariablesRepository _repository;
    private readonly ILogger _logger;

    public VariableViewModel(
        CustomEnvironmentVariable model,
        IVariablesRepository repository,
        ILogger logger)
    {
        _model = model;
        _repository = repository;
        _logger = logger;

        _key = model.Key;
        _value = model.Value;
        _comment = model.Comment ?? string.Empty;
    }

    [ObservableProperty]
    private string _key;

    [ObservableProperty]
    private string _value;

    [ObservableProperty]
    private string _comment;

    partial void OnValueChanged(string value)
    {
        if (_model.Value == value) return;

        _model.Value = value;

        _logger.Information("Изменение переменной среды: {Key} = '{Value}'", _key, value);

        Task.Run(async () => await SaveChangesAsync());
    }

    partial void OnCommentChanged(string? value)
    {
        if (_model.Comment == value) return;

        _model.Comment = value;

        _logger.Information("Изменение комментария для переменной {Key}: '{Comment}'", _key, value);

        Task.Run(async () => await SaveChangesAsync());
    }

    private async Task SaveChangesAsync()
    {
        try
        {
            await _repository.UpdateVariableAsync(_model, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Ошибка при сохранении переменной {Key} в репозиторий", Key);
        }
    }
}