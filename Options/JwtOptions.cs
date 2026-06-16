namespace StoreManagement.Options;

public class JwtOptions
{
    public string Secret { get; set; } = string.Empty;

    public string SignerKey { get; set; } = string.Empty;

    public long Expiration { get; set; } = 86400000;
}
