namespace StoreManagement.Services;

public interface ISystemSettingService
{
    Task<int> GetReturnWindowDaysAsync(CancellationToken cancellationToken = default);
    Task UpdateReturnWindowAsync(int days, CancellationToken cancellationToken = default);
    Task<string?> GetAutoFreeShippingPromotionAsync(CancellationToken cancellationToken = default);
    Task UpdateAutoFreeShippingPromotionAsync(string? code, CancellationToken cancellationToken = default);
    Task<int> GetReviewEditWindowHoursAsync(CancellationToken cancellationToken = default);
    Task UpdateReviewEditWindowAsync(int hours, CancellationToken cancellationToken = default);
}
