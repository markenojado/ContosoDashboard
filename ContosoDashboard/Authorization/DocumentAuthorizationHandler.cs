using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using ContosoDashboard.Data;
using ContosoDashboard.Models;
using Microsoft.EntityFrameworkCore;

namespace ContosoDashboard.Authorization;

public class DocumentAuthorizationHandler : AuthorizationHandler<DocumentAuthorizationRequirement, Document>
{
    private readonly ApplicationDbContext _db;

    public DocumentAuthorizationHandler(ApplicationDbContext db)
    {
        _db = db;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, DocumentAuthorizationRequirement requirement, Document resource)
    {
        if (context.User == null || resource == null)
        {
            return;
        }

        // Admin bypass
        if (context.User.IsInRole("Administrator"))
        {
            context.Succeed(requirement);
            return;
        }

        // Try to get user id from claims
        var idClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? context.User.FindFirst("sub")?.Value;

        if (!int.TryParse(idClaim, out var userId))
        {
            // training fallback: deny if no proper claim
            return;
        }

        // Owner always has edit permissions
        if (resource.UploadedByUserId == userId)
        {
            context.Succeed(requirement);
            return;
        }

        // Check DocumentShare
        var share = await _db.DocumentShares.FirstOrDefaultAsync(ds => ds.DocumentId == resource.DocumentId && ds.SharedWithUserId == userId);
        if (share != null)
        {
            // View or Edit depending on share
            if (requirement.MinimumPermission == SharePermission.View || share.Permission == SharePermission.Edit)
            {
                context.Succeed(requirement);
                return;
            }
        }

        // Check project membership if document belongs to a project
        if (resource.ProjectId.HasValue)
        {
            var member = await _db.ProjectMembers.FirstOrDefaultAsync(pm => pm.ProjectId == resource.ProjectId.Value && pm.UserId == userId);
            if (member != null)
            {
                // project members get view access; optionally edit for certain roles could be added
                if (requirement.MinimumPermission == SharePermission.View)
                {
                    context.Succeed(requirement);
                    return;
                }
            }
        }
    }
}
