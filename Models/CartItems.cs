using System;
using System.Collections.Generic;

namespace StoreManagement.Models;

public partial class CartItems
{
    public int IdCartItem { get; set; }

    public int IdCart { get; set; }

    public int IdProduct { get; set; }

    public int Quantity { get; set; }

    public DateTime? AddedAt { get; set; }

    public virtual Carts IdCartNavigation { get; set; } = null!;

    public virtual Products IdProductNavigation { get; set; } = null!;
}
