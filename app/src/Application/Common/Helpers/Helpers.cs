using Amazon.DynamoDBv2.Model;
using Application.Common.Interfaces;
using Domain.Entities;

namespace Application.Common.Helpers;

public class Helpers : IHelpers
{
    public DateTime? SafeParseDate(Dictionary<string, AttributeValue> item, string key)
    {
        if (item.ContainsKey(key))
        {
            var value = item[key]?.S;
            if (!string.IsNullOrWhiteSpace(value) && DateTime.TryParse(value, out var parsed))
            {
                return parsed;
            }
        }
        return null;
    }
    public Address ParseAddress(Dictionary<string, AttributeValue> map)
    {
        return new Address
        {
            Reference = map.ContainsKey("reference") ? map["reference"].S : null,
            Country = map.ContainsKey("country") ? map["country"].S : null,
            Default = map.ContainsKey("default") ? map["default"].BOOL ?? false : false,
            City = map.ContainsKey("city") ? map["city"].S : null,
            Location = map.ContainsKey("location") ? ParseGeoLocation(map["location"].M) : null,
            State = map.ContainsKey("state") ? map["state"].S : null,
            Type = map.ContainsKey("type") ? map["type"].S : null,
            Line1 = map.ContainsKey("line1") ? map["line1"].S : null,
            Line2 = map.ContainsKey("line2") ? map["line2"].S : null
        };
    }
    public GeoLocation ParseGeoLocation(Dictionary<string, AttributeValue> map)
    {
        double lat = map.ContainsKey("lat") && !string.IsNullOrEmpty(map["lat"].N)
            ? double.Parse(map["lat"].N)
            : 0.0;

        double lon = map.ContainsKey("lon") && !string.IsNullOrEmpty(map["lon"].N)
            ? double.Parse(map["lon"].N)
            : 0.0;

        return new GeoLocation
        {
            Lat = lat,
            Lon = lon
        };
    }
    public List<OrderItem> ParseOrderItems(List<AttributeValue> items)
    {
        var list = new List<OrderItem>();
        foreach (var item in items)
        {
            var map = item.M;
            list.Add(new OrderItem
            {
                SkuId = map["sku_id"].S,
                Price = decimal.Parse(map["price"].N),
                Quantity = int.Parse(map["quantity"].N)
            });
        }
        return list;
    }
}