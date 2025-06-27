namespace Domain.Entities;
public class GetOrderByIdResponse
{
    public string UserId { get; set; }
    public string OrderId { get; set; }
    public string Address { get; set; }
    public string CreatedAt { get; set; }
    public string DeliveredAt { get; set; }
    public string DeliveryRating { get; set; }
    public string DeliveryStartedAt { get; set; }
    public string Items { get; set; }
    public string Notes { get; set; }
    public string PaidAt { get; set; }
    public string PaymentMethod { get; set; }
    public string Status { get; set; }
    public string StoreId { get; set; }
    public string TotalAmount { get; set; }
    public string TrackingStatus { get; set; }
}
