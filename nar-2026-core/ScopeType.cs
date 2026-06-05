namespace Nar2026.Core;

/// <summary>Scope type katalog — conScopeType. TINYINT; root_scope (255) sistem reserved.</summary>
public enum ScopeType : byte
{
    Individual = 0,
    Property = 1,
    Organization = 2,
    RootScope = 255
}
