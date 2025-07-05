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
                order = orderId,
                detail = "Error interno del servidor",
                httpStatusCode = (int)HttpStatusCode.InternalServerError
            };
        }
    }

    private OrderResponse MappingResponse(DeleteItemResponse responseDB, string orderId)
    {
        return new OrderResponse()
        {
            order = (int)responseDB.HttpStatusCode == 200 ? orderId : string.Empty,
            detail = (int)responseDB.HttpStatusCode == 200 ? "Se eliminado el registro satisfactoriamente" : "Ha ocurrido un error al eliminar el registro",
            httpStatusCode = (int)responseDB.HttpStatusCode
        };
    }
    #endregion
}
