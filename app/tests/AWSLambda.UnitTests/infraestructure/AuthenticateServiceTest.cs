using FluentAssertions;
using Infrastructure.Identity;
using Infrastructure.Services;
using Xunit;

namespace AWSLambda.Tests.infraestructure;
public class AuthenticateServiceTest 
{
    [Fact]
    public void AuthenticateService_GeneraToken_OK()
    {
        //arrange
        var _datetime = new DateTimeService();
        var jwtSetting = new JwtSettings() {
            Key = "11111111111111111-CHANGE-ME-1111111111111111111111",
            Issuer= "CoreIdentity",
            Audience = "CoreIdentityUser",
            DurationInMinutes = 30,
            DurationRefreshTokeCookieInDays = 1,
        };

        var authenticateService = new AuthenticateService(jwtSetting, _datetime);

        // Act
        var token = authenticateService.GenerateJWToken();

        //Assert
        token.Should().BeOfType<string>();
        token.Should().NotBeEmpty();
        token.Should().NotBeNull();
        (token.Length > 0).Should().Be(true);
    }
}
