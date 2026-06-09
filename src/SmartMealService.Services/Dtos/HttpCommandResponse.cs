namespace SmartMealService.Services.Dtos;

public sealed class HttpCommandResponse<T>
{
    public string? Command { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public T? Data { get; set; }
}