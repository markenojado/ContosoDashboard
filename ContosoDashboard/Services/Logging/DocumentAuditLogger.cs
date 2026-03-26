using ContosoDashboard.Data;
using ContosoDashboard.Models;

namespace ContosoDashboard.Services.Logging;

public class DocumentAuditLogger : IDocumentAuditLogger
{
    private readonly ApplicationDbContext _db;

    public DocumentAuditLogger(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task LogAsync(int documentId, int performedByUserId, string action, string? details = null, CancellationToken cancellationToken = default)
    {
        var audit = new DocumentAudit
        {
            DocumentId = documentId,
            PerformedByUserId = performedByUserId,
            Action = action,
            Details = details,
            PerformedAt = DateTime.UtcNow
        };

        _db.DocumentAudits.Add(audit);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
