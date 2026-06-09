using SmartMealService.Domain.Models;

namespace SmartMealService.Services.Interfaces;

public interface IOrderService
{
    Task<List<SmsMenuItem>> GetMenuAsync(bool withPrice, CancellationToken cancellationToken);
    Task<bool> SendOrderAsync(SmsOrder smsOrder, CancellationToken cancellationToken);
}