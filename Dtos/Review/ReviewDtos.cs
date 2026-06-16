namespace StoreManagement.Dtos.Review;

public class CreateReviewRequestDto
{
    public int OrderDetailId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}

public class UpdateReviewRequestDto
{
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}

public class AdminReplyRequestDto
{
    public string AdminReply { get; set; } = string.Empty;
}

public class ProductReviewDto
{
    public int? IdReview { get; set; }
    public int? IdProduct { get; set; }
    public int? IdCustomer { get; set; }
    public string? CustomerName { get; set; }
    public int? IdOrder { get; set; }
    public int? IdOrderDetail { get; set; }
    public int? Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? AdminReply { get; set; }
    public int? EditCount { get; set; }
}