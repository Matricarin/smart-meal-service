using System.Text.Json.Serialization;

namespace SmartMealService.Services.Dtos;

public sealed class HttpCommandResponse<T>
{
    [JsonPropertyName("Command")]
    public string? Command { get; set; }
    [JsonPropertyName("Success")]
    public bool Success { get; set; }

    [JsonPropertyName("ErrorMessage")]
    public string? ErrorMessage { get; set; }

    [JsonPropertyName("Data")]
    public T? Data { get; set; }
}