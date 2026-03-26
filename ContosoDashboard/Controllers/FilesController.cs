using ContosoDashboard.Models;
using ContosoDashboard.Services;
using Microsoft.AspNetCore.Mvc;

namespace ContosoDashboard.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly DocumentService _documentService;
    private readonly IFileStorageService _storage;

    public FilesController(DocumentService documentService, IFileStorageService storage)
    {
        _documentService = documentService;
        _storage = storage;
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
        return Ok(doc);
    }

    [HttpGet("download/{id}")]
    public async Task<IActionResult> Download(int id)
    {
        var doc = await _documentService.GetAsync(id);
        if (doc == null) return NotFound();
        var stream = await _storage.GetFileAsync(doc.FilePath);
        if (stream == null) return NotFound();
        return File(stream, doc.ContentType ?? "application/octet-stream", doc.FileName);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var doc = await _documentService.GetAsync(id);
        if (doc == null) return NotFound();
        await _storage.DeleteFileAsync(doc.FilePath);
        return NoContent();
    }
}
