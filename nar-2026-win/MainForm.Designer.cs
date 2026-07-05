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
        splitMain = new SplitContainer();
        lvUsers = new ListView();
        colUserId = new ColumnHeader();
        colUsername = new ColumnHeader();
        colPrincipal = new ColumnHeader();
        colTier = new ColumnHeader();
        lblUsers = new Label();
        splitRight = new SplitContainer();
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
        lblSession = new Label();
        tabControl = new TabControl();
        tpHierarchy = new TabPage();
        splitTree = new SplitContainer();
        tvHierarchy = new TreeView();
        lblTree = new Label();
        dgvItemDetail = new DataGridView();
        lblDetail = new Label();
        tpScopes = new TabPage();
        dgvScopes = new DataGridView();
        lblAccessPath = new Label();
        tpLog = new TabPage();
        txtLog = new TextBox();
        ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
        splitMain.Panel1.SuspendLayout();
        splitMain.Panel2.SuspendLayout();
        splitMain.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitRight).BeginInit();
        splitRight.Panel1.SuspendLayout();
        splitRight.Panel2.SuspendLayout();
        splitRight.SuspendLayout();
        pnlSession.SuspendLayout();
        tabControl.SuspendLayout();
        tpHierarchy.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitTree).BeginInit();
        splitTree.Panel1.SuspendLayout();
        splitTree.Panel2.SuspendLayout();
        splitTree.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvItemDetail).BeginInit();
        tpScopes.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvScopes).BeginInit();
        tpLog.SuspendLayout();
        SuspendLayout();
        // 
        // splitMain
        // 
        splitMain.Dock = DockStyle.Fill;
        splitMain.FixedPanel = FixedPanel.Panel1;
        splitMain.Location = new Point(0, 0);
        splitMain.Name = "splitMain";
        // 
        // splitMain.Panel1
        // 
        splitMain.Panel1.Controls.Add(lvUsers);
        splitMain.Panel1.Controls.Add(lblUsers);
        // 
        // splitMain.Panel2
        // 
        splitMain.Panel2.Controls.Add(splitRight);
        splitMain.Size = new Size(1200, 700);
        splitMain.SplitterDistance = 355;
        splitMain.TabIndex = 0;
        // 
        // lvUsers
        // 
        lvUsers.Columns.AddRange(new ColumnHeader[] { colUserId, colUsername, colPrincipal, colTier });
        lvUsers.Dock = DockStyle.Fill;
        lvUsers.Font = new Font("Segoe UI", 8.5F);
        lvUsers.FullRowSelect = true;
        lvUsers.GridLines = true;
        lvUsers.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        lvUsers.Location = new Point(0, 24);
        lvUsers.Name = "lvUsers";
        lvUsers.Size = new Size(355, 676);
        lvUsers.TabIndex = 0;
        lvUsers.UseCompatibleStateImageBehavior = false;
        lvUsers.View = View.Details;
        lvUsers.SelectedIndexChanged += LvUsers_SelectedIndexChanged;
        // 
        // colUserId
        // 
        colUserId.Text = "ID";
        colUserId.Width = 45;
        // 
        // colUsername
        // 
        colUsername.Text = "Kullanıcı";
        colUsername.Width = 150;
        // 
        // colPrincipal
        // 
        colPrincipal.Text = "Principal";
        colPrincipal.Width = 100;
        // 
        // colTier
        // 
        colTier.Text = "Tier";
        colTier.Width = 40;
        // 
        // lblUsers
        // 
        lblUsers.Dock = DockStyle.Top;
        lblUsers.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblUsers.Location = new Point(0, 0);
        lblUsers.Name = "lblUsers";
        lblUsers.Padding = new Padding(4, 0, 0, 0);
        lblUsers.Size = new Size(355, 24);
        lblUsers.TabIndex = 1;
        lblUsers.Text = "KULLANICILAR";
        lblUsers.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // splitRight
        // 
        splitRight.Dock = DockStyle.Fill;
        splitRight.FixedPanel = FixedPanel.Panel1;
        splitRight.Location = new Point(0, 0);
        splitRight.Name = "splitRight";
        splitRight.Orientation = Orientation.Horizontal;
        // 
        // splitRight.Panel1
        // 
        splitRight.Panel1.Controls.Add(pnlSession);
        splitRight.Panel1.Controls.Add(lblSession);
        // 
        // splitRight.Panel2
        // 
        splitRight.Panel2.Controls.Add(tabControl);
        splitRight.Size = new Size(841, 700);
        splitRight.SplitterDistance = 277;
        splitRight.TabIndex = 0;
        // 
        // pnlSession
        // 
        pnlSession.AutoScroll = true;
        pnlSession.Controls.Add(lblSid);
        pnlSession.Controls.Add(lblSuser);
        pnlSession.Controls.Add(lblSprincipal);
        pnlSession.Controls.Add(lblStier);
        pnlSession.Controls.Add(lblSceiling);
        pnlSession.Controls.Add(lblSguard);
        pnlSession.Controls.Add(lblSreadonly);
        pnlSession.Controls.Add(lblSsystem);
        pnlSession.Controls.Add(lblSactive);
        pnlSession.Controls.Add(lblSworking);
        pnlSession.Dock = DockStyle.Fill;
        pnlSession.Location = new Point(0, 24);
        pnlSession.Name = "pnlSession";
        pnlSession.Padding = new Padding(8);
        pnlSession.Size = new Size(841, 253);
        pnlSession.TabIndex = 0;
        // 
        // lblSid
        // 
        lblSid.AutoSize = true;
        lblSid.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblSid.Location = new Point(8, 4);
        lblSid.Name = "lblSid";
        lblSid.Size = new Size(60, 15);
        lblSid.TabIndex = 0;
        lblSid.Text = "User ID: -";
        // 
        // lblSuser
        // 
        lblSuser.AutoSize = true;
        lblSuser.Font = new Font("Segoe UI", 9F);
        lblSuser.Location = new Point(8, 24);
        lblSuser.Name = "lblSuser";
        lblSuser.Size = new Size(63, 15);
        lblSuser.TabIndex = 1;
        lblSuser.Text = "Kullanıcı: -";
        // 
        // lblSprincipal
        // 
        lblSprincipal.AutoSize = true;
        lblSprincipal.Font = new Font("Segoe UI", 9F);
        lblSprincipal.Location = new Point(8, 42);
        lblSprincipal.Name = "lblSprincipal";
        lblSprincipal.Size = new Size(64, 15);
        lblSprincipal.TabIndex = 2;
        lblSprincipal.Text = "Principal: -";
        // 
        // lblStier
        // 
        lblStier.AutoSize = true;
        lblStier.Font = new Font("Segoe UI", 9F);
        lblStier.Location = new Point(8, 60);
        lblStier.Name = "lblStier";
        lblStier.Size = new Size(80, 15);
        lblStier.TabIndex = 3;
        lblStier.Text = "Tier: - | App: -";
        // 
        // lblSceiling
        // 
        lblSceiling.AutoSize = true;
        lblSceiling.Font = new Font("Segoe UI", 9F);
        lblSceiling.Location = new Point(8, 78);
        lblSceiling.Name = "lblSceiling";
        lblSceiling.Size = new Size(55, 15);
        lblSceiling.TabIndex = 4;
        lblSceiling.Text = "Ceiling: -";
        // 
        // lblSguard
        // 
        lblSguard.AutoSize = true;
        lblSguard.Font = new Font("Segoe UI", 9F);
        lblSguard.Location = new Point(8, 96);
        lblSguard.Name = "lblSguard";
        lblSguard.Size = new Size(89, 15);
        lblSguard.TabIndex = 5;
        lblSguard.Text = "Guard Bypass: -";
        // 
        // lblSreadonly
        // 
        lblSreadonly.AutoSize = true;
        lblSreadonly.Font = new Font("Segoe UI", 9F);
        lblSreadonly.Location = new Point(8, 114);
        lblSreadonly.Name = "lblSreadonly";
        lblSreadonly.Size = new Size(72, 15);
        lblSreadonly.TabIndex = 6;
        lblSreadonly.Text = "Read Only: -";
        // 
        // lblSsystem
        // 
        lblSsystem.AutoSize = true;
        lblSsystem.Font = new Font("Segoe UI", 9F);
        lblSsystem.Location = new Point(8, 132);
        lblSsystem.Name = "lblSsystem";
        lblSsystem.Size = new Size(82, 15);
        lblSsystem.TabIndex = 7;
        lblSsystem.Text = "System User: -";
        // 
        // lblSactive
        // 
        lblSactive.AutoSize = true;
        lblSactive.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblSactive.Location = new Point(8, 150);
        lblSactive.Name = "lblSactive";
        lblSactive.Size = new Size(91, 15);
        lblSactive.TabIndex = 8;
        lblSactive.Text = "Active Scope: -";
        // 
        // lblSworking
        // 
        lblSworking.AutoSize = true;
        lblSworking.Font = new Font("Segoe UI", 9F);
        lblSworking.Location = new Point(8, 170);
        lblSworking.Name = "lblSworking";
        lblSworking.Size = new Size(82, 15);
        lblSworking.TabIndex = 9;
        lblSworking.Text = "Working Set: -";
        // 
        // lblSession
        // 
        lblSession.Dock = DockStyle.Top;
        lblSession.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblSession.Location = new Point(0, 0);
        lblSession.Name = "lblSession";
        lblSession.Padding = new Padding(4, 0, 0, 0);
        lblSession.Size = new Size(841, 24);
        lblSession.TabIndex = 1;
        lblSession.Text = "OTURUM DETAY";
        lblSession.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // tabControl
        // 
        tabControl.Controls.Add(tpHierarchy);
        tabControl.Controls.Add(tpScopes);
        tabControl.Controls.Add(tpLog);
        tabControl.Dock = DockStyle.Fill;
        tabControl.Font = new Font("Segoe UI", 9F);
        tabControl.Location = new Point(0, 0);
        tabControl.Name = "tabControl";
        tabControl.SelectedIndex = 0;
        tabControl.Size = new Size(841, 419);
        tabControl.TabIndex = 0;
        // 
        // tpHierarchy
        // 
        tpHierarchy.BackColor = SystemColors.Control;
        tpHierarchy.Controls.Add(splitTree);
        tpHierarchy.Location = new Point(4, 24);
        tpHierarchy.Name = "tpHierarchy";
        tpHierarchy.Padding = new Padding(4);
        tpHierarchy.Size = new Size(833, 391);
        tpHierarchy.TabIndex = 0;
        tpHierarchy.Text = "Hiyerarşi (Org → Property)";
        // 
        // splitTree
        // 
        splitTree.Dock = DockStyle.Fill;
        splitTree.Location = new Point(4, 4);
        splitTree.Name = "splitTree";
        // 
        // splitTree.Panel1
        // 
        splitTree.Panel1.Controls.Add(tvHierarchy);
        splitTree.Panel1.Controls.Add(lblTree);
        // 
        // splitTree.Panel2
        // 
        splitTree.Panel2.Controls.Add(dgvItemDetail);
        splitTree.Panel2.Controls.Add(lblDetail);
        splitTree.Size = new Size(825, 383);
        splitTree.SplitterDistance = 665;
        splitTree.TabIndex = 0;
        // 
        // tvHierarchy
        // 
        tvHierarchy.Dock = DockStyle.Fill;
        tvHierarchy.Font = new Font("Segoe UI", 9F);
        tvHierarchy.HideSelection = false;
        tvHierarchy.Location = new Point(0, 22);
        tvHierarchy.Name = "tvHierarchy";
        tvHierarchy.Size = new Size(665, 361);
        tvHierarchy.TabIndex = 0;
        tvHierarchy.AfterSelect += TvHierarchy_AfterSelect;
        // 
        // lblTree
        // 
        lblTree.Dock = DockStyle.Top;
        lblTree.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        lblTree.Location = new Point(0, 0);
        lblTree.Name = "lblTree";
        lblTree.Size = new Size(665, 22);
        lblTree.TabIndex = 1;
        lblTree.Text = "Organizasyon / Mülk Ağacı";
        lblTree.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // dgvItemDetail
        // 
        dgvItemDetail.AllowUserToAddRows = false;
        dgvItemDetail.AllowUserToDeleteRows = false;
        dgvItemDetail.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvItemDetail.BackgroundColor = SystemColors.Window;
        dgvItemDetail.Dock = DockStyle.Fill;
        dgvItemDetail.Font = new Font("Segoe UI", 8.5F);
        dgvItemDetail.Location = new Point(0, 22);
        dgvItemDetail.Name = "dgvItemDetail";
        dgvItemDetail.ReadOnly = true;
        dgvItemDetail.RowHeadersVisible = false;
        dgvItemDetail.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvItemDetail.Size = new Size(156, 361);
        dgvItemDetail.TabIndex = 0;
        // 
        // lblDetail
        // 
        lblDetail.Dock = DockStyle.Top;
        lblDetail.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        lblDetail.Location = new Point(0, 0);
        lblDetail.Name = "lblDetail";
        lblDetail.Size = new Size(156, 22);
        lblDetail.TabIndex = 1;
        lblDetail.Text = "Detay (Inherited)";
        lblDetail.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // tpScopes
        // 
        tpScopes.BackColor = SystemColors.Control;
        tpScopes.Controls.Add(dgvScopes);
        tpScopes.Controls.Add(lblAccessPath);
        tpScopes.Location = new Point(4, 24);
        tpScopes.Name = "tpScopes";
        tpScopes.Padding = new Padding(4);
        tpScopes.Size = new Size(17, 0);
        tpScopes.TabIndex = 1;
        tpScopes.Text = "Erişilebilir Scope'lar";
        // 
        // dgvScopes
        // 
        dgvScopes.AllowUserToAddRows = false;
        dgvScopes.AllowUserToDeleteRows = false;
        dgvScopes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvScopes.BackgroundColor = SystemColors.Window;
        dgvScopes.Dock = DockStyle.Fill;
        dgvScopes.Font = new Font("Segoe UI", 8.5F);
        dgvScopes.Location = new Point(4, 4);
        dgvScopes.Name = "dgvScopes";
        dgvScopes.ReadOnly = true;
        dgvScopes.RowHeadersVisible = false;
        dgvScopes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvScopes.Size = new Size(9, 0);
        dgvScopes.TabIndex = 0;
        dgvScopes.SelectionChanged += DgvScopes_SelectionChanged;
        // 
        // lblAccessPath
        // 
        lblAccessPath.BackColor = Color.FromArgb(245, 245, 220);
        lblAccessPath.Dock = DockStyle.Bottom;
        lblAccessPath.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
        lblAccessPath.Location = new Point(4, -28);
        lblAccessPath.Name = "lblAccessPath";
        lblAccessPath.Padding = new Padding(4, 0, 0, 0);
        lblAccessPath.Size = new Size(9, 24);
        lblAccessPath.TabIndex = 1;
        lblAccessPath.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // tpLog
        // 
        tpLog.BackColor = SystemColors.Control;
        tpLog.Controls.Add(txtLog);
        tpLog.Location = new Point(4, 24);
        tpLog.Name = "tpLog";
        tpLog.Padding = new Padding(4);
        tpLog.Size = new Size(17, 0);
        tpLog.TabIndex = 2;
        tpLog.Text = "İşlem Logu";
        // 
        // txtLog
        // 
        txtLog.BackColor = Color.FromArgb(30, 30, 30);
        txtLog.Dock = DockStyle.Fill;
        txtLog.Font = new Font("Consolas", 9F);
        txtLog.ForeColor = Color.FromArgb(200, 220, 200);
        txtLog.Location = new Point(4, 4);
        txtLog.Multiline = true;
        txtLog.Name = "txtLog";
        txtLog.ReadOnly = true;
        txtLog.ScrollBars = ScrollBars.Both;
        txtLog.Size = new Size(9, 0);
        txtLog.TabIndex = 0;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1200, 700);
        Controls.Add(splitMain);
        Font = new Font("Segoe UI", 9F);
        MinimumSize = new Size(900, 500);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Netloji Auth Refactor 2026 — GUI Test Tool";
        splitMain.Panel1.ResumeLayout(false);
        splitMain.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
        splitMain.ResumeLayout(false);
        splitRight.Panel1.ResumeLayout(false);
        splitRight.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitRight).EndInit();
        splitRight.ResumeLayout(false);
        pnlSession.ResumeLayout(false);
        pnlSession.PerformLayout();
        tabControl.ResumeLayout(false);
        tpHierarchy.ResumeLayout(false);
        splitTree.Panel1.ResumeLayout(false);
        splitTree.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitTree).EndInit();
        splitTree.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvItemDetail).EndInit();
        tpScopes.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvScopes).EndInit();
        tpLog.ResumeLayout(false);
        tpLog.PerformLayout();
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
