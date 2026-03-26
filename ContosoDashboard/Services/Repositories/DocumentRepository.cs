using ContosoDashboard.Data;
using ContosoDashboard.Models;
using Microsoft.EntityFrameworkCore;

namespace ContosoDashboard.Services.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly ApplicationDbContext _db;

    public DocumentRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Document> AddAsync(Document document, CancellationToken cancellationToken = default)
    {
        _db.Documents.Add(document);
        await _db.SaveChangesAsync(cancellationToken);
        return document;
    }

    public async Task<Document?> GetByIdAsync(int documentId, CancellationToken cancellationToken = default)
    {
        return await _db.Documents.FirstOrDefaultAsync(d => d.DocumentId == documentId, cancellationToken);
    }

    public async Task DeleteAsync(Document document, CancellationToken cancellationToken = default)
    {
        _db.Documents.Remove(document);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<DocumentSearchResult> SearchAsync(string? query, int page, int pageSize, int? projectId, int? uploaderId, CancellationToken cancellationToken = default)
    {
        var q = _db.Documents.AsQueryable();
        if (!string.IsNullOrWhiteSpace(query))
        {
            q = q.Where(d => d.Title.Contains(query) || d.FileName.Contains(query));
        }

        if (projectId.HasValue)
            q = q.Where(d => d.ProjectId == projectId.Value);

        if (uploaderId.HasValue)
            q = q.Where(d => d.UploadedByUserId == uploaderId.Value);

        var total = await q.CountAsync(cancellationToken);
        var items = await q.OrderByDescending(d => d.CreatedDate).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new DocumentSearchResult(items, total);
    }
}
