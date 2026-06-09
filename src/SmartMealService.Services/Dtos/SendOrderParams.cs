using System.Text.Json.Serialization;

namespace SmartMealService.Services.Dtos;

public class SendOrderParams
{
    [JsonPropertyName("OrderId")]
    public string OrderId { get; set; }
    [JsonPropertyName("MenuItems")]
    public List<HttpOrderItemDto> MenuItems { get; set; }
}