using System.ComponentModel.DataAnnotations;

namespace ContosoDashboard.Models;

public enum SharePermission
{
    View = 0,
    Edit = 1
}

public class DocumentShare
{
    [Key]
    public int DocumentShareId { get; set; }

    public int DocumentId { get; set; }

    public int SharedWithUserId { get; set; }

    public SharePermission Permission { get; set; } = SharePermission.View;

    public int SharedByUserId { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
