namespace Nar2026.Core;

/// <summary>Bir scope üzerindeki bağlamsal principal grant'i — core.UserAccessibleScopes satırı.</summary>
public sealed record ScopeGrant(int ScopeId, int PrincipalTypeId, string CeilingLevel, string Source);

/// <summary>
/// Auth oturum bağlamı — login (ssp_CheckForLogin) sonrası oluşur. CLI ve GUI bu TEK tipi paylaşır.
/// Effective principal/ceiling AKTİF scope'tan çözülür (cross-scope).
/// </summary>
public sealed class AuthContext
{
    public int UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public long GsmNo { get; init; }
    public long UserKey { get; init; }

    // Login/identity principal (tblUser.PRINCIPAL_TYPE_ID)
    public int PrincipalTypeId { get; init; }
    public short TierId { get; init; }
    public short AppId { get; init; }
    public string CeilingLevel { get; init; } = string.Empty;

    // Matrix: scope → bağlamsal principal grant (core.UserAccessibleScopes TVF)
    public Dictionary<int, ScopeGrant> ScopeGrants { get; init; } = [];
    public HashSet<int> AccessibleScopes => [.. ScopeGrants.Keys];

    // Yapısal alt iniş (parent org-scope → child property-scope) — §5.2
    public Dictionary<int, HashSet<int>> Descendants { get; init; } = [];

    public int? ActiveScopeId { get; set; }
    public HashSet<int> WorkingSet { get; set; } = [];

    public ScopeGrant? ActiveGrant =>
        ActiveScopeId is int s && ScopeGrants.TryGetValue(s, out var g) ? g : null;

    public int EffectivePrincipalTypeId => ActiveGrant?.PrincipalTypeId ?? PrincipalTypeId;
    public string EffectiveCeiling => ActiveGrant?.CeilingLevel ?? CeilingLevel;

    /// <summary>Effective hardcoded sistem tavanı (Kat1) — aktif scope principal'inden çözülür; domain'de null.</summary>
    public ISystemPrinciple? SystemPrinciple => SystemPrincipleRegistry.Resolve(EffectivePrincipalTypeId);

    /// <summary>Sistem kullanıcısı mı? Principal TIER'dan türer (ham USER_ID işaretinden değil).</summary>
    public bool IsSystemUser => TierId < 0;
    public bool HasGuardBypass => SystemPrinciple?.BypassGuard ?? false;
    public bool IsReadOnly => SystemPrinciple is { } sp ? !sp.CanWrite : EffectiveCeiling == "read_only";

    public override string ToString() =>
        $"[{UserId}] {Username} | login={PrincipalTypeId} tier={TierId} | scopes={ScopeGrants.Count} active={ActiveScopeId?.ToString() ?? "none"} eff={EffectivePrincipalTypeId} ceiling={EffectiveCeiling}";
}
