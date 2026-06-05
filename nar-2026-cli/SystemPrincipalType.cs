namespace NetlojiAuthRefactor2026;

/// <summary>
/// System principal types — conPrincipalType.PRINCIPAL_TYPE_ID karşılıkları.
/// PRINCIPAL_TYPE_ID = TIER_ID × 65536 + APP_ID (computed).
/// Negative PRINCIPAL_TYPE_ID'ler sistem principal'larıdır.
/// </summary>
public enum SystemPrincipalType : int
{
    /// <summary>Bireysel okuma — TIER=-5, APP=0 — DB: -327680</summary>
    ScopePublic = -327680,

    /// <summary>Scope kök yöneticisi — TIER=-4, APP=0 — DB: -262144</summary>
    ScopeRoot = -262144,

    /// <summary>Genel okuma — TIER=-3, APP=0 — DB: -196608</summary>
    SystemPublic = -196608,

    /// <summary>Servis kullanıcısı — TIER=-2, APP=0 — DB: -131072</summary>
    SystemService = -131072,

    /// <summary>Sistem kök — TIER=-1, APP=0 — DB: -65536. Guard bypass, sınırsız yetki.</summary>
    SystemRoot = -65536
}
