using System.IO;

namespace ContosoDashboard.Services.Scanner;

public record VirusScanResult(bool IsClean, string? Report = null);

public interface IVirusScanner
{
    Task<VirusScanResult> ScanAsync(Stream fileStream, CancellationToken cancellationToken = default);
}
