using System.Text.Json.Serialization;

namespace SmartMealService.Services.Dtos;

public class HttpCommandRequest<T>
{
    [JsonPropertyName("Command")]
    public string? Command { get; set; }
    [JsonPropertyName("CommandParameters")]
    public T? CommandParameters { get; set; }
}