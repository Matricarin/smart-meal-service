namespace SmartMealService.Services.Dtos;

public class RequestDto
{
    public string? Command { get; set; }
    public object? CommandParameters { get; set; }
}