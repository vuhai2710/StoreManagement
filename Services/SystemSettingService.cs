using StoreManagement.Models;
using StoreManagement.Repositories;

namespace StoreManagement.Services;

public class SystemSettingService : ISystemSettingService
{
    private const string ReturnWindowDaysKey = "RETURN_WINDOW_DAYS";
    private const int DefaultReturnWindowDays = 7;
    private const string AutoFreeShippingPromotionKey = "AUTO_FREE_SHIPPING_PROMOTION";
    private const string ReviewEditWindowHoursKey = "REVIEW_EDIT_WINDOW_HOURS";
    private const int DefaultReviewEditWindowHours = 24;

    private readonly ISystemSettingRepository _systemSettingRepository;

    public SystemSettingService(ISystemSettingRepository systemSettingRepository)
    {
        _systemSettingRepository = systemSettingRepository;
    }

    public async Task<int> GetReturnWindowDaysAsync(CancellationToken cancellationToken = default)
    {
        var setting = await _systemSettingRepository.GetByKeyAsync(ReturnWindowDaysKey, cancellationToken);
        return int.TryParse(setting?.SettingValue, out var days) ? days : DefaultReturnWindowDays;
    }

    public async Task UpdateReturnWindowAsync(int days, CancellationToken cancellationToken = default)
    {
        if (days <= 0)
        {
            throw new ArgumentException("Số ngày đổi trả phải lớn hơn 0");
        }

        var setting = await GetOrCreateAsync(ReturnWindowDaysKey, "Số ngày cho phép khách hàng yêu cầu đổi/trả hàng sau khi nhận hàng", cancellationToken);
        setting.SettingValue = days.ToString();
        setting.UpdatedAt = DateTime.UtcNow;
        await _systemSettingRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<string?> GetAutoFreeShippingPromotionAsync(CancellationToken cancellationToken = default)
    {
        var setting = await _systemSettingRepository.GetByKeyAsync(AutoFreeShippingPromotionKey, cancellationToken);
        return string.IsNullOrWhiteSpace(setting?.SettingValue) ? null : setting.SettingValue.Trim();
    }

    public async Task UpdateAutoFreeShippingPromotionAsync(string? code, CancellationToken cancellationToken = default)
    {
        var normalized = string.IsNullOrWhiteSpace(code) ? null : code.Trim();
        if (normalized is null)
        {
            await _systemSettingRepository.DeleteByKeyAsync(AutoFreeShippingPromotionKey, cancellationToken);
            await _systemSettingRepository.SaveChangesAsync(cancellationToken);
            return;
        }

        var setting = await GetOrCreateAsync(AutoFreeShippingPromotionKey, "Mã khuyến mãi freeship tự động áp dụng trên trang chủ", cancellationToken);
        setting.SettingValue = normalized;
        setting.UpdatedAt = DateTime.UtcNow;
        await _systemSettingRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> GetReviewEditWindowHoursAsync(CancellationToken cancellationToken = default)
    {
        var setting = await _systemSettingRepository.GetByKeyAsync(ReviewEditWindowHoursKey, cancellationToken);
        return int.TryParse(setting?.SettingValue, out var hours) ? hours : DefaultReviewEditWindowHours;
    }

    public async Task UpdateReviewEditWindowAsync(int hours, CancellationToken cancellationToken = default)
    {
        if (hours <= 0)
        {
            throw new ArgumentException("Số giờ sửa đánh giá phải lớn hơn 0");
        }

        var setting = await GetOrCreateAsync(ReviewEditWindowHoursKey, "Số giờ cho phép khách hàng chỉnh sửa đánh giá sau khi tạo", cancellationToken);
        setting.SettingValue = hours.ToString();
        setting.UpdatedAt = DateTime.UtcNow;
        await _systemSettingRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task<SystemSettings> GetOrCreateAsync(string key, string description, CancellationToken cancellationToken)
    {
        var setting = await _systemSettingRepository.GetByKeyAsync(key, cancellationToken);
        if (setting is not null)
        {
            return setting;
        }

        setting = new SystemSettings
        {
            SettingKey = key,
            SettingValue = string.Empty,
            Description = description,
            UpdatedAt = DateTime.UtcNow
        };

        await _systemSettingRepository.AddAsync(setting, cancellationToken);
        return setting;
    }
}
