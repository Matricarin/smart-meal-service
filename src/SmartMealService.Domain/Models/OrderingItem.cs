namespace SmartMealService.Domain.Models;

public sealed class OrderingItem
{
    public required MenuItem MenuItem { get; init; }
    public required double Quantity { get; init; }
}