using System.Collections.Generic;

namespace ContosoDashboard.Services.Validation;

public static class FileValidation
{
    private static readonly HashSet<string> _allowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf",
        ".doc",
        ".docx",
        ".xls",
        ".xlsx",
        ".png",
        ".jpg",
        ".jpeg",
        ".txt"
    };

    public const long MaxFileSizeBytes = 50 * 1024 * 1024; // 50 MB

    public static bool IsExtensionAllowed(string fileName)
    {
        var ext = System.IO.Path.GetExtension(fileName);
        return !string.IsNullOrEmpty(ext) && _allowedExtensions.Contains(ext);
    }

    public static bool IsSizeAllowed(long size)
    {
        return size <= MaxFileSizeBytes;
    }
}
