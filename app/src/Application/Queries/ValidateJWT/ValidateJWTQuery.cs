using Application.Common.Infrastructure;
using AWS.Lambda.Powertools.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;


namespace Application.Queries.ValidateJWT;
public class ValidateJWTQuery
{
    #region Declaraciones y Constructor
    private readonly IValidateTokenRepository _validateTokenRepository;

    public ValidateJWTQuery(IValidateTokenRepository validateTokenRepository)
    {
        _validateTokenRepository = validateTokenRepository;
    }
    #endregion

    #region Métodos
    public async Task<bool> Execute(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = "pqrVLmMTjYP8Sz2YqMW3eZPD7mfUKloGiwgvgcOp3b1cDTYtQDlMbQw8GmoJW998pwUY7WU3/11APjm2aBKvN3A8i8n+GA+NUx3jI4lVK+gCQMtPeV1AZPCjVzvGtgmQhcHQOVklSoZYLvpH40MmiiZroIfzdFQmmhrtbSbgivayvzyb4B5tnebu6UQz400WXRbTr5ZOGjcZuL9ICBXiuiFQ4YjhQ8+zZMmIAesdfY2+AFbEsApIcCGFy6zhS2XXNlksmKXyLsGuITrx9ZQWDZ2TazHFpUJpIYSVOaEVkQ80DNsUqlEA3G34QwrxmvGGGG+g0bfHu6KtJvhMmp3PTEQRSyKOYyq0wIVwZyMk4wU53FFjNOYwavzX9kHqWFjW+so3rtyW56Wcs6Z01sjI6+lvrrH+SZ1H6qhv6Ug/IjxE+2QfNLhCgFZPm3GJGAv0iQFFTDlXVvtRGre1Li1jJzGentazkikv0CAfQW9gCg8QQQiR4gp4HqCMeCr2qbYLwOPjtRAsaxY8cvLZe8KAo6rFJo924BpFLCKsqDGifvEqGLovCOHVNspHUOIHiYfN6+JlJ2nohKunhcgKgVoLsm2mYezTqmKL8ou1nXLA/iv1B4/uX+Pnytqx4YF/5K7xAIaIaqckFkJZLUo1vuMSXWkWv0NVj0v/52CmYwpSivdnzVr0FRn0U6dcN9T7+3C5"; // ⚠️ Usa tu clave de firma adecuada o la pública si es RS256

        try
        {
            var parameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(key)),
                ValidateIssuer = false, // Cambia a true y pon tu issuer si quieres
                ValidateAudience = false, // Cambia a true si tu JWT tiene audience
                                          //ValidIssuer = "tu-issuer",
                                          //ValidAudience = "tu-audience",
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, parameters, out SecurityToken validatedToken);

            await _validateTokenRepository.ValidateTokenWithRemoteAsync(token);

            Logger.LogInformation("Token validad: {principal}", principal.Claims);

            return (true);
        }
        catch
        {
            return (false);
        }
    }
    #endregion
}
