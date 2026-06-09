using System.Text.Json.Serialization;

namespace SmartMealService.Services.Dtos;

public class HttpMenuItemDto
{
    [JsonPropertyName("Id")]
    public string Id { get; set; }
    [JsonPropertyName("Article")]
    public string Article { get; set; }
    [JsonPropertyName("Name")]
    public string Name { get; set; }
    [JsonPropertyName("Price")]
    public decimal Price { get; set; }
    [JsonPropertyName("IsWeighted")]
    public bool IsWeighted { get; set; }
    [JsonPropertyName("FullPath")]
    public string FullPath { get; set; }
    [JsonPropertyName("Barcodes")]
    public List<string> Barcodes { get; set; }
}