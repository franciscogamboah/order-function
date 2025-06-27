using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Application.Common.Extensions;
public static class StringExtensions
{
    public static bool ValidateJson(this string s)
    {
        try
        {
            JToken.Parse(s);
            return true;
        }
        catch (JsonReaderException ex)
        {
            Console.WriteLine($"{nameof(StringExtensions.ValidateJson)} ex.Message: ==> {ex.Message}");
            return false;
        }
    }
}
