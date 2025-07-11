using Amazon.DynamoDBv2.Model;
using Application.Common.Infrastructure;
using Application.Common.Response;
using AWS.Lambda.Powertools.Logging;
using System.Net;

namespace Application.Commands.Delete;
public class DeleteOrderCommand
{
    #region Declaraciones y Constructor
    private readonly IDynamoDbService _db;
    public DeleteOrderCommand(IDynamoDbService db)
    {
        _db = db;
    }
    #endregion

    #region Métodos
    public async Task<OrderResponse> Execute(string userId, string orderId)
    {
        try
        {
            var response = await _db.DeleteOrderAsync(userId, orderId);

            var orderResponse = MappingResponse(response, orderId);

            Logger.LogInformation("Se realizó la actualización satisfactoriamente {@orderResponse}", orderResponse);

            return orderResponse;
        }
        catch (Exception ex)
        {
            var request = new
            {
                UserId = userId,
                OrderId = orderId
            };

            Logger.LogError(ex, "Error al eliminar el registro para la orden {@request}", request);

            return new OrderResponse
            {
                Status = (int)HttpStatusCode.InternalServerError,
                Message = "Error interno del servidor",
                Data = null
            };
        }
    }

    private OrderResponse MappingResponse(DeleteItemResponse responseDB, string orderId)
    {
        var orderResponse = new
        {
            OrderId = orderId,
        };

        return new OrderResponse()
        {
            Status = (int)responseDB.HttpStatusCode,
            Message = (int)responseDB.HttpStatusCode == 200 ? "Se eliminado el registro satisfactoriamente" : "Ha ocurrido un error al eliminar el registro",
            Data = orderResponse
        };
    }
    #endregion
}
