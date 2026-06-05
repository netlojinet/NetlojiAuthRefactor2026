namespace Nar2026.Core;

/// <summary>Sistem principal'inin erişim genişliği (Kat1 reach — §5.3).</summary>
public enum SystemReach
{
    GrantedScopes = 0,  // scope_root / scope_public
    AllScopes = 1       // system_root / system_service / system_public
}

/// <summary>
/// ≤0 sistem principal'ları için HARDCODED Kat1 tavanı. Veriyle/rolle DEĞİŞTİRİLEMEZ.
/// conPrincipalType kataloğu ile hizalı (Anayasa §2 Kat1 + §5.3 reach).
/// </summary>
public interface ISystemPrinciple
{
    int PrincipalTypeId { get; }
    string Code { get; }
    SystemReach Reach { get; }
    bool BypassGuard { get; }
    bool CanWrite { get; }
    bool PublicOnly { get; }
}

public sealed record SystemRootPrinciple : ISystemPrinciple
{
    public int PrincipalTypeId => (int)SystemPrincipalType.SystemRoot;
    public string Code => "system_root";
    public SystemReach Reach => SystemReach.AllScopes;
    public bool BypassGuard => true;
    public bool CanWrite => true;
    public bool PublicOnly => false;
}

public sealed record SystemServicePrinciple : ISystemPrinciple
{
    public int PrincipalTypeId => (int)SystemPrincipalType.SystemService;
    public string Code => "system_service";
    public SystemReach Reach => SystemReach.AllScopes;
    public bool BypassGuard => false;
    public bool CanWrite => true;
    public bool PublicOnly => false;
}

public sealed record SystemPublicPrinciple : ISystemPrinciple
{
    public int PrincipalTypeId => (int)SystemPrincipalType.SystemPublic;
    public string Code => "system_public";
    public SystemReach Reach => SystemReach.AllScopes;
    public bool BypassGuard => false;
    public bool CanWrite => false;
    public bool PublicOnly => true;
}

public sealed record ScopeRootPrinciple : ISystemPrinciple
{
    public int PrincipalTypeId => (int)SystemPrincipalType.ScopeRoot;
    public string Code => "scope_root";
    public SystemReach Reach => SystemReach.GrantedScopes;
    public bool BypassGuard => false;
    public bool CanWrite => true;
    public bool PublicOnly => false;
}

public sealed record ScopePublicPrinciple : ISystemPrinciple
{
    public int PrincipalTypeId => (int)SystemPrincipalType.ScopePublic;
    public string Code => "scope_public";
    public SystemReach Reach => SystemReach.GrantedScopes;
    public bool BypassGuard => false;
    public bool CanWrite => false;
    public bool PublicOnly => true;
}

/// <summary>Hardcoded sistem principal kataloğu — PRINCIPAL_TYPE_ID → ISystemPrinciple (kapalı küme).</summary>
public static class SystemPrincipleRegistry
{
    private static readonly Dictionary<int, ISystemPrinciple> Map = new()
    {
        [(int)SystemPrincipalType.SystemRoot]    = new SystemRootPrinciple(),
        [(int)SystemPrincipalType.SystemService] = new SystemServicePrinciple(),
        [(int)SystemPrincipalType.SystemPublic]  = new SystemPublicPrinciple(),
        [(int)SystemPrincipalType.ScopeRoot]     = new ScopeRootPrinciple(),
        [(int)SystemPrincipalType.ScopePublic]   = new ScopePublicPrinciple(),
    };

    /// <summary>Sistem principal'i çöz; sistem değilse (≥0 / bilinmeyen) null → domain yolu.</summary>
    public static ISystemPrinciple? Resolve(int principalTypeId) =>
        Map.TryGetValue(principalTypeId, out var p) ? p : null;

    public static IReadOnlyCollection<ISystemPrinciple> All => Map.Values;
}
