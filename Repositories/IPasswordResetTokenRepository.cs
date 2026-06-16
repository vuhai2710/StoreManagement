using StoreManagement.Models;

namespace StoreManagement.Repositories;

public interface IPasswordResetTokenRepository
{
    Task<PasswordResetTokens?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task InvalidateAllTokensForUserAsync(int userId, CancellationToken cancellationToken = default);
    Task AddAsync(PasswordResetTokens token, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
