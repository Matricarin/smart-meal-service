using System.Text.Json.Serialization;

namespace SmartMealService.Services.Dtos;

public class HttpOrderItemDto
{
    [JsonPropertyName("Id")]
    public string? Id { get; set; }

    [JsonPropertyName("Quantity")]
    public string? Quantity { get; set; }
}