using Microsoft.Data.SqlClient;

namespace Nar2026.Core;

/// <summary>
/// Paylaşılan auth motoru — CLI ve GUI bu TEK motordan çalışır (ortak kanal).
/// Login + erişim çözümü GERÇEK core SP/TVF üzerinden: core.ssp_CheckForLogin, core.UserAccessibleScopes.
/// Saf çözüm yardımcıları (filter/working-set/owned) DB'siz statiktir.
/// </summary>
public sealed class AuthEngine(string connectionString)
{
    private readonly string _connectionString = connectionString;

    // ── Login (gerçek SP: ssp_CheckForLogin → R1 header, R2 grants, R3 descendants) ──

    public async Task<AuthContext?> LoginByUserIdAsync(int userId)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        long gsm;
        await using (var gcmd = new SqlCommand(
            "SET QUOTED_IDENTIFIER ON; SELECT GSM_NO FROM core.tblUser WHERE USER_ID=@uid AND DELETED=0", conn))
        {
            gcmd.Parameters.AddWithValue("@uid", userId);
            var g = await gcmd.ExecuteScalarAsync();
            if (g is null or DBNull) return null;
            gsm = Convert.ToInt64(g);
        }
        return await LoginCoreAsync(conn, gsm);
    }

    public async Task<AuthContext?> LoginByGsmAsync(long gsm)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        return await LoginCoreAsync(conn, gsm);
    }

    private static async Task<AuthContext?> LoginCoreAsync(SqlConnection conn, long gsm)
    {
        await using var cmd = new SqlCommand(
            "SET QUOTED_IDENTIFIER ON; EXEC core.ssp_CheckForLogin @GSM_NO = @gsm", conn);
        cmd.Parameters.AddWithValue("@gsm", gsm);
        await using var reader = await cmd.ExecuteReaderAsync();

        // R1: header
        if (!await reader.ReadAsync()) return null;
        var userId = reader.GetInt32(reader.GetOrdinal("USER_ID"));
        var username = reader.GetString(reader.GetOrdinal("USERNAME"));
        var principalTypeId = reader.GetInt32(reader.GetOrdinal("PRINCIPAL_TYPE_ID"));
        var tierId = reader.IsDBNull(reader.GetOrdinal("TIER_ID")) ? 0 : reader.GetInt32(reader.GetOrdinal("TIER_ID"));
        var appId = reader.IsDBNull(reader.GetOrdinal("APP_ID")) ? 0 : reader.GetInt32(reader.GetOrdinal("APP_ID"));
        var ceiling = reader.IsDBNull(reader.GetOrdinal("CEILING_LEVEL")) ? "none" : reader.GetString(reader.GetOrdinal("CEILING_LEVEL"));
        long userKey = 0;
        var ukOrd = reader.GetOrdinal("USER_KEY");
        if (!reader.IsDBNull(ukOrd)) userKey = reader.GetInt64(ukOrd);

        // R2: grants (scope → principal)
        var grants = new Dictionary<int, ScopeGrant>();
        if (await reader.NextResultAsync())
            while (await reader.ReadAsync())
            {
                var scopeId = reader.GetInt32(0);
                grants[scopeId] = new ScopeGrant(scopeId, reader.GetInt32(1),
                    reader.IsDBNull(2) ? "none" : reader.GetString(2),
                    reader.IsDBNull(3) ? "direct" : reader.GetString(3));
            }

        // R3: descendants (parent org-scope → child property-scope)
        var descendants = new Dictionary<int, HashSet<int>>();
        if (await reader.NextResultAsync())
            while (await reader.ReadAsync())
            {
                var parent = reader.GetInt32(0);
                var child = reader.GetInt32(1);
                if (!descendants.TryGetValue(parent, out var set)) descendants[parent] = set = [];
                set.Add(child);
            }

        return new AuthContext
        {
            UserId = userId, Username = username, GsmNo = gsm, UserKey = userKey,
            PrincipalTypeId = principalTypeId, TierId = tierId, AppId = appId, CeilingLevel = ceiling,
            ScopeGrants = grants, Descendants = descendants
        };
    }

    // ── Saf çözüm yardımcıları (DB'siz) ──

    /// <summary>OWNED (veri-güdümlü, F2/Aşama 5) — core.tblScope.OWNER_USER_ID = userId. Sezgisel değil, kanonik.</summary>
    public async Task<HashSet<int>> GetOwnedScopeIdsAsync(int userId)
    {
        var owned = new HashSet<int>();
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(
            "SET QUOTED_IDENTIFIER ON; SELECT SCOPE_ID FROM core.tblScope WHERE OWNER_USER_ID = @uid AND DELETED = 0", conn);
        cmd.Parameters.AddWithValue("@uid", userId);
        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync()) owned.Add(r.GetInt32(0));
        return owned;
    }

    /// <summary>§5.2: working_set = ({active} ∪ descendants(active)) ∩ accessible.</summary>
    public static HashSet<int> ComputeWorkingSet(AuthContext ctx)
    {
        if (ctx.ActiveScopeId is not int active) return [];
        var ws = new HashSet<int> { active };
        if (ctx.Descendants.TryGetValue(active, out var kids)) ws.UnionWith(kids);
        ws.IntersectWith(ctx.AccessibleScopes);
        return ws;
    }

    /// <summary>
    /// Veri filtresi → (scopeIds, publicOnly). Reach (hangi scope'lar) artık TVF'de
    /// (UserAccessibleScopes — reach='all' tüm evreni döner). Burada yalnız §5.3 BYPASS kısa devresi:
    /// system_root guard'ı tümüyle atlar (aktif scope dahil) → tüm data. publicOnly → IS_PUBLIC=1.
    /// </summary>
    public static (string? scopeIds, bool publicOnly) ResolveDataFilter(AuthContext ctx)
    {
        var publicOnly = ctx.SystemPrinciple?.PublicOnly ?? false;
        if (ctx.HasGuardBypass)   // yalnız system_root: tam bypass
            return (null, publicOnly);
        var scopes = ctx.WorkingSet.Count > 0 ? ctx.WorkingSet : ctx.AccessibleScopes;
        if (scopes.Count == 0) return ("-2147483648", publicOnly); // eşleşmeyen sentinel
        return (string.Join(",", scopes), publicOnly);
    }

    // ── Scope katalog (id → tür+ad) — GUI ağaç/grid metinsel alanları için ──

    public async Task<Dictionary<int, (string Type, string Name)>> LoadScopeCatalogAsync()
    {
        var map = new Dictionary<int, (string, string)>();
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        await using (var cmd = new SqlCommand(
            "SET QUOTED_IDENTIFIER ON; SELECT s.SCOPE_ID, st.CODE FROM core.tblScope s LEFT JOIN core.conScopeType st ON st.SCOPE_TYPE = s.SCOPE_TYPE WHERE s.DELETED=0", conn))
        await using (var r = await cmd.ExecuteReaderAsync())
            while (await r.ReadAsync())
            {
                var id = r.GetInt32(0);
                map[id] = (r.IsDBNull(1) ? "scope" : r.GetString(1), $"scope {id}");
            }

        await using (var cmd = new SqlCommand("SET QUOTED_IDENTIFIER ON; SELECT SCOPE_ID, ORG_NAME FROM core.tblOrganization WHERE DELETED=0", conn))
        await using (var r = await cmd.ExecuteReaderAsync())
            while (await r.ReadAsync()) map[r.GetInt32(0)] = ("organization", r.GetString(1));

        await using (var cmd = new SqlCommand("SET QUOTED_IDENTIFIER ON; SELECT SCOPE_ID, PROP_NAME FROM core.tblProperty WHERE DELETED=0", conn))
        await using (var r = await cmd.ExecuteReaderAsync())
            while (await r.ReadAsync()) map[r.GetInt32(0)] = ("property", r.GetString(1));

        return map;
    }

    /// <summary>
    /// §1.2: principal yetenek kataloğunu DB'den (conPrincipalType) yükle → SystemPrincipleRegistry.
    /// C# elle 5 record tutmaz; tek doğruluk kaynağı DB. App başlangıcında bir kez çağrılır.
    /// </summary>
    public async Task LoadCatalogAsync()
    {
        var list = new List<ISystemPrinciple>();
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(
            "SET QUOTED_IDENTIFIER ON; SELECT PRINCIPAL_TYPE_ID, CODE, REACH_LEVEL, BYPASS_GUARD, CAN_WRITE, PUBLIC_ONLY FROM core.conPrincipalType WHERE DELETED = 0", conn);
        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync())
        {
            var reachStr = r.IsDBNull(2) ? "granted" : r.GetString(2);
            var reach = reachStr == "all" ? SystemReach.AllScopes
                      : reachStr == "none" ? SystemReach.None
                      : SystemReach.GrantedScopes;
            list.Add(new CatalogPrinciple(r.GetInt32(0), r.GetString(1), reach, r.GetBoolean(3), r.GetBoolean(4), r.GetBoolean(5)));
        }
        SystemPrincipleRegistry.Load(list);
    }
}
