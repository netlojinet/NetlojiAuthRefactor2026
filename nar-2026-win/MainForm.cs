using System.Data;
using Microsoft.Data.SqlClient;

namespace NetlojiAuthTestTool;

public partial class MainForm : Form
{
    private const string ConnectionString =
        "Server=.;Database=NetlojiAuthRefactor2026;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;";

    private UserSession? _session;

    // Tüm scope'ların tür+ad sözlüğü (org/property/individual/root) — grid + erişim yolu için, login başına bir kez yüklenir.
    private Dictionary<int, (string Type, string Name)> _scopeInfo = new();

    public MainForm()
    {
        InitializeComponent();
        Load += Form1_Load;
    }

    private async void Form1_Load(object? sender, EventArgs e)
    {
        Log("Uygulama başlatılıyor...");
        await LoadUsers();
        Log("Kullanıcılar yüklendi. Bir kullanıcı seçin.");
    }

    // ══════════════════════════════════════ Kullanıcı yükleme
    private async Task LoadUsers()
    {
        lvUsers.Items.Clear();
        await using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        const string sql = """
            SET QUOTED_IDENTIFIER ON;
            SELECT u.USER_ID, u.USERNAME, u.GSM_NO, u.PRINCIPAL_TYPE_ID,
                   pt.CODE, pt.TIER_ID, pt.APP_ID, pt.CEILING_LEVEL
            FROM core.tblUser u
            LEFT JOIN core.conPrincipalType pt ON u.PRINCIPAL_TYPE_ID = pt.PRINCIPAL_TYPE_ID
            WHERE u.DELETED = 0
            ORDER BY u.USER_ID
            """;

        await using var cmd = new SqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var uid = reader.GetInt32(0);
            var uname = reader.GetString(1);
            var gsm = reader.GetInt64(2);
            var pid = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);
            var pcode = reader.IsDBNull(4) ? "N/A" : reader.GetString(4);
            var tier = reader.GetInt16(5);

            var item = new ListViewItem(uid.ToString());
            item.SubItems.Add(uname);
            item.SubItems.Add(pcode);
            item.SubItems.Add(tier.ToString());
            item.Tag = new UserInfo(uid, uname, gsm, pid, pcode, tier);
            lvUsers.Items.Add(item);
        }
        Log($"  {lvUsers.Items.Count} kullanıcı yüklendi.");
    }

    // ══════════════════════════════════════ Login (ssp_CheckForLogin: R1 header, R2 grants, R3 descendants)
    private async void LvUsers_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (lvUsers.SelectedItems.Count == 0) return;
        var info = (UserInfo)lvUsers.SelectedItems[0].Tag!;
        Log($"Seçilen: [{info.UserId}] {info.Username} (GSM={info.GsmNo})");

        await using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(
            "SET QUOTED_IDENTIFIER ON; EXEC core.ssp_CheckForLogin @GSM_NO = @gsm", conn);
        cmd.Parameters.AddWithValue("@gsm", info.GsmNo);
        await using var reader = await cmd.ExecuteReaderAsync();

        if (!await reader.ReadAsync()) { Log("  LOGIN BAŞARISIZ!"); return; }

        var username = reader.GetString(reader.GetOrdinal("USERNAME"));
        var principalTypeId = reader.GetInt32(reader.GetOrdinal("PRINCIPAL_TYPE_ID"));
        var tierId = reader.IsDBNull(reader.GetOrdinal("TIER_ID")) ? (short)0 : reader.GetInt16(reader.GetOrdinal("TIER_ID"));
        var appId = reader.IsDBNull(reader.GetOrdinal("APP_ID")) ? (short)0 : reader.GetInt16(reader.GetOrdinal("APP_ID"));
        var ceilingLevel = reader.IsDBNull(reader.GetOrdinal("CEILING_LEVEL")) ? "none" : reader.GetString(reader.GetOrdinal("CEILING_LEVEL"));
        var userKey = reader.GetInt64(reader.GetOrdinal("USER_KEY"));
        Log($"  Login OK: principal={principalTypeId} tier={tierId} ceiling={ceilingLevel}");

        var grants = new Dictionary<int, ScopeGrant>();
        if (await reader.NextResultAsync())
            while (await reader.ReadAsync())
            {
                var scopeId = reader.GetInt32(0);
                grants[scopeId] = new ScopeGrant(scopeId, reader.GetInt32(1),
                    reader.IsDBNull(2) ? "none" : reader.GetString(2),
                    reader.IsDBNull(3) ? "direct" : reader.GetString(3));
            }

        var descendants = new Dictionary<int, HashSet<int>>();
        if (await reader.NextResultAsync())
            while (await reader.ReadAsync())
            {
                var parent = reader.GetInt32(0);
                var child = reader.GetInt32(1);
                if (!descendants.TryGetValue(parent, out var set)) descendants[parent] = set = [];
                set.Add(child);
            }

        await reader.CloseAsync();

        _session = new UserSession
        {
            UserId = info.UserId, Username = username, GsmNo = info.GsmNo,
            PrincipalTypeId = principalTypeId, TierId = tierId, AppId = appId,
            CeilingLevel = ceilingLevel, UserKey = userKey,
            ScopeGrants = grants, Descendants = descendants,
            ActiveScopeId = null, WorkingSet = []
        };

        _scopeInfo = await LoadScopeInfo();   // scope tür+ad sözlüğü (bir kez)

        UpdateSessionPanel();
        BuildHierarchyTree();
        UpdateScopesGrid();
        Log($"Oturum açıldı: [{_session.UserId}] {_session.Username}");
    }

    // ══════════════════════════════════════ Oturum paneli
    private void UpdateSessionPanel()
    {
        if (_session is null) return;
        var isSys = _session.TierId < 0;
        var sp = SystemPrincipleRegistry.Resolve(_session.PrincipalTypeId);
        lblSid.Text = $"User ID: {_session.UserId}";
        lblSuser.Text = $"Kullanıcı: {_session.Username} (GSM: {_session.GsmNo})";
        lblSprincipal.Text = $"Principal: {_session.PrincipalTypeId} ({sp?.Code ?? "domain"})";
        lblStier.Text = $"Tier: {_session.TierId} | App: {_session.AppId}";
        lblSceiling.Text = $"Ceiling: {_session.CeilingLevel}";
        lblSguard.Text = $"Guard Bypass: {(sp?.BypassGuard == true ? "EVET" : "HAYIR")}";
        lblSreadonly.Text = $"Read Only: {(sp is { CanWrite: false } ? "EVET" : "HAYIR")}";
        lblSsystem.Text = $"System User: {(isSys ? "EVET" : "HAYIR")}";
        lblSactive.Text = "Active Scope: (seçin)";
        lblSworking.Text = "Working Set: -";
    }

    // ══════════════════════════════════════ Sahiplik ağacı = SADECE owned scope'lar
    //  owned = kullanıcının DOĞRUDAN domain (tenant) grant'leri (PRINCIPAL_TYPE_ID > 0).
    //  cross-scope erişimi (<0: scope_root/scope_public) owned DEĞİL → "Erişilebilir Scope'lar" gridinde görünür.
    //  (Not: veride ayrı owner sinyali yok — org/property CREATOR_USER_ID hepsinde -1.)
    private void BuildHierarchyTree()
    {
        tvHierarchy.Nodes.Clear();
        dgvItemDetail.DataSource = null;
        if (_session is null) return;

        HashSet<int> owned = [.. _session.ScopeGrants.Values
            .Where(g => g.Source == "direct" && g.PrincipalTypeId > 0)
            .Select(g => g.ScopeId)];

        var rootNode = new TreeNode($"[{_session.Username}] sahip olunan scope'lar ({owned.Count})")
        {
            ForeColor = Color.DimGray,
            Tag = new ScopeNodeInfo(0, "owned_root", 0, "Sahiplik ağacı kökü")
        };
        tvHierarchy.Nodes.Add(rootNode);

        if (owned.Count == 0)
        {
            rootNode.Nodes.Add(new TreeNode("(sahip olunan scope yok — yetki/erişim için sağdaki 'Erişilebilir Scope'lar' gridine bakın)"));
            rootNode.Expand();
            return;
        }

        // org + property yapısal verisi (reader'lar sırayla kapanır → MARS yok)
        var orgData = new List<(int OrgId, int ScopeId, string Name)>();
        var propData = new List<(int PropId, int ScopeId, string Name, int? OwnerOrgId)>();
        using (var conn = new SqlConnection(ConnectionString))
        {
            conn.Open();
            using (var cmd = new SqlCommand("SET QUOTED_IDENTIFIER ON; SELECT ORGANIZATION_ID, SCOPE_ID, ORG_NAME FROM core.tblOrganization WHERE DELETED=0", conn))
            using (var r = cmd.ExecuteReader())
                while (r.Read()) orgData.Add((r.GetInt32(0), r.GetInt32(1), r.GetString(2)));
            using (var cmd = new SqlCommand("SET QUOTED_IDENTIFIER ON; SELECT PROPERTY_ID, SCOPE_ID, PROP_NAME, OWNER_ORGANIZATION_ID FROM core.tblProperty WHERE DELETED=0", conn))
            using (var r = cmd.ExecuteReader())
                while (r.Read()) propData.Add((r.GetInt32(0), r.GetInt32(1), r.GetString(2), r.IsDBNull(3) ? (int?)null : r.GetInt32(3)));
        }

        var orgScopeMap = orgData.ToDictionary(o => o.OrgId, o => o.ScopeId);
        var orgNodes = new Dictionary<int, TreeNode>();

        foreach (var (orgId, scopeId, name) in orgData)
        {
            if (!owned.Contains(scopeId)) continue;
            var node = new TreeNode(FormatScopeLabel("ORG", name, scopeId)) { ForeColor = Color.DarkBlue, Tag = new OrgNodeInfo(orgId, scopeId, name) };
            AnnotateGrant(node, scopeId);
            rootNode.Nodes.Add(node);
            orgNodes[scopeId] = node;
        }

        foreach (var (propId, scopeId, name, ownerOrgId) in propData)
        {
            if (!owned.Contains(scopeId)) continue;
            var node = new TreeNode(FormatScopeLabel("PROP", name, scopeId)) { ForeColor = Color.DarkGreen, Tag = new PropNodeInfo(propId, scopeId, name, ownerOrgId) };
            AnnotateGrant(node, scopeId);
            if (ownerOrgId.HasValue && orgScopeMap.TryGetValue(ownerOrgId.Value, out var parentScope) && orgNodes.TryGetValue(parentScope, out var orgNode))
                orgNode.Nodes.Add(node);
            else
                rootNode.Nodes.Add(node);
        }

        rootNode.ExpandAll();
    }

    private void AnnotateGrant(TreeNode node, int scopeId)
    {
        if (_session?.ScopeGrants.TryGetValue(scopeId, out var g) == true)
            node.Text += $"   →  {ResolvePrincipalCode(g.PrincipalTypeId)} / {g.CeilingLevel}";
    }

    private static string FormatScopeLabel(string type, string name, int scopeId) => $"[{type}] {name} (scope={scopeId})";

    private static string ResolvePrincipalCode(int principalTypeId)
    {
        var sp = SystemPrincipleRegistry.Resolve(principalTypeId);
        return sp?.Code ?? $"domain({principalTypeId})";
    }

    // ══════════════════════════════════════ Tree seçim → detay
    private void TvHierarchy_AfterSelect(object? sender, TreeViewEventArgs e)
    {
        if (e.Node?.Tag is null) return;
        var dt = new DataTable();
        dt.Columns.Add("Alan", typeof(string));
        dt.Columns.Add("Değer", typeof(string));

        switch (e.Node.Tag)
        {
            case OrgNodeInfo org:
                dt.Rows.Add("Tür", "Organization");
                dt.Rows.Add("ORGANIZATION_ID", org.OrgId.ToString());
                dt.Rows.Add("SCOPE_ID", org.ScopeId.ToString());
                dt.Rows.Add("Ad", org.Name);
                AddGrantRows(dt, org.ScopeId);
                if (_session?.Descendants.TryGetValue(org.ScopeId, out var kids) == true)
                    dt.Rows.Add("Alt property scope'lar", string.Join(", ", kids.OrderBy(x => x)));
                break;
            case PropNodeInfo prop:
                dt.Rows.Add("Tür", "Property");
                dt.Rows.Add("PROPERTY_ID", prop.PropId.ToString());
                dt.Rows.Add("SCOPE_ID", prop.ScopeId.ToString());
                dt.Rows.Add("Ad", prop.Name);
                dt.Rows.Add("OWNER_ORG_ID", prop.OwnerOrgId?.ToString() ?? "yok (standalone)");
                AddGrantRows(dt, prop.ScopeId);
                break;
            default:
                dt.Rows.Add("Düğüm", e.Node.Text);
                break;
        }
        dgvItemDetail.DataSource = dt;
    }

    private void AddGrantRows(DataTable dt, int scopeId)
    {
        if (_session?.ScopeGrants.TryGetValue(scopeId, out var g) == true)
        {
            dt.Rows.Add("Grant Principal", $"{ResolvePrincipalCode(g.PrincipalTypeId)} ({g.PrincipalTypeId})");
            dt.Rows.Add("Grant Ceiling", g.CeilingLevel);
            dt.Rows.Add("Grant Source", g.Source);
        }
    }

    // ══════════════════════════════════════ Erişilebilir scope grid (core.UserAccessibleScopes TVF, read-then-enrich)
    private void UpdateScopesGrid()
    {
        if (_session is null) { dgvScopes.DataSource = null; lblAccessPath.Text = ""; return; }

        var dt = new DataTable();
        dt.Columns.Add("Scope", typeof(int));
        dt.Columns.Add("Tür", typeof(string));
        dt.Columns.Add("Ad", typeof(string));
        dt.Columns.Add("Principal", typeof(string));
        dt.Columns.Add("Tavan (ceiling)", typeof(string));
        dt.Columns.Add("Kaynak", typeof(string));

        // 1) TVF sonuçlarını listeye al (reader kapanır) — sonra zenginleştir. Nested reader YOK → MARS hatası yok.
        var rows = new List<ScopeGrant>();
        using (var conn = new SqlConnection(ConnectionString))
        {
            conn.Open();
            using var cmd = new SqlCommand(
                "SET QUOTED_IDENTIFIER ON; SELECT SCOPE_ID, PRINCIPAL_TYPE_ID, CEILING_LEVEL, SOURCE FROM core.UserAccessibleScopes(@userId) ORDER BY SCOPE_ID", conn);
            cmd.Parameters.AddWithValue("@userId", _session.UserId);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                rows.Add(new ScopeGrant(r.GetInt32(0), r.GetInt32(1),
                    r.IsDBNull(2) ? "none" : r.GetString(2),
                    r.IsDBNull(3) ? "direct" : r.GetString(3)));
        }

        // 2) preloaded _scopeInfo ile metinsel tür+ad ekle
        foreach (var g in rows)
        {
            var (type, name) = _scopeInfo.TryGetValue(g.ScopeId, out var si) ? si : ("?", $"scope {g.ScopeId}");
            dt.Rows.Add(g.ScopeId, type, name, ResolvePrincipalCode(g.PrincipalTypeId), g.CeilingLevel, g.Source);
        }

        dgvScopes.DataSource = dt;
        lblAccessPath.Text = rows.Count == 0 ? "(erişilebilir scope yok)" : "Bir satır seçin → erişim yolu (pattern) burada görünür.";
    }

    // tüm scope'ların tür+ad sözlüğü (tek connection, sıralı reader)
    private async Task<Dictionary<int, (string Type, string Name)>> LoadScopeInfo()
    {
        var map = new Dictionary<int, (string, string)>();
        await using var conn = new SqlConnection(ConnectionString);
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

    // ══════════════════════════════════════ Grid seçim → erişim yolu (pattern)
    private void DgvScopes_SelectionChanged(object? sender, EventArgs e)
    {
        if (dgvScopes.SelectedRows.Count == 0 || _session is null) { lblAccessPath.Text = ""; return; }
        var row = dgvScopes.SelectedRows[0];
        if (row.Cells["Scope"].Value is not int scopeId) return;

        var (type, name) = _scopeInfo.TryGetValue(scopeId, out var si) ? si : ("?", $"scope {scopeId}");
        var source = row.Cells["Kaynak"].Value?.ToString() ?? "";
        var principal = row.Cells["Principal"].Value?.ToString() ?? "";

        string path;
        if (source == "direct")
        {
            path = $"DOĞRUDAN eşleşen scope = {type} '{name}' (scope={scopeId})   →   principal {principal}";
        }
        else if (source == "descendant")
        {
            var parentScope = _session.Descendants.FirstOrDefault(kv => kv.Value.Contains(scopeId)).Key;
            var (pType, pName) = _scopeInfo.TryGetValue(parentScope, out var ps) ? ps : ("organization", $"scope {parentScope}");
            path = $"YAPISAL: eşleşen scope = {pType} '{pName}' (scope={parentScope})   →   child {type} '{name}' (scope={scopeId})   →   principal org'tan miras ({principal})";
        }
        else
        {
            path = $"{source}: {type} '{name}' (scope={scopeId})   →   principal {principal}";
        }
        lblAccessPath.Text = "Erişim Yolu:   " + path;
    }

    private void Log(string message)
    {
        var ts = DateTime.Now.ToString("HH:mm:ss.fff");
        txtLog.AppendText($"[{ts}] {message}{Environment.NewLine}");
    }
}

// ══════════════════════════════════════ Modeller
public record UserInfo(int UserId, string Username, long GsmNo, int PrincipalTypeId, string PrincipalCode, short TierId);

public sealed class UserSession
{
    public int UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public long GsmNo { get; init; }
    public int PrincipalTypeId { get; init; }
    public short TierId { get; init; }
    public short AppId { get; init; }
    public string CeilingLevel { get; init; } = string.Empty;
    public long UserKey { get; init; }
    public Dictionary<int, ScopeGrant> ScopeGrants { get; init; } = [];
    public Dictionary<int, HashSet<int>> Descendants { get; init; } = [];
    public int? ActiveScopeId { get; set; }
    public HashSet<int> WorkingSet { get; set; } = [];
}

public record ScopeGrant(int ScopeId, int PrincipalTypeId, string CeilingLevel, string Source);

// ══════════════════════════════════════ ISystemPrinciple (MVP kopyası)
public enum SystemReach { GrantedScopes = 0, AllScopes = 1 }

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
    public int PrincipalTypeId => -65536;
    public string Code => "system_root";
    public SystemReach Reach => SystemReach.AllScopes;
    public bool BypassGuard => true;
    public bool CanWrite => true;
    public bool PublicOnly => false;
}

public sealed record SystemServicePrinciple : ISystemPrinciple
{
    public int PrincipalTypeId => -131072;
    public string Code => "system_service";
    public SystemReach Reach => SystemReach.AllScopes;
    public bool BypassGuard => false;
    public bool CanWrite => true;
    public bool PublicOnly => false;
}

public sealed record SystemPublicPrinciple : ISystemPrinciple
{
    public int PrincipalTypeId => -196608;
    public string Code => "system_public";
    public SystemReach Reach => SystemReach.AllScopes;
    public bool BypassGuard => false;
    public bool CanWrite => false;
    public bool PublicOnly => true;
}

public sealed record ScopeRootPrinciple : ISystemPrinciple
{
    public int PrincipalTypeId => -262144;
    public string Code => "scope_root";
    public SystemReach Reach => SystemReach.GrantedScopes;
    public bool BypassGuard => false;
    public bool CanWrite => true;
    public bool PublicOnly => false;
}

public sealed record ScopePublicPrinciple : ISystemPrinciple
{
    public int PrincipalTypeId => -327680;
    public string Code => "scope_public";
    public SystemReach Reach => SystemReach.GrantedScopes;
    public bool BypassGuard => false;
    public bool CanWrite => false;
    public bool PublicOnly => true;
}

public static class SystemPrincipleRegistry
{
    private static readonly Dictionary<int, ISystemPrinciple> Map = new()
    {
        [-65536] = new SystemRootPrinciple(),
        [-131072] = new SystemServicePrinciple(),
        [-196608] = new SystemPublicPrinciple(),
        [-262144] = new ScopeRootPrinciple(),
        [-327680] = new ScopePublicPrinciple(),
    };

    public static ISystemPrinciple? Resolve(int principalTypeId) =>
        Map.TryGetValue(principalTypeId, out var p) ? p : null;
}

// ══════════════════════════════════════ Tree node modelleri
public record ScopeNodeInfo(int ScopeId, string ScopeTypeCode, int ScopeTypeId, string Description);
public record OrgNodeInfo(int OrgId, int ScopeId, string Name);
public record PropNodeInfo(int PropId, int ScopeId, string Name, int? OwnerOrgId);
