namespace StoreManagement.Options;

public class GhnOptions
{
    public bool Enabled { get; set; }
    public string BaseUrl { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public int ShopId { get; set; }
    public int? FromDistrictId { get; set; }
}

public class PayOsOptions
{
    public bool Enabled { get; set; }
    public string BaseUrl { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ChecksumKey { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = "http://localhost:3003/payment/success";
    public string CancelUrl { get; set; } = "http://localhost:3003/payment/cancel";
}

public class RecommenderOptions
{
    public string BaseUrl { get; set; } = string.Empty;
}