using ContosoDashboard.Models;
using ContosoDashboard.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ContosoDashboard.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly DocumentService _documentService;
    private readonly IFileStorageService _storage;
    private readonly IAuthorizationService _authorizationService;
    private readonly Services.Logging.IDocumentAuditLogger _auditLogger;

    public FilesController(DocumentService documentService, IFileStorageService storage, IAuthorizationService authorizationService, Services.Logging.IDocumentAuditLogger auditLogger)
    {
        _documentService = documentService;
        _storage = storage;
        _authorizationService = authorizationService;
        _auditLogger = auditLogger;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromForm] int? projectId)
    {
        if (file == null) return BadRequest("No file provided");

        if (file.Length == 0) return BadRequest("Empty file");

        try
        {
            using var stream = file.OpenReadStream();
            // For training, use a fixed user id (replace with real user claims in production)
            var userId = 1;
            var doc = await _documentService.UploadAsync(userId, projectId, file.FileName, stream, file.ContentType);
            // audit upload
            await _auditLogger.LogAsync(doc.DocumentId, userId, "upload", null);
            return Ok(new { doc.DocumentId, doc.Title, doc.FileName, doc.FilePath, doc.ScanStatus });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMetadata(int id)
    {
        var doc = await _documentService.GetAsync(id);
        if (doc == null) return NotFound();

        var auth = await _authorizationService.AuthorizeAsync(User, doc, new ContosoDashboard.Authorization.DocumentAuthorizationRequirement(SharePermission.View));
        if (!auth.Succeeded) return Forbid();

        return Ok(doc);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? q, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] int? projectId = null, [FromQuery] int? uploaderId = null)
    {
        var result = await _documentService.SearchAsync(q, page, pageSize, projectId, uploaderId);
        return Ok(new { items = result.Documents, total = result.TotalCount });
    }

    [HttpGet("download/{id}")]
    public async Task<IActionResult> Download(int id)
    {
        var doc = await _documentService.GetAsync(id);
        if (doc == null) return NotFound();

        var auth = await _authorizationService.AuthorizeAsync(User, doc, new ContosoDashboard.Authorization.DocumentAuthorizationRequirement(SharePermission.View));
        if (!auth.Succeeded) return Forbid();

        var stream = await _storage.GetFileAsync(doc.FilePath);
        if (stream == null) return NotFound();
        // audit download
        var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        int.TryParse(idClaim, out var userId);
        await _auditLogger.LogAsync(doc.DocumentId, userId == 0 ? 1 : userId, "download", null);
        return File(stream, doc.ContentType ?? "application/octet-stream", doc.FileName);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var doc = await _documentService.GetAsync(id);
        if (doc == null) return NotFound();

        var auth = await _authorizationService.AuthorizeAsync(User, doc, new ContosoDashboard.Authorization.DocumentAuthorizationRequirement(SharePermission.Edit));
        if (!auth.Succeeded) return Forbid();

        var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        int.TryParse(idClaim, out var userId);

        await _storage.DeleteFileAsync(doc.FilePath);
        await _auditLogger.LogAsync(doc.DocumentId, userId == 0 ? 1 : userId, "delete", null);
        await _documentService.DeleteAsync(doc.DocumentId);
        return NoContent();
    }
}
