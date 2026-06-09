namespace SmartMealService.Domain.Models;

public sealed class Order
{
    public required Guid Id { get; init; }
    public required List<OrderingItem> MenuItems { get; init; }
}