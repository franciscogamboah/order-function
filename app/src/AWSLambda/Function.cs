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
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AWSLambda;
public class Function
{
    private readonly IDynamoDbService _db = new DynamoDbService(new AmazonDynamoDBClient());

    //[Logging(LogEvent = true)] // Esto loggea automáticamente el evento recibido
    [Logging(LogEvent = true)]
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        Logger.LogInformation("Inicio de la función");
        //var logger = Logger.GetLogger(); // obtiene instancia de logger actual

        // Removed the line causing the error as Logger does not have a GetLogger method.
        // The Logger class already provides static methods for logging, so no instance is needed.

        if (request.Path?.ToLower().Contains("/order/swagger") == true && request.HttpMethod == "GET")
        {
            var swaggerJson = await LoadSwaggerJson();
            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = swaggerJson,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }

        if (request.Path?.ToLower().Contains("/swagger/ui") == true && request.HttpMethod == "GET")
        {
            var content = await LoadEmbeddedStaticFile(request.Path!);

            if (content == null)
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = 404,
                    Body = "<h1>Archivo no encontrado</h1>",
                    Headers = new Dictionary<string, string> { { "Content-Type", "text/html" } }
                };
            }

            // Detecta tipo de contenido según extensión
            var extension = Path.GetExtension(request.Path!).ToLowerInvariant();
            var contentType = extension switch
            {
                ".html" => "text/html",
                ".css" => "text/css",
                ".js" => "application/javascript",
                ".png" => "image/png",
                ".json" => "application/json",
                _ => "text/plain"
            };

            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = content,
                Headers = new Dictionary<string, string> { { "Content-Type", contentType } }
            };
        }

        if (string.IsNullOrEmpty(request.Body))
        {
            Logger.LogError("Request.Body is null or empty. Cannot deserialize.");
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = "Invalid request body"
            };
        }

        var order = JsonSerializer.Deserialize<OrderRequest>(request.Body);
        var userId = order!.UserId;
        var orderId = order.OrderId;

        OrderResponse result;

        switch (request.HttpMethod)
        {
            case "GET":
                result = await new GetOrderByIdQuery(_db).Execute(userId, orderId);
                break;
            case "POST":
                result = await new CreateOrderCommand(_db).Execute(order);
                break;
            case "PUT":
                result = await new UpdateOrderCommand(_db).Execute(order);
                break;
            case "DELETE":
                result = await new DeleteOrderCommand(_db).Execute(userId, orderId);
                break;
            default:
                result = new OrderResponse { httpStatusCode = 405, detail = "Method Not Allowed" };
                break;
        }

        var response = Response(result);

        Logger.LogInformation("Fin de la función");

        return response;
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


}
