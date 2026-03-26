using ContosoDashboard.Data;
using ContosoDashboard.Models;
using ContosoDashboard.Services.Helpers;
using ContosoDashboard.Services.Scanner;
using ContosoDashboard.Services.Validation;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading;
using System;

namespace ContosoDashboard.Services;

public class DocumentService
{
    private readonly ApplicationDbContext _db;
    private readonly IFileStorageService _storage;
    private readonly IVirusScanner _scanner;

    public DocumentService(ApplicationDbContext db, IFileStorageService storage, IVirusScanner scanner)
    {
        _db = db;
        _storage = storage;
        _scanner = scanner;
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

        _db.Documents.Add(doc);
        await _db.SaveChangesAsync(cancellationToken);

        // Perform scan (training stub will be fast)
        try
        {
            using var stream = await _storage.GetFileAsync(doc.FilePath, cancellationToken) ?? Stream.Null;
            var result = await _scanner.ScanAsync(stream, cancellationToken);
            doc.ScanStatus = result.IsClean ? ScanStatus.Clean : ScanStatus.Infected;
            doc.ScanReport = result.Report;
            _db.Documents.Update(doc);
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
        return await _db.Documents.FirstOrDefaultAsync(d => d.DocumentId == documentId, cancellationToken);
    }
}
