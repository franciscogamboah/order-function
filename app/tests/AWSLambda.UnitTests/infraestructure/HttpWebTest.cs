using System.Net;
using System.Text.Json;
using Application;
using Application.Common.Exceptions;
using Application.Common.Infrastructure;
using Application.Services;
using FluentAssertions;
using Infrastructure.Common.Rest;
using Moq;
using Moq.Protected;
using Xunit;
using static Application.Common.Constants.HttpWeb;

namespace AWSLambda.Tests.infraestructure;
public class HttpWebTest
{
    private const string _endPoint = "http://localost/*";

    [Theory]
    [InlineData(MethodProtocolHttp.Post, "test content POST")]
    [InlineData(MethodProtocolHttp.Get, "test content GET")]
    [InlineData(MethodProtocolHttp.Put, "test content PUT")]
    [InlineData(MethodProtocolHttp.Delete, "test content DELETE")]
    [InlineData(MethodProtocolHttp.Patch, "test content PATCH")]
    public async Task HttpWebStatusCodeOKTest(string method, string response)
    {
        //arrange
        var mockMessageHandler = new Mock<HttpMessageHandler>();
        mockMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(response)
            });

        var httpWeb = new HttpWeb(new HttpClient(mockMessageHandler.Object));

        var httpWebObject = CrearMessageComisionSyndeo(method);

        // Act
        var httpWebResponse = await httpWeb.ConsumeServiceAsync(httpWebObject, 0);

        //Assert
        httpWebResponse.response.Should().BeOfType<string>();
        httpWebResponse.response.Should().Be(response);
    }
    [Theory]
    [InlineData(MethodProtocolHttp.Post)]
    [InlineData(MethodProtocolHttp.Get)]
    [InlineData(MethodProtocolHttp.Put)]
    [InlineData(MethodProtocolHttp.Delete)]
    [InlineData(MethodProtocolHttp.Patch)]
    public async Task HttpWebStatusCodeErrorTest(string method)
    {
        //arrange
        var mockMessageHandler = new Mock<HttpMessageHandler>();
        mockMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Throws(new Exception());

        var httpWeb = new HttpWeb(new HttpClient(mockMessageHandler.Object));

        var httpWebObject = CrearMessageComisionSyndeo(method);

        // Act
        Func<Task> response = async () => await httpWeb.ConsumeServiceAsync(httpWebObject, 0);

        //Assert
        await response.Should().ThrowAsync<HttpClientException>();
    }
    private static HttpWebObject CrearMessageComisionSyndeo(string method)
    {
        return new HttpWebObject()
        {
            MethodProtocolHTTP = method,
            DataJson = JsonSerializer.Serialize(new MensajeProductor
            {
                Id = Guid.NewGuid(),
                IdSolicitud = 1,
                Cuerpo = new MensajeProductorCuerpo()
                {
                    IdSolicitud = 1,
                    IdServicio = 1,
                    TipoComision = 1,
                    IdMoneda = 1,
                    Monto = 1,
                    ValorFijo = 1,
                    ValorPorcentual = 1,
                    ValorTope = 1,
                }
            }),
            Url = _endPoint
        };
    }
}
