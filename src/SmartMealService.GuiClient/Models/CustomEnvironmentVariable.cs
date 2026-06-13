namespace SmartMealService.GuiClient.Models;

public sealed class CustomEnvironmentVariable
{
    public required string Key { get; set; } = string.Empty;
    public required string Value { get; set; } = string.Empty;
    public string? Comment { get; set; }
}