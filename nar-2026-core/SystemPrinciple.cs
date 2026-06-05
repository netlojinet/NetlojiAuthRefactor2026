namespace Nar2026.Core;

/// <summary>Sistem principal'inin erişim genişliği (Kat1 reach — §5.3).</summary>
public enum SystemReach
{
    None = -1,          // denied
    GrantedScopes = 0,  // scope_root / scope_public / domain
    AllScopes = 1       // system_root / system_service / system_public
}

/// <summary>
/// Principal Kat1 yeteneği (Anayasa §2). conPrincipalType kataloğu ile hizalı.
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

/// <summary>
/// Katalogdan YÜKLENEN principal yeteneği — conPrincipalType satırı.
/// Elle yazılmaz; tek doğruluk kaynağı DB kataloğu (§1.2 invariant 4).
/// </summary>
public sealed record CatalogPrinciple(
    int PrincipalTypeId, string Code, SystemReach Reach,
    bool BypassGuard, bool CanWrite, bool PublicOnly) : ISystemPrinciple;

/// <summary>
/// Principal kataloğu — conPrincipalType'tan RUNTIME yüklenir (AuthEngine.LoadCatalogAsync).
/// C# artık 5 record'u ELLE tutmuyor → drift biter (§1.2). Yüklenmeden Resolve null döner (fail-closed).
/// </summary>
public static class SystemPrincipleRegistry
{
    private static IReadOnlyDictionary<int, ISystemPrinciple> _map = new Dictionary<int, ISystemPrinciple>();

    /// <summary>Kataloğu yükle (AuthEngine çağırır).</summary>
    public static void Load(IEnumerable<ISystemPrinciple> principals)
        => _map = principals.ToDictionary(p => p.PrincipalTypeId);

    public static bool IsLoaded => _map.Count > 0;

    /// <summary>Principal'i çöz; katalogda yoksa null → denied (fail-closed).</summary>
    public static ISystemPrinciple? Resolve(int principalTypeId) => _map.GetValueOrDefault(principalTypeId);

    public static IReadOnlyCollection<ISystemPrinciple> All => _map.Values.ToList();
}
