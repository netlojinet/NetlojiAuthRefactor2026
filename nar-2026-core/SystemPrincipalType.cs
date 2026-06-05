namespace Nar2026.Core;

/// <summary>
/// System principal types — conPrincipalType.PRINCIPAL_TYPE_ID karşılıkları.
/// PRINCIPAL_TYPE_ID = TIER_ID × 65536 + APP_ID (computed). Negatif = sistem principal.
/// </summary>
public enum SystemPrincipalType : int
{
    ScopePublic = -327680,   // TIER=-5
    ScopeRoot = -262144,     // TIER=-4
    SystemPublic = -196608,  // TIER=-3
    SystemService = -131072, // TIER=-2
    SystemRoot = -65536      // TIER=-1 (guard bypass, sınırsız)
}
