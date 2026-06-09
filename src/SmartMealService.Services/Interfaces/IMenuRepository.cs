using SmartMealService.Domain.Models;

namespace SmartMealService.Infrastructure;

public interface IMenuRepository
{
    Task SaveMenuItems(List<SmsMenuItem>  menuItems);
}