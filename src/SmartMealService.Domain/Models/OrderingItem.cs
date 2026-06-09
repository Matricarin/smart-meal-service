namespace SmartMealService.Domain.Models;

public sealed class OrderingItem
{
    public required MenuItem MenuItem { get; init; }
    public required float Quantity { get; init; }
}