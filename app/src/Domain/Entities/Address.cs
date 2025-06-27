namespace Domain.Entities;
public class Address
{
    public string Reference { get; set; }
    public string Country { get; set; }
    public bool Default { get; set; }
    public string City { get; set; }
    public GeoLocation Location { get; set; }
    public string State { get; set; }
    public string Type { get; set; }
    public string Line1 { get; set; }
    public string Line2 { get; set; }
}
