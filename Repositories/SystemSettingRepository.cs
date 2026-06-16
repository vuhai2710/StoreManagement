using Microsoft.EntityFrameworkCore;
using StoreManagement.Data;
using StoreManagement.Models;

namespace StoreManagement.Repositories;

public class SystemSettingRepository : ISystemSettingRepository
{
    private readonly AppDbContext _dbContext;

    public SystemSettingRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<SystemSettings?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return _dbContext.SystemSettings.FirstOrDefaultAsync(x => x.SettingKey == key, cancellationToken);
    }

    public async Task DeleteByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        var setting = await _dbContext.SystemSettings.FirstOrDefaultAsync(x => x.SettingKey == key, cancellationToken);
        if (setting is not null)
        {
            _dbContext.SystemSettings.Remove(setting);
        }
    }

    public async Task AddAsync(SystemSettings setting, CancellationToken cancellationToken = default)
    {
        await _dbContext.SystemSettings.AddAsync(setting, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
