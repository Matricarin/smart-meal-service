using System.Text.Json.Serialization;

namespace SmartMealService.Services.Dtos;

public class GetMenuData
{
    [JsonPropertyName("MenuItems")]
    public List<HttpMenuItemDto>? MenuItems { get; set; }
}