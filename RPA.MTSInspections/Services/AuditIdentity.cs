using System.Security.Claims;
using Microsoft.Extensions.Options;

namespace RPA.MTSInspections.Services;

/// <summary>Configures which claim supplies the audit user name.</summary>
public sealed class AuditIdentityOptions
{
    /// <summary>
    /// The required claim type on the authenticated principal. Identity.Name is deliberately not used as a fallback.
    /// </summary>
    public string AuditNameClaimType { get; set; }
}

/// <summary>The signed-in user as recorded in audit rows.</summary>
public interface IUser
{
    string Name { get; }
}

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor, IOptions<AuditIdentityOptions> options) : IUser
{
    public string Name => AuditIdentity.GetName(httpContextAccessor.HttpContext?.User, options.Value.AuditNameClaimType);
}

/// <summary>
/// Converts the configured claim into the user name stored in audit records. Authentication and role
/// authorization are handled elsewhere; this class does not validate tokens or grant access.
/// </summary>
public static class AuditIdentity
{
    /// <summary>
    /// Requires exactly one non-blank claim of the configured type and applies the legacy username formatting:
    /// lowercase, remove "earth\" and replace "m0" with "m". This preserves the existing audit format; it does not
    /// map Entra identities to legacy directory accounts.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The claim type is missing, the principal is unauthenticated, or the claim is missing, duplicated or blank.
    /// No fallback identity is recorded.
    /// </exception>
    public static string GetName(ClaimsPrincipal principal, string claimType)
    {
        if (string.IsNullOrWhiteSpace(claimType))
            throw new InvalidOperationException("An explicit audit-name claim type is required.");

        var claims = principal?.FindAll(claimType).ToArray() ?? [];

        if (principal?.Identity?.IsAuthenticated != true || claims.Length != 1 ||
            string.IsNullOrWhiteSpace(claims[0].Value))
        {
            throw new InvalidOperationException("The authenticated user must have exactly one non-blank configured audit-name claim.");
        }

        var name = claims[0].Value.ToLower().Replace("earth\\", "").Replace("m0", "m");

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("The configured audit-name claim does not produce a valid audit name.");
        }

        return name;
    }
}
