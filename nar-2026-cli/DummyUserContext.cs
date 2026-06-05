namespace NetlojiAuthRefactor2026;

/// <summary>
/// Bir scope üzerindeki bağlamsal principal grant'i — core.UserAccessibleScopes (matrix) satırı.
/// Aynı kimlik, scope'a göre farklı principal/ceiling taşıyabilir.
/// </summary>
public sealed record ScopeGrant(int ScopeId, int PrincipalTypeId, string CeilingLevel, string Source);

/// <summary>
/// Mock kullanıcı oturum bağlamı — gerçek UserContext yapısını simüle eder.
/// Login akışından (core.ssp_CheckForLogin) sonra oluşturulur; tüm yetki kontrollerinde kullanılır.
/// </summary>
public sealed class DummyUserContext
{
    /// <summary>Authenticating user ID — tblUser.USER_ID (negatif = system, pozitif = real).</summary>
    public int UserId { get; init; }

    /// <summary>Kullanıcı adı — tblUser.USERNAME.</summary>
    public string Username { get; init; } = string.Empty;

    // ── Login/identity principal (tblUser.PRINCIPAL_TYPE_ID) — kimliğin "ev" principal'i ──

    /// <summary>Login principal type ID — tblUser.PRINCIPAL_TYPE_ID.</summary>
    public int PrincipalTypeId { get; init; }

    /// <summary>Login tier — conPrincipalType.TIER_ID (negatif = system).</summary>
    public short TierId { get; init; }

    /// <summary>App scope — conPrincipalType.APP_ID.</summary>
    public short AppId { get; init; }

    /// <summary>Login tavan seviyesi — conPrincipalType.CEILING_LEVEL.</summary>
    public string CeilingLevel { get; init; } = string.Empty;

    // ── Matrix: scope → bağlamsal principal grant (core.UserAccessibleScopes TVF) ──

    /// <summary>Erişilebilir scope → o scope'taki bağlamsal principal grant'i.</summary>
    public Dictionary<int, ScopeGrant> ScopeGrants { get; init; } = [];

    /// <summary>Erişilebilir scope ID'leri (grant anahtarları).</summary>
    public HashSet<int> AccessibleScopes => [.. ScopeGrants.Keys];

    /// <summary>Aktif scope (context switch) — working set kökü.</summary>
    public int? ActiveScopeId { get; set; }

    /// <summary>Çalışma kümesi — active scope (+ descendants) ∩ accessible.</summary>
    public HashSet<int> WorkingSet { get; set; } = [];

    /// <summary>Scope → yapısal alt scope'lar (org-scope → property scope'ları). §5.2 working set için.</summary>
    public Dictionary<int, HashSet<int>> Descendants { get; init; } = [];

    // ── Effective (aktif scope'a göre) çözüm ──

    /// <summary>Aktif scope'taki grant; yoksa null.</summary>
    public ScopeGrant? ActiveGrant =>
        ActiveScopeId is int s && ScopeGrants.TryGetValue(s, out var g) ? g : null;

    /// <summary>Effective principal — aktif scope'un principal'i; aktif scope yoksa login principal'a düşer.</summary>
    public int EffectivePrincipalTypeId => ActiveGrant?.PrincipalTypeId ?? PrincipalTypeId;

    /// <summary>Effective ceiling — aktif scope'un ceiling'i; yoksa login ceiling.</summary>
    public string EffectiveCeiling => ActiveGrant?.CeilingLevel ?? CeilingLevel;

    /// <summary>
    /// EFFECTIVE hardcoded tavan (Kat1) — AKTİF scope'un principal'inden çözülür (cross-scope).
    /// Aynı kimlik, active scope değişince tavan değişir. Domain principal'da null.
    /// </summary>
    public ISystemPrinciple? SystemPrinciple => SystemPrincipleRegistry.Resolve(EffectivePrincipalTypeId);

    /// <summary>Sistem kullanıcısı mı? Ham USER_ID işaretinden DEĞİL, principal TIER'dan türer (#2 fix).</summary>
    public bool IsSystemUser => TierId < 0;

    /// <summary>Guard bypass — yalnız system_root (hardcoded interface'ten).</summary>
    public bool HasGuardBypass => SystemPrinciple?.BypassGuard ?? false;

    /// <summary>Read-only tavan — sistem principal'da CanWrite=false; domain'de effective ceiling.</summary>
    public bool IsReadOnly => SystemPrinciple is { } sp ? !sp.CanWrite : EffectiveCeiling == "read_only";

    public override string ToString()
    {
        var active = ActiveScopeId?.ToString() ?? "none";
        return $"[{UserId}] {Username} | login_principal={PrincipalTypeId} tier={TierId} | scopes={ScopeGrants.Count} active={active} eff={EffectivePrincipalTypeId} ceiling={EffectiveCeiling}";
    }
}
