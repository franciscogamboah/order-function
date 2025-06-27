using Domain.Entities;

namespace Application.Common.Request;
public class OrderRequest
{
    public string UserId { get; set; }
    public string OrderId { get; set; }
    public Address Address { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public int? DeliveryRating { get; set; }
    public DateTime? DeliveryStartedAt { get; set; }
    public List<OrderItem> Items { get; set; }
    public string Notes { get; set; }
    public DateTime? PaidAt { get; set; }
    public string PaymentMethod { get; set; }
    public string Status { get; set; }
    public string StoreId { get; set; }
    public decimal TotalAmount { get; set; }
    public string TrackingStatus { get; set; }
}