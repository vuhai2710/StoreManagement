using System;
using System.Collections.Generic;

namespace StoreManagement.Models;

public partial class ShippingAddresses
{
    public int IdShippingAddress { get; set; }

    public int IdCustomer { get; set; }

    public string RecipientName { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string Address { get; set; } = null!;

    public bool? IsDefault { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// ID tỉnh/thành phố từ GHN API
    /// </summary>
    public int? ProvinceId { get; set; }

    /// <summary>
    /// ID quận/huyện từ GHN API
    /// </summary>
    public int? DistrictId { get; set; }

    /// <summary>
    /// Code phường/xã từ GHN API
    /// </summary>
    public string? WardCode { get; set; }

    public virtual Customers IdCustomerNavigation { get; set; } = null!;

    public virtual ICollection<Orders> Orders { get; set; } = new List<Orders>();
}
