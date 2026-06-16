using Microsoft.Extensions.Options;
using StoreManagement.Options;

namespace StoreManagement.Services;

public class FileStorageService : IFileStorageService
{
    private readonly FileUploadOptions _options;
    private readonly IWebHostEnvironment _environment;

    public FileStorageService(IOptions<FileUploadOptions> options, IWebHostEnvironment environment)
    {
        _options = options.Value;
        _environment = environment;
    }

    public async Task<string> SaveImageAsync(IFormFile file, string folder, CancellationToken cancellationToken = default)
    {
        if (file.Length == 0)
        {
            throw new InvalidOperationException("File không hợp lệ");
        }

        var rootPath = Path.IsPathRooted(_options.Directory)
            ? _options.Directory
            : Path.Combine(_environment.ContentRootPath, _options.Directory);
        var targetFolder = Path.Combine(rootPath, folder);
        Directory.CreateDirectory(targetFolder);

        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var absolutePath = Path.Combine(targetFolder, fileName);

        await using var stream = File.Create(absolutePath);
        await file.CopyToAsync(stream, cancellationToken);

        return $"/uploads/{folder}/{fileName}".Replace("\\", "/");
    }

    public Task DeleteImageAsync(string? relativeUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(relativeUrl))
        {
            return Task.CompletedTask;
        }

        var sanitized = relativeUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString());
        if (sanitized.StartsWith($"uploads{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
        {
            sanitized = sanitized.Substring("uploads".Length + 1);
        }

        var rootPath = Path.IsPathRooted(_options.Directory)
            ? _options.Directory
            : Path.Combine(_environment.ContentRootPath, _options.Directory);
        var absolutePath = Path.Combine(rootPath, sanitized);

        if (File.Exists(absolutePath))
        {
            File.Delete(absolutePath);
        }

        return Task.CompletedTask;
    }
}
