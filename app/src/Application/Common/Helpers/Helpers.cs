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
            Reference = map.ContainsKey("Reference") ? map["Reference"].S : null,
            Country = map.ContainsKey("Country") ? map["Country"].S : null,
            Default = map.ContainsKey("Default") ? map["Default"].BOOL ?? false : false,
            City = map.ContainsKey("City") ? map["City"].S : null,
            Location = map.ContainsKey("Location") ? ParseGeoLocation(map["Location"].M) : null,
            State = map.ContainsKey("State") ? map["State"].S : null,
            Type = map.ContainsKey("Type") ? map["Type"].S : null,
            Line1 = map.ContainsKey("Line1") ? map["Line1"].S : null,
            Line2 = map.ContainsKey("Line2") ? map["Line2"].S : null
        };
    }
    public GeoLocation ParseGeoLocation(Dictionary<string, AttributeValue> map)
    {
        double lat = map.ContainsKey("Lat") && !string.IsNullOrEmpty(map["Lat"].N)
            ? double.Parse(map["Lat"].N)
            : 0.0;

        double lon = map.ContainsKey("Lon") && !string.IsNullOrEmpty(map["Lon"].N)
            ? double.Parse(map["Lon"].N)
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