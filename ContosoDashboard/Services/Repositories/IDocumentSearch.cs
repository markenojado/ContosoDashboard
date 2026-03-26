using ContosoDashboard.Models;

namespace ContosoDashboard.Services.Repositories;

public record DocumentSearchResult(IEnumerable<Document> Documents, int TotalCount);

public interface IDocumentSearch
{
    Task<DocumentSearchResult> SearchAsync(string? query, int page, int pageSize, int? projectId, int? uploaderId, CancellationToken cancellationToken = default);
}
