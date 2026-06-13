using Serilog;

using SmartMealService.Domain.Models;
using SmartMealService.Services.Interfaces;

namespace SmartMealService.Infrastructure;

public sealed class MenuRepository : IMenuRepository
{
    private readonly MenuDbContext _context;
    private readonly ILogger _logger;

    public MenuRepository(MenuDbContext context, ILogger logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task SaveMenuItems(List<SmsMenuItem> menuItems)
    {
        throw new NotImplementedException();
    }

    public Task InitializeDatabaseAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}