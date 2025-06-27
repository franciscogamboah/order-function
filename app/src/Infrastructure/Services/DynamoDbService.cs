using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Application.Common.Infrastructure;
using Domain.Entities;

public class DynamoDbService: IDynamoDbService
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly string _tableName = "orders";

    public DynamoDbService(IAmazonDynamoDB dynamoDb)
    {
        _dynamoDb = dynamoDb;        
    }

    public async Task<PutItemResponse> SaveOrderAsync(Order order)
    {
        var request = new PutItemRequest
        {
            TableName = _tableName,
            Item = new Dictionary<string, AttributeValue>
            {
                ["user_id"] = new AttributeValue { S = order.UserId },
                ["order_id"] = new AttributeValue { S = order.OrderId },
                // Dirección serializada como un Map
                ["address"] = new AttributeValue
                {
                    M = new Dictionary<string, AttributeValue>
                    {
                        ["reference"] = new AttributeValue { S = order.Address!.Reference ?? string.Empty },
                        ["country"] = new AttributeValue { S = order.Address.Country ?? string.Empty },
                        ["default"] = new AttributeValue { BOOL = order.Address.Default },
                        ["city"] = new AttributeValue { S = order.Address.City ?? string.Empty },
                        ["state"] = new AttributeValue { S = order.Address.State ?? string.Empty },
                        ["type"] = new AttributeValue { S = order.Address.Type ?? string.Empty },
                        ["line1"] = new AttributeValue { S = order.Address.Line1 ?? string.Empty },
                        ["line2"] = new AttributeValue { S = order.Address.Line2 ?? string.Empty },
                        ["location"] = new AttributeValue
                        {
                            M = new Dictionary<string, AttributeValue>
                            {
                                ["lat"] = new AttributeValue { N = order.Address.Location.Lat.ToString() },
                                ["lon"] = new AttributeValue { N = order.Address.Location.Lon.ToString() }
                            }
                        }
                    }
                },
                ["created_at"] = order.CreatedAt.HasValue
                    ? new AttributeValue { S = order.CreatedAt.Value.ToString("o") }
                    : new AttributeValue { NULL = true },
                ["delivered_at"] = order.DeliveredAt.HasValue
                    ? new AttributeValue { S = order.DeliveredAt.Value.ToString("o") }
                    : new AttributeValue { NULL = true },
                ["delivery_rating"] = order.DeliveryRating.HasValue
                    ? new AttributeValue { N = order.DeliveryRating.Value.ToString() }
                    : new AttributeValue { NULL = true },
                ["delivery_started_at"] = order.DeliveryStartedAt.HasValue
                    ? new AttributeValue { S = order.DeliveryStartedAt.Value.ToString("o") }
                    : new AttributeValue { NULL = true },
                // Items es una lista de mapas
                ["items"] = new AttributeValue
                {
                    L = order.Items!.Select(item => new AttributeValue
                    {
                        M = new Dictionary<string, AttributeValue>
                        {
                            ["sku_id"] = new AttributeValue { S = item.SkuId },
                            ["price"] = new AttributeValue { N = item.Price.ToString("F2") },
                            ["quantity"] = new AttributeValue { N = item.Quantity.ToString() }
                        }
                    }).ToList()
                },
                ["notes"] = new AttributeValue { S = order.Notes ?? string.Empty },
                ["paid_at"] = order.PaidAt.HasValue
                    ? new AttributeValue { S = order.PaidAt.Value.ToString("o") }
                    : new AttributeValue { NULL = true },
                ["payment_method"] = new AttributeValue { S = order.PaymentMethod },
                ["status"] = new AttributeValue { S = order.Status },
                ["store_id"] = new AttributeValue { S = order.StoreId },
                ["total_amount"] = new AttributeValue { N = order.TotalAmount.ToString("F2") },
                ["tracking_status"] = new AttributeValue { S = order.TrackingStatus ?? string.Empty }
            }
        };

        return await _dynamoDb.PutItemAsync(request);
    }

    public async Task<DeleteItemResponse> DeleteOrderAsync(string userId, string orderId)
    {
        var request = new DeleteItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                ["user_id"] = new AttributeValue { S = userId },
                ["order_id"] = new AttributeValue { S = orderId }
            }
        };

        return await _dynamoDb.DeleteItemAsync(request);
    }

    public async Task<GetItemResponse> GetOrderAsync(string userId, string orderId)
    {
        var request = new GetItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                ["user_id"] = new AttributeValue { S = userId },
                ["order_id"] = new AttributeValue { S = orderId }
            }
        };

        var response = await _dynamoDb.GetItemAsync(request);
        if (!response.IsItemSet) return null;

        var item = response.Item;

        return response;
    }
}
