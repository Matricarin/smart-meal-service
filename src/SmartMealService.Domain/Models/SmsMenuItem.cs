namespace SmartMealService.Domain.Models;

public sealed class SmsMenuItem
{
    public required long Id { get; init; }
    public required string Article { get; init; }
    public required string Name { get; set; }
    public required decimal Price { get; set; }
    public required bool IsWeighted { get; init; }
    public required string FullPath { get; init; }
    public required List<string> Barcodes { get; init; } = new();
}