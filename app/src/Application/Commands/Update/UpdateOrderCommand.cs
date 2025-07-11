using Amazon.DynamoDBv2.Model;
using Application.Common.Infrastructure;
using Application.Common.Request;
using Application.Common.Response;
using AWS.Lambda.Powertools.Logging;
using Domain.Entities;
using System.Net;
using System.Text.Json;

namespace Application.Commands.Update;
public class UpdateOrderCommand
{
    #region Declaraciones y Constructor
    private readonly IDynamoDbService _db;
    public UpdateOrderCommand(IDynamoDbService db)
    {
        _db = db;
    }
    #endregion

    #region Métodos
    public async Task<OrderResponse> Execute(OrderRequest request)
    {
        try
        {
            var order = new Order()
            {
                Address = request.Address,
                CreatedAt = request.CreatedAt,
                DeliveredAt = request.DeliveredAt,
                DeliveryRating = request.DeliveryRating,
                DeliveryStartedAt = request.DeliveryStartedAt,
                Items = request.Items,
                Notes = request.Notes,
                OrderId = request.OrderId,
                PaidAt = request.PaidAt,
                PaymentMethod = request.PaymentMethod,
                Status = request.Status,
                StoreId = request.PaymentMethod,
                TotalAmount = request.TotalAmount,
                TrackingStatus = request.TrackingStatus,
                UserId = request.UserId
            };

            var response = await _db.SaveOrderAsync(order);

            var orderResponse = MappingResponse(response, order.OrderId);

            Logger.LogInformation("Se realizó la actualización satisfactoriamente {@orderResponse}", orderResponse);

            return orderResponse;
        }
        catch(Exception ex)
        {
            Logger.LogError(ex, "Error al actualizar el registro {@request}", request);

            return new OrderResponse
            {
                Status = (int)HttpStatusCode.InternalServerError,
                Message = "Error interno del servidor",
                Data = null
            };
        }
    }
    private OrderResponse MappingResponse(PutItemResponse responseDB, string orderId)
    {
        var orderResponse = new
        {
            OrderId = orderId,
        };

        return new OrderResponse()
        {
            Status = (int)responseDB.HttpStatusCode,
            Message = (int)responseDB.HttpStatusCode == 200 ? "Se actualizado el registro exitosamente" : "Ha ocurrido un error al eliminar el registro",
            Data = orderResponse
        };
    }
    #endregion
}
