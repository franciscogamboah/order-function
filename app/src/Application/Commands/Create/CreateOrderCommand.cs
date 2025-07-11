using System.Net;
using Amazon.DynamoDBv2.Model;
using Application.Common.Infrastructure;
using Application.Common.Request;
using Application.Common.Response;
using Domain.Entities;
using AWS.Lambda.Powertools.Logging;
using System.Text.Json;

namespace Application.Commands.Create;
public class CreateOrderCommand
{
    #region Declaraciones y Constructor
    private readonly IDynamoDbService _db;
    public CreateOrderCommand(IDynamoDbService db)
    {
        _db = db;
    }
    #endregion

    #region Métodos
    public async Task<OrderResponse> Execute(OrderRequest request)
    {
        try
        {
            var newOrder = Guid.NewGuid().ToString();

            var order = new Order()
            {
                Address = request.Address,
                CreatedAt = DateTime.UtcNow,
                DeliveredAt = DateTime.UtcNow,
                DeliveryRating = request.DeliveryRating,
                DeliveryStartedAt = DateTime.UtcNow,
                Items = request.Items,
                Notes = request.Notes,
                OrderId = newOrder.ToString(),
                PaidAt = DateTime.UtcNow,
                PaymentMethod = request.PaymentMethod,
                Status = request.Status,
                StoreId = request.PaymentMethod,
                TotalAmount = request.TotalAmount,
                TrackingStatus = request.TrackingStatus,
                UserId = request.UserId
            };

            var response = await _db.SaveOrderAsync(order);

            var orderResponse = MappingResponse(response, newOrder);

            Logger.LogInformation("Se realizó el registro satisfactoriamente {@orderResponse}", orderResponse);

            return orderResponse;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error al insertar el registro para la orden {@request}", request);

            return new OrderResponse
            {
                order = string.Empty,
                detail = "Error interno del servidor",
                httpStatusCode = (int)HttpStatusCode.InternalServerError
            };
        }
    }

    private OrderResponse MappingResponse(PutItemResponse responseDB, string newOrder)
    {
        var orderResponse = new CreateOrderResponse()
        {
            OrderId = newOrder
        };

        return new OrderResponse()
        {
            order = (int)responseDB.HttpStatusCode == 200 ? newOrder : string.Empty,
            detail = (int)responseDB.HttpStatusCode == 200 ? "El registro se realizó exitosamente" : "Ha ocurrido un error en la inserción",
            httpStatusCode = (int)responseDB.HttpStatusCode,
            data = JsonSerializer.Serialize(orderResponse)
        };
    }
    #endregion
}
