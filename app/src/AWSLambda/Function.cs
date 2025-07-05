using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using AWS.Lambda.Powertools.Logging;
using Domain.Entities;
using Domain.Services;
using Infrastructure.Services;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;

// Para Lambda
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

public class Function
{
    private readonly IDynamoDbService _db = new DynamoDbService(new Amazon.DynamoDBv2.AmazonDynamoDBClient());

    [Logging(LogEvent = true)]
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        Logger.LogInformation("Inicio de la función");

        // ⚠️ Manejar rutas de Swagger UI y sus archivos estáticos
        if (request.Path?.ToLower().StartsWith("/api/order/ui") == true && request.HttpMethod == "GET")
        {
            return await HandleSwaggerUiRequest(request);
        }

        // 🎯 Swagger JSON
        if (request.Path?.ToLower().EndsWith("/swagger") == true && request.HttpMethod == "GET")
        {
            var swaggerJson = await LoadEmbeddedResource("swagger.json", "application/json");
            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = swaggerJson,
                Headers = new() { { "Content-Type", "application/json" } }
            };
        }

        // 📦 Procesar payload JSON
        if (string.IsNullOrEmpty(request.Body))
        {
            Logger.LogError("El cuerpo de la solicitud está vacío");
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = "Bad Request: Falta body"
            };
        }

        var order = JsonSerializer.Deserialize<OrderRequest>(request.Body);
        if (order == null)
        {
            Logger.LogError("El JSON no pudo deserializarse correctamente");
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = "Bad Request: JSON inválido"
            };
        }

        OrderResponse result;
        try
        {
            switch (request.HttpMethod)
            {
                case "GET":
                    result = await new GetOrderByIdQuery(_db).Execute(order.UserId, order.OrderId);
                    break;
                case "POST":
                    result = await new CreateOrderCommand(_db).Execute(order);
                    break;
                case "PUT":
                    result = await new UpdateOrderCommand(_db).Execute(order);
                    break;
                case "DELETE":
                    result = await new DeleteOrderCommand(_db).Execute(order.UserId, order.OrderId);
                    break;
                default:
                    result = new OrderResponse { httpStatusCode = 405, detail = "Method Not Allowed" };
                    break;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error en el procesamiento de la orden");
            result = new OrderResponse
            {
                httpStatusCode = 500,
                detail = "Internal Server Error"
            };
        }

        Logger.LogInformation("Fin de la función");
        return FormatResponse(result);
    }

    private APIGatewayProxyResponse FormatResponse(OrderResponse response)
    {
        return new()
        {
            StatusCode = response.httpStatusCode,
            Body = JsonSerializer.Serialize(new
            {
                response.detail,
                data = string.IsNullOrEmpty(response.data) ? null : JsonSerializer.Deserialize<object>(response.data)
            }),
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }

    private async Task<APIGatewayProxyResponse> HandleSwaggerUiRequest(APIGatewayProxyRequest request)
    {
        var relativePath = request.Path.Replace("/api/order/ui", "").TrimStart('/');
        if (string.IsNullOrEmpty(relativePath))
            relativePath = "index.html";

        string contentType = GetMimeType(relativePath);
        try
        {
            var content = await LoadEmbeddedResource(relativePath, contentType);

            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = content,
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", contentType }
                }
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Archivo embebido no encontrado: {relativePath}");
            return new APIGatewayProxyResponse
            {
                StatusCode = 404,
                Body = "Archivo no encontrado"
            };
        }
    }

    private async Task<string> LoadEmbeddedResource(string fileName, string contentType)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));

        if (resourceName == null)
            throw new FileNotFoundException($"No se encontró el archivo embebido: {fileName}");

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var content = await reader.ReadToEndAsync();

        // Reemplazar URL de Swagger JSON (opcional)
        if (fileName == "index.html")
        {
            content = content.Replace("https://petstore.swagger.io/v2/swagger.json", "/api/order/swagger");
        }

        return content;
    }

    private string GetMimeType(string file)
    {
        return Path.GetExtension(file).ToLower() switch
        {
            ".html" => "text/html",
            ".js" => "application/javascript",
            ".css" => "text/css",
            ".json" => "application/json",
            ".png" => "image/png",
            ".svg" => "image/svg+xml",
            ".ico" => "image/x-icon",
            _ => "application/octet-stream"
        };
    }
}
