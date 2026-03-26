using System.Threading.Tasks;
using ContosoDashboard.Controllers;
using ContosoDashboard.Data;
using ContosoDashboard.Models;
using ContosoDashboard.Services.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Authorization;
using ContosoDashboard.Services.Logging;
using ContosoDashboard.Services;
using ContosoDashboard.Services.Scanner;
using System.IO;

namespace ContosoDashboard.Tests;

public class FilesControllerSearchTests
{
    private ApplicationDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Search_ReturnsResults()
    {
        var db = CreateInMemoryDb();
        db.Documents.Add(new Document { DocumentId = 1, Title = "Report", FileName = "report.pdf", CreatedDate = System.DateTime.UtcNow, UploadedByUserId = 1 });
        db.Documents.Add(new Document { DocumentId = 2, Title = "Plan", FileName = "plan.docx", CreatedDate = System.DateTime.UtcNow, UploadedByUserId = 2 });
        await db.SaveChangesAsync();

        var repository = new DocumentRepository(db);
        var storageMock = new Mock<ContosoDashboard.Services.IFileStorageService>();
        var scannerMock = new Mock<IVirusScanner>();
        var auditMock = new Mock<IDocumentAuditLogger>();
        var documentService = new DocumentService(db, repository, storageMock.Object, scannerMock.Object, auditMock.Object);

        var authMock = new Mock<IAuthorizationService>();
        authMock.Setup(a => a.AuthorizeAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<object>())).ReturnsAsync(AuthorizationResult.Success());

        var controller = new FilesController(documentService, storageMock.Object, authMock.Object, auditMock.Object);

        var result = await controller.Search(null, 1, 10, null, null);
        Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);
    }
}
