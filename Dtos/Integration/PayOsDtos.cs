namespace StoreManagement.Dtos.Integration;

public class PayOsPaymentDataDto
{
    public string? PaymentLinkId { get; set; }
    public string? CheckoutUrl { get; set; }
    public string? QrCode { get; set; }
    public long? OrderCode { get; set; }
    public decimal? Amount { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
}

public class PayOsPaymentResponseDto
{
    public string? Code { get; set; }
    public string? Desc { get; set; }
    public PayOsPaymentDataDto? Data { get; set; }
}

public class PayOsWebhookDataDto
{
    public string? PaymentLinkId { get; set; }
    public long? OrderCode { get; set; }
}

public class PayOsWebhookDto
{
    public string? Code { get; set; }
    public string? Desc { get; set; }
    public string? Signature { get; set; }
    public PayOsWebhookDataDto? Data { get; set; }
}