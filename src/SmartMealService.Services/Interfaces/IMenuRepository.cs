using SmartMealService.Domain.Models;

namespace SmartMealService.Services.Interfaces;

public interface IMenuRepository
{
    Task SaveMenuItems(List<SmsMenuItem>  menuItems);
    Task InitializeDatabaseAsync(CancellationToken  cancellationToken);
}