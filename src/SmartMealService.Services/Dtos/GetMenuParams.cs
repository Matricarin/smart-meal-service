using System.Text.Json.Serialization;

namespace SmartMealService.Services.Dtos;

public class GetMenuParams
{
    [JsonPropertyName("WithPrice")]
    public bool WithPrice { get; set; }
}