using System.Data;
using Microsoft.Data.SqlClient;
using Netloji.Core.Auth.Authorization;   // Auth v3: GERÇEK core motoru — CoreAuthProbe + ScopeAuthEngine + ported AuthContext/SystemPrincipleRegistry

namespace NetlojiAuthTestTool;

public partial class MainForm : Form
{
    private const string ConnectionString =
        "Server=.;Database=NL_OtoSistem_v2_25;Trusted_Connection=True;TrustServerCertificate=True;";
        //"Server=.;Database=NetlojiAuthRefactor2026;Trusted_Connection=True;TrustServerCertificate=True;";

    // GERÇEK core motoru — login + erişim çözümü core.ssp_CheckForLogin / core.UserAccessibleScopes üzerinden.
    // Araç ne gösteriyorsa framework runtime'da da aynısını hesaplar (canlı doğrulama).
    private readonly CoreAuthProbe _engine = new(ConnectionString);

    private AuthContext? _session;
    private HashSet<int> _ownedScopes = [];   // F2: core.tblScope.OWNER_USER_ID'den (veri-güdümlü)
    private Dictionary<int, ScopeCatalogEntry> _scopeCatalog = new();   // gerçek core scope kataloğu (tip+ad+parent)
    private Dictionary<int, (string Type, string Name)> _scopeInfo = new();

    public MainForm()
    {
        InitializeComponent();
        Load += Form1_Load;
    }

    private async void Form1_Load(object? sender, EventArgs e)
    {
        Log("Uygulama başlatılıyor...");
        await _engine.LoadCatalogAsync();   // §1.2: principal kataloğu DB'den (tek doğruluk kaynağı)
        await LoadUsers();
        Log("Kullanıcılar yüklendi. Bir kullanıcı seçin.");
    }

    // ══════════════════════════════════════ Kullanıcı listesi (UI'a özel)
    private async Task LoadUsers()
    {
        lvUsers.Items.Clear();
        await using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        const string sql = """
            SET QUOTED_IDENTIFIER ON;
            SELECT u.USER_ID, u.USERNAME, u.GSM_NO, u.PRINCIPAL_TYPE_ID, pt.CODE, pt.TIER_ID
            FROM core.tblUser u
            LEFT JOIN core.conPrincipalType pt ON u.PRINCIPAL_TYPE_ID = pt.PRINCIPAL_TYPE_ID
            WHERE u.DELETED = 0
            ORDER BY u.USER_ID
            """;

        await using var cmd = new SqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var uid = reader["USER_ID"] as int? ?? 0;
            var uname = reader["USERNAME"] as string ?? "";
            var gsm = reader["GSM_NO"] as long? ?? 0L;
            var pid = reader["PRINCIPAL_TYPE_ID"] as int? ?? 0;
            var pcode = reader["CODE"] as string ?? "N/A";
            var tier = reader["TIER_ID"] as short? ?? (short)0;

            var item = new ListViewItem(uid.ToString());
            item.SubItems.Add(uname);
            item.SubItems.Add(pcode);
            item.SubItems.Add(tier.ToString());
            item.Tag = new UserInfo(uid, uname, gsm, pid, pcode, tier);
            lvUsers.Items.Add(item);
        }
        Log($"  {lvUsers.Items.Count} kullanıcı yüklendi.");
    }

    // ══════════════════════════════════════ Kullanıcı seçimi → login (PAYLAŞILAN AuthEngine)
    private async void LvUsers_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (lvUsers.SelectedItems.Count == 0) return;
        var info = (UserInfo)lvUsers.SelectedItems[0].Tag!;
        Log($"Seçilen: [{info.UserId}] {info.Username} (GSM={info.GsmNo})");

        _session = await _engine.LoginByGsmAsync(info.GsmNo);   // core.ssp_CheckForLogin
        if (_session is null) { Log("  LOGIN BAŞARISIZ!"); return; }
        Log($"  Login OK: principal={_session.PrincipalTypeId} tier={_session.TierId} ceiling={_session.CeilingLevel} | scopes={_session.ScopeGrants.Count}");

        _scopeCatalog = await _engine.LoadScopeCatalogAsync();
        _scopeInfo = _scopeCatalog.ToDictionary(kv => kv.Key, kv => (kv.Value.TypeCode, kv.Value.Name));
        _ownedScopes = await _engine.GetOwnedScopeIdsAsync(_session.UserId);   // F2: veri-güdümlü sahiplik

        UpdateSessionPanel();
        BuildHierarchyTree();
        UpdateScopesGrid();
        Log($"Oturum açıldı: [{_session.UserId}] {_session.Username}");
    }

    // ══════════════════════════════════════ Oturum paneli
    private void UpdateSessionPanel()
    {
        if (_session is null) return;
        var sp = _session.SystemPrinciple;   // aktif scope yok → login principal
        lblSid.Text = $"User ID: {_session.UserId}";
        lblSuser.Text = $"Kullanıcı: {_session.Username} (GSM: {_session.GsmNo})";
        lblSprincipal.Text = $"Principal: {_session.PrincipalTypeId} ({sp?.Code ?? "domain"})";
        lblStier.Text = $"Tier: {_session.TierId} | App: {_session.AppId}";
        lblSceiling.Text = $"Ceiling: {_session.CeilingLevel}";
        lblSguard.Text = $"Guard Bypass: {(_session.HasGuardBypass ? "EVET" : "HAYIR")}";
        lblSreadonly.Text = $"Read Only: {(_session.IsReadOnly ? "EVET" : "HAYIR")}";
        lblSsystem.Text = $"System User: {(_session.IsSystemUser ? "EVET" : "HAYIR")}";
        lblSactive.Text = "Active Scope: (seçin)";
        lblSworking.Text = "Working Set: -";
    }

    // ══════════════════════════════════════ Sahiplik ağacı = owned (AuthEngine.OwnedScopeIds — CLI ile aynı kural)
    private void BuildHierarchyTree()
    {
        tvHierarchy.Nodes.Clear();
        dgvItemDetail.DataSource = null;
        if (_session is null) return;

        HashSet<int> owned = _ownedScopes;   // F2: veri-güdümlü (core.tblScope.OWNER_USER_ID)
        var placed = new HashSet<int>();

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

        // Auth v3: scope kataloğundan (gerçek core: tblScope/tblOrganization/tblProperty) — ham MVP-şema sorgusu yok.
        var orgNodes = new Dictionary<int, TreeNode>();

        // owned organization scope'ları
        foreach (var scopeId in owned)
        {
            if (!_scopeCatalog.TryGetValue(scopeId, out var e) || e.TypeCode != "organization") continue;
            var node = new TreeNode(FormatScopeLabel("ORG", e.Name, scopeId)) { ForeColor = Color.DarkBlue, Tag = new OrgNodeInfo(e.OrganizationId ?? 0, scopeId, e.Name) };
            AnnotateGrant(node, scopeId);
            rootNode.Nodes.Add(node);
            orgNodes[scopeId] = node;
            placed.Add(scopeId);
        }

        // owned property scope'ları — parent (OWNER_SCOPE_ID) owned bir org ise onun altına asılır
        foreach (var scopeId in owned)
        {
            if (!_scopeCatalog.TryGetValue(scopeId, out var e) || e.TypeCode != "property") continue;
            var node = new TreeNode(FormatScopeLabel("PROP", e.Name, scopeId)) { ForeColor = Color.DarkGreen, Tag = new PropNodeInfo(e.PropertyId ?? 0, scopeId, e.Name, e.ParentScopeId) };
            AnnotateGrant(node, scopeId);
            if (e.ParentScopeId is int parentScope && orgNodes.TryGetValue(parentScope, out var orgNode))
                orgNode.Nodes.Add(node);
            else
                rootNode.Nodes.Add(node);
            placed.Add(scopeId);
        }

        // owned ama org/property OLMAYAN scope'lar (individual, root_scope, vs.) → standalone node
        // (F2: individual <-> scope artık görünür)
        foreach (var scopeId in owned)
        {
            if (placed.Contains(scopeId)) continue;
            var (type, name) = _scopeInfo.TryGetValue(scopeId, out var si) ? si : ("scope", $"scope {scopeId}");
            var node = new TreeNode(FormatScopeLabel(type.ToUpperInvariant(), name, scopeId))
            {
                ForeColor = Color.DarkSlateGray,
                Tag = new ScopeNodeInfo(scopeId, type, 0, "owned scope")
            };
            AnnotateGrant(node, scopeId);
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
        var sp = SystemPrincipleRegistry.Resolve(principalTypeId);   // core
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
                dt.Rows.Add("OWNER_SCOPE_ID", prop.OwnerScopeId?.ToString() ?? "yok (standalone)");
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

    // ══════════════════════════════════════ Erişilebilir grid = login'in ScopeGrants'ı (core.UserAccessibleScopes = CLI ile AYNI veri)
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

        foreach (var g in _session.ScopeGrants.Values.OrderBy(x => x.ScopeId))
        {
            var (type, name) = _scopeInfo.TryGetValue(g.ScopeId, out var si) ? si : ("?", $"scope {g.ScopeId}");
            dt.Rows.Add(g.ScopeId, type, name, ResolvePrincipalCode(g.PrincipalTypeId), g.CeilingLevel, g.Source);
        }

        dgvScopes.DataSource = dt;
        lblAccessPath.Text = _session.ScopeGrants.Count == 0 ? "(erişilebilir scope yok)" : "Bir satır seçin → erişim yolu (pattern) burada görünür.";
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

// ══════════════════════════════════════ UI-özel modeller (auth modeli core'da)
public record UserInfo(int UserId, string Username, long GsmNo, int PrincipalTypeId, string PrincipalCode, short TierId);

public record ScopeNodeInfo(int ScopeId, string ScopeTypeCode, int ScopeTypeId, string Description);
public record OrgNodeInfo(int OrgId, int ScopeId, string Name);
public record PropNodeInfo(int PropId, int ScopeId, string Name, int? OwnerScopeId);
