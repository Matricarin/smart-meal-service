namespace SmartMealService.Domain.Models;

public sealed class MenuItem
{
    public required long Id { get; init; }
    public required string Article { get; init; }
    public required string Name { get; init; }
    public required decimal Price { get; init; }
    public required bool IsWeighted { get; init; }
    public required string FullPath { get; init; }
    public required List<string> Barcodes { get; init; } = new();
}