using System.ComponentModel.DataAnnotations;

namespace ContosoDashboard.Models;

public class DocumentAudit
{
    [Key]
    public int DocumentAuditId { get; set; }

    public int DocumentId { get; set; }

    public int PerformedByUserId { get; set; }

    public string Action { get; set; } = string.Empty; // upload, download, delete, share

    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;

    public string? Details { get; set; }
}
