using Amazon.DynamoDBv2.Model;
using Application.Common.Infrastructure;
using Application.Common.Response;

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
        var response = await _db.DeleteOrderAsync(userId, orderId);
        return MappingResponse(response, orderId);
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
