using Serilog;

using SmartMealService.Domain.Models;

namespace SmartMealService.Infrastructure;

public sealed class MenuRepository : IMenuRepository
{
    private readonly ILogger _logger;

    public Task SaveMenuItems(List<SmsMenuItem> menuItems)
    {
        throw new NotImplementedException();
    }
}