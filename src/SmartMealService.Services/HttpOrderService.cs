using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

using Serilog;

using SmartMealService.Domain.Models;
using SmartMealService.Services.Dtos;
using SmartMealService.Services.Interfaces;

namespace SmartMealService.Services;

public sealed class HttpOrderService : IOrderService
{
    private readonly HttpClient _client;
    private readonly ILogger _logger;

    public HttpOrderService(HttpClient client, AuthData authData, Uri baseUri, ILogger logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _client.BaseAddress = baseUri;
        var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(authData.ToString()));
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
    }

    public async Task<List<SmsMenuItem>> GetMenuAsync(bool withPrice, CancellationToken cancellationToken)
    {
        _logger.Debug("Отправка запроса на получение меню.");

        var requestBody = new HttpCommandRequest<GetMenuParams>
        {
            Command = "GetMenu", CommandParameters = new GetMenuParams { WithPrice = withPrice }
        };

        var response = await _client.PostAsJsonAsync(_client.BaseAddress, requestBody, cancellationToken);

        response.EnsureSuccessStatusCode();

        var jsonResponse =
            await response.Content.ReadFromJsonAsync<HttpCommandResponse<GetMenuData>>(cancellationToken);

        if (jsonResponse is null || !jsonResponse.Success)
        {
            throw new Exception();
        }

        if (jsonResponse.Data is null)
        {
            throw new Exception();
        }

        if (jsonResponse.Data.MenuItems is null)
        {
            throw new Exception();
        }

        return jsonResponse.Data.MenuItems.Select(menuItem => new SmsMenuItem
            {
                Id = long.Parse(menuItem.Id ?? throw new ArgumentNullException(nameof(menuItem.Id))),
                Article = menuItem.Article ?? "(пустой код)",
                Name = menuItem.Name ?? "(пустое имя)",
                Price = menuItem.Price,
                IsWeighted = menuItem.IsWeighted,
                FullPath = menuItem.FullPath ?? "(пустой путь)",
                Barcodes = menuItem.Barcodes ?? []
            })
            .ToList();
    }

    public async Task<bool> SendOrderAsync(SmsOrder smsOrder, CancellationToken cancellationToken)
    {
        _logger.Debug("Отправка заказа на сервер.");

        var menuItemsDto = new List<HttpOrderItemDto>();

        foreach (SmsOrderingItem orderingItem in smsOrder.Items)
        {
            menuItemsDto.Add(new HttpOrderItemDto
            {
                Id = orderingItem.MenuItemId.ToString(),
                Quantity = orderingItem.Quantity.ToString(CultureInfo.CurrentCulture)
            });
        }

        var requestBody = new HttpCommandRequest<SendOrderParams>
        {
            Command = "SendOrder",
            CommandParameters = new SendOrderParams { OrderId = smsOrder.Id.ToString(), MenuItems = menuItemsDto }
        };

        var response = await _client.PostAsJsonAsync(_client.BaseAddress, requestBody, cancellationToken);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadFromJsonAsync<HttpCommandResponse<object>>(cancellationToken);
        return jsonResponse?.Success ?? false;
    }
}