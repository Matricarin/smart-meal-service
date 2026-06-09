namespace SmartMealService.Services.Dtos;

public class HttpCommandRequest<T>
{
    public string? Command { get; set; }
    public T? CommandParameters { get; set; }
}