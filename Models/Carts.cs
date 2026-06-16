using System;
using System.Collections.Generic;

namespace StoreManagement.Models;

public partial class Carts
{
    public int IdCart { get; set; }

    public int IdCustomer { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<CartItems> CartItems { get; set; } = new List<CartItems>();

    public virtual Customers IdCustomerNavigation { get; set; } = null!;
}
