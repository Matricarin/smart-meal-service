using SmartMealService.Domain.Models;

namespace SmartMealService.Services.Interfaces;

public interface IOrderService
{
    Task<List<MenuItem>> GetMenuAsync(bool withPrice, CancellationToken cancellationToken);
    Task<bool> SendOrderAsync(Order order, CancellationToken cancellationToken);
}