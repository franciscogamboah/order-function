using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Application.Commands.Create;
using Application.Commands.Delete;
using Application.Commands.Update;
using Application.Common.Infrastructure;
using Application.Common.Request;
using Application.Common.Response;
using Application.Queries;
using AWS.Lambda.Powertools.Logging;
using Infrastructure.Repositories;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AWSLambda;
public class Function
{
    private readonly IDynamoDbService _db = new DynamoDbService(new AmazonDynamoDBClient());
    private readonly IValidateTokenRepository _validateTokenRepository = new ValidateTokenRepository();

    [Logging(LogEvent = true)]
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        Logger.LogInformation("Inicio de la función");

        try
        {
            // 1. Obtener el token del header
            if (!request.Headers.TryGetValue("Authorization", out var authHeader) || string.IsNullOrEmpty(authHeader))
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = 401,
                    Body = "Token no proporcionado"
                };
            }

            // El formato esperado es "Bearer <token>"
            var token = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? authHeader.Substring("Bearer ".Length).Trim()
                : authHeader.Trim();

            var tokenIsValid = await _validateTokenRepository.ValidateTokenWithRemoteAsync(token);

            if(!tokenIsValid)
                {
                Logger.LogWarning("Token inválido: {token}", token);
                return new APIGatewayProxyResponse
                {
                    StatusCode = 401,
                    Body = "Token inválido"
                };
            }

            Logger.LogInformation("Token recibido: {token}", token);

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
}
