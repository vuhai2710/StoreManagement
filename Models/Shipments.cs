using System;
using System.Collections.Generic;

namespace StoreManagement.Models;

public partial class Shipments
{
    public int IdShipment { get; set; }

    public int IdOrder { get; set; }

    public string? ShippingStatus { get; set; }

    public string? TrackingNumber { get; set; }

    /// <summary>
    /// Vĩ độ
    /// </summary>
    public decimal? LocationLat { get; set; }

    /// <summary>
    /// Kinh độ
    /// </summary>
    public decimal? LocationLong { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Mã đơn hàng từ GHN API
    /// </summary>
    public string? GhnOrderCode { get; set; }

    /// <summary>
    /// Phí vận chuyển từ GHN
    /// </summary>
    public decimal? GhnShippingFee { get; set; }

    /// <summary>
    /// Thời gian giao hàng dự kiến từ GHN
    /// </summary>
    public DateTime? GhnExpectedDeliveryTime { get; set; }

    /// <summary>
    /// Trạng thái đơn hàng từ GHN (ready_to_pick, delivering, delivered, etc.)
    /// </summary>
    public string? GhnStatus { get; set; }

    /// <summary>
    /// Thời gian cập nhật trạng thái từ GHN webhook
    /// </summary>
    public DateTime? GhnUpdatedAt { get; set; }

    /// <summary>
    /// Ghi chú hoặc lý do từ GHN (ví dụ: lý do giao thất bại)
    /// </summary>
    public string? GhnNote { get; set; }

    /// <summary>
    /// Phương thức vận chuyển: GHN hoặc khách tự đến lấy
    /// </summary>
    public string? ShippingMethod { get; set; }

    public virtual Orders IdOrderNavigation { get; set; } = null!;
}
