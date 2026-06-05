using System.Data;
using Microsoft.Data.SqlClient;

namespace NetlojiAuthTestTool;

public partial class MainForm : Form
{
    private const string ConnectionString =
        "Server=.;Database=NetlojiAuthRefactor2026;Trusted_Connection=True;TrustServerCertificate=True;";

    private UserSession? _session;

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

    // ══════════════════════════════════════
    // Kullanıcı yükleme
    // ══════════════════════════════════════

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

    // ══════════════════════════════════════
    // Kullanıcı seçimi → oturum açma
    // ══════════════════════════════════════

    private async void LvUsers_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (lvUsers.SelectedItems.Count == 0) return;

        var info = (UserInfo)lvUsers.SelectedItems[0].Tag!;
        Log($"Seçilen: [{info.UserId}] {info.Username} (GSM={info.GsmNo})");

        await using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        // Login simülasyonu
        await using var cmd = new SqlCommand(
            "SET QUOTED_IDENTIFIER ON; EXEC core.ssp_CheckForLogin @GSM_NO = @gsm", conn);
        cmd.Parameters.AddWithValue("@gsm", info.GsmNo);

        await using var reader = await cmd.ExecuteReaderAsync();

        // Result 1: principal header
        if (!await reader.ReadAsync())
        {
            Log("  LOGIN BAŞARISIZ!");
            return;
        }

        var username = reader.GetString(reader.GetOrdinal("USERNAME"));
        var principalTypeId = reader.GetInt32(reader.GetOrdinal("PRINCIPAL_TYPE_ID"));
        var tierId = reader.IsDBNull(reader.GetOrdinal("TIER_ID")) ? (short)0 : reader.GetInt16(reader.GetOrdinal("TIER_ID"));
        var appId = reader.IsDBNull(reader.GetOrdinal("APP_ID")) ? (short)0 : reader.GetInt16(reader.GetOrdinal("APP_ID"));
        var ceilingLevel = reader.IsDBNull(reader.GetOrdinal("CEILING_LEVEL")) ? "none" : reader.GetString(reader.GetOrdinal("CEILING_LEVEL"));
        var userKey = reader.GetInt64(reader.GetOrdinal("USER_KEY"));

        Log($"  Login OK: principal={principalTypeId} tier={tierId} ceiling={ceilingLevel}");

        // Result 2: accessible scopes (matrix-based)
        var grants = new Dictionary<int, ScopeGrant>();
        if (await reader.NextResultAsync())
        {
            while (await reader.ReadAsync())
            {
                var scopeId = reader.GetInt32(0);
                var ptype = reader.GetInt32(1);
                var ceil = reader.IsDBNull(2) ? "none" : reader.GetString(2);
                var src = reader.IsDBNull(3) ? "direct" : reader.GetString(3);
                grants[scopeId] = new ScopeGrant(scopeId, ptype, ceil, src);
                Log($"  Grant: scope={scopeId} principal={ptype} ceiling={ceil} ({src})");
            }
        }

        // Result 3: descendants (org→property)
        var descendants = new Dictionary<int, HashSet<int>>();
        if (await reader.NextResultAsync())
        {
            while (await reader.ReadAsync())
            {
                var parent = reader.GetInt32(0);
                var child = reader.GetInt32(1);
                if (!descendants.TryGetValue(parent, out var set))
                    descendants[parent] = set = [];
                set.Add(child);
                Log($"  Descendant: org_scope={parent} → prop_scope={child}");
            }
        }

        await reader.CloseAsync();
        await cmd.DisposeAsync();

        // Session oluştur
        _session = new UserSession
        {
            UserId = info.UserId,
            Username = username,
            GsmNo = info.GsmNo,
            PrincipalTypeId = principalTypeId,
            TierId = tierId,
            AppId = appId,
            CeilingLevel = ceilingLevel,
            UserKey = userKey,
            ScopeGrants = grants,
            Descendants = descendants,
            ActiveScopeId = null,
            WorkingSet = []
        };

        UpdateSessionPanel();
        BuildHierarchyTree();
        UpdateScopesGrid();
        Log($"Oturum açıldı: [{_session.UserId}] {_session.Username}");
    }

    // ══════════════════════════════════════
    // Oturum paneli güncelleme
    // ══════════════════════════════════════

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

    // ══════════════════════════════════════
    // Hiyerarşi ağacı oluşturma
    // ══════════════════════════════════════

    private void BuildHierarchyTree()
    {
        tvHierarchy.Nodes.Clear();
        dgvItemDetail.DataSource = null;

        if (_session is null) return;

        var rootNode = new TreeNode("Tüm Scope'lar (-1)");
        rootNode.Tag = new ScopeNodeInfo(-1, "root_scope", 255, "Sistem kök scope");

        var orgScopes = new Dictionary<int, TreeNode>();
        var propScopes = new Dictionary<int, TreeNode>();

        using var conn = new SqlConnection(ConnectionString);
        conn.Open();

        // 1) Org'ları oku
        var orgData = new List<(int OrgId, int ScopeId, string Name)>();
        using (var cmd = new SqlCommand(
            "SET QUOTED_IDENTIFIER ON; SELECT o.ORGANIZATION_ID, o.SCOPE_ID, o.ORG_NAME FROM core.tblOrganization o WHERE o.DELETED=0", conn))
        using (var r = cmd.ExecuteReader())
        {
            while (r.Read())
            {
                orgData.Add((r.GetInt32(0), r.GetInt32(1), r.GetString(2)));
            }
        }

        foreach (var (orgId, scopeId, name) in orgData)
        {
            var node = new TreeNode($"[ORG] {name} (scope={scopeId})");
            node.Tag = new OrgNodeInfo(orgId, scopeId, name);
            rootNode.Nodes.Add(node);
            orgScopes[scopeId] = node;
        }

        // 2) Property'leri oku
        var propData = new List<(int PropId, int ScopeId, string Name, int? OwnerOrgId)>();
        using (var cmd = new SqlCommand(
            "SET QUOTED_IDENTIFIER ON; SELECT p.PROPERTY_ID, p.SCOPE_ID, p.PROP_NAME, p.OWNER_ORGANIZATION_ID FROM core.tblProperty p WHERE p.DELETED=0", conn))
        using (var r = cmd.ExecuteReader())
        {
            while (r.Read())
            {
                var ownerOrgId = r.IsDBNull(3) ? (int?)null : r.GetInt32(3);
                propData.Add((r.GetInt32(0), r.GetInt32(1), r.GetString(2), ownerOrgId));
            }
        }

        // 3) Property'leri ağaca ekle
        foreach (var (propId, scopeId, name, ownerOrgId) in propData)
        {
            var node = new TreeNode($"[PROP] {name} (scope={scopeId})");
            node.Tag = new PropNodeInfo(propId, scopeId, name, ownerOrgId);

            if (ownerOrgId.HasValue)
            {
                // Org'un scope'unu ayrı sorgu ile bul
                using var orgCmd = new SqlCommand(
                    "SET QUOTED_IDENTIFIER ON; SELECT SCOPE_ID FROM core.tblOrganization WHERE ORGANIZATION_ID=@oid AND DELETED=0", conn);
                orgCmd.Parameters.AddWithValue("@oid", ownerOrgId.Value);
                var result = orgCmd.ExecuteScalar();
                if (result is int os && orgScopes.TryGetValue(os, out var orgNode))
                    orgNode.Nodes.Add(node);
                else
                    rootNode.Nodes.Add(node);
            }
            else
            {
                rootNode.Nodes.Add(node);
            }

            propScopes[scopeId] = node;
        }

        // 4) Grant'leri ağaca işaretle
        foreach (var grant in _session.ScopeGrants.Values)
        {
            var marker = $"  [{grant.PrincipalTypeId}] {grant.CeilingLevel}";

            if (orgScopes.TryGetValue(grant.ScopeId, out var orgNode))
            {
                orgNode.Text += marker;
                orgNode.ForeColor = Color.DarkBlue;
            }
            else if (propScopes.TryGetValue(grant.ScopeId, out var propNode))
            {
                propNode.Text += marker;
                propNode.ForeColor = Color.DarkGreen;
            }
            else if (grant.ScopeId == -1)
            {
                rootNode.Text += marker;
                rootNode.ForeColor = Color.DarkRed;
            }
        }

        tvHierarchy.Nodes.Add(rootNode);
        rootNode.Expand();
    }

    // ══════════════════════════════════════
    // Tree seçim → detay göster
    // ══════════════════════════════════════

    private void TvHierarchy_AfterSelect(object? sender, TreeViewEventArgs e)
    {
        if (e.Node?.Tag is null) return;

        var dt = new DataTable();
        dt.Columns.Add("Alan", typeof(string));
        dt.Columns.Add("Değer", typeof(string));

        switch (e.Node.Tag)
        {
            case ScopeNodeInfo scope:
                dt.Rows.Add("SCOPE_ID", scope.ScopeId);
                dt.Rows.Add("SCOPE_TYPE", $"{scope.ScopeTypeCode} ({scope.ScopeTypeId})");
                dt.Rows.Add("Açıklama", scope.Description);
                if (_session?.ScopeGrants.TryGetValue(scope.ScopeId, out var g1) == true)
                {
                    dt.Rows.Add("Grant Principal", $"{g1.PrincipalTypeId}");
                    dt.Rows.Add("Grant Ceiling", g1.CeilingLevel);
                    dt.Rows.Add("Grant Source", g1.Source);
                }
                break;

            case OrgNodeInfo org:
                dt.Rows.Add("ORGANIZATION_ID", org.OrgId);
                dt.Rows.Add("SCOPE_ID", org.ScopeId);
                dt.Rows.Add("ORG_NAME", org.Name);
                if (_session?.ScopeGrants.TryGetValue(org.ScopeId, out var g2) == true)
                {
                    dt.Rows.Add("Grant Principal", $"{g2.PrincipalTypeId}");
                    dt.Rows.Add("Grant Ceiling", g2.CeilingLevel);
                    dt.Rows.Add("Grant Source", g2.Source);
                }
                // Alt property'leri göster
                if (_session?.Descendants.TryGetValue(org.ScopeId, out var kids) == true)
                    dt.Rows.Add("Alt Scope'lar", string.Join(", ", kids));
                break;

            case PropNodeInfo prop:
                dt.Rows.Add("PROPERTY_ID", prop.PropId);
                dt.Rows.Add("SCOPE_ID", prop.ScopeId);
                dt.Rows.Add("PROP_NAME", prop.Name);
                dt.Rows.Add("OWNER_ORG_ID", prop.OwnerOrgId?.ToString() ?? "yok");
                if (_session?.ScopeGrants.TryGetValue(prop.ScopeId, out var g3) == true)
                {
                    dt.Rows.Add("Grant Principal", $"{g3.PrincipalTypeId}");
                    dt.Rows.Add("Grant Ceiling", g3.CeilingLevel);
                    dt.Rows.Add("Grant Source", g3.Source);
                    // Inherited mi?
                    if (g3.Source == "descendant")
                        dt.Rows.Add("Inherited From", $"org_scope (principal miras)");
                }
                break;
        }

        dgvItemDetail.DataSource = dt;
        Log($"Tree seçim: {e.Node.Text}");
    }

    // ══════════════════════════════════════
    // Erişilebilir scope grid
    // ══════════════════════════════════════

    private void UpdateScopesGrid()
    {
        if (_session is null) { dgvScopes.DataSource = null; return; }

        var dt = new DataTable();
        dt.Columns.Add("SCOPE_ID", typeof(int));
        dt.Columns.Add("PRINCIPAL_TYPE_ID", typeof(int));
        dt.Columns.Add("CEILING_LEVEL", typeof(string));
        dt.Columns.Add("SOURCE", typeof(string));
        dt.Columns.Add("DESCENDANTS", typeof(string));

        foreach (var g in _session.ScopeGrants.Values.OrderBy(x => x.ScopeId))
        {
            string? desc = null;
            if (_session.Descendants.TryGetValue(g.ScopeId, out var kids))
                desc = string.Join(", ", kids);

            dt.Rows.Add(g.ScopeId, g.PrincipalTypeId, g.CeilingLevel, g.Source, desc ?? "");
        }

        dgvScopes.DataSource = dt;
    }

    // ══════════════════════════════════════
    // Log
    // ══════════════════════════════════════

    private void Log(string message)
    {
        var ts = DateTime.Now.ToString("HH:mm:ss.fff");
        txtLog.AppendText($"[{ts}] {message}{Environment.NewLine}");
    }
}

// ══════════════════════════════════════
// Modeller
// ══════════════════════════════════════

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

// ══════════════════════════════════════
// ISystemPrinciple (MVP kopyası)
// ══════════════════════════════════════

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

// ══════════════════════════════════════
// Tree node modelleri
// ══════════════════════════════════════

public record ScopeNodeInfo(int ScopeId, string ScopeTypeCode, int ScopeTypeId, string Description);
public record OrgNodeInfo(int OrgId, int ScopeId, string Name);
public record PropNodeInfo(int PropId, int ScopeId, string Name, int? OwnerOrgId);
