﻿using System.Net;
using System.Text.Json;
using Amazon.DynamoDBv2.Model;
using Application.Common.Infrastructure;
using Application.Common.Interfaces;
using Application.Common.Response;
using Domain.Entities;

namespace Application.Queries;
public class GetOrderByIdQuery
{
    #region Declaraciones y Constructor
    private readonly IDynamoDbService _db;
    private readonly IHelpers _helpers;
    public GetOrderByIdQuery(IDynamoDbService db) => _db = db;
    #endregion

    #region Métodos
    public async Task<OrderResponse> Execute(string userId, string orderId)
    {
        try
        {
            var response = await _db.GetOrderAsync(userId, orderId);

            if (response is not null)
            {
                return new OrderResponse()
                {
                    order = orderId,
                    detail = $"No se encontró el recurso con el Id {orderId}",
                    httpStatusCode = (int)HttpStatusCode.NotFound,
                    data = null,
                };
            }

            return MappingResponse(response!);
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Error al obtener la orden", ex);
        }
    }
    private OrderResponse MappingResponse(GetItemResponse responseDB)
    {
        var item = new Dictionary<string, AttributeValue>(responseDB.Item, StringComparer.OrdinalIgnoreCase);

        var order = new Order
        {
            UserId = item["user_id"].S,
            OrderId = item["order_id"].S,
            Address = _helpers.ParseAddress(item["address"].M),
            CreatedAt = item.ContainsKey("created_at") ? DateTime.Parse(item["created_at"].S) : null,
            DeliveredAt = _helpers.SafeParseDate(item, "created_at"),
            DeliveryRating = item.ContainsKey("delivery_rating") ? int.Parse(item["delivery_rating"].N) : 0,
            DeliveryStartedAt = _helpers.SafeParseDate(item, "delivery_started_at"),
            Items = _helpers.ParseOrderItems(item["items"].L),
            Notes = item.ContainsKey("notes") ? item["notes"].S : null,
            PaidAt = _helpers.SafeParseDate(item, "paid_at"),
            PaymentMethod = item.ContainsKey("payment_method") ? item["payment_method"].S : null,
            Status = item.ContainsKey("status") ? item["status"].S : null,
            StoreId = item.ContainsKey("store_id") ? item["store_id"].S : null,
            TotalAmount = item.ContainsKey("total_amount") && item["total_amount"].N != null ? decimal.Parse(item["total_amount"].N) : 0,
            TrackingStatus = item.ContainsKey("tracking_status") ? item["tracking_status"].S : null
        };

        return new OrderResponse()
        {
            order = (int)responseDB.HttpStatusCode == 200 ? order.OrderId : string.Empty,
            detail = (int)responseDB.HttpStatusCode == 200 ? "Se ha obtenido la información satisfactoriamente" : "Ha ocurrido un error al consultar la información",
            httpStatusCode = (int)responseDB.HttpStatusCode,
            data = JsonSerializer.Serialize(order),
        };
    }
    #endregion
}
