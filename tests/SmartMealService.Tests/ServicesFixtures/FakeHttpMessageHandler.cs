using System.Net;

namespace SmartMealService.Tests.ServicesFixtures;

internal sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    public HttpRequestMessage? LastRequest { get; private set; }

    public Func<HttpRequestMessage, Task<HttpResponseMessage>> ResponseFactory { get; init; }
        = req => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

    protected override async Task<HttpResponseMessage> SendAsync
    (
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        LastRequest = request;
        return await ResponseFactory(request);
    }
}