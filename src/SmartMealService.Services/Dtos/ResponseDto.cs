namespace SmartMealService.Services.Dtos;

public sealed class ResponseDto
{
    public string? Command { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public object? Data { get; set; }
}