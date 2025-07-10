using Amazon.DynamoDBv2.Model;

namespace Application.Common.Interfaces;
public interface IValidateJWT
{
    DateTime? SafeParseDate(Dictionary<string, AttributeValue> item, string key);
}
