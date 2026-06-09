using SmartMealService.Services.Dtos;

namespace SmartMealService.Services;

public interface IOrderingService
{
    Task<List<MenuItemDto>> GetMenu(RequestDto requestDto);
    Task<ResponseDto> SendOrder(RequestDto requestDto);
}