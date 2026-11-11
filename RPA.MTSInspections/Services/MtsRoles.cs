using System.Security.Claims;
using Microsoft.Extensions.Options;

namespace RPA.MTSInspections.Services;

/// <summary>
/// Entra app-role values for the four MTS roles. Values are token-safe identifiers (no spaces) defined in the
/// application registration; they replace the former SQL IDT role labels.
/// </summary>
public sealed class MtsRoleOptions
{
    public string BasicAccess { get; set; }
    public string SupportTeam { get; set; }
    public string Inspector { get; set; }
    public string Admin { get; set; }

    public string[] All => [BasicAccess, SupportTeam, Inspector, Admin];
}

/// <summary>Named authorization policies. Each maps to the capability, not to a role label.</summary>
public static class MtsPolicies
{
    /// <summary>Any MTS role: home page, plant list and plant details.</summary>
    public const string ReadAccess = "MTS Read Access";

    /// <summary>Plant administration, audit history and CSV export.</summary>
    public const string SupportOrAdmin = "MTS Support Or Admin";

    /// <summary>Criteria, option and weighting administration, bulk import, recalculating all risk scores.</summary>
    public const string AdminOnly = "MTS Admin";

    /// <summary>BLS, BCC and PCG scheme edits. Inspectors are further restricted to a subset of fields.</summary>
    public const string SchemeEdit = "MTS Scheme Edit";

    /// <summary>SCC scheme edits, which the current screens allow for every MTS role.</summary>
    public const string SccEdit = "MTS SCC Edit";
}

/// <summary>
/// Single source of role decisions for controllers and views, so the screens and the server cannot disagree.
/// </summary>
public sealed class RoleAccess(IOptions<MtsRoleOptions> options)
{
    private readonly MtsRoleOptions roles = options.Value;

    public bool IsAdmin(ClaimsPrincipal user) => user.IsInRole(roles.Admin);

    public bool IsSupport(ClaimsPrincipal user) => user.IsInRole(roles.SupportTeam);

    public bool IsInspector(ClaimsPrincipal user) => user.IsInRole(roles.Inspector);

    public bool IsSupportOrAdmin(ClaimsPrincipal user) => IsSupport(user) || IsAdmin(user);

    public bool CanEditScheme(ClaimsPrincipal user) => IsAdmin(user) || IsInspector(user);

    public bool CanEditScc(ClaimsPrincipal user) => roles.All.Any(user.IsInRole);
}
