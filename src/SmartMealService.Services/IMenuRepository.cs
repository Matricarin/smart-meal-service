using SmartMealService.Domain.Models;

namespace SmartMealService.Infrastructure;

public interface IMenuRepository
{
    Task SendMenuItems(List<MenuItem>  menuItems);
}