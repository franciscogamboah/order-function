namespace Domain.Entities;
public class OrderItem
{
    public string SkuId { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
