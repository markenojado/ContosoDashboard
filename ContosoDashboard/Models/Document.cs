using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoDashboard.Models;

public enum ScanStatus
{
    Unknown = 0,
    Pending = 1,
    Clean = 2,
    Infected = 3
}

public class Document
{
    [Key]
    public int DocumentId { get; set; }

    [Required]
    [MaxLength(256)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(512)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(512)]
    public string FilePath { get; set; } = string.Empty;

    [MaxLength(128)]
    public string? ContentType { get; set; }

    public long Size { get; set; }

    public int UploadedByUserId { get; set; }

    public int? ProjectId { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public ScanStatus ScanStatus { get; set; } = ScanStatus.Pending;

    public string? ScanReport { get; set; }
}
