using Serilog;

using SmartMealService.Domain.Models;

namespace SmartMealService.Infrastructure;

public sealed class MenuRepository : IMenuRepository
{
    private readonly ILogger _logger;

    public Task SaveMenuItems(List<MenuItem> menuItems)
    {
        throw new NotImplementedException();
    }
}