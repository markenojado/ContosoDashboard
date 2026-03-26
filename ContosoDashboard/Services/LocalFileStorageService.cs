using ContosoDashboard.Configuration;
using Microsoft.Extensions.Options;
using System.IO;

namespace ContosoDashboard.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;

    public LocalFileStorageService(IOptions<FileStorageOptions> options)
    {
        _basePath = options.Value.BasePath ?? "AppData/uploads";
        if (!Path.IsPathRooted(_basePath))
        {
            // make relative to content root
            _basePath = Path.Combine(AppContext.BaseDirectory, _basePath);
        }

        Directory.CreateDirectory(_basePath);
    }

    private string FullPath(string relativePath)
    {
        var safe = relativePath.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return Path.Combine(_basePath, safe);
    }

    public async Task<string> SaveFileAsync(string relativePath, Stream content, CancellationToken cancellationToken = default)
    {
        var path = FullPath(relativePath);
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

        using var fs = File.Create(path);
        await content.CopyToAsync(fs, cancellationToken);
        await fs.FlushAsync(cancellationToken);
        return path;
    }

    public Task<Stream?> GetFileAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var path = FullPath(relativePath);
        if (!File.Exists(path)) return Task.FromResult<Stream?>(null);
        Stream s = File.OpenRead(path);
        return Task.FromResult<Stream?>(s);
    }

    public Task<bool> DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var path = FullPath(relativePath);
        if (File.Exists(path))
        {
            File.Delete(path);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<bool> FileExistsAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var path = FullPath(relativePath);
        return Task.FromResult(File.Exists(path));
    }
}
