using System.Globalization;
using System.Net.Http.Json;

using Serilog;

using SmartMealService.Domain.Models;
using SmartMealService.Services.Dtos;
using SmartMealService.Services.Interfaces;

namespace SmartMealService.Services;

public sealed class HttpOrderService : IOrderService
{
    private readonly HttpClient _client;
    private readonly ILogger _logger;

    public HttpOrderService(HttpClient client, ILogger logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<SmsMenuItem>> GetMenuAsync(bool withPrice, CancellationToken cancellationToken)
    {
        _logger.Debug("Отправка запроса на получение меню.");

        HttpCommandRequest<GetMenuParams> requestBody = new()
        {
            Command = "GetMenu", CommandParameters = new GetMenuParams { WithPrice = withPrice }
        };

        HttpResponseMessage response =
            await _client.PostAsJsonAsync(_client.BaseAddress, requestBody, cancellationToken);

        response.EnsureSuccessStatusCode();

        HttpCommandResponse<GetMenuData>? jsonResponse =
            await response.Content.ReadFromJsonAsync<HttpCommandResponse<GetMenuData>>(cancellationToken);

        if (jsonResponse is null || !jsonResponse.Success)
        {
            throw new Exception("Получен ошибочный ответ от сервера");
        }

        if (jsonResponse.Data is null)
        {
            throw new Exception("Не получены данные от сервера.");
        }

        if (jsonResponse.Data.MenuItems is null)
        {
            throw new Exception("Не получено меню от сервера.");
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

        List<HttpOrderItemDto> menuItemsDto = new();

        foreach (SmsOrderingItem orderingItem in smsOrder.Items)
        {
            menuItemsDto.Add(new HttpOrderItemDto
            {
                Id = orderingItem.MenuItemId.ToString(),
                Quantity = orderingItem.Quantity.ToString(CultureInfo.CurrentCulture)
            });
        }

        HttpCommandRequest<SendOrderParams> requestBody = new()
        {
            Command = "SendOrder",
            CommandParameters = new SendOrderParams { OrderId = smsOrder.Id.ToString(), MenuItems = menuItemsDto }
        };

        HttpResponseMessage response =
            await _client.PostAsJsonAsync(_client.BaseAddress, requestBody, cancellationToken);
        response.EnsureSuccessStatusCode();

        HttpCommandResponse<object>? jsonResponse =
            await response.Content.ReadFromJsonAsync<HttpCommandResponse<object>>(cancellationToken);
        return jsonResponse?.Success ?? false;
    }
}