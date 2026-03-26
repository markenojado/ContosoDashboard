using ContosoDashboard.Data;
using ContosoDashboard.Models;

namespace ContosoDashboard.Services.Logging;

public class DocumentAuditLogger : IDocumentAuditLogger
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<DocumentAuditLogger> _logger;

    public DocumentAuditLogger(ApplicationDbContext db, ILogger<DocumentAuditLogger> logger)
    {
        _db = db;
        _logger = logger;
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

        try
        {
            _db.DocumentAudits.Add(audit);
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
        {
            // If the audit table doesn't exist (migration not applied), log a warning and continue
            _logger.LogWarning(ex, "Failed to write document audit. This may be due to missing migration/table. Action={Action} DocumentId={DocumentId}", action, documentId);
            return;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error writing document audit for DocumentId={DocumentId}", documentId);
            return;
        }
    }
}
