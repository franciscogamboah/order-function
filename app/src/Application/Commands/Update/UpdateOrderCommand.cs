﻿using Amazon.DynamoDBv2.Model;
using Application.Common.Infrastructure;
using Application.Common.Request;
using Application.Common.Response;
using Domain.Entities;

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
        return MappingResponse(response, order.OrderId);
    }
    private OrderResponse MappingResponse(PutItemResponse responseDB, string orderId)
    {
        return new OrderResponse()
        {
            order = (int)responseDB.HttpStatusCode == 200 ? orderId : string.Empty,
            detail = (int)responseDB.HttpStatusCode == 200 ? "Se actualizado el registro exitosamente" : "Ha ocurrido un error en la actualización",
            httpStatusCode = (int)responseDB.HttpStatusCode
        };
    }
    #endregion
}
