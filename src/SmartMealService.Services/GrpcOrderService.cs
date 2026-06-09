using Serilog;

using SmartMealService.Domain.Models;
using SmartMealService.Services.Dtos;
using SmartMealService.Services.Interfaces;

namespace SmartMealService.Services;

public sealed class GrpcOrderService : IOrderService
{
    public readonly ILogger _logger;
    public Task<List<MenuItem>> GetMenuAsync(bool withPrice, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SendOrderAsync(Order order, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}