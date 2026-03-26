using ContosoDashboard.Models;

namespace ContosoDashboard.Services.Repositories;

public interface IDocumentRepository
{
    Task<Document> AddAsync(Document document, CancellationToken cancellationToken = default);
    Task<Document?> GetByIdAsync(int documentId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Document document, CancellationToken cancellationToken = default);
    Task<DocumentSearchResult> SearchAsync(string? query, int page, int pageSize, int? projectId, int? uploaderId, CancellationToken cancellationToken = default);
}
