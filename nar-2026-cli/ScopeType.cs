namespace NetlojiAuthRefactor2026;

/// <summary>
/// Scope type katalog — conScopeType tablosunun C# karşılığı.
/// TINYINT olarak saklanır; root_scope (255) sistem reserved'dir.
/// </summary>
public enum ScopeType : byte
{
    /// <summary>Bireysel kullanici scope'u — DB: 0, CODE: individual</summary>
    Individual = 0,

    /// <summary>Mulk/tesis scope'u — DB: 1, CODE: property</summary>
    Property = 1,

    /// <summary>Organizasyon scope'u (kok) — DB: 2, CODE: organization</summary>
    Organization = 2,

    /// <summary>Sistem kok scope — DB: 255, CODE: root_scope. Tum agacin en ustunu temsil eder.</summary>
    RootScope = 255
}
