namespace StoreManagement.Services;

public interface IFileStorageService
{
    Task<string> SaveImageAsync(IFormFile file, string folder, CancellationToken cancellationToken = default);
    Task DeleteImageAsync(string? relativeUrl, CancellationToken cancellationToken = default);
}
