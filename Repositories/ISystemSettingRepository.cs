using StoreManagement.Models;

namespace StoreManagement.Repositories;

public interface ISystemSettingRepository
{
    Task<SystemSettings?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task DeleteByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task AddAsync(SystemSettings setting, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
