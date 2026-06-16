using Microsoft.EntityFrameworkCore;
using StoreManagement.Data;
using StoreManagement.Models;

namespace StoreManagement.Repositories;

public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly AppDbContext _dbContext;

    public PasswordResetTokenRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<PasswordResetTokens?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return _dbContext.PasswordResetTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Token == token, cancellationToken);
    }

    public async Task InvalidateAllTokensForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var tokens = await _dbContext.PasswordResetTokens
            .Where(x => x.UserId == userId && (x.Used ?? false) == false)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.Used = true;
        }
    }

    public async Task AddAsync(PasswordResetTokens token, CancellationToken cancellationToken = default)
    {
        await _dbContext.PasswordResetTokens.AddAsync(token, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
