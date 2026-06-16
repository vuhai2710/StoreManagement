using System.Text.Json.Serialization;

namespace StoreManagement.Dtos.Base;

public abstract class BaseDto
{
    [JsonPropertyName("createdAt")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime? UpdatedAt { get; set; }
}
