using Application.Common.Infrastructure;
using System.Net.Http.Headers;


namespace Infrastructure.Repositories;

public class ValidateTokenRepository : IValidateTokenRepository
{
    public async Task<bool> ValidateTokenWithRemoteAsync(string token)
    {
        using (var client = new HttpClient())
        {
            // Agrega el header Authorization
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Endpoint a consultar
            var url = "https://r36c7jyp0b.execute-api.us-east-1.amazonaws.com/dev/api/users/profile";

            // Realiza la petición GET
            var response = await client.GetAsync(url);

            // Si responde 200, el token es válido
            if (response.IsSuccessStatusCode)
                return true;

            // Opcional: podrías loguear el status code o el contenido para debug
            var errorContent = await response.Content.ReadAsStringAsync();

            return false;
        }
    }    
}
