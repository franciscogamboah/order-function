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
using Application.Queries.ValidateJWT;
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

        if (request.HttpMethod == "OPTIONS")
        {
            return CreateCorsResponse(200, string.Empty);
        }

        try
        {
            if (!request.Headers.TryGetValue("Authorization", out var authHeader) || string.IsNullOrEmpty(authHeader))
            {
                return CreateCorsResponse(401, "Token no proporcionado");
            }

            var token = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? authHeader.Substring("Bearer ".Length).Trim()
                : authHeader.Trim();

            var tokenIsValid = await new ValidateJWTQuery(_validateTokenRepository).Execute(token);

            if(!tokenIsValid)
            {
                Logger.LogWarning("Token inválido: {token}", token);
                return CreateCorsResponse(401, "Token inválido");
            }

            Logger.LogInformation("Token recibido: {token}", token);

            string userId = string.Empty;
            string orderId = string.Empty;
            OrderRequest? order = null;

            if (request.HttpMethod == "GET")
            {
                if (request.QueryStringParameters == null ||
                    !request.QueryStringParameters.TryGetValue("userid", out userId!) ||
                    !request.QueryStringParameters.TryGetValue("orderid", out orderId!))
                    return CreateCorsResponse(400, "Faltan parámetros obligatorios: userid y orderid");
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(request.Body))
                    order = JsonSerializer.Deserialize<OrderRequest>(request.Body);

                if (order == null)
                    return CreateCorsResponse(400, "El body es requerido y debe tener formato válido");

            }

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
                    result = await new DeleteOrderCommand(_db).Execute(order.UserId, order.OrderId);
                    break;
                default:
                    result = new OrderResponse { httpStatusCode = 405, detail = "Method Not Allowed" };
                    break;
            }

            Logger.LogInformation("Fin de la función");
            return CreateCorsResponse(result.httpStatusCode, result.data!);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error interno en la función");
            return CreateCorsResponse(500, "Internal Server Error");
        }
    }

    private APIGatewayProxyResponse CreateCorsResponse(int statusCode, string body)
    {
        return new APIGatewayProxyResponse
        {
            StatusCode = statusCode,
            Body = body,
            Headers = new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS" },
                { "Access-Control-Allow-Headers", "Content-Type, Authorization" }
            },
        };
    }

}
