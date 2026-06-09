namespace SmartMealService.Services.Dtos;

public sealed class OrderDto
{
    public string? OrderId { get; set; }
    public List<OrderingItemDto>? MenuItems { get; set; }
}