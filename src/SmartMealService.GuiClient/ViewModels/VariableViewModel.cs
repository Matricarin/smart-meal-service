using CommunityToolkit.Mvvm.ComponentModel;

using Serilog;

using SmartMealService.GuiClient.Application;
using SmartMealService.GuiClient.Models;

namespace SmartMealService.GuiClient.ViewModels;

public partial class VariableViewModel : ObservableObject
{
    private readonly CustomEnvironmentVariable? _model;
    [ObservableProperty] private string _comment;

    [ObservableProperty] private string _key;

    private ILogger? _logger;

    private IVariablesRepository? _repository;

    [ObservableProperty] private string _value;

    public bool IsNew => _repository is null;

    public VariableViewModel
    (
        CustomEnvironmentVariable model,
        IVariablesRepository repository,
        ILogger logger
    )
    {
        _model = model;
        _repository = repository;
        _logger = logger;

        _key = model.Key;
        _value = model.Value;
        _comment = model.Comment ?? string.Empty;
    }

    public VariableViewModel()
    {
        _model = new CustomEnvironmentVariable { Key = string.Empty, Value = string.Empty };
        _key = string.Empty;
        _value = string.Empty;
        _comment = string.Empty;
    }

    public void InitializeDependencies(IVariablesRepository repository, ILogger logger)
    {
        _repository = repository;
        _logger = logger;
    }

    partial void OnValueChanged(string value)
    {
        if (_model?.Value == value)
        {
            return;
        }

        _model?.Value = value;

        if (_repository == null || _logger == null)
        {
            return;
        }

        _logger.Information("Изменение переменной среды: {Key} = '{Value}'", _key, value);

        Task.Run(async () => await SaveChangesAsync());
    }

    partial void OnCommentChanged(string value)
    {
        if (_model?.Comment == value)
        {
            return;
        }

        _model?.Comment = value;

        if (_repository == null || _logger == null)
        {
            return;
        }

        _logger.Information("Изменение комментария для переменной {Key}: '{Comment}'", _key, value);

        Task.Run(async () => await SaveChangesAsync());
    }

    private async Task SaveChangesAsync()
    {
        try
        {
            if (_repository is not null && _model is not null)
            {
                await _repository.UpdateVariableAsync(_model, CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Ошибка при сохранении переменной {Key} в репозиторий", Key);
        }
    }
}