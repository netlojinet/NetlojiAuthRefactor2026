namespace NetlojiAuthRefactor2026;

/// <summary>Sistem principal'inin erişim genişliği (Kat1 reach — §5.3).</summary>
public enum SystemReach
{
    /// <summary>Yalnız grant'li scope(lar) — scope_root / scope_public.</summary>
    GrantedScopes = 0,

    /// <summary>Tüm scope'lar — system_root / system_service / system_public.</summary>
    AllScopes = 1
}

/// <summary>
/// ≤0 sistem principal'ları için HARDCODED Kat1 tavanı.
/// Veriyle/rolle DEĞİŞTİRİLEMEZ; kapalı küme; conPrincipalType kataloğu ile hizalı.
/// Anayasa §2 (Kat1 tavan) + §5.3 (reach kısa devresi).
/// </summary>
public interface ISystemPrinciple
{
    int PrincipalTypeId { get; }
    string Code { get; }
    SystemReach Reach { get; }
    bool BypassGuard { get; }   // yalnız system_root: tüm guard'ları atlar
    bool CanWrite { get; }      // public tier'lar yazamaz
    bool PublicOnly { get; }    // public tier'lar yalnız IS_PUBLIC=1 okur
}

/// <summary>system_root (-65536): sınırsız, guard bypass.</summary>
public sealed record SystemRootPrinciple : ISystemPrinciple
{
    public int PrincipalTypeId => (int)SystemPrincipalType.SystemRoot;
    public string Code => "system_root";
    public SystemReach Reach => SystemReach.AllScopes;
    public bool BypassGuard => true;
    public bool CanWrite => true;
    public bool PublicOnly => false;
}

/// <summary>system_service (-131072): tüm scope'lar, guard'a tabi (job-darlık Faz 2 / APP_ID).</summary>
public sealed record SystemServicePrinciple : ISystemPrinciple
{
    public int PrincipalTypeId => (int)SystemPrincipalType.SystemService;
    public string Code => "system_service";
    public SystemReach Reach => SystemReach.AllScopes;
    public bool BypassGuard => false;
    public bool CanWrite => true;
    public bool PublicOnly => false;
}

/// <summary>system_public (-196608): tüm scope'lar ama read-only + IS_PUBLIC=1.</summary>
public sealed record SystemPublicPrinciple : ISystemPrinciple
{
    public int PrincipalTypeId => (int)SystemPrincipalType.SystemPublic;
    public string Code => "system_public";
    public SystemReach Reach => SystemReach.AllScopes;
    public bool BypassGuard => false;
    public bool CanWrite => false;
    public bool PublicOnly => true;
}

/// <summary>scope_root (-262144): yalnız grant'li scope içinde tam yetki.</summary>
public sealed record ScopeRootPrinciple : ISystemPrinciple
{
    public int PrincipalTypeId => (int)SystemPrincipalType.ScopeRoot;
    public string Code => "scope_root";
    public SystemReach Reach => SystemReach.GrantedScopes;
    public bool BypassGuard => false;
    public bool CanWrite => true;
    public bool PublicOnly => false;
}

/// <summary>scope_public (-327680): yalnız grant'li scope'un public vitrini (read-only + IS_PUBLIC).</summary>
public sealed record ScopePublicPrinciple : ISystemPrinciple
{
    public int PrincipalTypeId => (int)SystemPrincipalType.ScopePublic;
    public string Code => "scope_public";
    public SystemReach Reach => SystemReach.GrantedScopes;
    public bool BypassGuard => false;
    public bool CanWrite => false;
    public bool PublicOnly => true;
}

/// <summary>
/// Hardcoded sistem principal kataloğu — PRINCIPAL_TYPE_ID → ISystemPrinciple.
/// Kapalı küme; gerçekte DAL enum'undan üretilir (Anayasa §1.2). MVP'de elle sabit.
/// </summary>
public static class SystemPrincipleRegistry
{
    private static readonly Dictionary<int, ISystemPrinciple> Map = new()
    {
        [(int)SystemPrincipalType.SystemRoot]    = new SystemRootPrinciple(),
        [(int)SystemPrincipalType.SystemService] = new SystemServicePrinciple(),
        [(int)SystemPrincipalType.SystemPublic]  = new SystemPublicPrinciple(),
        [(int)SystemPrincipalType.ScopeRoot]     = new ScopeRootPrinciple(),
        [(int)SystemPrincipalType.ScopePublic]   = new ScopePublicPrinciple(),
        // NOT: system_service_trash (-131071, APP_ID=1) = service varyantı; job-darlık Faz 2.
    };

    /// <summary>Sistem principal'i çöz; sistem değilse (≥0 / bilinmeyen) null → domain yolu.</summary>
    public static ISystemPrinciple? Resolve(int principalTypeId) =>
        Map.TryGetValue(principalTypeId, out var p) ? p : null;

    /// <summary>Tüm hardcoded sistem principal'ları.</summary>
    public static IReadOnlyCollection<ISystemPrinciple> All => Map.Values;
}
