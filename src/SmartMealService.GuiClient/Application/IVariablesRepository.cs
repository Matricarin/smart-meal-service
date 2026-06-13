using SmartMealService.GuiClient.Models;

namespace SmartMealService.GuiClient.Application;

public interface IVariablesRepository
{
    Task AddVariableAsync(CustomEnvironmentVariable variable, CancellationToken token);

    Task DeleteVariableAsync(CustomEnvironmentVariable variable, CancellationToken token);

    Task UpdateVariableAsync(CustomEnvironmentVariable variable, CancellationToken token);

    Task<List<CustomEnvironmentVariable>> GetAllVariablesAsync(CancellationToken token);
}