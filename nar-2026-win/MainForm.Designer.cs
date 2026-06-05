#nullable enable
namespace NetlojiAuthTestTool;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        // ── Split: Ana (sol-sağ) ──
        splitMain = new SplitContainer();
        // ── Split: Sağ (üst-alt) ──
        splitRight = new SplitContainer();
        // ── Split: Tree detay (sol-sağ) ──
        splitTree = new SplitContainer();

        // ── Sol: Kullanıcı listesi ──
        lblUsers = new Label();
        lvUsers = new ListView();
        colUserId = new ColumnHeader();
        colUsername = new ColumnHeader();
        colPrincipal = new ColumnHeader();
        colTier = new ColumnHeader();

        // ── Sağ üst: Oturum künyesi ──
        lblSession = new Label();
        pnlSession = new Panel();
        lblSid = new Label();
        lblSuser = new Label();
        lblSprincipal = new Label();
        lblStier = new Label();
        lblSceiling = new Label();
        lblSguard = new Label();
        lblSreadonly = new Label();
        lblSsystem = new Label();
        lblSactive = new Label();
        lblSworking = new Label();

        // ── Sağ alt: TabControl ──
        tabControl = new TabControl();
        tpHierarchy = new TabPage();
        tpScopes = new TabPage();
        tpLog = new TabPage();

        // ── TP1: Tree + Detail ──
        lblTree = new Label();
        tvHierarchy = new TreeView();
        lblDetail = new Label();
        dgvItemDetail = new DataGridView();

        // ── TP2: Scopes grid ──
        dgvScopes = new DataGridView();
        lblAccessPath = new Label();

        // ── TP3: Log ──
        txtLog = new TextBox();

        // ── SplitContainer ayarları ──
        ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
        splitMain.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitRight).BeginInit();
        splitRight.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitTree).BeginInit();
        splitTree.SuspendLayout();
        pnlSession.SuspendLayout();
        tabControl.SuspendLayout();
        tpHierarchy.SuspendLayout();
        tpScopes.SuspendLayout();
        tpLog.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvItemDetail).BeginInit();
        ((System.ComponentModel.ISupportInitialize)dgvScopes).BeginInit();
        SuspendLayout();

        // ══════════════════════════════════════
        // splitMain
        // ══════════════════════════════════════
        splitMain.Dock = DockStyle.Fill;
        splitMain.SplitterDistance = 280;
        splitMain.FixedPanel = FixedPanel.Panel1;

        // ══════════════════════════════════════
        // SOL PANEL: Kullanıcı listesi
        // ══════════════════════════════════════
        lblUsers.Dock = DockStyle.Top;
        lblUsers.Text = "KULLANICILAR";
        lblUsers.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblUsers.Height = 24;
        lblUsers.TextAlign = ContentAlignment.MiddleLeft;
        lblUsers.Padding = new Padding(4, 0, 0, 0);

        lvUsers.Dock = DockStyle.Fill;
        lvUsers.View = View.Details;
        lvUsers.FullRowSelect = true;
        lvUsers.GridLines = true;
        lvUsers.Font = new Font("Segoe UI", 8.5F);
        lvUsers.Columns.AddRange([colUserId, colUsername, colPrincipal, colTier]);
        lvUsers.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        lvUsers.SelectedIndexChanged += LvUsers_SelectedIndexChanged;

        colUserId.Text = "ID";
        colUserId.Width = 45;
        colUsername.Text = "Kullanıcı";
        colUsername.Width = 150;
        colPrincipal.Text = "Principal";
        colPrincipal.Width = 100;
        colTier.Text = "Tier";
        colTier.Width = 40;

        splitMain.Panel1.Controls.Add(lvUsers);
        splitMain.Panel1.Controls.Add(lblUsers);

        // ══════════════════════════════════════
        // SAĞ PANEL: splitRight (üst-alt)
        // ══════════════════════════════════════
        splitRight.Dock = DockStyle.Fill;
        splitRight.Orientation = Orientation.Horizontal;
        splitRight.SplitterDistance = 130;
        splitRight.FixedPanel = FixedPanel.Panel1;

        // ══════════════════════════════════════
        // SAĞ ÜST: Oturum künyesi
        // ══════════════════════════════════════
        lblSession.Dock = DockStyle.Top;
        lblSession.Text = "OTURUM DETAY";
        lblSession.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblSession.Height = 24;
        lblSession.TextAlign = ContentAlignment.MiddleLeft;
        lblSession.Padding = new Padding(4, 0, 0, 0);

        pnlSession.Dock = DockStyle.Fill;
        pnlSession.AutoScroll = true;
        pnlSession.Padding = new Padding(8);

        var sessionFont = new Font("Segoe UI", 9F);
        var sessionBold = new Font("Segoe UI", 9F, FontStyle.Bold);
        int y = 4;

        void AddLabel(ref Label lbl, string text, int yPos, Font? font = null)
        {
            lbl = new Label();
            lbl.Text = text;
            lbl.Font = font ?? sessionFont;
            lbl.Location = new Point(8, yPos);
            lbl.AutoSize = true;
            pnlSession.Controls.Add(lbl);
        }

        AddLabel(ref lblSid, "User ID: -", y, sessionBold); y += 20;
        AddLabel(ref lblSuser, "Kullanıcı: -", y); y += 18;
        AddLabel(ref lblSprincipal, "Principal: -", y); y += 18;
        AddLabel(ref lblStier, "Tier: - | App: -", y); y += 18;
        AddLabel(ref lblSceiling, "Ceiling: -", y); y += 18;
        AddLabel(ref lblSguard, "Guard Bypass: -", y); y += 18;
        AddLabel(ref lblSreadonly, "Read Only: -", y); y += 18;
        AddLabel(ref lblSsystem, "System User: -", y); y += 18;
        AddLabel(ref lblSactive, "Active Scope: -", y, sessionBold); y += 20;
        AddLabel(ref lblSworking, "Working Set: -", y);

        splitRight.Panel1.Controls.Add(pnlSession);
        splitRight.Panel1.Controls.Add(lblSession);

        // ══════════════════════════════════════
        // SAĞ ALT: TabControl
        // ══════════════════════════════════════
        tabControl.Dock = DockStyle.Fill;
        tabControl.Font = new Font("Segoe UI", 9F);

        // ── TP1: Hiyerarşi (tree + detail) ──
        tpHierarchy.Text = "Hiyerarşi (Org → Property)";
        tpHierarchy.Padding = new Padding(4);
        tpHierarchy.BackColor = SystemColors.Control;

        splitTree.Dock = DockStyle.Fill;
        splitTree.SplitterDistance = 260;

        lblTree.Dock = DockStyle.Top;
        lblTree.Text = "Organizasyon / Mülk Ağacı";
        lblTree.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        lblTree.Height = 22;
        lblTree.TextAlign = ContentAlignment.MiddleLeft;

        tvHierarchy.Dock = DockStyle.Fill;
        tvHierarchy.Font = new Font("Segoe UI", 9F);
        tvHierarchy.ShowLines = true;
        tvHierarchy.ShowPlusMinus = true;
        tvHierarchy.ShowRootLines = true;
        tvHierarchy.HideSelection = false;
        tvHierarchy.AfterSelect += TvHierarchy_AfterSelect;

        lblDetail.Dock = DockStyle.Top;
        lblDetail.Text = "Detay (Inherited)";
        lblDetail.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        lblDetail.Height = 22;
        lblDetail.TextAlign = ContentAlignment.MiddleLeft;

        dgvItemDetail.Dock = DockStyle.Fill;
        dgvItemDetail.ReadOnly = true;
        dgvItemDetail.AllowUserToAddRows = false;
        dgvItemDetail.AllowUserToDeleteRows = false;
        dgvItemDetail.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvItemDetail.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvItemDetail.RowHeadersVisible = false;
        dgvItemDetail.Font = new Font("Segoe UI", 8.5F);
        dgvItemDetail.BackgroundColor = SystemColors.Window;

        splitTree.Panel1.Controls.Add(tvHierarchy);
        splitTree.Panel1.Controls.Add(lblTree);
        splitTree.Panel2.Controls.Add(dgvItemDetail);
        splitTree.Panel2.Controls.Add(lblDetail);

        tpHierarchy.Controls.Add(splitTree);
        tabControl.TabPages.Add(tpHierarchy);

        // ── TP2: Erişilebilir Scope'lar ──
        tpScopes.Text = "Erişilebilir Scope'lar";
        tpScopes.Padding = new Padding(4);
        tpScopes.BackColor = SystemColors.Control;

        dgvScopes.Dock = DockStyle.Fill;
        dgvScopes.ReadOnly = true;
        dgvScopes.AllowUserToAddRows = false;
        dgvScopes.AllowUserToDeleteRows = false;
        dgvScopes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvScopes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvScopes.RowHeadersVisible = false;
        dgvScopes.Font = new Font("Segoe UI", 8.5F);
        dgvScopes.BackgroundColor = SystemColors.Window;
        dgvScopes.SelectionChanged += DgvScopes_SelectionChanged;

        lblAccessPath.Dock = DockStyle.Bottom;
        lblAccessPath.Text = "";
        lblAccessPath.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
        lblAccessPath.Height = 24;
        lblAccessPath.TextAlign = ContentAlignment.MiddleLeft;
        lblAccessPath.Padding = new Padding(4, 0, 0, 0);
        lblAccessPath.BackColor = Color.FromArgb(245, 245, 220);

        tpScopes.Controls.Add(dgvScopes);
        tpScopes.Controls.Add(lblAccessPath);
        tabControl.TabPages.Add(tpScopes);

        // ── TP3: Log ──
        tpLog.Text = "İşlem Logu";
        tpLog.Padding = new Padding(4);
        tpLog.BackColor = SystemColors.Control;

        txtLog.Dock = DockStyle.Fill;
        txtLog.Multiline = true;
        txtLog.ScrollBars = ScrollBars.Both;
        txtLog.Font = new Font("Consolas", 9F);
        txtLog.ReadOnly = true;
        txtLog.BackColor = Color.FromArgb(30, 30, 30);
        txtLog.ForeColor = Color.FromArgb(200, 220, 200);

        tpLog.Controls.Add(txtLog);
        tabControl.TabPages.Add(tpLog);

        splitRight.Panel2.Controls.Add(tabControl);

        // ── Ana split'e sağ paneli ekle ──
        splitMain.Panel2.Controls.Add(splitRight);

        // ══════════════════════════════════════
        // Form
        // ══════════════════════════════════════
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1200, 700);
        Controls.Add(splitMain);
        MinimumSize = new Size(900, 500);
        Text = "Netloji Auth Refactor 2026 — GUI Test Tool";
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9F);

        ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
        splitMain.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitRight).EndInit();
        splitRight.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitTree).EndInit();
        splitTree.ResumeLayout(false);
        pnlSession.ResumeLayout(false);
        tabControl.ResumeLayout(false);
        tpHierarchy.ResumeLayout(false);
        tpScopes.ResumeLayout(false);
        tpLog.ResumeLayout(false);
        tpLog.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)dgvItemDetail).EndInit();
        ((System.ComponentModel.ISupportInitialize)dgvScopes).EndInit();
        ResumeLayout(false);
    }

    #endregion

    // ── Split containers ──
    private SplitContainer splitMain;
    private SplitContainer splitRight;
    private SplitContainer splitTree;

    // ── Sol: Kullanıcı listesi ──
    private Label lblUsers;
    private ListView lvUsers;
    private ColumnHeader colUserId;
    private ColumnHeader colUsername;
    private ColumnHeader colPrincipal;
    private ColumnHeader colTier;

    // ── Sağ üst: Oturum ──
    private Label lblSession;
    private Panel pnlSession;
    private Label lblSid;
    private Label lblSuser;
    private Label lblSprincipal;
    private Label lblStier;
    private Label lblSceiling;
    private Label lblSguard;
    private Label lblSreadonly;
    private Label lblSsystem;
    private Label lblSactive;
    private Label lblSworking;

    // ── Tab control ──
    private TabControl tabControl;
    private TabPage tpHierarchy;
    private TabPage tpScopes;
    private TabPage tpLog;

    // ── TP1: Hiyerarşi ──
    private Label lblTree;
    private TreeView tvHierarchy;
    private Label lblDetail;
    private DataGridView dgvItemDetail;

    // ── TP2: Scopes ──
    private DataGridView dgvScopes;
    private Label lblAccessPath;

    // ── TP3: Log ──
    private TextBox txtLog;
}
