using FluentAssertions;

using Google.Protobuf.WellKnownTypes;

using Grpc.Core;
using Grpc.Core.Testing;

using NSubstitute;

using Serilog;

using SmartMealService.Domain.Models;
using SmartMealService.Services;
using SmartMealService.Services.Grpc;

namespace SmartMealService.Tests.ServicesFixtures;

public sealed class GrpcOrderServiceFixture
{
    private readonly SmsTestService.SmsTestServiceClient _mockGrpcClient;
    private readonly ILogger _mockLogger;
    private readonly GrpcOrderService _sut;

    public GrpcOrderServiceFixture()
    {
        _mockGrpcClient = Substitute.For<SmsTestService.SmsTestServiceClient>();
        _mockLogger = Substitute.For<ILogger>();
        _sut = new GrpcOrderService(_mockGrpcClient, _mockLogger);
    }
    
    [Fact]
    public void Constructor_WhenGrpcClientIsNull_ShouldThrowArgumentNullException()
    {
        Action act = () => new GrpcOrderService(null!, _mockLogger);
        act.Should().Throw<ArgumentNullException>().WithParameterName("grpcClient");
    }

    [Fact]
    public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException()
    {
        Action act = () => new GrpcOrderService(_mockGrpcClient, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public async Task GetMenuAsync_WhenServerReturnsSuccess_ShouldMapAndReturnDomainItems()
    {
        const bool inputWithPrice = true;
        var cancellationToken = CancellationToken.None;

        var grpcResponse = new GetMenuResponse { Success = true };

        grpcResponse.MenuItems.Add(new MenuItem
        {
            Id = "12345",
            Article = "ART-01",
            Name = "Борщ",
            Price = 250.50,
            IsWeighted = false,
            FullPath = "Супы/Борщ",
            Barcodes = { "11111111", "22222222" }
        });

        var fakeCall = TestCalls.AsyncUnaryCall(
            Task.FromResult(grpcResponse),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => [],
            () => { });

        _mockGrpcClient.GetMenuAsync(
                Arg.Any<BoolValue>(),
                Arg.Any<Metadata>(),
                Arg.Any<DateTime?>(),
                Arg.Any<CancellationToken>())
            .Returns(fakeCall);

        var result = await _sut.GetMenuAsync(inputWithPrice, cancellationToken);

        result.Should().NotBeNull().And.HaveCount(1);

        var mappedItem = result.First();
        mappedItem.Id.Should().Be(12345L);
        mappedItem.Article.Should().Be("ART-01");
        mappedItem.Name.Should().Be("Борщ");
        mappedItem.Price.Should().Be(250.50m);
        mappedItem.IsWeighted.Should().BeFalse();
        mappedItem.FullPath.Should().Be("Супы/Борщ");
        mappedItem.Barcodes.Should().ContainInOrder("11111111", "22222222");
    }

    [Fact]
    public async Task GetMenuAsync_WhenServerReturnsSuccessFalse_ShouldThrowException()
    {
        var grpcResponse = new GetMenuResponse { Success = false };
        var fakeCall = TestCalls.AsyncUnaryCall(
            Task.FromResult(grpcResponse),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });

        _mockGrpcClient.GetMenuAsync(
                Arg.Any<BoolValue>(),
                Arg.Any<Metadata>(),
                Arg.Any<DateTime?>(),
                Arg.Any<CancellationToken>())
            .Returns(fakeCall);

        Func<Task> act = async () => await _sut.GetMenuAsync(true, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task GetMenuAsync_WhenIdIsNotValidNumber_ShouldThrowFormatException()
    {
        // Arrange
        var grpcResponse = new GetMenuResponse { Success = true };
        grpcResponse.MenuItems.Add(new MenuItem { Id = "not-a-number" }); // Вызовет падение на long.Parse()

        var fakeCall = TestCalls.AsyncUnaryCall(
            Task.FromResult(grpcResponse),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });

        _mockGrpcClient.GetMenuAsync(
                Arg.Any<BoolValue>(),
                Arg.Any<Metadata>(),
                Arg.Any<DateTime?>(),
                Arg.Any<CancellationToken>())
            .Returns(fakeCall);

        Func<Task> act = async () => await _sut.GetMenuAsync(true, CancellationToken.None);

        await act.Should().ThrowAsync<FormatException>();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SendOrderAsync_WhenCalled_ShouldMapOrderAndReturnServerStatus(bool serverResult)
    {
        var cancellationToken = CancellationToken.None;
        var guid = Guid.NewGuid();
        var domainOrder = new SmsOrder
        {
            Id = guid, Items = new List<SmsOrderingItem> { new() { MenuItemId = 555, Quantity = 2 } }
        };

        var grpcResponse = new SendOrderResponse { Success = serverResult };
        var fakeCall = TestCalls.AsyncUnaryCall(
            Task.FromResult(grpcResponse),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });

        _mockGrpcClient.SendOrderAsync(
                Arg.Any<Order>(),
                Arg.Any<Metadata>(),
                Arg.Any<DateTime?>(),
                Arg.Any<CancellationToken>())
            .Returns(fakeCall);

        var result = await _sut.SendOrderAsync(domainOrder, cancellationToken);

        result.Should().Be(serverResult);
    }
}