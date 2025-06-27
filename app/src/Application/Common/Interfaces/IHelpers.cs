using Amazon.DynamoDBv2.Model;
using Domain.Entities;

namespace Application.Common.Interfaces;
public interface IHelpers
{
    DateTime? SafeParseDate(Dictionary<string, AttributeValue> item, string key);
    Address ParseAddress(Dictionary<string, AttributeValue> map);
    GeoLocation ParseGeoLocation(Dictionary<string, AttributeValue> map);
    List<OrderItem> ParseOrderItems(List<AttributeValue> items);
}
