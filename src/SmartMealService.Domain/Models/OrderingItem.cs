namespace SmartMealService.Domain.Models;

public sealed class OrderingItem
{
    public required long MenuItemId { get; init; }
    public required double Quantity { get; init; }
}