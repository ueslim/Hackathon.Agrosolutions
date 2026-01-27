using System.Security.Claims;

namespace FIAP.AgroSolutions.Farm.Api.Security;

public static class UserContext
{
    public static Guid GetUserId(HttpContext httpContext)
    {
        // bypass dev (útil até o Identity ficar pronto)
        if (httpContext.Request.Headers.TryGetValue("x-dev-user-id", out var dev) &&
            Guid.TryParse(dev.ToString(), out var devGuid))
        {
            return devGuid;
        }

        var claim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? httpContext.User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(claim) || !Guid.TryParse(claim, out var userId))
        {
            throw new UnauthorizedAccessException("Missing/invalid userId claim.");
        }

        return userId;
    }
}
