using System.Text.Json;
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

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AWSLambda;
public class Function
{
    private readonly IDynamoDbService _db = new DynamoDbService(new AmazonDynamoDBClient());
    [Logging(LogEvent = true)] // Esto loggea automáticamente el evento recibido

    #region BK
    //public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    //{
    //    Logger.LogInformation("Inicio de la función");

    //    var order = JsonSerializer.Deserialize<OrderRequest>(request.Body);

    //    var userId = order!.UserId;
    //    var orderId = order.OrderId;

    //    OrderResponse result = new();

    //    switch (request.HttpMethod)
    //    {
    //        case "GET":
    //            result = await new GetOrderByIdQuery(_db).Execute(userId, orderId);
    //            return Response(result);
    //        case "POST":
    //            result = await new CreateOrderCommand(_db).Execute(order!);
    //            return Response(result);
    //        case "PUT":
    //            result = await new UpdateOrderCommand(_db).Execute(order!);
    //            return Response(result);
    //        case "DELETE":
    //            result = await new DeleteOrderCommand(_db).Execute(userId, orderId);
    //            return Response(result);
    //        default:
    //            result.httpStatusCode = 405;
    //            return Response(result);
    //    }
    //    ;
    //}
    //private APIGatewayProxyResponse Response(OrderResponse response)
    //{
    //    return new()
    //    {
    //        StatusCode = response.httpStatusCode,
    //        Body = JsonSerializer.Serialize(new
    //        {
    //            response.detail,
    //            data = string.IsNullOrEmpty(response.data) ? null : JsonSerializer.Deserialize<object>(response.data)
    //        }),
    //        Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
    //    };
    //}

    #endregion

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        Logger.LogInformation("Inicio de la función");

        if (request.Path?.ToLower() == "/swagger" && request.HttpMethod == "GET")
        {
            var swaggerJson = await LoadSwaggerJson();
            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = swaggerJson,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
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

        return Response(result);
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

}
