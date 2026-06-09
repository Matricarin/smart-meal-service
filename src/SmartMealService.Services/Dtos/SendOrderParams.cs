namespace SmartMealService.Services.Dtos;

public class SendOrderParams
{
    public string OrderId { get; set; }
    public List<HttpOrderItemDto> MenuItems { get; set; }
}