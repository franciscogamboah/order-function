using System.Net;
using Application;
using Application.Common;
using Application.Common.Infrastructure;
using Application.Common.Interfaces;
using Application.Common.Settings;
using Application.Services;
using Application.Services.Validator;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace AWSLambda.Tests.Lambda;
public class ComisionSyndeoServiceTest 
{
    private readonly MensajeProductorValidator _validator;
    private readonly ComisionSettings _comisionSettings;
    private readonly Mock<IEventServiceSqs> _eventServiceSqs;
    private readonly string _token;
    private readonly Mock<IIdentityService> _jwt = null!;
    public ComisionSyndeoServiceTest() {
        _validator = new MensajeProductorValidator();
        _comisionSettings = new() { ApiUrl = "ApiUrl", PathProcesar = "PathProcesar", PathRegistrarDlq = "PathRegistrarDlq" };
        _eventServiceSqs = new Mock<IEventServiceSqs>();
        _token = "akj345lj6j34h6h456o4n8v45860485673867v830476n840357n6v83045760";
        _jwt = new Mock<IIdentityService>();
    }

    [Fact]
    public async Task ComisionSyndeoTest_CalculoComision_CalculoOk()
    {
        // arrange
        _jwt.Setup(s => s.GenerateJWToken()).Returns(_token);
        
        var httpMock = new Mock<IHttpWeb>();
        httpMock.Setup(s => s.ConsumeServiceAsync(It.IsAny<HttpWebObject>(), 51190, false)).Returns(Task.FromResult((HttpStatusCode.OK, "{ \"idSolicitud\" : 51190, \"respuesta\": 1, \"codigoError\": 200, \"mensajeError\": \"OK\"}")));

        var request = CrearMessageComisionSyndeo();
        var comisionSyndeo = new ComisionSyndeoService(_jwt.Object, httpMock.Object, _validator, _comisionSettings, _eventServiceSqs.Object);

        //acc
        var response = await comisionSyndeo.ExecCalcularComisionAsync(request);

        //Assert
        response.Respuesta.Should().Be(1);
        response.IdSolicitud.Should().Be(51190);
    }
    
    [Fact]
    public async Task ComisionSyndeoTest_CalculoComisionUpdateEstadoNoAccesible_ValidaBodyVacio()
    {
        var mensajeSqs = string.Empty;
        var idSolicitud = 51190;
       
        _jwt.Setup(s => s.GenerateJWToken()).Returns(_token);
        
        var httpMock = new Mock<IHttpWeb>();
        httpMock.Setup(s => s.ConsumeServiceAsync(It.IsAny<HttpWebObject>(), idSolicitud, false))
            .Returns(Task.FromResult((HttpStatusCode.InternalServerError, "{ \"IdSolicitud\" : 51190, \"Respuesta\": 1}")));

        //acc
        var comisionSyndeo = new ComisionSyndeoService(_jwt.Object, httpMock.Object, _validator, _comisionSettings, _eventServiceSqs.Object);
        
        var response = await comisionSyndeo.ExecCalcularComisionAsync(mensajeSqs);

        //Assert
        response.CodigoError.Should().Be((int)HttpStatusCode.InternalServerError);
        response.MensajeError.Should().Be(Constants.Exception.PayloadVacio);
    }
    [Fact]
    public async Task ComisionSyndeoTest_CalculoComisionUpdateEstadoNoAccesible_ValidaBodyFormatoError()
    {
        var mensajeSqs = "jason formato error";
        var idSolicitud = 51190;

        _jwt.Setup(s => s.GenerateJWToken()).Returns(_token);

        var httpMock = new Mock<IHttpWeb>();
        httpMock.Setup(s => s.ConsumeServiceAsync(It.IsAny<HttpWebObject>(), idSolicitud, false))
            .Returns(Task.FromResult((HttpStatusCode.InternalServerError, "{ \"IdSolicitud\" : 51190, \"Respuesta\": 1}")));

        //acc
        var comisionSyndeo = new ComisionSyndeoService(_jwt.Object, httpMock.Object, _validator, _comisionSettings, _eventServiceSqs.Object);

        var response = await comisionSyndeo.ExecCalcularComisionAsync(mensajeSqs);

        //Assert
        response.CodigoError.Should().Be((int)HttpStatusCode.InternalServerError);
        response.MensajeError.Should().Be(Constants.Exception.PayloadFormatoJasonIncorrecto);
    }

    [Fact]
    public async Task ComisionSyndeoTest_CalculoComisionUpdateEstadoNoAccesible_ValidaBodyValoresError()
    {
        var mensajeSqs = "{\"IdSolicitud\":0,\"Id\":\"00000000-0000-0000-0000-000000000000\",\"Cuerpo\":null}";
        var idSolicitud = 51190;

        _jwt.Setup(s => s.GenerateJWToken()).Returns(_token);

        var httpMock = new Mock<IHttpWeb>();
        httpMock.Setup(s => s.ConsumeServiceAsync(It.IsAny<HttpWebObject>(), idSolicitud, false))
            .Returns(Task.FromResult((HttpStatusCode.InternalServerError, "{ \"IdSolicitud\" : 51190, \"Respuesta\": 1}")));

        //acc
        var comisionSyndeo = new ComisionSyndeoService(_jwt.Object, httpMock.Object, _validator, _comisionSettings, _eventServiceSqs.Object);

        var response = await comisionSyndeo.ExecCalcularComisionAsync(mensajeSqs);

        //Assert
        response.CodigoError.Should().Be((int)HttpStatusCode.InternalServerError);
        response.MensajeError.Should().Be(Constants.Exception.PayloadFalloValidador);
    }
    [Fact]
    public async Task ComisionSyndeoTest_CalculoComision_CalculoError500()
    {
        // arrange
        _jwt.Setup(s => s.GenerateJWToken()).Returns(_token);

        var httpMock = new Mock<IHttpWeb>();
        httpMock.Setup(s => s.ConsumeServiceAsync(It.IsAny<HttpWebObject>(), 51190, false)).Returns(Task.FromResult((HttpStatusCode.ServiceUnavailable, "{ \"IdSolicitud\" : 51190, \"Respuesta\": 1, \"CodigoError\": 200, \"MensajeError\": \"OK\"}")));

        var request = CrearMessageComisionSyndeo();
        var comisionSyndeo = new ComisionSyndeoService(_jwt.Object, httpMock.Object, _validator, _comisionSettings, _eventServiceSqs.Object);

        //acc
        var response = await comisionSyndeo.ExecCalcularComisionAsync(request);

        //Assert
        response.CodigoError.Should().Be((int)HttpStatusCode.ServiceUnavailable);
        response.MensajeError.Should().Be(Constants.Exception.FalloConsultarApi);
    }
    [Fact]
    public async Task ComisionSyndeoTest_CalculoComision_CalculoOkErrorCalculoApi()
    {
        // arrange
        _jwt.Setup(s => s.GenerateJWToken()).Returns(_token);

        var httpMock = new Mock<IHttpWeb>();
        httpMock.Setup(s => s.ConsumeServiceAsync(It.IsAny<HttpWebObject>(), 51190, false)).Returns(Task.FromResult((HttpStatusCode.OK, "{ \"IdSolicitud\" : 51190, \"Respuesta\": 1, \"CodigoError\": 500, \"MensajeError\": \"OK\"}")));

        var request = CrearMessageComisionSyndeo();
        var comisionSyndeo = new ComisionSyndeoService(_jwt.Object, httpMock.Object, _validator, _comisionSettings, _eventServiceSqs.Object);

        //acc
        var response = await comisionSyndeo.ExecCalcularComisionAsync(request);

        //Assert
        response.CodigoError.Should().Be((int)HttpStatusCode.InternalServerError);
        response.MensajeError.Should().Be(Constants.Exception.FalloCalcularComisionApi);
    }
  
    private static string CrearMessageComisionSyndeo()
    {
        return JsonConvert.SerializeObject(new MensajeProductor
        {
            Id = Guid.NewGuid(),
            IdSolicitud = 51190,
            Cuerpo = new MensajeProductorCuerpo()
            {
                IdSolicitud = 51190,
                IdServicio = 1,
                TipoComision = 1,
                IdMoneda = 1,
                Monto = 1,
                ValorFijo = 1,
                ValorPorcentual = 1,
                ValorTope = 1,
            }
        });
    }
}
