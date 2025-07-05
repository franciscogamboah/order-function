using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Runtime.Internal;
using Application.Commands.Create;
using Application.Commands.Delete;
using Application.Commands.Update;
using Application.Common.Infrastructure;
using Application.Common.Request;
using Application.Common.Response;
using Application.Queries;
using AWS.Lambda.Powertools.Logging;
using System.Reflection;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AWSLambda;
public class Function
{
    private readonly IDynamoDbService _db = new DynamoDbService(new AmazonDynamoDBClient());

    [Logging(LogEvent = true)]
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        Logger.LogInformation("Inicio de la función");

        try
        {
            // 1. Manejo de Swagger JSON
            if (request.Path?.ToLower().EndsWith("/swagger") == true && request.HttpMethod == "GET")
            {
                var swaggerJson = await LoadSwaggerJson();
                return new APIGatewayProxyResponse
                {
                    StatusCode = 200,
                    Body = swaggerJson,
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
            }

            // 2. Manejo de Swagger UI (index.html + archivos estáticos)
            if (request.Path?.ToLower().Contains("/ui") == true && request.HttpMethod == "GET")
            {
                var html = await LoadSwaggerUiHtml();
                return new APIGatewayProxyResponse
                {
                    StatusCode = 200,
                    Body = html,
                    Headers = new Dictionary<string, string> { { "Content-Type", "text/html" } }
                };
            }

            // 3. Validar que el cuerpo exista si se requiere
            if (request.HttpMethod != "GET" && string.IsNullOrWhiteSpace(request.Body))
            {
                Logger.LogError("El body está vacío en una solicitud que lo requiere");
                return new APIGatewayProxyResponse
                {
                    StatusCode = 400,
                    Body = "Bad Request: Body requerido"
                };
            }

            OrderRequest order = null!;
            if (!string.IsNullOrWhiteSpace(request.Body))
            {
                order = JsonSerializer.Deserialize<OrderRequest>(request.Body)
                    ?? throw new Exception("Error al deserializar el cuerpo.");
            }

            OrderResponse result;

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

            Logger.LogInformation("Fin de la función");
            return Response(result);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error interno en la función");
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = "Internal Server Error"
            };
        }
    }


    private APIGatewayProxyResponse Response(OrderResponse response)
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

    private async Task<string> LoadSwaggerJson()
    {
        var assembly = typeof(Function).Assembly;
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith("swagger.json"));

        if (resourceName == null) return "{}";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream!);
        return await reader.ReadToEndAsync();
    }

    private async Task<string> LoadSwaggerUiHtml()
    {
        var assembly = typeof(Function).Assembly;
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith("index.html")); // Puede cambiar según el archivo principal

        if (resourceName == null) return "<h1>No se encontró Swagger UI</h1>";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream!);
        var html = await reader.ReadToEndAsync();

        // Puedes inyectar aquí tu endpoint JSON
        return html.Replace("https://petstore.swagger.io/v2/swagger.json", "/api/order/swagger");
    }

    private async Task<string> LoadEmbeddedStaticFile(string resourcePath)
    {
        var assembly = typeof(Function).Assembly;

        // Normaliza el path recibido, por ejemplo:
        // "/api/order/ui/swagger-ui.css" -> "swagger-ui.css"
        var fileName = Path.GetFileName(resourcePath);

        // Busca el recurso que termina con ese nombre
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));

        if (resourceName == null)
        {
            Logger.LogWarning("Archivo embebido no encontrado: {FileName}", fileName);
            return null!;
        }

        using var stream = assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream!);
        return await reader.ReadToEndAsync();
    }

    private string GetContentType(string fileName) =>
    Path.GetExtension(fileName) switch
    {
        ".html" => "text/html",
        ".js" => "application/javascript",
        ".css" => "text/css",
        ".json" => "application/json",
        ".png" => "image/png",
        _ => "application/octet-stream"
    };

}
