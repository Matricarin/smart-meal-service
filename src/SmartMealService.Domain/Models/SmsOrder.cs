namespace SmartMealService.Domain.Models;

public sealed class SmsOrder
{
    public required Guid Id { get; init; }
    public required List<SmsOrderingItem> Items { get; init; } = [];
}