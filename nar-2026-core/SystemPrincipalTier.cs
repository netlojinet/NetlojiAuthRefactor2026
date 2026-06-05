namespace Nar2026.Core;

/// <summary>System principal tier IDs — conPrincipalType.TIER_ID. Negatif = sistem.</summary>
public enum SystemPrincipalTier : short
{
    ScopePublic = -5,
    ScopeRoot = -4,
    SystemPublic = -3,
    SystemService = -2,
    SystemRoot = -1
}
