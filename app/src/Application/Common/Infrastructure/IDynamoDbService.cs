using Amazon.DynamoDBv2.Model;
using Domain.Entities;

namespace Application.Common.Infrastructure;
public interface IDynamoDbService
{
    Task<PutItemResponse> SaveOrderAsync(Order order);
    Task<DeleteItemResponse> DeleteOrderAsync(string userId, string orderId);
    Task<GetItemResponse> GetOrderAsync(string userId, string orderId);
}
