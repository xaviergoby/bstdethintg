using Moq;
using Moq.Protected;
using System.Net;
using System.Reflection;

namespace CoinMaeketCapAPI.Test.HttpHandler;
internal static class MockHttpHandler
{
    internal static Mock<HttpClientHandler> _httpHandler;


    internal static Mock<HttpClientHandler> HttpHandler
    {
        get
        {
            if (_httpHandler == null)
            {
                _httpHandler = new Mock<HttpClientHandler>();

                _httpHandler
                    .Protected()
                    .Setup<Task<HttpResponseMessage>>("SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync((HttpRequestMessage request, CancellationToken cancellationToken) =>
                    {
                        if (request.Method == HttpMethod.Get)
                        {
                            return new HttpResponseMessage()
                            {
                                StatusCode = HttpStatusCode.OK,
                                Content = new StringContent(MockData(request.RequestUri.AbsolutePath)),
                            };
                        }
                        else
                        {
                            return new HttpResponseMessage()
                            {
                                StatusCode = HttpStatusCode.NotFound,
                            };
                        }
                    });
            }

            return _httpHandler;
        }
    }

    private static string MockData(string path)
    {
        var assembly = Assembly.GetExecutingAssembly();

        string resourceFileName = path.Trim('/').Replace("/", "-") + ".json";
        string resourceName = assembly.GetManifestResourceNames().SingleOrDefault(str => str.EndsWith(resourceFileName)) ??
            throw new Exception($"JSON result {resourceFileName} not found.");

        using Stream stream = assembly.GetManifestResourceStream(resourceName);
        using StreamReader reader = new(stream);

        return reader.ReadToEnd();
    }

}
