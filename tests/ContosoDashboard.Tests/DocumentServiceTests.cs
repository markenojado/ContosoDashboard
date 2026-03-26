using System;
using System.IO;
using System.Threading.Tasks;
using ContosoDashboard.Data;
using ContosoDashboard.Models;
using ContosoDashboard.Services;
using ContosoDashboard.Services.Repositories;
using ContosoDashboard.Services.Scanner;
using ContosoDashboard.Services.Logging;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace ContosoDashboard.Tests;

public class DocumentServiceTests
{
    private ApplicationDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task UploadAsync_Success_SavesDocument()
    {
        var db = CreateInMemoryDb();
        var repository = new DocumentRepository(db);
        var storageMock = new Mock<IFileStorageService>();
        storageMock.Setup(s => s.SaveFileAsync(It.IsAny<string>(), It.IsAny<Stream>(), default)).ReturnsAsync((string path, Stream s, System.Threading.CancellationToken ct) => path);
        storageMock.Setup(s => s.GetFileAsync(It.IsAny<string>(), default)).ReturnsAsync(new MemoryStream());

        var scannerMock = new Mock<IVirusScanner>();
        scannerMock.Setup(s => s.ScanAsync(It.IsAny<Stream>(), default)).ReturnsAsync(new Services.Scanner.VirusScanResult(true, "ok"));

        var auditMock = new Mock<IDocumentAuditLogger>();
        var service = new DocumentService(db, repository, storageMock.Object, scannerMock.Object, auditMock.Object);

        var content = new MemoryStream(new byte[] { 1, 2, 3 });
        var doc = await service.UploadAsync(1, null, "file.pdf", content, "application/pdf");

        Assert.NotNull(doc);
        Assert.Equal("file.pdf", doc.FileName);
    }

    [Fact]
    public async Task UploadAsync_Rejects_InvalidExtension()
    {
        var db = CreateInMemoryDb();
        var repository = new DocumentRepository(db);
        var storageMock = new Mock<IFileStorageService>();
        var scannerMock = new Mock<IVirusScanner>();
        var auditMock = new Mock<IDocumentAuditLogger>();
        var service = new DocumentService(db, repository, storageMock.Object, scannerMock.Object, auditMock.Object);

        var content = new MemoryStream(new byte[] { 1, 2, 3 });
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await service.UploadAsync(1, null, "file.exe", content, "application/octet-stream");
        });
    }
}
