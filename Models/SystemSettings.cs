using System;
using System.Collections.Generic;

namespace StoreManagement.Models;

public partial class SystemSettings
{
    public int Id { get; set; }

    public string SettingKey { get; set; } = null!;

    public string SettingValue { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
