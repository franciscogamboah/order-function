namespace Application.Common.Response;
public class OrderResponse
{
    public int Status { get; set; }
    public string? Message { get; set; }
    public dynamic? Data {  get; set; }
}
