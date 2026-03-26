using System.IO;
using ContosoDashboard.Services.Scanner;

namespace ContosoDashboard.Services.Scanner;

public class StubVirusScanner : IVirusScanner
{
    public Task<VirusScanResult> ScanAsync(Stream fileStream, CancellationToken cancellationToken = default)
    {
        // Training stub: always return clean
        return Task.FromResult(new VirusScanResult(true, "Stub scanner: no threats detected"));
    }
}
