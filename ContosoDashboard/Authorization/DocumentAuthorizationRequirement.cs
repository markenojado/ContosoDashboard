using Microsoft.AspNetCore.Authorization;
using ContosoDashboard.Models;

namespace ContosoDashboard.Authorization;

public class DocumentAuthorizationRequirement : IAuthorizationRequirement
{
    public SharePermission MinimumPermission { get; }

    public DocumentAuthorizationRequirement(SharePermission minimumPermission)
    {
        MinimumPermission = minimumPermission;
    }
}
