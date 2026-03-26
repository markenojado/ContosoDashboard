using ContosoDashboard.Models;

namespace ContosoDashboard.Services.Logging;

public interface IDocumentAuditLogger
{
    Task LogAsync(int documentId, int performedByUserId, string action, string? details = null, CancellationToken cancellationToken = default);
}
