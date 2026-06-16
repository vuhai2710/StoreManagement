namespace StoreManagement.Dtos.Shipment;

public class ShipmentDto
{
    public int? IdShipment { get; set; }
    public int? IdOrder { get; set; }
    public string? ShippingStatus { get; set; }
    public string? TrackingNumber { get; set; }
    public decimal? LocationLat { get; set; }
    public decimal? LocationLong { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? GhnOrderCode { get; set; }
    public decimal? GhnShippingFee { get; set; }
    public DateTime? GhnExpectedDeliveryTime { get; set; }
    public string? GhnStatus { get; set; }
    public DateTime? GhnUpdatedAt { get; set; }
    public string? GhnNote { get; set; }
    public string? ShippingMethod { get; set; }
}