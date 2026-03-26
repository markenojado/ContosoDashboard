using System;

namespace ContosoDashboard.Services.Helpers;

public static class FilePathGenerator
{
    // Example: {userId}/{projectId|personal}/{guid}{ext}
    public static string Generate(int userId, int? projectId, string originalFileName)
    {
        var ext = System.IO.Path.GetExtension(originalFileName);
        var guid = Guid.NewGuid().ToString("N");
        var folder = projectId.HasValue ? $"project_{projectId.Value}" : "personal";
        return System.IO.Path.Combine(userId.ToString(), folder, guid + ext).Replace('\\', '/');
    }
}
