using Google.Protobuf.WellKnownTypes;

using Serilog;

using SmartMealService.Domain.Models;
using SmartMealService.Services.Grpc;
using SmartMealService.Services.Interfaces;

namespace SmartMealService.Services;

public sealed class GrpcOrderService : IOrderService
{
    private readonly SmsTestService.SmsTestServiceClient _grpcClient;
    public readonly ILogger _logger;

    public GrpcOrderService(SmsTestService.SmsTestServiceClient grpcClient, ILogger logger)
    {
        _grpcClient = grpcClient ?? throw new ArgumentNullException(nameof(grpcClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<SmsMenuItem>> GetMenuAsync(bool withPrice, CancellationToken cancellationToken)
    {
        var request = new BoolValue { Value = withPrice };
        var response = await _grpcClient.GetMenuAsync(request, null, null, cancellationToken);

        if (!response.Success)
        {
            throw new Exception("Получен ошибочный ответ от сервера."); 
        }

        var domainItems = new List<SmsMenuItem>();

        foreach (var grpcItem in response.MenuItems)
        {
            domainItems.Add(new SmsMenuItem
            {
                Id = long.Parse(grpcItem.Id),
                Article = grpcItem.Article,
                Name = grpcItem.Name,
                Price = (decimal)grpcItem.Price,
                IsWeighted = grpcItem.IsWeighted,
                FullPath = grpcItem.FullPath,
                Barcodes = [..grpcItem.Barcodes]
            });
        }

        return domainItems;
    }

    public async Task<bool> SendOrderAsync(SmsOrder order, CancellationToken cancellationToken)
    {
        var grpcOrder = new Order { Id = order.Id.ToString() };

        foreach (var item in order.Items)
        {
            grpcOrder.OrderItems.Add(new OrderItem { Id = item.MenuItemId.ToString(), Quantity = item.Quantity });
        }

        var response = await _grpcClient.SendOrderAsync(grpcOrder, null, null, cancellationToken);

        return response.Success;
    }
}