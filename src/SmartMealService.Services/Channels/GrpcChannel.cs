using Serilog;

using SmartMealService.Domain.Models;
using SmartMealService.Services.Dtos;

namespace SmartMealService.Services.Channels;

public sealed class GrpcChannel : IOrderingService
{
    public readonly ILogger _logger;
    public Task<List<MenuItem>> GetMenuAsync()
    {
        throw new NotImplementedException();
    }

    public Task<ResponseDto> SendOrder(Order order)
    {
        throw new NotImplementedException();
    }
}