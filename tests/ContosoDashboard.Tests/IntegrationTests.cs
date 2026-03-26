using System.IO;
using System.Threading.Tasks;
using ContosoDashboard.Configuration;
using ContosoDashboard.Controllers;
using ContosoDashboard.Data;
using ContosoDashboard.Models;
using ContosoDashboard.Services;
using ContosoDashboard.Services.Logging;
using ContosoDashboard.Services.Repositories;
using ContosoDashboard.Services.Scanner;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace ContosoDashboard.Tests;

public class IntegrationTests
{
    [Fact]
    public async Task Upload_Metadata_Download_Delete_Flow_Works()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "contoso_test_" + Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);

        try
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            await using var db = new ApplicationDbContext(options);

            var fileStorageOptions = Options.Create(new FileStorageOptions { BasePath = tempDir });
            var storage = new ContosoDashboard.Services.LocalFileStorageService(Microsoft.Extensions.Options.Options.Create(new FileStorageOptions { BasePath = tempDir }));

            var repository = new DocumentRepository(db);
            var scanner = new StubVirusScanner();
            var auditLogger = new DocumentAuditLogger(db);

            var documentService = new DocumentService(db, repository, storage, scanner, auditLogger);

            var authServiceMock = new Mock<Microsoft.AspNetCore.Authorization.IAuthorizationService>();
            authServiceMock
                .Setup(a => a.AuthorizeAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<object>()))
                .ReturnsAsync(Microsoft.AspNetCore.Authorization.AuthorizationResult.Success());

            var controller = new FilesController(documentService, storage, authServiceMock.Object, auditLogger);

            // Create form file
            var bytes = new byte[] { 1, 2, 3, 4 };
            var ms = new MemoryStream(bytes);
            ms.Position = 0;
            IFormFile formFile = new FormFile(ms, 0, ms.Length, "file", "test.pdf")
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/pdf"
            };

            // Upload
            var uploadResult = await controller.Upload(formFile, null) as OkObjectResult;
            Assert.NotNull(uploadResult);
            dynamic uploaded = uploadResult.Value!;
            int docId = (int)uploaded.DocumentId;

            // Metadata
            var metaResult = await controller.GetMetadata(docId) as OkObjectResult;
            Assert.NotNull(metaResult);

            // Download
            var downloadResult = await controller.Download(docId) as FileStreamResult;
            Assert.NotNull(downloadResult);

            // Delete
            var deleteResult = await controller.Delete(docId) as StatusCodeResult;
            Assert.True(deleteResult == null || deleteResult.StatusCode == 204);

            // Verify deletion
            var doc = await repository.GetByIdAsync(docId);
            Assert.Null(doc);
        }
        finally
        {
            try { Directory.Delete(tempDir, true); } catch { }
        }
    }
}
