using System;
using System.Collections.Generic;

namespace StoreManagement.Models;

public partial class ProductView
{
    public long Id { get; set; }

    public int? UserId { get; set; }

    public string? SessionId { get; set; }

    public int ProductId { get; set; }

    public string ActionType { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
