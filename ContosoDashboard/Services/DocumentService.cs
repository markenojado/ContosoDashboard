using ContosoDashboard.Data;
using ContosoDashboard.Models;
using ContosoDashboard.Services.Helpers;
using ContosoDashboard.Services.Scanner;
using ContosoDashboard.Services.Validation;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading;
using System;
using ContosoDashboard.Services.Repositories;

namespace ContosoDashboard.Services;

public class DocumentService
{
    private readonly ApplicationDbContext _db;
    private readonly IDocumentRepository _repository;
    private readonly IFileStorageService _storage;
    private readonly IVirusScanner _scanner;
    private readonly Services.Logging.IDocumentAuditLogger _auditLogger;

    public DocumentService(ApplicationDbContext db, IDocumentRepository repository, IFileStorageService storage, IVirusScanner scanner, Services.Logging.IDocumentAuditLogger auditLogger)
    {
        _db = db;
        _repository = repository;
        _storage = storage;
        _scanner = scanner;
        _auditLogger = auditLogger;
    }

    public async Task<Document> UploadAsync(int uploadedByUserId, int? projectId, string originalFileName, Stream content, string? contentType, CancellationToken cancellationToken = default)
    {
        if (!FileValidation.IsExtensionAllowed(originalFileName))
            throw new InvalidOperationException("File type not allowed");

        // Save file path
        var relativePath = FilePathGenerator.Generate(uploadedByUserId, projectId, originalFileName);

        // Save stream to storage
        await _storage.SaveFileAsync(relativePath, content, cancellationToken);

        var doc = new Document
        {
            Title = Path.GetFileNameWithoutExtension(originalFileName),
            FileName = originalFileName,
            FilePath = relativePath,
            ContentType = contentType,
            Size = content.Length > 0 ? content.Length : 0,
            UploadedByUserId = uploadedByUserId,
            ProjectId = projectId,
            ScanStatus = ScanStatus.Pending,
            CreatedDate = DateTime.UtcNow
        };

        try
        {
            await _repository.AddAsync(doc, cancellationToken);
        }
        catch (Exception)
        {
            // Rollback: delete file from storage to avoid orphaned files when DB save fails
            try
            {
                await _storage.DeleteFileAsync(relativePath, cancellationToken);
            }
            catch
            {
                // best-effort cleanup; swallow to not hide original exception
            }

            throw;
        }

        // Perform scan (training stub will be fast)
        try
        {
            using var stream = await _storage.GetFileAsync(doc.FilePath, cancellationToken) ?? Stream.Null;
            var result = await _scanner.ScanAsync(stream, cancellationToken);
            doc.ScanStatus = result.IsClean ? ScanStatus.Clean : ScanStatus.Infected;
            doc.ScanReport = result.Report;
            // store scan result
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // Keep ScanStatus Pending on errors
        }

        return doc;
    }

    public async Task<Document?> GetAsync(int documentId, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByIdAsync(documentId, cancellationToken);
    }

    public async Task<DocumentSearchResult> SearchAsync(string? query, int page = 1, int pageSize = 20, int? projectId = null, int? uploaderId = null, CancellationToken cancellationToken = default)
    {
        return await _repository.SearchAsync(query, page, pageSize, projectId, uploaderId, cancellationToken);
    }

    public async Task DeleteAsync(int documentId, CancellationToken cancellationToken = default)
    {
        var doc = await _repository.GetByIdAsync(documentId, cancellationToken);
        if (doc == null) return;
        await _repository.DeleteAsync(doc, cancellationToken);
    }
}
