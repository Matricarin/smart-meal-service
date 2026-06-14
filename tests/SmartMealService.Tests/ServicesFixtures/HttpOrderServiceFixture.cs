using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

using FluentAssertions;

using NSubstitute;

using Serilog;

using SmartMealService.Domain.Models;
using SmartMealService.Services;
using SmartMealService.Services.Dtos;

namespace SmartMealService.Tests.ServicesFixtures;

public sealed class HttpOrderServiceFixture
{
    private readonly AuthData _authData;
    private readonly Uri _baseUri;
    private readonly ILogger _loggerMock;

    public HttpOrderServiceFixture()
    {
        _loggerMock = Substitute.For<ILogger>();
        _baseUri = new Uri("https://api.smartmeal.com/v1/");
        _authData = new AuthData("password", "user");
    }
    
    [Fact]
    public void Constructor_WithNullClient_ShouldThrowArgumentNullException()
    {
        Action act = () => new HttpOrderService(null!, _authData, _baseUri, _loggerMock);

        act.Should().Throw<ArgumentNullException>().WithParameterName("client");
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        using var client = new HttpClient(new FakeHttpMessageHandler());

        Action act = () => new HttpOrderService(client, _authData, _baseUri, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public void Constructor_ValidArguments_ShouldConfigureClientCorrectly()
    {
        using var client = new HttpClient(new FakeHttpMessageHandler());
        var expectedAuthToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(_authData.ToString()));

        var service = new HttpOrderService(client, _authData, _baseUri, _loggerMock);

        client.BaseAddress.Should().Be(_baseUri);
        client.DefaultRequestHeaders.Authorization.Should().NotBeNull();
        client.DefaultRequestHeaders.Authorization!.Scheme.Should().Be("Basic");
        client.DefaultRequestHeaders.Authorization!.Parameter.Should().Be(expectedAuthToken);
    }

    [Fact]
    public async Task GetMenuAsync_WhenResponseIsSuccessful_ShouldReturnMappedMenuItems()
    {
        var apiResponse = new HttpCommandResponse<GetMenuData>
        {
            Success = true,
            Data = new GetMenuData
            {
                MenuItems = new List<HttpMenuItemDto>
                {
                    new()
                    {
                        Id = "408",
                        Article = "ART-01",
                        Name = "Смарт Обеды",
                        Price = 350.50m,
                        IsWeighted = true,
                        FullPath = "/menu/lunch",
                        Barcodes = new List<string> { "123456789" }
                    }
                }
            }
        };

        var handler = new FakeHttpMessageHandler
        {
            ResponseFactory = req => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(apiResponse)
            })
        };

        using var client = new HttpClient(handler);
        var service = new HttpOrderService(client, _authData, _baseUri, _loggerMock);

        List<SmsMenuItem> result = await service.GetMenuAsync(true, CancellationToken.None);

        result.Should().NotBeNull().And.HaveCount(1);

        result[0].Id.Should().Be(408L);
        result[0].Article.Should().Be("ART-01");
        result[0].Name.Should().Be("Смарт Обеды");
        result[0].Price.Should().Be(350.50m);
        result[0].IsWeighted.Should().BeTrue();
        result[0].FullPath.Should().Be("/menu/lunch");
        result[0].Barcodes.Should().ContainSingle().Which.Should().Be("123456789");
    }

    [Fact]
    public async Task GetMenuAsync_WhenHttpErrorOccurs_ShouldThrowHttpRequestException()
    {
        var handler = new FakeHttpMessageHandler
        {
            ResponseFactory = req => Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError))
        };

        using var client = new HttpClient(handler);

        var service = new HttpOrderService(client, _authData, _baseUri, _loggerMock);

        Func<Task> act = async () => await service.GetMenuAsync(false, CancellationToken.None);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Theory]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(false, false, false)]
    public async Task GetMenuAsync_WhenBusinessValidationFails_ShouldThrowException
    (
        bool returnNullResponse,
        bool successFlag,
        bool returnNullData
    )
    {
        HttpResponseMessage httpResponse;
        if (returnNullResponse)
        {
            httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("null", Encoding.UTF8, "application/json")
            };
        }
        else
        {
            var apiResponse = new HttpCommandResponse<GetMenuData>
            {
                Success = successFlag, Data = returnNullData ? null : new GetMenuData { MenuItems = [] }
            };
            httpResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(apiResponse) };
        }

        var handler = new FakeHttpMessageHandler { ResponseFactory = req => Task.FromResult(httpResponse) };
        using var client = new HttpClient(handler);
        var service = new HttpOrderService(client, _authData, _baseUri, _loggerMock);

        Func<Task> act = async () => await service.GetMenuAsync(true, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task SendOrderAsync_WhenSuccessful_ShouldPostCorrectJsonAndReturnTrue()
    {
        var handler = new FakeHttpMessageHandler
        {
            ResponseFactory = req => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new HttpCommandResponse<object> { Success = true })
            })
        };

        using var client = new HttpClient(handler);
        var service = new HttpOrderService(client, _authData, _baseUri, _loggerMock);
        var guid = Guid.NewGuid();
        var smsOrder = new SmsOrder
        {
            Id = guid, Items = [new SmsOrderingItem { MenuItemId = 408L, Quantity = 0.408 }]
        };

        bool result = await service.SendOrderAsync(smsOrder, CancellationToken.None);

        result.Should().BeTrue();
        handler.LastRequest.Should().NotBeNull();
        handler.LastRequest!.Method.Should().Be(HttpMethod.Post);

        var rawJson = await handler.LastRequest.Content!.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(rawJson);

        jsonDoc.RootElement.GetProperty("Command").GetString().Should().Be("SendOrder");

        var cmdParams = jsonDoc.RootElement.GetProperty("CommandParameters");
        cmdParams.GetProperty("OrderId").GetString().Should().Be(guid.ToString());

        var menuItemsArray = cmdParams.GetProperty("MenuItems");
        menuItemsArray.EnumerateArray().Count().Should().Be(1);

        var firstItem = menuItemsArray[0];
        firstItem.GetProperty("Id").GetString().Should().Be("408");

        var expectedQuantityStr = 0.408m.ToString(CultureInfo.CurrentCulture);
        firstItem.GetProperty("Quantity").GetString().Should().Be(expectedQuantityStr);
    }

    [Fact]
    public async Task SendOrderAsync_WhenApiReturnsSuccessFalse_ShouldReturnFalse()
    {
        var handler = new FakeHttpMessageHandler
        {
            ResponseFactory = req => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new HttpCommandResponse<object> { Success = false })
            })
        };

        using var client = new HttpClient(handler);
        var service = new HttpOrderService(client, _authData, _baseUri, _loggerMock);
        var smsOrder = new SmsOrder { Id = Guid.NewGuid(), Items = [] };

        bool result = await service.SendOrderAsync(smsOrder, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendOrderAsync_WhenHttpErrorOccurs_ShouldThrowHttpRequestException()
    {
        var handler = new FakeHttpMessageHandler
        {
            ResponseFactory = req => Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest))
        };

        using var client = new HttpClient(handler);
        var service = new HttpOrderService(client, _authData, _baseUri, _loggerMock);
        var smsOrder = new SmsOrder { Id = Guid.NewGuid(), Items = [] };

        Func<Task> act = async () => await service.SendOrderAsync(smsOrder, CancellationToken.None);

        await act.Should().ThrowAsync<HttpRequestException>();
    }
}