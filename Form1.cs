using Syncfusion.GridHelperClasses;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Grid;
using Syncfusion.Pdf.Tables;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml.Linq;

namespace TestFat
{
    public partial class Form1 : Form
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        private int familyIDInContext = 0;
        public string anbiyamAddress = "";
        private Point _hoveredCell = new Point(-1, -1);

        // Add these members to the Form1 class (e.g., near the top of the class)
        public string LoggedInUser { get; private set; }
        public Form1(string loggedInUser)
        {
            InitializeComponent();
            // Without Dock=Fill the BuildTopBar Dock=Top panel would overlap the tab control
            // hiding all filter panels that live near the top of each tab page.
            familytab.Dock = DockStyle.Fill;
            LoggedInUser = loggedInUser ?? string.Empty;

            // Optionally show username in the title bar so it's visible:
            if (!string.IsNullOrWhiteSpace(LoggedInUser))
            {
                this.Text = $"{this.Text} - {LoggedInUser}";

                if (LoggedInUser == "GUEST")
                {
                    btncreate.Enabled = false;
                    btnedit.Enabled = false;
                    btnCemetery.Enabled = false;
                    btnFamilyCreate.Enabled = false;
                    btnFamilyEdit.Enabled = false;
                    btnOutsideParsihMember.Enabled = false;
                }
            }

            this.FormBorderStyle = FormBorderStyle.None;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.ShowIcon        = false;
            // 1 px border painted in the form's Paint event
            this.Paint += (s, pe) =>
                pe.Graphics.DrawRectangle(new Pen(AppTheme.Navy, 1), 0, 0, Width - 1, Height - 1);

            ApplyTheme();
            LoadAnbiyamGrid(null);
            LoadOccupation();
            LoadAnbiyam();

            LoadAgeGroupChart();
            LoadGenderGroupChart();
            LoadZoneFamilyChart();

            // LoadFamilyBasicDetails(null);
            // LoadAllCemeteryData(null);

            // Make all grids read-only and disable empty row
            familygrid.ReadOnly = true;
            familygrid.AllowUserToAddRows = false;
            anbiyamGrid.ReadOnly = true;
            anbiyamGrid.AllowUserToAddRows = false;
            familyMembersGrid.ReadOnly = true;
            familyMembersGrid.AllowUserToAddRows = false;
            cemeteryGridView.ReadOnly = true;
            cemeteryGridView.AllowUserToAddRows = false;
            subscriptionGrid.AllowUserToAddRows = false;

            // Full-row selection so SelectionChanged fires when any cell is clicked
            familygrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            familygrid.MultiSelect   = false;

            this.familygrid.SelectionChanged += familygrid_SelectionChanged;
            this.familygrid.RowPrePaint += familygrid_RowPrePaint;

            // Icons are applied by ApplyIcons() inside ApplyTheme() — no overrides here
            anbiyamGrid.CellClick += anbiyamGrid_CellClick;
            anbiyamGrid.SelectionChanged += anbiyamGrid_SelectionChanged;
            cemeteryGridView.CellClick -= cemeteryGridView_CellClick;
            cemeteryGridView.CellClick += cemeteryGridView_CellClick;
            cemeteryGridView.CellFormatting -= cemeteryGridView_CellFormatting;
            cemeteryGridView.CellFormatting += cemeteryGridView_CellFormatting;

            WireGridButtonPainting(familygrid);
            WireGridButtonPainting(anbiyamGrid);
            WireGridButtonPainting(cemeteryGridView);

            ShowAnbiyamOnMap(anbiyamAddress);
            parishCombobox.SelectedIndex = 0;

            // panel7 - Top Left
            this.panel7.Location = new System.Drawing.Point(10, 10);
            this.panel7.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // panel8 - Top Right
            this.panel8.Location = new System.Drawing.Point(this.ClientSize.Width - this.panel8.Width - 10, 10);
            this.panel8.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            SetupPaginationControls();
            AdjustFamilyTabLayout();
        }

        private void SetupPaginationControls()
        {
            var navFont = new Font("Georgia", 10F);
            var navBack = Color.FromArgb(216, 226, 220);

            // ── Families tab: add pagination row inside panel5 (action buttons) ──
            btnFamilyPrevious = new System.Windows.Forms.Button { Text = "< Prev", Font = navFont, Width = 80, Height = 28, Location = new Point(810, 36) };
            btnFamilyNext     = new System.Windows.Forms.Button { Text = "Next >", Font = navFont, Width = 80, Height = 28, Location = new Point(900, 36) };
            lblFamilyPageInfo = new System.Windows.Forms.Label  { Text = "Page 1 of 1", Font = navFont, AutoSize = true, Location = new Point(990, 42) };
            btnFamilyPrevious.Click += (s, e) => { if (_familyCurrentPage > 1) { _familyCurrentPage--; LoadFamilyBasicDetails(null); } };
            btnFamilyNext.Click     += (s, e) => {
                int tp = (int)Math.Ceiling((double)_familyTotalCount / PageSize);
                if (_familyCurrentPage < tp) { _familyCurrentPage++; LoadFamilyBasicDetails(null); }
            };
            panel5.Controls.AddRange(new Control[] { btnFamilyPrevious, btnFamilyNext, lblFamilyPageInfo });

            // Anbiyam pagination removed per UX improvements

            // ── Cemetery tab: shrink grid and add a pagination panel below it ──
            cemeteryGridView.Size = new Size(cemeteryGridView.Width, cemeteryGridView.Height - 42);
            var cemeteryPagPanel = new System.Windows.Forms.Panel
            {
                BackColor = navBack,
                Location  = new Point(cemeteryGridView.Left, cemeteryGridView.Bottom + 2),
                Size      = new Size(cemeteryGridView.Width, 38)
            };
            btnCemeteryPrevious = new System.Windows.Forms.Button { Text = "< Prev", Font = navFont, Width = 80, Height = 28, Location = new Point(10, 5) };
            btnCemeteryNext     = new System.Windows.Forms.Button { Text = "Next >", Font = navFont, Width = 80, Height = 28, Location = new Point(100, 5) };
            lblCemeteryPageInfo = new System.Windows.Forms.Label  { Text = "Page 1 of 1", Font = navFont, AutoSize = true, Location = new Point(195, 11) };
            btnCemeteryPrevious.Click += (s, e) => { if (_cemeteryCurrentPage > 1) { _cemeteryCurrentPage--; LoadAllCemeteryData(null); } };
            btnCemeteryNext.Click     += (s, e) => {
                int tp = (int)Math.Ceiling((double)_cemeteryTotalCount / PageSize);
                if (_cemeteryCurrentPage < tp) { _cemeteryCurrentPage++; LoadAllCemeteryData(null); }
            };
            cemeteryPagPanel.Controls.AddRange(new Control[] { btnCemeteryPrevious, btnCemeteryNext, lblCemeteryPageInfo });
            cemeteryPage.Controls.Add(cemeteryPagPanel);
        }

        private void AdjustFamilyTabLayout()
        {
            int pageHeight = familyPage.Height;
            int pageWidth = familyPage.Width;
            int pad = 4;

            int h15 = (int)(pageHeight * 0.15);
            int h50 = (int)(pageHeight * 0.50);
            int h20 = (int)(pageHeight * 0.20);
            int h15b = pageHeight - h15 - h50 - h20 - (pad * 3);

            panel4.Location = new Point(0, 0);
            panel4.Size = new Size(pageWidth, h15);
            panel4.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            familygrid.Location = new Point(0, h15 + pad);
            familygrid.Size = new Size(pageWidth, h50);
            familygrid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            var lblMembers = familyPage.Controls.OfType<Label>().FirstOrDefault(l => l.Text.Contains("Family Members"));
            if (lblMembers != null)
            {
                lblMembers.Location = new Point(0, h15 + pad + h50 + pad);
                lblMembers.Size = new Size(pageWidth, 22);
                lblMembers.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            }

            familyMembersGrid.Location = new Point(0, h15 + pad + h50 + pad + 22 + pad);
            familyMembersGrid.Size = new Size(pageWidth, h20);
            familyMembersGrid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            panel5.Location = new Point(0, h15 + pad + h50 + pad + 22 + pad + h20 + pad);
            panel5.Size = new Size(pageWidth, h15b);
            panel5.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        }

        private void ApplyTheme()
        {
            // ── Form background ──
            this.BackColor = AppTheme.OffWhite;

            // ── Hide native tab headers; we replace them with a custom nav bar ──
            familytab.Appearance = TabAppearance.Normal;
            familytab.SizeMode   = TabSizeMode.Fixed;
            familytab.ItemSize   = new Size(1, 1);   // 1px tall = effectively invisible
            familytab.Padding    = new Point(0, 0);

            // ── Tab page backgrounds ──
            foreach (System.Windows.Forms.TabPage tp in familytab.TabPages)
            {
                tp.BackColor = AppTheme.OffWhite;
                tp.ForeColor = AppTheme.Navy;
            }

            // ── Grids ──
            AppTheme.StyleGrid(familygrid);
            AppTheme.StyleGrid(anbiyamGrid);
            AppTheme.StyleGrid(cemeteryGridView);
            AppTheme.StyleGrid(familyMembersGrid);

            // ── Family members section divider label ──
            {
                var lblMembers = new Label
                {
                    Text      = "  Family Members",
                    Font      = AppTheme.BoldSmall,
                    ForeColor = AppTheme.Gold,
                    BackColor = AppTheme.Navy,
                    AutoSize  = false,
                    Size      = new Size(familyMembersGrid.Width, 22),
                    Location  = new Point(familyMembersGrid.Left, familyMembersGrid.Top - 22),
                    Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                    TextAlign = ContentAlignment.MiddleLeft,
                };
                familyPage.Controls.Add(lblMembers);
                lblMembers.BringToFront();
            }

            // ── Anbiyam tab filter panel & action panel ──
            //panel2.BackColor = AppTheme.FilterBar;
           // panel2.Paint += (s, pe) => pe.Graphics.FillRectangle(new SolidBrush(AppTheme.Teal), 0, 0, 4, ((Panel)s).Height);
            panel3.BackColor = AppTheme.ActionBar;
            AppTheme.StyleButtonPrimary(btncreate);
            AppTheme.StyleButtonSecondary(btnedit);
            AppTheme.StyleButtonOutline(btnpdfExport);
            //AppTheme.StyleButtonPrimary(searchButton);
            //StylePanelLabels(panel2);

            // Anbiyam reset filters removed per UX improvements

            // ── Family tab filter panel & action panel ──
            panel4.BackColor = AppTheme.FilterBar;
            panel5.BackColor = AppTheme.ActionBar;
            AppTheme.StyleButtonPrimary(btnFamilyCreate);
            AppTheme.StyleButtonSecondary(btnFamilyEdit);
            AppTheme.StyleButtonPrimary(searchFamily);
            AppTheme.StyleButtonSecondary(btnCemetery);
            AppTheme.StyleButtonOutline(btn_exportfamily);
            // Disable button: amber/warning colour — distinct from danger red
            btn_disablefamily.FlatStyle = FlatStyle.Flat;
            btn_disablefamily.BackColor = Color.FromArgb(180, 95, 15);
            btn_disablefamily.ForeColor = Color.White;
            btn_disablefamily.FlatAppearance.BorderColor = Color.FromArgb(150, 75, 5);
            btn_disablefamily.FlatAppearance.BorderSize  = 0;
            btn_disablefamily.Font   = AppTheme.BoldSmall;
            btn_disablefamily.Cursor = Cursors.Hand;
            StylePanelLabels(panel4);

            // ── Family tab: wire existing Reset Filter button (button1) ──
            AppTheme.StyleButtonOutline(button1);
            button1.Click += (s, e) =>
            {
                familyAnbiyamCombobox.SelectedIndex  = 0;
                txtFamilyHead.Clear();
                familyOccupationComboxbox.SelectedIndex = 0;
                cemeteryComboBox.SelectedIndex       = 0;
                _familyAnbiyamFilter    = 0;
                _familyHeadFilter       = null;
                _familyOccFilter        = null;
                _familyCemeteryFilter   = DBNull.Value;
                _familyCurrentPage      = 1;
                LoadFamilyBasicDetails(null);
            };

            // ── Selection-dependent buttons start disabled ──
            btn_disablefamily.Enabled = false;
            btnFamilyEdit.Enabled = false;
            btnCemetery.Enabled = false;
            btnedit.Enabled = false;

            // ── Export buttons are always available ──
            btnpdfExport.Enabled = true;
            btn_exportfamily.Enabled = true;

            // ── Cemetery tab filter & action panels ──
            panel7.BackColor   = AppTheme.FilterBar;
            panel7.BorderStyle = BorderStyle.None;
            panel7.Paint += (s, pe) => pe.Graphics.FillRectangle(new SolidBrush(AppTheme.Teal), 0, 0, 4, ((Panel)s).Height);
            panel8.BackColor = AppTheme.ActionBar;
            AppTheme.StyleButtonPrimary(cemeterySearchBtn);
            AppTheme.StyleButtonPrimary(btnOutsideParsihMember);
            StylePanelLabels(panel7);

            // ── Cemetery tab: reset filter button below Outside Parish ──
            {
                var btnCemReset = new Button
                {
                    Text   = "Reset Filters",
                    Size   = new Size(btnOutsideParsihMember.Width, 36),
                    Location = new Point(btnOutsideParsihMember.Left, btnOutsideParsihMember.Bottom + 8),
                    Anchor = btnOutsideParsihMember.Anchor,
                };
                panel8.Height = Math.Max(panel8.Height, btnCemReset.Bottom + 10);
                AppTheme.StyleButtonOutline(btnCemReset);
                AppTheme.SetIcon(btnCemReset, AppTheme.IconClose(14), "Reset Filters", 14);
                btnCemReset.Click += (s, e) =>
                {
                    dtBurialStart.Checked   = false;
                    dtBurialEnd.Checked     = false;
                    dtDeceasedStart.Checked = false;
                    dtDeceasedEnd.Checked   = false;
                    parishCombobox.SelectedIndex = 0;
                    _cemBurialFrom   = null;
                    _cemBurialTo     = null;
                    _cemDeceasedFrom = null;
                    _cemDeceasedTo   = null;
                    _cemIsOurParish  = DBNull.Value;
                    _cemeteryCurrentPage = 1;
                    LoadAllCemeteryData(null);
                };
                panel8.Controls.Add(btnCemReset);
            }

            // ── Subscription tab filter & pagination panels ──
            subscriptionFilterPanel.BackColor = AppTheme.FilterBar;
            subscriptionFilterPanel.Paint += (s, pe) => pe.Graphics.FillRectangle(new SolidBrush(AppTheme.Teal), 0, 0, 4, ((Panel)s).Height);
            subscriptionPaginationPanel.BackColor = AppTheme.ActionBar;
            AppTheme.StyleButtonPrimary(btnSubscriptionSearch);
            AppTheme.StyleButtonOutline(btnSubscriptionPrevious);
            AppTheme.StyleButtonOutline(btnSubscriptionNext);
            StylePanelLabels(subscriptionFilterPanel);
            StylePanelLabels(subscriptionPaginationPanel);

            // ── Subscription tab: reset filter button ──
            {
                var btnSubReset = new Button
                {
                    Text     = "Reset Filters",
                    Size     = new Size(150, 26),
                    Location = new Point(840, 85),
                };
                AppTheme.StyleButtonOutline(btnSubReset);
                AppTheme.SetIcon(btnSubReset, AppTheme.IconClose(14), "Reset", 14);
                btnSubReset.Click += (s, e) =>
                {
                    if (cmbSubscriptionAmbiyam.DataSource == null) return;
                    txtSubscriptionFamilyName.Clear();
                    cmbSubscriptionAmbiyam.SelectedIndex  = 0;
                    cmbSubscriptionType.SelectedIndex     = 0;
                    cmbSubscriptionStatus.SelectedIndex   = 0;
                    cmbSubscriptionYear.SelectedIndex     = 0;
                    _subscriptionCurrentPage = 1;
                    LoadSubscriptions();
                };
                subscriptionFilterPanel.Controls.Add(btnSubReset);
            }
            lblSubscriptionPageInfo.ForeColor = AppTheme.Navy;
            lblSubscriptionPageInfo.Font = AppTheme.BoldSmall;

            // ── Family members sub-grid panel ──
            //panel6.BackColor = AppTheme.OffWhite;
            AppTheme.StyleGrid(familyMembersGrid);

            // ── Header bar + custom tab nav (single panel, always above familytab) ──
            BuildTopBar();

            // ── Dashboard layout: resize charts + stat cards on right ──
            AddDashboardStatCards();
            AdjustDashboardLayout();
            dashboardPage.Resize += (s, e) => AdjustDashboardLayout();

            // ── Icons on all buttons ──
            ApplyIcons();
        }

        private void ApplyIcons()
        {
            int sz = 18;

            // ── Anbiyam tab ──
            AppTheme.SetIcon(btncreate,    AppTheme.IconPlus(sz),                          "Create",     sz);
            AppTheme.SetIcon(btnedit,      AppTheme.IconPencil(sz, AppTheme.Gold),         "Edit",       sz);
            AppTheme.SetIcon(btnpdfExport, AppTheme.IconDownload(sz, AppTheme.Teal),       "Export PDF", sz);
            //AppTheme.SetIcon(searchButton, AppTheme.IconSearch(sz),                        "Search",     sz);

            // ── Family tab ──
            AppTheme.SetIcon(btnFamilyCreate,   AppTheme.IconPlus(sz),                          "Create",        sz);
            AppTheme.SetIcon(btnFamilyEdit,     AppTheme.IconPencil(sz, AppTheme.Gold),         "Edit",          sz);
            AppTheme.SetIcon(btnCemetery,       AppTheme.IconCemetery(sz),                      "Cemetery Info", sz);
            AppTheme.SetIcon(btn_disablefamily, AppTheme.IconDisable(sz),                       "Disable",       sz);
            AppTheme.SetIcon(btn_exportfamily,  AppTheme.IconDownload(sz, AppTheme.Teal),       "Export PDF",    sz);
            AppTheme.SetIcon(searchFamily,      AppTheme.IconSearch(sz),                        "Search",        sz);
            AppTheme.SetIcon(button1,           AppTheme.IconClose(14),                         "Reset Filter",  14);

            // ── Cemetery tab ──
            AppTheme.SetIcon(cemeterySearchBtn,      AppTheme.IconSearch(sz),    "Search",         sz);
            AppTheme.SetIcon(btnOutsideParsihMember, AppTheme.IconAddPerson(sz), "Outside Parish", sz);

            // ── Subscription tab ──
            AppTheme.SetIcon(btnSubscriptionSearch,   AppTheme.IconSearch(sz),  "Search",   sz);
            AppTheme.SetIcon(btnSubscriptionPrevious, AppTheme.IconClose(14),   "Prev",     14);
            AppTheme.SetIcon(btnSubscriptionNext,     AppTheme.IconClose(14),   "Next",     14);
            // Override previous/next with arrow bitmaps drawn inline
            btnSubscriptionPrevious.Image = MakeArrowIcon(sz, left: true);
            btnSubscriptionPrevious.Text  = "  Prev";
            btnSubscriptionNext.Image     = MakeArrowIcon(sz, left: false);
            btnSubscriptionNext.Text      = "  Next";
        }

        private static Bitmap MakeArrowIcon(int size, bool left)
        {
            var bmp = new Bitmap(size, size);
            using (var g = System.Drawing.Graphics.FromImage(bmp))
            using (var pen = new Pen(Color.White, 2f))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                int mx = size / 2, my = size / 2, hw = size / 3;
                if (left)
                {
                    g.DrawLine(pen, mx + hw, my - hw, mx - hw, my);
                    g.DrawLine(pen, mx - hw, my,      mx + hw, my + hw);
                }
                else
                {
                    g.DrawLine(pen, mx - hw, my - hw, mx + hw, my);
                    g.DrawLine(pen, mx + hw, my,      mx - hw, my + hw);
                }
            }
            return bmp;
        }

        private static void StylePanelLabels(Panel panel)
        {
            foreach (Control c in panel.Controls)
            {
                if (c is Label lbl)
                {
                    lbl.ForeColor = AppTheme.Navy;
                    lbl.Font = AppTheme.BoldSmall;
                    lbl.BackColor = Color.Transparent;
                }
            }
        }

        private Button[] _tabNavButtons;

        // Builds a single Dock=Top wrapper that holds the church title row (50px)
        // and the tab navigation row (40px) — one panel, zero z-order ambiguity.
        private void BuildTopBar()
        {
            familytab.SizeMode = TabSizeMode.Fixed;
            familytab.ItemSize = new Size(1, 1);
            familytab.Padding  = new Point(0, 0);

            const int headerH = 50;
            const int navH    = 40;

            var topBar = new Panel
            {
                Height    = headerH + navH,
                Dock      = DockStyle.Top,
                BackColor = AppTheme.Navy,
            };

            // ── Nav row — Dock=Bottom inside topBar (reserves bottom navH pixels) ──
            var navPanel = new Panel
            {
                Height    = navH,
                Dock      = DockStyle.Bottom,
                BackColor = Color.FromArgb(22, 40, 60),
            };

            int count = familytab.TabCount;
            int btnW  = Math.Max(120, this.ClientSize.Width / count);
            _tabNavButtons = new Button[count];
            for (int i = 0; i < count; i++)
            {
                int  idx = i;
                bool sel = (i == 0);
                var navBtn = new Button
                {
                    Text      = familytab.TabPages[i].Text,
                    Width     = btnW,
                    Height    = navH,
                    Location  = new Point(i * btnW, 0),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = sel ? Color.FromArgb(35, 65, 90) : Color.FromArgb(22, 40, 60),
                    ForeColor = sel ? Color.White : Color.FromArgb(160, 200, 215),
                    Font      = new Font("Georgia", 10F, sel ? FontStyle.Bold : FontStyle.Regular),
                    Cursor    = Cursors.Hand,
                    Tag       = idx,
                };
                navBtn.FlatAppearance.BorderSize  = 0;
                navBtn.FlatAppearance.BorderColor = Color.FromArgb(22, 40, 60);
                navBtn.Click += (s, e) => { familytab.SelectedIndex = idx; UpdateTabNav(idx); };
                navBtn.Paint += (s, pe) =>
                {
                    if (familytab.SelectedIndex == (int)((Button)s).Tag)
                    {
                        var b = (Button)s;
                        using (var gold = new SolidBrush(AppTheme.Gold))
                            pe.Graphics.FillRectangle(gold, 0, b.Height - 3, b.Width, 3);
                    }
                };
                _tabNavButtons[i] = navBtn;
                navPanel.Controls.Add(navBtn);
            }

            // ── Window controls — Dock=Right inside header fill panel ──
            // Using Dock avoids Anchor mis-calculation before topBar is sized.
            Color wcNorm = Color.FromArgb(42, 68, 98);
            Color wcBord = Color.FromArgb(80, 120, 160);
            Color wcFg   = Color.FromArgb(210, 228, 245);

            var btnClose = new Button
            {
                Text      = "✕",
                Size      = new Size(44, headerH),
                Location  = new Point(48, 0),
                FlatStyle = FlatStyle.Flat,
                BackColor = wcNorm,
                ForeColor = wcFg,
                Font      = new Font("Segoe UI", 12F, FontStyle.Bold),
                Cursor    = Cursors.Hand,
                TabStop   = false,
            };
            btnClose.FlatAppearance.BorderSize  = 1;
            btnClose.FlatAppearance.BorderColor = wcBord;
            btnClose.MouseEnter += (s, e) => { ((Button)s).BackColor = Color.FromArgb(192, 57, 43); ((Button)s).FlatAppearance.BorderColor = Color.FromArgb(192, 57, 43); };
            btnClose.MouseLeave += (s, e) => { ((Button)s).BackColor = wcNorm; ((Button)s).FlatAppearance.BorderColor = wcBord; };
            btnClose.Click      += (s, e) => this.Close();

            var btnMin = new Button
            {
                Text      = "─",
                Size      = new Size(44, headerH),
                Location  = new Point(4, 0),
                FlatStyle = FlatStyle.Flat,
                BackColor = wcNorm,
                ForeColor = wcFg,
                Font      = new Font("Segoe UI", 13F, FontStyle.Regular),
                Cursor    = Cursors.Hand,
                TabStop   = false,
            };
            btnMin.FlatAppearance.BorderSize  = 1;
            btnMin.FlatAppearance.BorderColor = wcBord;
            btnMin.MouseEnter += (s, e) => { ((Button)s).BackColor = Color.FromArgb(55, 88, 120); ((Button)s).FlatAppearance.BorderColor = Color.FromArgb(100, 150, 200); };
            btnMin.MouseLeave += (s, e) => { ((Button)s).BackColor = wcNorm; ((Button)s).FlatAppearance.BorderColor = wcBord; };
            btnMin.Click      += (s, e) => this.WindowState = FormWindowState.Minimized;

            // ctrlPanel docked right — always 92px from the right edge, no Anchor needed
            var ctrlPanel = new Panel { Width = 92, Dock = DockStyle.Right, BackColor = AppTheme.Navy };
            ctrlPanel.Controls.Add(btnMin);
            ctrlPanel.Controls.Add(btnClose);

            // ── Header fill panel (fills headerH above nav) ──
            var headerFill = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.Navy };

            var titleLbl = new Label
            {
                Text      = "✝  Our Lady of Fatima Church  —  Tambaram, Chennai",
                Font      = new Font("Georgia", 13F, FontStyle.Bold),
                ForeColor = AppTheme.Gold,
                AutoSize  = true,
                Location  = new Point(14, 12),
            };
            var lblUser = new Label
            {
                Text      = $"  {LoggedInUser}  ",
                Font      = AppTheme.BoldSmall,
                ForeColor = Color.White,
                BackColor = AppTheme.Teal,
                AutoSize  = true,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding   = new Padding(6, 3, 6, 3),
                Anchor    = AnchorStyles.Top | AnchorStyles.Right,
                Location  = new Point(this.ClientSize.Width - 92 - 130, 13),
            };
            // Separator at bottom of header fill
            var sep = new Panel { Height = 1, Dock = DockStyle.Bottom, BackColor = Color.FromArgb(60, 255, 255, 255) };

            // Add docked controls first so they reserve space before non-docked ones
            headerFill.Controls.Add(ctrlPanel);  // Dock=Right
            headerFill.Controls.Add(sep);         // Dock=Bottom
            headerFill.Controls.Add(titleLbl);
            headerFill.Controls.Add(lblUser);

            // Drag: wire headerFill and the title label
            void EnableDrag(Control c)
            {
                c.MouseDown += (s, e) =>
                {
                    if (e.Button == MouseButtons.Left) { ReleaseCapture(); SendMessage(Handle, 0xA1, 0x2, 0); }
                };
            }
            EnableDrag(headerFill);
            EnableDrag(titleLbl);

            // Assemble: navPanel (Dock=Bottom) added BEFORE headerFill (Dock=Fill)
            // so WinForms layout reserves the bottom row first, then Fill gets the rest.
            topBar.Controls.Add(navPanel);
            topBar.Controls.Add(headerFill);

            familytab.SelectedIndexChanged += (s, e) => UpdateTabNav(familytab.SelectedIndex);

            this.SuspendLayout();
            this.Controls.Add(topBar);
            topBar.BringToFront();
            this.ResumeLayout(true);
        }

        private void UpdateTabNav(int selectedIndex)
        {
            if (_tabNavButtons == null) return;
            for (int i = 0; i < _tabNavButtons.Length; i++)
            {
                bool sel = (i == selectedIndex);
                _tabNavButtons[i].BackColor = sel ? Color.FromArgb(30, 60, 85) : AppTheme.Navy;
                _tabNavButtons[i].ForeColor = sel ? Color.White : Color.FromArgb(160, 200, 215);
                _tabNavButtons[i].Font = new Font("Georgia", 10F, sel ? FontStyle.Bold : FontStyle.Regular);
                _tabNavButtons[i].Invalidate(); // repaint gold underline
            }
        }

        private void exitMenuItem3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }



        private void AddDashboardStatCards()
        {
            // Query KPIs
            int totalFamilies = 0, totalAnbiyams = 0, paidSubs = 0, cemeteryRecords = 0;
            int unpaidChurch = 0, unpaidCemetery = 0;
            try
            {
                object v;
                v = DatabaseHelper.ExecuteScalarStoredProcedure("sp_GetFamilyTotalCount",
                    new SqlParameter("@anbiyam_id", 0),
                    new SqlParameter("@family_head", DBNull.Value),
                    new SqlParameter("@occupation", DBNull.Value),
                    new SqlParameter("@cemetery_available", DBNull.Value));
                totalFamilies = v != null && v != DBNull.Value ? Convert.ToInt32(v) : 0;

                v = DatabaseHelper.ExecuteScalarStoredProcedure("sp_GetTotalAnbiyamsCount");
                totalAnbiyams = v != null && v != DBNull.Value ? Convert.ToInt32(v) : 0;

                v = DatabaseHelper.ExecuteScalarStoredProcedure("sp_GetSubscriptionTotalCount",
                    new SqlParameter("@familyName", DBNull.Value),
                    new SqlParameter("@anbiyamId", DBNull.Value),
                    new SqlParameter("@subscriptionType", DBNull.Value),
                    new SqlParameter("@status", "Paid"),
                    new SqlParameter("@yearFrom", DateTime.Today.Year),
                    new SqlParameter("@yearTo", DateTime.Today.Year));
                paidSubs = v != null && v != DBNull.Value ? Convert.ToInt32(v) : 0;

                var dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetAllCemeteriesTotalCount",
                    new SqlParameter("@burial_date_from", DBNull.Value),
                    new SqlParameter("@burial_date_to", DBNull.Value),
                    new SqlParameter("@deceased_date_from", DBNull.Value),
                    new SqlParameter("@deceased_date_to", DBNull.Value),
                    new SqlParameter("@IsOurparish", DBNull.Value));
                if (dt.Rows.Count > 0 && dt.Rows[0][0] != DBNull.Value)
                    cemeteryRecords = Convert.ToInt32(dt.Rows[0][0]);

                // Unpaid church subscriptions (Pending + Overdue) for current year
                object vPending = DatabaseHelper.ExecuteScalarStoredProcedure("sp_GetSubscriptionTotalCount",
                    new SqlParameter("@familyName", DBNull.Value),
                    new SqlParameter("@anbiyamId", DBNull.Value),
                    new SqlParameter("@subscriptionType", DBNull.Value),
                    new SqlParameter("@status", "Pending"),
                    new SqlParameter("@yearFrom", DateTime.Today.Year),
                    new SqlParameter("@yearTo", DateTime.Today.Year));
                object vOverdue = DatabaseHelper.ExecuteScalarStoredProcedure("sp_GetSubscriptionTotalCount",
                    new SqlParameter("@familyName", DBNull.Value),
                    new SqlParameter("@anbiyamId", DBNull.Value),
                    new SqlParameter("@subscriptionType", DBNull.Value),
                    new SqlParameter("@status", "Overdue"),
                    new SqlParameter("@yearFrom", DateTime.Today.Year),
                    new SqlParameter("@yearTo", DateTime.Today.Year));
                int pending = vPending != null && vPending != DBNull.Value ? Convert.ToInt32(vPending) : 0;
                int overdue = vOverdue != null && vOverdue != DBNull.Value ? Convert.ToInt32(vOverdue) : 0;
                unpaidChurch = pending + overdue;

                // Unpaid cemetery subscriptions (Pending + Overdue) for current year
                try
                {
                    var dtCemPending = DatabaseHelper.ExecuteStoredProcedure("sp_GetCemeterySubscriptionCount",
                        new SqlParameter("@subscription_year", DateTime.Today.Year),
                        new SqlParameter("@payment_status", "Pending"));
                    var dtCemOverdue = DatabaseHelper.ExecuteStoredProcedure("sp_GetCemeterySubscriptionCount",
                        new SqlParameter("@subscription_year", DateTime.Today.Year),
                        new SqlParameter("@payment_status", "Overdue"));
                    int cemPending = dtCemPending.Rows.Count > 0 && dtCemPending.Rows[0][0] != DBNull.Value ? Convert.ToInt32(dtCemPending.Rows[0][0]) : 0;
                    int cemOverdue = dtCemOverdue.Rows.Count > 0 && dtCemOverdue.Rows[0][0] != DBNull.Value ? Convert.ToInt32(dtCemOverdue.Rows[0][0]) : 0;
                    unpaidCemetery = cemPending + cemOverdue;
                }
                catch { /* SP may not exist yet */ }
            }
            catch { /* KPI load failure is non-fatal */ }

            var cards = new[]
            {
                (totalFamilies.ToString(),   "Total Families",          AppTheme.Teal),
                (totalAnbiyams.ToString(),   "Anbiyam Zones",           AppTheme.Gold),
                (paidSubs.ToString(),        "Subscriptions Paid",      Color.FromArgb(42, 157, 92)),
                (cemeteryRecords.ToString(), "Cemetery Records",        AppTheme.Navy),
                (unpaidChurch.ToString(),    "Unpaid Church Families",  Color.FromArgb(204, 102, 0)),
                (unpaidCemetery.ToString(),  "Unpaid Cemetery Families",Color.FromArgb(132, 32, 41)),
            };

            // Cards stacked vertically on the right side
            int cardWidth = 220, cardHeight = 80, gap = 8;
            int startX = dashboardPage.ClientSize.Width - cardWidth - 12;
            int startY = 6;

            for (int i = 0; i < cards.Length; i++)
            {
                var (num, lbl, accent) = cards[i];
                var capturedAccent = accent;

                var card = new Panel
                {
                    Size     = new Size(cardWidth, cardHeight),
                    Location = new Point(startX, startY + i * (cardHeight + gap)),
                    Anchor   = AnchorStyles.Top | AnchorStyles.Right,
                    BackColor = Color.White,
                };
                card.Paint += (s, e) =>
                {
                    var p = (Panel)s;
                    e.Graphics.FillRectangle(new SolidBrush(capturedAccent), 0, 0, 5, p.Height);
                    e.Graphics.DrawRectangle(new Pen(AppTheme.GridBorder), 0, 0, p.Width - 1, p.Height - 1);
                };

                card.Controls.Add(new Label
                {
                    Text = num,
                    Font = new Font("Georgia", 20F, FontStyle.Bold),
                    ForeColor = AppTheme.Navy,
                    AutoSize = false,
                    Size = new Size(cardWidth - 16, 36),
                    Location = new Point(16, 8),
                    TextAlign = ContentAlignment.MiddleLeft,
                });
                card.Controls.Add(new Label
                {
                    Text = lbl,
                    Font = AppTheme.SmallFont,
                    ForeColor = Color.FromArgb(100, 130, 150),
                    AutoSize = false,
                    Size = new Size(cardWidth - 16, 22),
                    Location = new Point(16, 46),
                    TextAlign = ContentAlignment.MiddleLeft,
                });
                dashboardPage.Controls.Add(card);
                card.BringToFront();
            }
        }

        // Lazily-created zone chart — added programmatically so no designer changes needed
        private System.Windows.Forms.DataVisualization.Charting.Chart _chart4;

        private System.Windows.Forms.DataVisualization.Charting.Chart EnsureZoneChart()
        {
            if (_chart4 != null) return _chart4;

            _chart4 = new System.Windows.Forms.DataVisualization.Charting.Chart
            {
                BackColor           = Color.White,
                AntiAliasing        = System.Windows.Forms.DataVisualization.Charting.AntiAliasingStyles.All,
                BorderlineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.NotSet,
            };
            _chart4.ChartAreas.Add(new System.Windows.Forms.DataVisualization.Charting.ChartArea("ChartArea1"));
            _chart4.Legends.Add(new System.Windows.Forms.DataVisualization.Charting.Legend("Legend1"));
            dashboardPage.Controls.Add(_chart4);
            return _chart4;
        }

        private void AdjustDashboardLayout()
        {
            // Hide old header banner and legacy chart panel
            tableLayoutPanel1.Visible = false;

            int cardColWidth = 244;  // 220 card + 12 px margin each side
            int pad          = 6;
            int h            = dashboardPage.ClientSize.Height - pad * 2;
            if (h < 100) return;     // form not yet sized

            int ageChartH = (int)(h * 0.80);  // Age chart takes 80%
            int zoneChartH = h - ageChartH - pad;  // Zone chart takes 20%

            // chart2 = Gender Distribution — fixed 300 px left column, full height
            chart2.Dock     = DockStyle.None;
            chart2.Size     = new Size(300, h);
            chart2.Location = new Point(pad, pad);

            int tlLeft  = chart2.Right + pad;
            int tlWidth = dashboardPage.ClientSize.Width - tlLeft - cardColWidth - pad;
            if (tlWidth < 60) tlWidth = 60;

            // tableLayoutPanel3 = Age Distribution — 80% of height
            tableLayoutPanel3.Dock     = DockStyle.None;
            tableLayoutPanel3.Location = new Point(tlLeft, pad);
            tableLayoutPanel3.Size     = new Size(tlWidth, ageChartH);

            // chart4 = Zone Families — 20% of height at bottom
            var zc = EnsureZoneChart();
            zc.Location = new Point(tlLeft, tableLayoutPanel3.Bottom + pad);
            zc.Size     = new Size(tlWidth, zoneChartH);

            // Styles
            chart2.BackColor            = Color.White;
            chart2.BorderlineDashStyle  = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.NotSet;
            tableLayoutPanel3.BackColor = Color.White;
        }

        private void LoadOccupation()
        {
            DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_AggregateOccupations");
            // Insert a default "Select" row at the top
            DataRow newRow = dt.NewRow();
            newRow[dt.Columns[0].ColumnName] = "Select";
            dt.Rows.InsertAt(newRow, 0);
            familyOccupationComboxbox.DataSource = dt;
            familyOccupationComboxbox.DisplayMember = dt.Columns[0].ColumnName;
            familyOccupationComboxbox.SelectedIndex = 0; // Ensure "Select" is shown by default
        }

        private void LoadAnbiyam()
        {
            DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetAllAnbiyam");

            // Insert a default "Select" row at the top
            DataRow newRow = dt.NewRow();
            newRow[dt.Columns[0].ColumnName] = "Select";
            newRow[dt.Columns[1].ColumnName] = 200; // or 0 if you prefer
            dt.Rows.InsertAt(newRow, 0);

            //anbiyamCombobox.DataSource = dt;
           // anbiyamCombobox.DisplayMember = dt.Columns[0].ColumnName;
           // anbiyamCombobox.ValueMember = dt.Columns[1].ColumnName;
            //anbiyamCombobox.SelectedIndex = 0; // Ensure "Select" is shown by default


            familyAnbiyamCombobox.DataSource = dt;
            familyAnbiyamCombobox.DisplayMember = dt.Columns[0].ColumnName;
            familyAnbiyamCombobox.ValueMember = dt.Columns[1].ColumnName;
            familyAnbiyamCombobox.SelectedIndex = 0; // Ensure "Select" is shown by default
        }

        public void LoadFamilyBasicDetails(DataTable dt)
        {
            if (dt == null)
            {
                // Get total count for pagination display
                var countParams = new[]
                {
                    new SqlParameter("@anbiyam_id", _familyAnbiyamFilter),
                    new SqlParameter("@family_head", _familyHeadFilter ?? (object)DBNull.Value),
                    new SqlParameter("@occupation",  _familyOccFilter  ?? (object)DBNull.Value),
                    new SqlParameter("@cemetery_available", _familyCemeteryFilter)
                };
                object cnt = DatabaseHelper.ExecuteScalarStoredProcedure("sp_GetFamilyTotalCount", countParams);
                _familyTotalCount = cnt != null && cnt != DBNull.Value ? Convert.ToInt32(cnt) : 0;

                var dataParams = new[]
                {
                    new SqlParameter("@pageNumber", _familyCurrentPage),
                    new SqlParameter("@pageSize",   PageSize),
                    new SqlParameter("@anbiyam_id", _familyAnbiyamFilter),
                    new SqlParameter("@family_head", _familyHeadFilter ?? (object)DBNull.Value),
                    new SqlParameter("@occupation",  _familyOccFilter  ?? (object)DBNull.Value),
                    new SqlParameter("@cemetery_available", _familyCemeteryFilter)
                };
                dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetFamilyBasicDetailsPaged", dataParams);

                if (lblFamilyPageInfo != null)
                {
                    int tp = _familyTotalCount == 0 ? 1 : (int)Math.Ceiling((double)_familyTotalCount / PageSize);
                    lblFamilyPageInfo.Text = $"Page {_familyCurrentPage} of {tp}  ({_familyTotalCount} records)";
                    btnFamilyPrevious.Enabled = _familyCurrentPage > 1;
                    btnFamilyNext.Enabled = _familyCurrentPage < tp;
                }
            }

            familygrid.DataSource = dt;
            familygrid.Columns["FamilyID"].Visible = false;
            familygrid.ColumnHeadersDefaultCellStyle.Font = new Font("Georgia", 12, FontStyle.Bold);
            familygrid.DefaultCellStyle.Font = new Font("Georgia", 11, FontStyle.Regular);
            familygrid.AlternatingRowsDefaultCellStyle.BackColor = Color.LightSlateGray;
            familygrid.BackgroundColor = Color.WhiteSmoke;
            familygrid.DefaultCellStyle.ForeColor = Color.Black;

            if (familygrid.Columns["SubscriptionInfo"] == null)
            {
                DataGridViewButtonColumn subCol = new DataGridViewButtonColumn();
                subCol.Name = "SubscriptionInfo";
                subCol.HeaderText = "";
                subCol.Text = "Sub Info";
                subCol.UseColumnTextForButtonValue = true;
                subCol.Width = 88;
                subCol.DefaultCellStyle.BackColor = AppTheme.Teal;
                subCol.DefaultCellStyle.ForeColor = Color.White;
                familygrid.Columns.Insert(Math.Min(10, familygrid.Columns.Count), subCol);
            }

            // Remove existing delete column if present to avoid duplicates
            if (familygrid.Columns["delete"] != null)
                familygrid.Columns.Remove("delete");

            // Add a button column for delete
            DataGridViewButtonColumn btnCol = new DataGridViewButtonColumn();
            btnCol.Name = "delete";
            btnCol.HeaderText = "";
            btnCol.Text = "Delete";
            btnCol.UseColumnTextForButtonValue = true;
            btnCol.Width = 60;
            btnCol.DefaultCellStyle.BackColor = Color.DarkRed;
            btnCol.DefaultCellStyle.ForeColor = Color.White;
            familygrid.Columns.Insert(Math.Min(10, familygrid.Columns.Count), btnCol);
        }


        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (((System.Windows.Forms.TabControl)sender).SelectedTab.Name == "dashboardPage")
            {
                LoadAgeGroupChart();
                LoadGenderGroupChart();
                LoadZoneFamilyChart();
            }
            else if (((System.Windows.Forms.TabControl)sender).SelectedTab.Name == "anbiyamPage")
            {
                LoadAnbiyam();
                LoadAnbiyamGrid(null);
                ShowAnbiyamOnMap(anbiyamAddress);
            }
            else if (((System.Windows.Forms.TabControl)sender).SelectedTab.Name == "familyPage")
            {
                LoadOccupation();
                LoadFamilyBasicDetails(null);
            }
            else if (((System.Windows.Forms.TabControl)sender).SelectedTab.Name == "subscriptionPage")
            {
                InitSubscriptionTab();
            }
            else
            {
                LoadAllCemeteryData(null);
            }
        }

        // ============================================================
        // Pagination – shared page size and per-tab state
        // ============================================================
        private const int PageSize = 50;

        // Families tab
        private int _familyCurrentPage = 1;
        private int _familyTotalCount = 0;
        private System.Windows.Forms.Button btnFamilyPrevious, btnFamilyNext;
        private System.Windows.Forms.Label lblFamilyPageInfo;
        private int _familyAnbiyamFilter = 0;
        private string _familyHeadFilter = null, _familyOccFilter = null;
        private object _familyCemeteryFilter = DBNull.Value;

        // Anbiyams tab
        private int _anbiyamCurrentPage = 1;
        private int _anbiyamTotalCount = 0;
        // Anbiyam pagination buttons removed per UX improvements
        private int? _anbiyamIdFilter = null;
        private string _anbiyamCoordFilter = null;

        // Cemetery tab
        private int _cemeteryCurrentPage = 1;
        private int _cemeteryTotalCount = 0;
        private System.Windows.Forms.Button btnCemeteryPrevious, btnCemeteryNext;
        private System.Windows.Forms.Label lblCemeteryPageInfo;
        private DateTime? _cemBurialFrom, _cemBurialTo, _cemDeceasedFrom, _cemDeceasedTo;
        private object _cemIsOurParish = DBNull.Value;

        // Subscription tab
        private int _subscriptionCurrentPage = 1;
        private const int SubscriptionPageSize = 50;
        private int _subscriptionTotalCount = 0;

        private void InitSubscriptionTab()
        {
            // Populate filter dropdowns once; guard against double-init
            if (cmbSubscriptionAmbiyam.DataSource == null)
            {
                DataTable dtAnbiyam = DatabaseHelper.ExecuteStoredProcedure("sp_GetAllAnbiyam");
                DataRow allRow = dtAnbiyam.NewRow();
                allRow[dtAnbiyam.Columns[0].ColumnName] = "All";
                allRow[dtAnbiyam.Columns[1].ColumnName] = 0;  // 0 = no filter sentinel
                dtAnbiyam.Rows.InsertAt(allRow, 0);
                cmbSubscriptionAmbiyam.DataSource = dtAnbiyam;
                cmbSubscriptionAmbiyam.DisplayMember = dtAnbiyam.Columns[0].ColumnName;
                cmbSubscriptionAmbiyam.ValueMember = dtAnbiyam.Columns[1].ColumnName;
                cmbSubscriptionAmbiyam.SelectedIndex = 0;
            }

            if (cmbSubscriptionType.Items.Count == 0)
            {
                cmbSubscriptionType.Items.AddRange(new object[] { "All", "Family", "Cemetery", "Outside Parish Cemetery" });
                cmbSubscriptionType.SelectedIndex = 0;
            }

            if (cmbSubscriptionStatus.Items.Count == 0)
            {
                cmbSubscriptionStatus.Items.AddRange(new object[] { "All", "Paid", "Pending" });
                cmbSubscriptionStatus.SelectedIndex = 0;
            }

            if (cmbSubscriptionYear.Items.Count == 0)
            {
                cmbSubscriptionYear.Items.Add("All");
                for (int y = DateTime.Now.Year; y >= 2023; y--)
                    cmbSubscriptionYear.Items.Add(y.ToString());
                cmbSubscriptionYear.SelectedIndex = 0;
            }

            // Wire up events once
            btnSubscriptionSearch.Click -= btnSubscriptionSearch_Click;
            btnSubscriptionSearch.Click += btnSubscriptionSearch_Click;
            btnSubscriptionPrevious.Click -= btnSubscriptionPrevious_Click;
            btnSubscriptionPrevious.Click += btnSubscriptionPrevious_Click;
            btnSubscriptionNext.Click -= btnSubscriptionNext_Click;
            btnSubscriptionNext.Click += btnSubscriptionNext_Click;
            subscriptionGrid.CellClick -= subscriptionGrid_CellClick;
            subscriptionGrid.CellClick += subscriptionGrid_CellClick;
            subscriptionGrid.CellFormatting -= subscriptionGrid_CellFormatting;
            subscriptionGrid.CellFormatting += subscriptionGrid_CellFormatting;
            AppTheme.StyleGrid(subscriptionGrid);

            _subscriptionCurrentPage = 1;
            LoadSubscriptions();
        }

        private void LoadSubscriptions()
        {
            var filters = GetSubscriptionFilters();
            var countParams = new[]
            {
                new SqlParameter("@familyName",        (object)filters.FamilyName       ?? DBNull.Value),
                new SqlParameter("@anbiyamId",         (object)filters.AnbiyamId        ?? DBNull.Value),
                new SqlParameter("@subscriptionType",  (object)filters.SubscriptionType ?? DBNull.Value),
                new SqlParameter("@status",            (object)filters.Status           ?? DBNull.Value),
                new SqlParameter("@yearFrom",          (object)filters.YearFrom         ?? DBNull.Value),
                new SqlParameter("@yearTo",            (object)filters.YearTo           ?? DBNull.Value)
            };

            object countResult = DatabaseHelper.ExecuteScalarStoredProcedure("sp_GetSubscriptionTotalCount", countParams);
            _subscriptionTotalCount = countResult != null && countResult != DBNull.Value ? Convert.ToInt32(countResult) : 0;

            var dataParams = new[]
            {
                new SqlParameter("@pageNumber",        _subscriptionCurrentPage),
                new SqlParameter("@pageSize",          SubscriptionPageSize),
                new SqlParameter("@familyName",        (object)filters.FamilyName       ?? DBNull.Value),
                new SqlParameter("@anbiyamId",         (object)filters.AnbiyamId        ?? DBNull.Value),
                new SqlParameter("@subscriptionType",  (object)filters.SubscriptionType ?? DBNull.Value),
                new SqlParameter("@status",            (object)filters.Status           ?? DBNull.Value),
                new SqlParameter("@yearFrom",          (object)filters.YearFrom         ?? DBNull.Value),
                new SqlParameter("@yearTo",            (object)filters.YearTo           ?? DBNull.Value)
            };

            DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetSubscriptionsFiltered", dataParams);

            subscriptionGrid.DataSource = dt;
            if (subscriptionGrid.Columns.Contains("SubscriptionId"))
                subscriptionGrid.Columns["SubscriptionId"].Visible = false;

            UpdateSubscriptionPaginationControls();
        }

        private (string FamilyName, int? AnbiyamId, string SubscriptionType, string Status, int? YearFrom, int? YearTo) GetSubscriptionFilters()
        {
            string familyName = string.IsNullOrWhiteSpace(txtSubscriptionFamilyName.Text) ? null : txtSubscriptionFamilyName.Text.Trim();

            int? anbiyamId = null;
            if (cmbSubscriptionAmbiyam.SelectedIndex > 0 && cmbSubscriptionAmbiyam.SelectedValue != null)
            {
                int raw = Convert.ToInt32(cmbSubscriptionAmbiyam.SelectedValue);
                if (raw > 0) anbiyamId = raw;
            }

            string subType = cmbSubscriptionType.SelectedIndex > 0 ? cmbSubscriptionType.SelectedItem?.ToString() : null;
            string status  = cmbSubscriptionStatus.SelectedIndex > 0 ? cmbSubscriptionStatus.SelectedItem?.ToString() : null;

            int? yearFrom = null, yearTo = null;
            if (cmbSubscriptionYear.SelectedIndex > 0 && int.TryParse(cmbSubscriptionYear.SelectedItem?.ToString(), out int y))
            {
                yearFrom = y;
                yearTo   = y;
            }

            return (familyName, anbiyamId, subType, status, yearFrom, yearTo);
        }

        private void UpdateSubscriptionPaginationControls()
        {
            int totalPages = _subscriptionTotalCount == 0 ? 1 : (int)Math.Ceiling((double)_subscriptionTotalCount / SubscriptionPageSize);
            lblSubscriptionPageInfo.Text = $"Page {_subscriptionCurrentPage} of {totalPages}  ({_subscriptionTotalCount} records)";
            btnSubscriptionPrevious.Enabled = _subscriptionCurrentPage > 1;
            btnSubscriptionNext.Enabled = _subscriptionCurrentPage < totalPages;
        }

        private void btnSubscriptionSearch_Click(object sender, EventArgs e)
        {
            _subscriptionCurrentPage = 1;
            LoadSubscriptions();
        }

        private void btnSubscriptionPrevious_Click(object sender, EventArgs e)
        {
            if (_subscriptionCurrentPage > 1)
            {
                _subscriptionCurrentPage--;
                LoadSubscriptions();
            }
        }

        private void btnSubscriptionNext_Click(object sender, EventArgs e)
        {
            int totalPages = (int)Math.Ceiling((double)_subscriptionTotalCount / SubscriptionPageSize);
            if (_subscriptionCurrentPage < totalPages)
            {
                _subscriptionCurrentPage++;
                LoadSubscriptions();
            }
        }

        private void subscriptionGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || subscriptionGrid.Rows[e.RowIndex].Cells["SubscriptionId"] == null) return;

            string subscriptionId = subscriptionGrid.Rows[e.RowIndex].Cells["SubscriptionId"].Value?.ToString();
            if (string.IsNullOrEmpty(subscriptionId)) return;

            // F_ prefix = family subscription, C_ prefix = cemetery subscription
            if (subscriptionId.StartsWith("F_"))
            {
                // Extract family_id from the FamilyCode column, then open SubscriptionPopup
                if (subscriptionGrid.Rows[e.RowIndex].Cells["FamilyCode"].Value == null) return;
                string familyCode = subscriptionGrid.Rows[e.RowIndex].Cells["FamilyCode"].Value.ToString();
                DataTable dtFamily = DatabaseHelper.ExecuteStoredProcedure("sp_GetFamilyByCode",
                    new SqlParameter("@family_code", familyCode));
                if (dtFamily.Rows.Count == 0) return;
                int familyId = Convert.ToInt32(dtFamily.Rows[0]["family_id"]);
                using (var popup = new SubscriptionPopup(familyId))
                {
                    popup.StartPosition = FormStartPosition.CenterParent;
                    popup.ShowDialog(this);
                }
                LoadSubscriptions();
            }
            else if (subscriptionId.StartsWith("C_"))
            {
                if (subscriptionGrid.Rows[e.RowIndex].Cells["FamilyCode"].Value == null) return;
                string familyCode = subscriptionGrid.Rows[e.RowIndex].Cells["FamilyCode"].Value.ToString();
                DataTable dtFamily = DatabaseHelper.ExecuteStoredProcedure("sp_GetFamilyByCode",
                    new SqlParameter("@family_code", familyCode));
                if (dtFamily.Rows.Count == 0) return;
                int familyId = Convert.ToInt32(dtFamily.Rows[0]["family_id"]);
                using (var popup = new CemeterySubscriptionPopup(familyId))
                {
                    popup.StartPosition = FormStartPosition.CenterParent;
                    popup.ShowDialog(this);
                }
                LoadSubscriptions();
            }
            else if (subscriptionId.StartsWith("NP_"))
            {
                // Outside-parish cemetery subscription
                int npSubId = Convert.ToInt32(subscriptionId.Substring(3));
                // Derive nonparish_cemetery_id from the subscription record
                DataTable dtNp = DatabaseHelper.ExecuteStoredProcedure("sp_GetNonParishSubById",
                    new SqlParameter("@subscription_id", npSubId));
                int cemeteryId = dtNp.Rows.Count > 0 ? Convert.ToInt32(dtNp.Rows[0]["nonparish_cemetery_id"]) : 0;
                if (cemeteryId == 0) return;
                using (var popup = new NonParishCemeterySubscriptionPopup(cemeteryId))
                {
                    popup.StartPosition = FormStartPosition.CenterParent;
                    popup.ShowDialog(this);
                }
                LoadSubscriptions();
            }
        }

        private void subscriptionGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var grid = (DataGridView)sender;

            // Status column badge
            if (grid.Columns.Contains("Status") && e.ColumnIndex == grid.Columns["Status"].Index)
            {
                string status = "";
                if (grid.Rows[e.RowIndex].DataBoundItem is DataRowView drv2)
                    status = drv2["Status"]?.ToString() ?? "";
                else
                    status = e.Value?.ToString() ?? "";
                ApplyStatusBadge(e, status);
                return;
            }

            // SubscriptionType column badge
            if (!grid.Columns.Contains("SubscriptionType")) return;
            if (e.ColumnIndex != grid.Columns["SubscriptionType"].Index) return;

            string subType = "";
            if (grid.Rows[e.RowIndex].DataBoundItem is DataRowView drv)
                subType = drv["SubscriptionType"]?.ToString() ?? "";
            else
                subType = e.Value?.ToString() ?? "";

            ApplySubTypeBadge(e, subType);
        }

        private static void ApplySubTypeBadge(DataGridViewCellFormattingEventArgs e, string subType)
        {
            if (subType == "Family")
            {
                e.CellStyle.BackColor = Color.FromArgb(215, 230, 250);
                e.CellStyle.ForeColor = Color.FromArgb(20,  60, 120);
                e.CellStyle.Font      = AppTheme.BoldSmall;
                e.Value               = "♦  Family";   // ♦ Family
                e.FormattingApplied   = true;
            }
            else if (subType == "Cemetery")
            {
                e.CellStyle.BackColor = Color.FromArgb(215, 245, 225);
                e.CellStyle.ForeColor = Color.FromArgb(25,  85,  50);
                e.CellStyle.Font      = AppTheme.BoldSmall;
                e.Value               = "✝  Cemetery";
                e.FormattingApplied   = true;
            }
            else if (subType == "Outside Parish Cemetery")
            {
                e.CellStyle.BackColor = Color.FromArgb(255, 240, 200);
                e.CellStyle.ForeColor = Color.FromArgb(120, 60,  0);
                e.CellStyle.Font      = AppTheme.BoldSmall;
                e.Value               = "⛪  Outside Parish";
                e.FormattingApplied   = true;
            }
        }

        private static void ApplyStatusBadge(DataGridViewCellFormattingEventArgs e, string status)
        {
            switch (status)
            {
                case "Paid":
                    e.CellStyle.BackColor = AppTheme.PaidBg;
                    e.CellStyle.ForeColor = AppTheme.PaidFg;
                    e.CellStyle.Font      = AppTheme.BoldSmall;
                    e.Value               = "✔  Paid";
                    e.FormattingApplied   = true;
                    break;
                case "Pending":
                    e.CellStyle.BackColor = AppTheme.PendingBg;
                    e.CellStyle.ForeColor = AppTheme.PendingFg;
                    e.CellStyle.Font      = AppTheme.BoldSmall;
                    e.Value               = "⏳  Pending";
                    e.FormattingApplied   = true;
                    break;
                case "Overdue":
                    e.CellStyle.BackColor = AppTheme.OverdueBg;
                    e.CellStyle.ForeColor = AppTheme.OverdueFg;
                    e.CellStyle.Font      = AppTheme.BoldSmall;
                    e.Value               = "⚠  Overdue";
                    e.FormattingApplied   = true;
                    break;
            }
        }

        private void cemeteryGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var grid = (DataGridView)sender;
            string colName = grid.Columns.Contains("SubscriptionStatus") ? "SubscriptionStatus"
                           : grid.Columns.Contains("Status")             ? "Status"
                           : null;
            if (colName == null || e.ColumnIndex != grid.Columns[colName].Index) return;

            string status = "";
            if (grid.Rows[e.RowIndex].DataBoundItem is DataRowView drv)
                status = drv[colName]?.ToString() ?? "";
            else
                status = e.Value?.ToString() ?? "";

            ApplyStatusBadge(e, status);
        }

        private void LoadZoneFamilyChart()
        {
            DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetFamiliesByZone");

            var chart = EnsureZoneChart();
            chart.Series.Clear();
            chart.Titles.Clear();
            chart.Titles.Add("Families per Zone");
            chart.Titles[0].Font      = AppTheme.HeaderFont;
            chart.Titles[0].ForeColor = AppTheme.Navy;

            chart.ChartAreas[0].BackColor                    = Color.White;
            chart.ChartAreas[0].BorderColor                  = AppTheme.GridBorder;
            chart.ChartAreas[0].BorderDashStyle              = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Solid;
            chart.ChartAreas[0].BorderWidth                  = 1;
            chart.ChartAreas[0].AxisX.LabelStyle.Font        = AppTheme.SmallFont;
            chart.ChartAreas[0].AxisY.LabelStyle.Font        = AppTheme.SmallFont;
            chart.ChartAreas[0].AxisX.MajorGrid.LineColor    = AppTheme.GridBorder;
            chart.ChartAreas[0].AxisY.MajorGrid.LineColor    = AppTheme.GridBorder;
            chart.ChartAreas[0].AxisX.Title                  = "Zone";
            chart.ChartAreas[0].AxisY.Title                  = "Families";
            chart.ChartAreas[0].AxisY.Minimum                = 0;
            chart.ChartAreas[0].AxisX.LabelStyle.Angle       = -30;

            chart.Legends.Clear();

            var series = chart.Series.Add("Families per Zone");
            series.ChartType           = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;
            series.Font                = new Font("Georgia", 10, FontStyle.Bold);
            series.IsValueShownAsLabel = true;
            series.LabelForeColor      = AppTheme.Navy;
            series.IsVisibleInLegend   = false;
            series.BorderColor         = Color.White;
            series.BorderWidth         = 2;

            Color[] zoneColors = AppTheme.ChartPalette;
            int colorIdx = 0;

            foreach (DataRow row in dt.Rows)
            {
                string zone  = row["ZoneName"].ToString();
                int    count = Convert.ToInt32(row["FamilyCount"]);
                int    pi    = series.Points.AddXY(zone, count);
                series.Points[pi].Color      = zoneColors[colorIdx % zoneColors.Length];
                series.Points[pi].Label      = count.ToString();
                series.Points[pi].ToolTip    = $"{zone}: {count} families";
                colorIdx++;
            }
        }

        private void LoadAgeGroupChart()
        {
            DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetAgeGroupChartData");

            chart3.Series.Clear();
            chart3.Titles.Clear(); // Clear any previous titles
            chart3.Titles.Add("Age Group Distribution");
            chart3.Titles[0].Font = AppTheme.HeaderFont;
            chart3.Titles[0].ForeColor = AppTheme.Navy;

            chart3.ChartAreas[0].AxisX.Title = "Age Group";
            chart3.ChartAreas[0].AxisY.Title = "Members";

            chart3.ChartAreas[0].BackColor = Color.White;
            chart3.ChartAreas[0].BorderColor = AppTheme.GridBorder;
            chart3.ChartAreas[0].BorderDashStyle = ChartDashStyle.Solid;
            chart3.ChartAreas[0].BorderWidth = 1;
            chart3.ChartAreas[0].ShadowColor = Color.Transparent;
            chart3.ChartAreas[0].AxisX.LabelStyle.Font = AppTheme.SmallFont;
            chart3.ChartAreas[0].AxisY.LabelStyle.Font = AppTheme.SmallFont;
            chart3.ChartAreas[0].AxisX.MajorGrid.LineColor = AppTheme.GridBorder;
            chart3.ChartAreas[0].AxisY.MajorGrid.LineColor = AppTheme.GridBorder;
            chart3.BackColor = Color.White;
            chart3.AntiAliasing = AntiAliasingStyles.All;
            chart3.BorderlineDashStyle = ChartDashStyle.NotSet;
            chart3.BorderlineColor = Color.Transparent;

            chart3.Legends.Clear();
            var legend = chart3.Legends.Add("AgeGroups");
            legend.Docking = Docking.Bottom;
            legend.Font = AppTheme.SmallFont;
            legend.BackColor = Color.Transparent;
            legend.BorderColor = AppTheme.GridBorder;
            legend.BorderWidth = 1;

            // Pie chart series
            var series = chart3.Series.Add("Members Age Group1");
            series.ChartType = SeriesChartType.Pie;
            series["PieDrawingStyle"] = "SoftEdge";
            series["PieLabelStyle"] = "Outside";
            series.Font = new Font("Georgia", 11, FontStyle.Bold);
            series.BorderColor = Color.White;
            series.BorderWidth = 2;
            series.IsValueShownAsLabel = false;

            Color[] pieColors = AppTheme.ChartPalette;

            int colorIndex = 0;
            int totalCount = dt.AsEnumerable().Sum(r => r.Field<int>("MemberCount"));

            foreach (DataRow row in dt.Rows)
            {
                string ageGroup = row["AgeGroup"].ToString();
                string ageGroupWithDesc = row["AgeGroupWithDesc"].ToString();
                int count = Convert.ToInt32(row["MemberCount"]);
                int pointIndex = series.Points.AddXY(ageGroup, count);

                // Set color for each slice
                series.Points[pointIndex].Color = pieColors[colorIndex % pieColors.Length];

                // Label: value and percentage, formatted
                double percent = totalCount > 0 ? (double)count / totalCount : 0;
                series.Points[pointIndex].Label = $"{ageGroupWithDesc}\n{count} ({percent:P1})";
                series.Points[pointIndex].LegendText = ageGroupWithDesc;

                // Tooltip for interactivity
                series.Points[pointIndex].ToolTip = $"{ageGroupWithDesc}: {count} members ({percent:P1})";

                colorIndex++;
            }

            // Explode largest slice for emphasis
            if (series.Points.Count > 0)
            {
                int maxIndex = series.Points.IndexOf(series.Points.OrderByDescending(p => p.YValues[0]).First());
                //series.Points[maxIndex].Exploded = true;
            }

            // Remove border around the chart control
            chart3.BorderlineDashStyle = ChartDashStyle.Solid;
            chart3.BorderlineColor = Color.Transparent;
        }

        private void LoadGenderGroupChart()
        {
            DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GenderData");

            chart2.Series.Clear();
            chart2.Titles.Clear(); // Clear any previous titles
            chart2.Titles.Add("Gender Distribution");
            chart2.Titles[0].Font = AppTheme.HeaderFont;
            chart2.Titles[0].ForeColor = AppTheme.Navy;

            chart2.ChartAreas[0].BackColor = Color.White;
            chart2.ChartAreas[0].BorderColor = AppTheme.GridBorder;
            chart2.ChartAreas[0].BorderDashStyle = ChartDashStyle.Solid;
            chart2.ChartAreas[0].BorderWidth = 1;
            chart2.ChartAreas[0].ShadowColor = Color.Transparent;
            chart2.ChartAreas[0].AxisX.LabelStyle.Font = AppTheme.SmallFont;
            chart2.ChartAreas[0].AxisY.LabelStyle.Font = AppTheme.SmallFont;
            chart2.ChartAreas[0].AxisX.MajorGrid.LineColor = AppTheme.GridBorder;
            chart2.ChartAreas[0].AxisY.MajorGrid.LineColor = AppTheme.GridBorder;
            chart2.BackColor = Color.White;
            chart2.AntiAliasing = AntiAliasingStyles.All;
            chart2.BorderlineDashStyle = ChartDashStyle.NotSet;
            chart2.BorderlineColor = Color.Transparent;

            chart2.Legends.Clear();
            var legend = chart2.Legends.Add("GenderGroups");
            legend.Docking = Docking.Bottom;
            legend.Font = AppTheme.SmallFont;
            legend.BackColor = Color.Transparent;
            legend.BorderColor = AppTheme.GridBorder;
            legend.BorderWidth = 1;

            // Column chart with custom colors and value labels
            var series = chart2.Series.Add("Gender Count");
            series.ChartType = SeriesChartType.Column;
            series.Font = new Font("Georgia", 11, FontStyle.Bold);
            series.IsValueShownAsLabel = true;
            series.LabelForeColor = Color.Black;
            series.IsVisibleInLegend = false;
            series.BorderColor = Color.White;
            series.BorderWidth = 2;

            Color[] columnColors = { AppTheme.MaleColor, AppTheme.FemaleColor };


            // Add Male and Female counts
            if (dt.Rows.Count > 0)
            {
                int maleCount = 0;
                int femaleCount = 0;

                object maleValue = dt.Rows[0]["MaleCount"];
                if (maleValue != DBNull.Value)
                {
                    maleCount = Convert.ToInt32(maleValue);
                }
                object femaleValue = dt.Rows[0]["FemaleCount"];
                if (femaleValue != DBNull.Value)
                {
                    femaleCount = Convert.ToInt32(femaleValue);
                }

                int pointIndexMale = series.Points.AddXY("Male", maleCount);
                series.Points[pointIndexMale].Color = columnColors[0];
                series.Points[pointIndexMale].Label = $"{maleCount}";
                series.Points[pointIndexMale].LegendText = "Male";
                series.Points[pointIndexMale].ToolTip = $"Male: {maleCount}";

                int pointIndexFemale = series.Points.AddXY("Female", femaleCount);
                series.Points[pointIndexFemale].Color = columnColors[1];
                series.Points[pointIndexFemale].Label = $"{femaleCount}";
                series.Points[pointIndexFemale].LegendText = "Female";
                series.Points[pointIndexFemale].ToolTip = $"Female: {femaleCount}";
            }

        }

        private void ShowPopup(string action)
        {
            if (action == "create")
            {
                using (var popup = new AnbiyamPopup())
                {
                    popup.ShowDialog(); // Shows as a modal dialog
                }
                LoadAnbiyamGrid(null);
            }
            else if (action == "edit")
            {
                if (anbiyamGrid.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Please select an Anbiyam to edit.");
                    return;
                }
                var selectedId = anbiyamGrid.SelectedRows[0];
                if (selectedId == null || selectedId.Cells["anbiyam_id"].Value == null || selectedId.Cells["anbiyam_id"].Value == "")
                {
                    MessageBox.Show("Please select an Anbiyam to edit.");
                    return;
                }
                using (var popup = new AnbiyamPopup((int)selectedId.Cells["anbiyam_id"].Value))
                {
                    popup.ShowDialog(); // Shows as a modal dialog
                }
                LoadAnbiyamGrid(null);
            }
            else
            {
                MessageBox.Show("Invalid action specified.");
                return;
            }
        }

        private void btncreate_Click(object sender, EventArgs e)
        {
            ShowPopup("create");

        }

        private void btnedit_Click(object sender, EventArgs e)
        {
            ShowPopup("edit");
        }

        private void LoadAnbiyamGrid(DataTable dt)
        {
            if (dt == null)
            {
                // Load all anbiyams without pagination
                var dataParams = new[]
                {
                    new SqlParameter("@pageNumber",       1),
                    new SqlParameter("@pageSize",         1000),
                    new SqlParameter("@anbiyam_id",       (object)_anbiyamIdFilter    ?? DBNull.Value),
                    new SqlParameter("@coordinator_name", (object)_anbiyamCoordFilter ?? DBNull.Value)
                };
                dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetAnbiyamGridPaged", dataParams);
            }

            anbiyamGrid.DataSource = dt;
            anbiyamGrid.Columns["anbiyam_id"].Visible = false;
            anbiyamGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Georgia", 12, FontStyle.Bold);
            anbiyamGrid.DefaultCellStyle.Font = new Font("Georgia", 11, FontStyle.Regular);
            anbiyamGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.LightSlateGray;
            anbiyamGrid.BackgroundColor = Color.WhiteSmoke;
            anbiyamGrid.DefaultCellStyle.ForeColor = Color.Black;

            // Remove existing delete column if present to avoid duplicates
            if (anbiyamGrid.Columns["delete"] != null)
                anbiyamGrid.Columns.Remove("delete");

            // Add a button column for delete
            DataGridViewButtonColumn btnCol = new DataGridViewButtonColumn();
            btnCol.Name = "delete";
            btnCol.HeaderText = "";
            btnCol.Text = "Delete";
            btnCol.UseColumnTextForButtonValue = true;
            btnCol.Width = 60;
            btnCol.DefaultCellStyle.BackColor = Color.DarkRed;
            btnCol.DefaultCellStyle.ForeColor = Color.White;
            anbiyamGrid.Columns.Insert(Math.Min(10, anbiyamGrid.Columns.Count), btnCol);
        }

        private void familygrid_SelectionChanged(object sender, EventArgs e)
        {
            bool hasSelection = familygrid.SelectedRows.Count == 1;
            if (hasSelection)
            {
                var selectedRow = familygrid.SelectedRows[0];
                int familyId = GetFamilyIdFromSelectedRow(selectedRow);
                familyIDInContext = familyId;
                LoadFamilyMembersForGrid(familyId);
            }
            else
            {
                familyIDInContext = 0;
            }

            if (LoggedInUser != "GUEST")
            {
                btn_disablefamily.Enabled = hasSelection;
                btnFamilyEdit.Enabled = hasSelection;
                btnCemetery.Enabled = hasSelection;
            }
        }

        private void anbiyamGrid_SelectionChanged(object sender, EventArgs e)
        {
            if (LoggedInUser != "GUEST")
                btnedit.Enabled = anbiyamGrid.SelectedRows.Count == 1;
        }

        private void anbiyamGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (e.RowIndex >= 0 && anbiyamGrid.Columns[e.ColumnIndex].Name == "delete")
            {
                if (LoggedInUser != "GUEST")
                {
                    if (ThemedDialog.Confirm("Are you sure you want to delete this Anbiyam?", "Delete Anbiyam", this))
                    {
                        int id = Convert.ToInt32(anbiyamGrid.Rows[e.RowIndex].Cells["anbiyam_id"].Value);
                        try
                        {
                            DatabaseHelper.ExecuteStoredProcedure("sp_DeleteAnbiyam", new SqlParameter("@anbiyamID", id));
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message != null && ex.Message.Contains("Cannot delete"))
                                ThemedDialog.Warn("This Anbiyam cannot be deleted — families are linked to it.", "Cannot Delete", this);
                            else
                                ThemedDialog.Error(ex.InnerException?.Message ?? ex.Message, "Error", this);
                        }
                        LoadAnbiyamGrid(null);
                    }
                }
            }
        }

        private int GetFamilyIdFromSelectedRow(DataGridViewRow row)
        {
            // If you have family_id as a hidden column:
            return Convert.ToInt32(row.Cells["FamilyID"].Value);
        }

        private void LoadFamilyMembersForGrid(int familyId)
        {
            var param = new SqlParameter("@family_id", familyId);
            DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetFamilyMembersByFamilyId", param);
            if (dt.Rows.Count > 0)
            {
                familyMembersGrid.DataSource = dt;
                familyMembersGrid.Columns["memberID"].Visible = false; // Hide the ID column if needed
                familyMembersGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Georgia", 11, FontStyle.Bold);
                familyMembersGrid.DefaultCellStyle.Font = new Font("Georgia", 11, FontStyle.Regular);
                familyMembersGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.LightSlateGray;
                familyMembersGrid.BackgroundColor = Color.WhiteSmoke;
                familyMembersGrid.DefaultCellStyle.ForeColor = Color.Black;

            }
            else
            {
                familyMembersGrid.DataSource = null;
            }
        }

        private void btnFamilyCreate_Click(object sender, EventArgs e)
        {
            int familyID = 0;
            using (var popup = new FamilyPopup(familyID))
            {
                popup.ShowDialog();
                // After closing, reload family basic details
                LoadFamilyBasicDetails(null);
            }
        }

        private void btnFamilyEdit_Click(object sender, EventArgs e)
        {
            if (familyIDInContext <= 0)
            {
                MessageBox.Show("Please select a family to edit.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var popup = new FamilyPopup(familyIDInContext))
            {
                popup.ShowDialog();
                // After closing, reload family basic details
                LoadFamilyBasicDetails(null);
            }
        }

        private void familygrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            string colName = familygrid.Columns[e.ColumnIndex].Name;

            if (colName == "SubscriptionInfo")
            {
                int familyId = Convert.ToInt32(familygrid.Rows[e.RowIndex].Cells["FamilyID"].Value);
                using (var popup = new SubscriptionPopup(familyId))
                {
                    popup.StartPosition = FormStartPosition.CenterParent;
                    popup.ShowDialog(this);
                }
                LoadFamilyBasicDetails(null);
                return;
            }

            if (colName == "delete")
            {
                if (LoggedInUser != "GUEST")
                {
                    if (ThemedDialog.Confirm("Are you sure you want to delete this family?", "Delete Family", this))
                    {
                        int id = Convert.ToInt32(familygrid.Rows[e.RowIndex].Cells["FamilyID"].Value);
                        try
                        {
                            DatabaseHelper.ExecuteStoredProcedure("sp_DeleteFamily", new SqlParameter("@familyID", id));
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message != null && ex.Message.Contains("Cannot delete"))
                                ThemedDialog.Warn("This family cannot be deleted.", "Cannot Delete", this);
                            else
                                ThemedDialog.Error(ex.Message, "Error", this);
                        }
                        LoadFamilyBasicDetails(null);
                    }
                }
            }
        }


        private void cemeteryGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (cemeteryGridView.Columns[e.ColumnIndex].Name != "SubscriptionInfo") return;

            if (cemeteryGridView.Columns["FamilyId"] == null)
            {
                ThemedDialog.Warn("Please run the updated stored procedure (sp_GetAllCemeteriesPaged) against the database first.", "DB Update Required", this);
                return;
            }

            var familyIdCell  = cemeteryGridView.Rows[e.RowIndex].Cells["FamilyId"];
            var cemeteryIdCell = cemeteryGridView.Rows[e.RowIndex].Cells["cemeteryid"];

            bool isParish = familyIdCell != null && familyIdCell.Value != null && familyIdCell.Value != DBNull.Value;

            if (isParish)
            {
                int familyId = Convert.ToInt32(familyIdCell.Value);
                using (var popup = new CemeterySubscriptionPopup(familyId))
                {
                    popup.StartPosition = FormStartPosition.CenterParent;
                    popup.ShowDialog(this);
                }
            }
            else if (cemeteryIdCell != null && cemeteryIdCell.Value != null && cemeteryIdCell.Value != DBNull.Value)
            {
                // Outside-parish cemetery subscription
                int npCemeteryId = Convert.ToInt32(cemeteryIdCell.Value);
                using (var popup = new NonParishCemeterySubscriptionPopup(npCemeteryId))
                {
                    popup.StartPosition = FormStartPosition.CenterParent;
                    popup.ShowDialog(this);
                }
            }
        }

        public void ShowAnbiyamOnMap(string anbiyamAddress)
        {
            string mapFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "anbiyam_map.html");
            webBrowser1.ScriptErrorsSuppressed = true;
            webBrowser1.Dock = DockStyle.Fill;
            if (File.Exists(mapFile))
                webBrowser1.Navigate(mapFile);
            else
                webBrowser1.Navigate("https://www.openstreetmap.org/?mlat=12.9308&mlon=80.0987#map=15/12.9308/80.0987");
        }
        private void mapBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            // Optionally handle any actions after navigation
        }

        private void btnCemetery_Click(object sender, EventArgs e)
        {
            if (familyIDInContext <= 0)
            {
                MessageBox.Show("Please select a family to view cemetery details.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            using (var popup = new Cemetery(familyIDInContext))
            {
                popup.ShowDialog();
                LoadFamilyBasicDetails(null);
            }
        }

        private void LoadAllCemeteryData(DataTable dtInput)
        {
            DataTable dt = null;
            if (dtInput == null)
            {
                var countParams = new[]
                {
                    new SqlParameter("@burial_date_from",   (object)_cemBurialFrom   ?? DBNull.Value),
                    new SqlParameter("@burial_date_to",     (object)_cemBurialTo     ?? DBNull.Value),
                    new SqlParameter("@deceased_date_from", (object)_cemDeceasedFrom ?? DBNull.Value),
                    new SqlParameter("@deceased_date_to",   (object)_cemDeceasedTo   ?? DBNull.Value),
                    new SqlParameter("@IsOurparish",        _cemIsOurParish)
                };
                object cnt = DatabaseHelper.ExecuteScalarStoredProcedure("sp_GetAllCemeteriesTotalCount", countParams);
                _cemeteryTotalCount = cnt != null && cnt != DBNull.Value ? Convert.ToInt32(cnt) : 0;

                var dataParams = new[]
                {
                    new SqlParameter("@pageNumber",         _cemeteryCurrentPage),
                    new SqlParameter("@pageSize",           PageSize),
                    new SqlParameter("@burial_date_from",   (object)_cemBurialFrom   ?? DBNull.Value),
                    new SqlParameter("@burial_date_to",     (object)_cemBurialTo     ?? DBNull.Value),
                    new SqlParameter("@deceased_date_from", (object)_cemDeceasedFrom ?? DBNull.Value),
                    new SqlParameter("@deceased_date_to",   (object)_cemDeceasedTo   ?? DBNull.Value),
                    new SqlParameter("@IsOurparish",        _cemIsOurParish)
                };
                dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetAllCemeteriesPaged", dataParams);

                if (lblCemeteryPageInfo != null)
                {
                    int tp = _cemeteryTotalCount == 0 ? 1 : (int)Math.Ceiling((double)_cemeteryTotalCount / PageSize);
                    lblCemeteryPageInfo.Text = $"Page {_cemeteryCurrentPage} of {tp}  ({_cemeteryTotalCount} records)";
                    btnCemeteryPrevious.Enabled = _cemeteryCurrentPage > 1;
                    btnCemeteryNext.Enabled = _cemeteryCurrentPage < tp;
                }
            }
            else
            {
                dt = dtInput;
            }

            cemeteryGridView.DataSource = dt;
            cemeteryGridView.Columns["cemeteryid"].Visible = false;
            if (cemeteryGridView.Columns["FamilyId"] != null)
                cemeteryGridView.Columns["FamilyId"].Visible = false;
            cemeteryGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Georgia", 12, FontStyle.Bold);
            cemeteryGridView.DefaultCellStyle.Font = new Font("Georgia", 10, FontStyle.Regular);
            cemeteryGridView.AlternatingRowsDefaultCellStyle.BackColor = Color.LightSlateGray;
            cemeteryGridView.BackgroundColor = Color.WhiteSmoke;
            cemeteryGridView.DefaultCellStyle.ForeColor = Color.Black;

            if (cemeteryGridView.Columns["SubscriptionInfo"] == null)
            {
                var subCol = new DataGridViewButtonColumn();
                subCol.Name = "SubscriptionInfo";
                subCol.HeaderText = "";
                subCol.Text = "SubscriptionInfo";
                subCol.UseColumnTextForButtonValue = true;
                subCol.Width = 110;
                subCol.DefaultCellStyle.BackColor = Color.SteelBlue;
                subCol.DefaultCellStyle.ForeColor = Color.White;
                cemeteryGridView.Columns.Add(subCol);
            }
        }

        //private void searchButton_Click(object sender, EventArgs e)
        //{
        //    using (var progress = new ProgressForm("Searching..."))
        //    {
        //        // Get selected anbiyam_id (handle "Select" as null)
        //        object anbiyamIdObj = anbiyamCombobox.SelectedValue;
        //        int? anbiyamId = null;
        //        if (anbiyamIdObj != null && int.TryParse(anbiyamIdObj.ToString(), out int parsedId) && parsedId != 200) // 200 is your "Select" value
        //            anbiyamId = parsedId;

        //        // Get coordinator name and head of family from textboxes
        //        string coordinatorName = coordinatorTetbox.Text.Trim();

        //        // Store filters for pagination and reset to page 1
        //        _anbiyamIdFilter    = anbiyamId;
        //        _anbiyamCoordFilter = string.IsNullOrEmpty(coordinatorName) ? null : coordinatorName;
        //        _anbiyamCurrentPage = 1;
        //        LoadAnbiyamGrid(null);
        //    }
        //}

        private void searchFamily_Click(object sender, EventArgs e)
        {
            using (var progress = new ProgressForm("Searching..."))
            {
                // Get selected anbiyam_id (handle "Select" as null)
                object anbiyamIdObj = familyAnbiyamCombobox.SelectedValue;
                int anbiyamId = 0;
                bool? isCemeteryAvailable = null; // Default to null for "Not Applicable"

                if (anbiyamIdObj != null && int.TryParse(anbiyamIdObj.ToString(), out int parsedId) && parsedId != 200) // 200 is your "Select" value
                    anbiyamId = parsedId;

                string familyHead = txtFamilyHead.Text == "" ? null : txtFamilyHead.Text.Trim();
                string occupation = familyOccupationComboxbox.SelectedText == "" ? null : familyOccupationComboxbox.SelectedText.Trim(); ;

                switch (cemeteryComboBox.SelectedItem)
                {
                    case "Not Applicable":
                        isCemeteryAvailable = null; // Handle "Select" as null
                        break;
                    case "":
                        isCemeteryAvailable = null; // Handle "Select" as null
                        break;
                    case "Yes":
                        isCemeteryAvailable = true; // Handle "Not Applicable" as null
                        break;
                    default:
                        isCemeteryAvailable = false;
                        break;
                }

                // Store filters for pagination and reset to page 1
                _familyAnbiyamFilter = anbiyamId;
                _familyHeadFilter    = string.IsNullOrEmpty(familyHead) ? null : familyHead;
                _familyOccFilter     = string.IsNullOrEmpty(occupation) ? null : occupation;
                _familyCemeteryFilter = isCemeteryAvailable.HasValue ? (object)(isCemeteryAvailable.Value ? 1 : 0) : DBNull.Value;
                _familyCurrentPage   = 1;
                LoadFamilyBasicDetails(null);
            }
        }

        private void cemeterySearchBtn_Click(object sender, EventArgs e)
        {
            using (var progress = new ProgressForm("Searching..."))
            {
                bool? isOurParish = null; // Default to null for "Not Applicable"

                switch (parishCombobox.SelectedItem)
                {
                    case "Not Applicable":
                        isOurParish = null; // Handle "Select" as null
                        break;
                    case "":
                        isOurParish = null; // Handle "Select" as null
                        break;
                    case "Fatima Church Parish":
                        isOurParish = true; // Handle "Not Applicable" as null
                        break;
                    default:
                        isOurParish = false;
                        break;
                }

                // Store filters for pagination and reset to page 1
                _cemBurialFrom   = dtBurialStart.Checked   ? (DateTime?)dtBurialStart.Value   : null;
                _cemBurialTo     = dtBurialEnd.Checked     ? (DateTime?)dtBurialEnd.Value     : null;
                _cemDeceasedFrom = dtDeceasedStart.Checked ? (DateTime?)dtDeceasedStart.Value : null;
                _cemDeceasedTo   = dtDeceasedEnd.Checked   ? (DateTime?)dtDeceasedEnd.Value   : null;
                _cemIsOurParish  = isOurParish.HasValue ? (object)(isOurParish.Value ? 1 : 0) : DBNull.Value;
                _cemeteryCurrentPage = 1;
                LoadAllCemeteryData(null);
            }
        }


        private void familygrid_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            var grid = sender as DataGridView;
            if (grid.Rows[e.RowIndex].DataBoundItem is DataRowView drv)
            {
                // Replace "IsSpecial" with your actual boolean column name
                if (drv.Row.Table.Columns.Contains("Multiple Family Cards") && drv.Row.Field<bool>("Multiple Family Cards"))
                {
                    grid.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Yellow;
                }
            }
        }


        private void btnOutsideParsihMember_Click(object sender, EventArgs e)
        {
            using (var popup = new NonParishFamily())
            {
                popup.StartPosition = FormStartPosition.CenterScreen; // Show in the middle of the screen
                popup.ShowDialog();
                // After closing, reload family basic details
                // LoadFamilyBasicDetails(null);
            }
        }

        private void btnpdfExport_Click(object sender, EventArgs e)
        {
            // Ensure the DataGridView has data
            if (anbiyamGrid.DataSource == null)
            {
                ThemedDialog.Warn("No Anbiyam data to export.", "Nothing to Export", this);
                return;
            }
            try
            {
                string filename = "anbiyam_export-" + Guid.NewGuid() + ".pdf";
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), filename);
                ExportPDFData(anbiyamGrid.DataSource, filePath);
                ThemedDialog.Info("Anbiyam data exported successfully.\n\nSaved to: " + filePath, "Export Complete", this);
            }
            catch (Exception ex)
            {
                ThemedDialog.Error("Export failed: " + ex.Message, "Export Error", this);
            }
        }

        private void ExportPDFData(object dgv, string filepath)
        {
            var pdfDocument = new PdfDocument();
            pdfDocument.PageSettings.Orientation = PdfPageOrientation.Landscape;
            pdfDocument.PageSettings.Margins.All = 30;

            float pageW = pdfDocument.PageSettings.Width
                        - pdfDocument.PageSettings.Margins.Left
                        - pdfDocument.PageSettings.Margins.Right;

            // ── Repeating page header ──────────────────────────────────────────────
            float headerH = 80f;
            var headerBounds = new RectangleF(
                0, 0,
                pdfDocument.PageSettings.Width,
                headerH + pdfDocument.PageSettings.Margins.Top);
            var headerTpl = new PdfPageTemplateElement(headerBounds);
            var hg = headerTpl.Graphics;

            // Navy background
            hg.DrawRectangle(
                new PdfSolidBrush(new PdfColor(15, 30, 60)),
                new RectangleF(0, 0, headerBounds.Width, headerBounds.Height));

            // Teal accent bar (left edge, 6 px wide)
            hg.DrawRectangle(
                new PdfSolidBrush(new PdfColor(32, 130, 150)),
                new RectangleF(0, 0, 6, headerBounds.Height));

            // Gold right accent bar (right edge, 6 px wide)
            hg.DrawRectangle(
                new PdfSolidBrush(new PdfColor(200, 165, 60)),
                new RectangleF(headerBounds.Width - 6, 0, 6, headerBounds.Height));

            // Cross glyph
            var crossFont = new PdfStandardFont(PdfFontFamily.Helvetica, 22f, PdfFontStyle.Bold);
            hg.DrawString("✝", crossFont,
                new PdfSolidBrush(new PdfColor(200, 165, 60)),
                new PointF(18, 14));

            // Church name
            var titleFont = new PdfStandardFont(PdfFontFamily.Helvetica, 16f, PdfFontStyle.Bold);
            hg.DrawString("Our Lady of Fatima Church", titleFont,
                new PdfSolidBrush(new PdfColor(200, 165, 60)),
                new PointF(48, 12));

            // Subtitle
            var subFont = new PdfStandardFont(PdfFontFamily.Helvetica, 10f, PdfFontStyle.Regular);
            hg.DrawString("Tambaram, Chennai", subFont,
                new PdfSolidBrush(new PdfColor(160, 190, 215)),
                new PointF(48, 36));

            // Generation date (right-aligned)
            string genDate = "Generated: " + DateTime.Now.ToString("dd MMM yyyy, hh:mm tt");
            var dateFont = new PdfStandardFont(PdfFontFamily.Helvetica, 9f, PdfFontStyle.Italic);
            SizeF dateSize = dateFont.MeasureString(genDate);
            hg.DrawString(genDate, dateFont,
                new PdfSolidBrush(new PdfColor(160, 190, 215)),
                new PointF(headerBounds.Width - dateSize.Width - 16, 36));

            // Teal separator line under header
            hg.DrawRectangle(
                new PdfSolidBrush(new PdfColor(32, 130, 150)),
                new RectangleF(0, headerBounds.Height - 3, headerBounds.Width, 3));

            pdfDocument.Template.Top = headerTpl;

            // ── Repeating page footer ──────────────────────────────────────────────
            float footerH = pdfDocument.PageSettings.Margins.Bottom;
            var footerBounds = new RectangleF(
                0,
                pdfDocument.PageSettings.Height - footerH,
                pdfDocument.PageSettings.Width,
                footerH);
            var footerTpl = new PdfPageTemplateElement(footerBounds);
            var fg = footerTpl.Graphics;

            fg.DrawRectangle(
                new PdfSolidBrush(new PdfColor(15, 30, 60)),
                new RectangleF(0, 0, footerBounds.Width, footerH));

            var footerFont = new PdfStandardFont(PdfFontFamily.Helvetica, 8f, PdfFontStyle.Italic);
            fg.DrawString("© Our Lady of Fatima Church, Tambaram, Chennai",
                footerFont,
                new PdfSolidBrush(new PdfColor(160, 190, 215)),
                new PointF(12, (footerH - 10) / 2));

            // Page number placeholder (right-aligned)
            fg.DrawString("Page", footerFont,
                new PdfSolidBrush(new PdfColor(160, 190, 215)),
                new PointF(footerBounds.Width - 70, (footerH - 10) / 2));

            pdfDocument.Template.Bottom = footerTpl;

            // ── Grid ──────────────────────────────────────────────────────────────
            PdfPage pdfPage = pdfDocument.Pages.Add();

            var pdfGrid = new PdfGrid();
            pdfGrid.DataSource = dgv;

            // Header row style — navy background, gold bold text
            var headerStyle = new PdfGridRowStyle();
            headerStyle.BackgroundBrush = new PdfSolidBrush(new PdfColor(15, 30, 60));
            headerStyle.TextBrush       = new PdfSolidBrush(new PdfColor(200, 165, 60));
            headerStyle.Font            = new PdfStandardFont(PdfFontFamily.Helvetica, 9f, PdfFontStyle.Bold);
            pdfGrid.Headers[0].Style    = headerStyle;

            // Cell padding and font for all rows
            var cellStyle = new PdfGridCellStyle();
            cellStyle.Font        = new PdfStandardFont(PdfFontFamily.Helvetica, 8.5f);
            cellStyle.TextBrush   = new PdfSolidBrush(new PdfColor(15, 30, 60));
            cellStyle.Borders.All = new PdfPen(new PdfColor(200, 210, 220), 0.5f);
            cellStyle.CellPadding = new PdfPaddings(4, 4, 3, 3);
            pdfGrid.ApplyBuiltinStyle(PdfGridBuiltinStyle.TableGrid);

            // Override with our cell style for every data row
            foreach (PdfGridRow row in pdfGrid.Rows)
                row.Style.Font      = cellStyle.Font;

            // Alternating row colours
            for (int i = 0; i < pdfGrid.Rows.Count; i++)
            {
                pdfGrid.Rows[i].Style.TextBrush = new PdfSolidBrush(new PdfColor(15, 30, 60));
                pdfGrid.Rows[i].Style.BackgroundBrush = (i % 2 == 0)
                    ? new PdfSolidBrush(new PdfColor(245, 248, 252))
                    : new PdfSolidBrush(new PdfColor(255, 255, 255));
            }

            // Teal column-header bottom border accent
            for (int c = 0; c < pdfGrid.Headers[0].Cells.Count; c++)
            {
                pdfGrid.Headers[0].Cells[c].Style.Borders.Bottom =
                    new PdfPen(new PdfColor(32, 130, 150), 1.5f);
            }

            // Allow the grid to span across pages; draw starting below the header space
            var layoutFormat = new PdfGridLayoutFormat
            {
                Layout = PdfLayoutType.Paginate,
                Break  = PdfLayoutBreakType.FitPage
            };
            pdfGrid.Draw(pdfPage, new RectangleF(0, 0, pageW, pdfPage.GetClientSize().Height), layoutFormat);

            pdfDocument.Save(filepath);
            pdfDocument.Close(true);
        }

        private async void btn_exportfamily_Click(object sender, EventArgs e)
        {
            if (familygrid.DataSource == null)
            {
                ThemedDialog.Warn("No family data to export.", "Nothing to Export", this);
                return;
            }
            try
            {
                DialogResult choice = ThemedDialog.ConfirmYesNoCancel(
                    "All Families — export every family in the system.\nFiltered Only — export the current filtered view.",
                    "Export Family Data", this);

                if (choice == DialogResult.Yes)
                {
                    string filename = "family_full_data_export-" + Guid.NewGuid() + ".pdf";
                    string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), filename);
                    //progressBar1.Style = ProgressBarStyle.Marquee;
                    //progressBar1.MarqueeAnimationSpeed = 30;
                    //progressBar1.Visible = true;
                    DataTable dt = new DataTable();
                    await Task.Run(() => {
                        try
                        {
                            dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetFamilyBasicDetailsForExport");
                            ExportPDFData(dt, filePath);
                            System.Threading.Thread.Sleep(3000);
                        }
                        catch { }
                    });
                    //progressBar1.Visible = false;
                    //progressBar1.Style = ProgressBarStyle.Blocks;
                    ThemedDialog.Info("All family data exported successfully.\n\nSaved to: " + filePath, "Export Complete", this);
                }
                else if (choice == DialogResult.No)
                {
                    string filename = "family_partialdata_export-" + Guid.NewGuid() + ".pdf";
                    string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), filename);
                    ExportPDFData(familygrid.DataSource, filePath);
                    ThemedDialog.Info("Filtered family data exported successfully.\n\nSaved to: " + filePath, "Export Complete", this);
                }
            }
            catch (Exception ex)
            {
                ThemedDialog.Error("Export failed: " + ex.Message, "Export Error", this);
            }
        }

        private void WireGridButtonPainting(DataGridView grid)
        {
            grid.CellPainting += GridButtonCellPainting;
            grid.CellMouseEnter += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
                {
                    var g = (DataGridView)s;
                    string col = g.Columns[e.ColumnIndex].Name;
                    if (col == "SubscriptionInfo" || col == "delete")
                    {
                        _hoveredCell = new Point(e.ColumnIndex, e.RowIndex);
                        g.InvalidateCell(e.ColumnIndex, e.RowIndex);
                    }
                }
            };
            grid.CellMouseLeave += (s, e) =>
            {
                var g = (DataGridView)s;
                var prev = _hoveredCell;
                _hoveredCell = new Point(-1, -1);
                if (prev.X >= 0 && prev.X < g.Columns.Count && prev.Y >= 0 && prev.Y < g.Rows.Count)
                    g.InvalidateCell(prev.X, prev.Y);
            };
        }

        private void GridButtonCellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            var grid = (DataGridView)sender;
            string colName = grid.Columns[e.ColumnIndex].Name;

            bool isSubInfo = colName == "SubscriptionInfo";
            bool isDelete  = colName == "delete";
            if (!isSubInfo && !isDelete) return;

            bool hovered = _hoveredCell.X == e.ColumnIndex && _hoveredCell.Y == e.RowIndex;

            // Softer muted red (#A94442 — warm dark rose, not danger red)
            Color baseBg = isDelete
                ? Color.FromArgb(169, 68, 66)
                : AppTheme.Teal;
            Color hoverBg = isDelete
                ? Color.FromArgb(185, 90, 88)
                : Color.FromArgb(55, 130, 145);
            Color borderColor = isDelete
                ? Color.FromArgb(140, 50, 48)
                : Color.FromArgb(30, 95, 108);

            Color bg = hovered ? hoverBg : baseBg;

            // Fill cell background with the grid row colour first (clean gap between rows)
            bool altRow = (e.RowIndex % 2 == 1);
            Color rowBg = altRow ? AppTheme.RowAlt : Color.White;
            using (var rowFill = new SolidBrush(rowBg))
                e.Graphics.FillRectangle(rowFill, e.CellBounds);

            // Button rect — inset 3px top/bottom so row boundary is visible
            const int vPad = 3, hPad = 4;
            var btnRect = new Rectangle(
                e.CellBounds.X + hPad,
                e.CellBounds.Y + vPad,
                e.CellBounds.Width - hPad * 2,
                e.CellBounds.Height - vPad * 2);

            // Button fill
            using (var fill = new SolidBrush(bg))
                e.Graphics.FillRectangle(fill, btnRect);

            // Button border
            using (var border = new Pen(borderColor, 1))
                e.Graphics.DrawRectangle(border, btnRect);

            // Left accent strip (inside button)
            using (var strip = new SolidBrush(borderColor))
                e.Graphics.FillRectangle(strip, btnRect.X, btnRect.Y, 3, btnRect.Height);

            // Icon + label centred in the button rect
            string label = isDelete ? "Delete" : "Sub Info";
            Bitmap icon  = isDelete ? AppTheme.IconTrash(13) : AppTheme.IconInfo(13);

            int iconW  = 13;
            int textW  = TextRenderer.MeasureText(label, AppTheme.BoldSmall).Width;
            int totalW = iconW + 3 + textW;
            int startX = btnRect.X + (btnRect.Width - totalW) / 2;
            int iconY  = btnRect.Y + (btnRect.Height - iconW) / 2;

            e.Graphics.DrawImage(icon, startX, iconY, iconW, iconW);

            TextRenderer.DrawText(
                e.Graphics, label, AppTheme.BoldSmall,
                new Rectangle(startX + iconW + 3, btnRect.Y, textW + 2, btnRect.Height),
                Color.White,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPadding);

            e.Handled = true;
        }

        private void btn_disablefamily_Click(object sender, EventArgs e)
        {
            if (familyIDInContext <= 0)
            {
                ThemedDialog.Warn("Please select a family to disable.", "No Selection", this);
                return;
            }

            if (!ThemedDialog.Confirm("Disable this family? They will no longer appear in active lists.", "Disable Family", this))
                return;

            try
            {
                DatabaseHelper.ExecuteStoredProcedure("sp_DisableFamily", new SqlParameter("@familyID", familyIDInContext));
                ThemedDialog.Info("The selected family has been disabled.", "Family Disabled", this);
                LoadFamilyBasicDetails(null);
            }
            catch (Exception ex)
            {
                if (ex.Message != null && ex.Message.Contains("Cannot delete"))
                    ThemedDialog.Warn("This family cannot be disabled.", "Cannot Disable", this);
                else
                    ThemedDialog.Error(ex.Message, "Error", this);
            }
        }
    }
}