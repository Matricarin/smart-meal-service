namespace SmartMealService.Domain.Models;

public sealed class SmsOrderingItem
{
    public required long MenuItemId { get; init; }
    public required double Quantity { get; init; }
}