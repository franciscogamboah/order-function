using Application;
using Application.Services;
using Application.Services.Validator;
using FluentAssertions;
using Xunit;

namespace AWSLambda.Tests.Lambda.Validator;
public class MensajeCalculoComisionValidatorTest
{
    private MensajeProductorValidator Validator { get; }
    public MensajeCalculoComisionValidatorTest()
    {
        Validator = new MensajeProductorValidator();
    }
    [Fact]
    public async Task MensajeCalculoComisionValidator_ErrorCuerpoVacioONull_IsValidFalse()
    {
        // arrange
        var address = new MensajeProductor(); 
        address.Id = Guid.NewGuid();
        address.IdSolicitud = 123123;
        //acc
        var valid = await Validator.ValidateAsync(address);

        //Assert
        valid.Errors.Any(a => a.PropertyName == nameof(address.Cuerpo)).Should().BeTrue();
        valid.IsValid.Should().BeFalse();
    }
    [Fact]
    public async Task MensajeCalculoComisionValidator_ErrorIdNull_IsValidFalse()
    {
        // arrange
        var address = new MensajeProductor();
        address.IdSolicitud = 123123;
        address.Cuerpo = new(); 
        //acc
        var valid = await Validator.ValidateAsync(address);

        //Assert
        valid.Errors.Any(a => a.PropertyName == nameof(address.Id)).Should().BeTrue();
        valid.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task MensajeCalculoComisionValidator_ErrorIdSolicitudNull_IsValidFalse()
    {
        // arrange
        var address = new MensajeProductor();
        address.Id = Guid.NewGuid();
        address.Cuerpo = new();
        ////acc
        var valid = await Validator.ValidateAsync(address);

        ////Assert
        valid.Errors.Any(a => a.PropertyName == nameof(address.IdSolicitud)).Should().BeTrue();
        valid.IsValid.Should().BeFalse();
    }
}
