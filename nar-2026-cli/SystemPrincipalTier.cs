namespace NetlojiAuthRefactor2026;

/// <summary>
/// System principal tier IDs — conPrincipalType.TIER_ID karşılıkları.
/// Negative TIER_ID'ler sistem principal'larıdır.
/// PRINCIPAL_TYPE_ID = TIER_ID × 65536 + APP_ID (computed).
/// </summary>
public enum SystemPrincipalTier : short
{
    /// <summary>Sınır tanımayan tam yetki — DB: -5, PRINCIPAL_TYPE_ID: -327680</summary>
    ScopePublic = -5,

    /// <summary>Scope kök yöneticisi — DB: -4, PRINCIPAL_TYPE_ID: -262144</summary>
    ScopeRoot = -4,

    /// <summary>Genel okuma — DB: -3, PRINCIPAL_TYPE_ID: -196608</summary>
    SystemPublic = -3,

    /// <summary>Servis kullanıcısı — DB: -2, PRINCIPAL_TYPE_ID: -131072</summary>
    SystemService = -2,

    /// <summary>Sistem kök — DB: -1, PRINCIPAL_TYPE_ID: -65536. Guard bypass.</summary>
    SystemRoot = -1
}
