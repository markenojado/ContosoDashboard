using System.IO;

namespace ContosoDashboard.Services;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(string relativePath, Stream content, CancellationToken cancellationToken = default);
    Task<Stream?> GetFileAsync(string relativePath, CancellationToken cancellationToken = default);
    Task<bool> DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default);
    Task<bool> FileExistsAsync(string relativePath, CancellationToken cancellationToken = default);
}
