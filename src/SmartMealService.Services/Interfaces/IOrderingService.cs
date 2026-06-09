using SmartMealService.Domain.Models;
using SmartMealService.Services.Dtos;

namespace SmartMealService.Services;

public  interface IOrderingService
{
    Task<List<MenuItem>> GetMenuAsync();
    Task<ResponseDto> SendOrder(Order order);
}