using SmartMealService.Domain.Models;

namespace SmartMealService.Services.Interfaces;

public interface IMenuRepository
{
    Task SaveMenuItems(List<SmsMenuItem>  menuItems, CancellationToken cancellationToken);
    Task InitializeDatabaseAsync(CancellationToken  cancellationToken);
}