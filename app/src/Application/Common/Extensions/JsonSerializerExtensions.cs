using System.Text.Json;

namespace Application.Common.Extensions;
public static class JsonSerializerExtensions
{
    public static TValue? Deserialize<TValue>(string json){
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        return JsonSerializer.Deserialize<TValue>(json, options);
    }
    public static string Serialize<TValue>(TValue json)
    {
        return JsonSerializer.Serialize(json);
    }
    
}
