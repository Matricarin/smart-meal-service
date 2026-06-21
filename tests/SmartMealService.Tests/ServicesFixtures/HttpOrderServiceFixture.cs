using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
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
        Action act = () => new HttpOrderService(null!, _loggerMock);

        act.Should().Throw<ArgumentNullException>().WithParameterName("client");
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        using HttpClient client = new(new FakeHttpMessageHandler()) { BaseAddress = _baseUri };

        Action act = () => new HttpOrderService(client, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public void Constructor_ValidArguments_ShouldConfigureClientCorrectly()
    {
        string expectedAuthToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(_authData.ToString()));

        using HttpClient client = new(new FakeHttpMessageHandler())
        {
            BaseAddress = _baseUri,
            DefaultRequestHeaders = { Authorization = new AuthenticationHeaderValue("Basic", expectedAuthToken) }
        };

        HttpOrderService service = new(client, _loggerMock);

        client.BaseAddress.Should().Be(_baseUri);
        client.DefaultRequestHeaders.Authorization.Should().NotBeNull();
        client.DefaultRequestHeaders.Authorization!.Scheme.Should().Be("Basic");
        client.DefaultRequestHeaders.Authorization!.Parameter.Should().Be(expectedAuthToken);
    }

    [Fact]
    public async Task GetMenuAsync_WhenResponseIsSuccessful_ShouldReturnMappedMenuItems()
    {
        HttpCommandResponse<GetMenuData> apiResponse = new()
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

        FakeHttpMessageHandler handler = new()
        {
            ResponseFactory = _ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(apiResponse)
            })
        };
        string expectedAuthToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(_authData.ToString()));

        using HttpClient client = new(handler);

        client.BaseAddress = _baseUri;

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", expectedAuthToken);

        HttpOrderService service = new(client, _loggerMock);

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
        FakeHttpMessageHandler handler = new()
        {
            ResponseFactory = req => Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError))
        };

        string expectedAuthToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(_authData.ToString()));

        using HttpClient client = new(handler);

        client.BaseAddress = _baseUri;

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", expectedAuthToken);

        HttpOrderService service = new(client, _loggerMock);

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
            HttpCommandResponse<GetMenuData> apiResponse = new()
            {
                Success = successFlag, Data = returnNullData ? null : new GetMenuData { MenuItems = [] }
            };
            httpResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(apiResponse) };
        }

        FakeHttpMessageHandler handler = new() { ResponseFactory = req => Task.FromResult(httpResponse) };
        string expectedAuthToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(_authData.ToString()));

        using HttpClient client = new(handler);

        client.BaseAddress = _baseUri;

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", expectedAuthToken);

        HttpOrderService service = new(client, _loggerMock);

        Func<Task> act = async () => await service.GetMenuAsync(true, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task SendOrderAsync_WhenSuccessful_ShouldPostCorrectJsonAndReturnTrue()
    {
        FakeHttpMessageHandler handler = new()
        {
            ResponseFactory = req => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new HttpCommandResponse<object> { Success = true })
            })
        };

        string expectedAuthToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(_authData.ToString()));

        using HttpClient client = new(handler);

        client.BaseAddress = _baseUri;

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", expectedAuthToken);

        HttpOrderService service = new(client, _loggerMock);
        Guid guid = Guid.NewGuid();
        SmsOrder smsOrder = new() { Id = guid, Items = [new SmsOrderingItem { MenuItemId = 408L, Quantity = 0.408 }] };

        bool result = await service.SendOrderAsync(smsOrder, CancellationToken.None);

        result.Should().BeTrue();
        handler.LastRequest.Should().NotBeNull();
        handler.LastRequest!.Method.Should().Be(HttpMethod.Post);

        string rawJson = await handler.LastRequest.Content!.ReadAsStringAsync();
        using JsonDocument jsonDoc = JsonDocument.Parse(rawJson);

        jsonDoc.RootElement.GetProperty("Command").GetString().Should().Be("SendOrder");

        JsonElement cmdParams = jsonDoc.RootElement.GetProperty("CommandParameters");
        cmdParams.GetProperty("OrderId").GetString().Should().Be(guid.ToString());

        JsonElement menuItemsArray = cmdParams.GetProperty("MenuItems");
        menuItemsArray.EnumerateArray().Count().Should().Be(1);

        JsonElement firstItem = menuItemsArray[0];
        firstItem.GetProperty("Id").GetString().Should().Be("408");

        string expectedQuantityStr = 0.408m.ToString(CultureInfo.CurrentCulture);
        firstItem.GetProperty("Quantity").GetString().Should().Be(expectedQuantityStr);
    }

    [Fact]
    public async Task SendOrderAsync_WhenApiReturnsSuccessFalse_ShouldReturnFalse()
    {
        FakeHttpMessageHandler handler = new()
        {
            ResponseFactory = req => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new HttpCommandResponse<object> { Success = false })
            })
        };

        string expectedAuthToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(_authData.ToString()));

        using HttpClient client = new(handler);

        client.BaseAddress = _baseUri;

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", expectedAuthToken);

        HttpOrderService service = new(client, _loggerMock);
        SmsOrder smsOrder = new() { Id = Guid.NewGuid(), Items = [] };

        bool result = await service.SendOrderAsync(smsOrder, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendOrderAsync_WhenHttpErrorOccurs_ShouldThrowHttpRequestException()
    {
        FakeHttpMessageHandler handler = new()
        {
            ResponseFactory = req => Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest))
        };

        string expectedAuthToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(_authData.ToString()));

        using HttpClient client = new(handler);

        client.BaseAddress = _baseUri;

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", expectedAuthToken);

        HttpOrderService service = new(client, _loggerMock);

        SmsOrder smsOrder = new() { Id = Guid.NewGuid(), Items = [] };

        Func<Task> act = async () => await service.SendOrderAsync(smsOrder, CancellationToken.None);

        await act.Should().ThrowAsync<HttpRequestException>();
    }
}