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

    public Task AddVariableAsync(CustomEnvironmentVariable variable, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task DeleteVariableAsync(CustomEnvironmentVariable variable, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task UpdateVariableAsync(CustomEnvironmentVariable variable, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<List<CustomEnvironmentVariable>> GetAllVariablesAsync(CancellationToken token)
    {
        throw new NotImplementedException();
    }
}