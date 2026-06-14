using Microsoft.EntityFrameworkCore;

using Serilog;

using SmartMealService.GuiClient.Application;
using SmartMealService.GuiClient.Models;

namespace SmartMealService.GuiClient.Infrastructure;

public sealed class VariablesRepository : IVariablesRepository
{
    private readonly VariablesDbContext _context;
    private readonly ILogger _logger;

    public VariablesRepository(VariablesDbContext context, ILogger logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<CustomEnvironmentVariable>> GetAllVariablesAsync(CancellationToken token)
    {
        try
        {
            var dbVariables = await _context.EnvironmentVariables
                .AsNoTracking()
                .ToDictionaryAsync(v => v.Key, v => v.Comment, token);

            var result = new List<CustomEnvironmentVariable>();

            foreach (var key in dbVariables.Keys)
            {
                string? osValue = Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.User);

                result.Add(new CustomEnvironmentVariable
                {
                    Key = key, Value = osValue ?? string.Empty, Comment = dbVariables[key]
                });
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Ошибка при получении списка переменных среды из БД/ОС");
            throw;
        }
    }

    public async Task AddVariableAsync(CustomEnvironmentVariable variable, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(variable);

        try
        {
            Environment.SetEnvironmentVariable(variable.Key, variable.Value, EnvironmentVariableTarget.User);

            _logger.Information("Переменная среды ОС успешно создана: {Key} = {Value}", variable.Key, variable.Value);

            var exists = await _context.EnvironmentVariables.AnyAsync(v => v.Key == variable.Key, token);

            if (!exists)
            {
                await _context.EnvironmentVariables.AddAsync(variable, token);
                await _context.SaveChangesAsync(token);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Ошибка при добавлении переменной среды {Key}", variable.Key);
            throw;
        }
    }

    public async Task UpdateVariableAsync(CustomEnvironmentVariable variable, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(variable);

        try
        {
            Environment.SetEnvironmentVariable(variable.Key, variable.Value, EnvironmentVariableTarget.User);

            _logger.Information("Переменная среды ОС обновлена: {Key} = {Value}", variable.Key, variable.Value);

            var dbVariable = await _context.EnvironmentVariables
                .FirstOrDefaultAsync(v => v.Key == variable.Key, token);

            if (dbVariable != null)
            {
                dbVariable.Value = variable.Value;
                dbVariable.Comment = variable.Comment;

                _context.EnvironmentVariables.Update(dbVariable);

                await _context.SaveChangesAsync(token);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Ошибка при обновлении переменной среды {Key}", variable.Key);
            throw;
        }
    }

    public async Task DeleteVariableAsync(CustomEnvironmentVariable variable, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(variable);

        try
        {
            Environment.SetEnvironmentVariable(variable.Key, null, EnvironmentVariableTarget.User);

            _logger.Information("Переменная среды ОС удалена: {Key}", variable.Key);

            var dbVariable = await _context.EnvironmentVariables
                .FirstOrDefaultAsync(v => v.Key == variable.Key, token);

            if (dbVariable != null)
            {
                _context.EnvironmentVariables.Remove(dbVariable);
                await _context.SaveChangesAsync(token);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Ошибка при удалении переменной среды {Key}", variable.Key);
            throw;
        }
    }
}