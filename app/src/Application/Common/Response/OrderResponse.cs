namespace Application.Common.Response;
public class OrderResponse
{
    public string order { get; set; }
    public string detail { get; set; }
    public int httpStatusCode { get; set; }
    public string? data {  get; set; }
}
