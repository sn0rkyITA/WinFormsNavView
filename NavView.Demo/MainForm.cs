// MainForm.cs
// Demo del NavigationView. Mostra tutte le funzionalità principali:
// voci semplici, gerarchia, separatori, group header, footer,
// cambio tema, SetContent con UserControl inline.

using System;
using System.Drawing;
using System.Windows.Forms;
using NavView;

namespace NavView.Demo
{
    public partial class MainForm : Form
    {
        private NavigationView _navView = null!;
        private Panel _statusBar = null!;
        private Label _statusLabel = null!;

        // Pagine demo (UserControl inline)
        private UserControl? _currentPage;

        public MainForm()
        {
            InitializeForm();
            InitializeNavView();
            PopulateMenu();
            NavigateToFirst();
        }

        // -------------------------------------------------------------------------
        // Setup form
        // -------------------------------------------------------------------------

        private void InitializeForm()
        {
            Text = "NavView Demo";
            Size = new Size(900, 620);
            MinimumSize = new Size(600, 400);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.White;

            // Status bar in fondo
            _statusBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 28,
                BackColor = Color.FromArgb(243, 243, 243)
            };

            _statusLabel = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(100, 100, 100),
                Padding = new Padding(8, 0, 0, 0),
                Text = "Pronto"
            };

            _statusBar.Controls.Add(_statusLabel);
            Controls.Add(_statusBar);
        }

        // -------------------------------------------------------------------------
        // Setup NavigationView
        // -------------------------------------------------------------------------

        private void InitializeNavView()
        {
            _navView = new NavigationView
            {
                Dock = DockStyle.Fill,
                AppTitle = "NavView Demo",
                PaneDisplayMode = PaneDisplayMode.LeftCompact,
                Theme = NavViewTheme.Light,
                PaneWidth = 220,
                CompactPaneWidth = 64,
            };

            _navView.SelectionChanged += OnSelectionChanged;
            _navView.PaneOpened += (s, e) => _statusLabel.Text = "Pane aperto";
            _navView.PaneClosed += (s, e) => _statusLabel.Text = "Pane chiuso";

            Controls.Add(_navView);
            _navView.BringToFront();
        }

        // -------------------------------------------------------------------------
        // Popolamento menu
        // -------------------------------------------------------------------------

        private void PopulateMenu()
        {
            // --- Menu principale ---------------------------------------------

            _navView.MenuItems.Add(NavItem.Create(
                "Home", FluentIcons.Home, tag: "home"));

            _navView.MenuItems.Add(NavItem.Create(
                "Dashboard", FluentIcons.Dashboard, tag: "dashboard"));

            _navView.MenuItems.Add(NavItem.Separator());

            // Group header
            _navView.MenuItems.Add(NavItem.GroupHeader("Gestione"));

            // Nodo padre con figli (accordion)
            var pratiche = NavItem.Create("Pratiche", FluentIcons.Document, tag: "pratiche");
            pratiche.Children.Add(NavItem.Create("Nuova Pratica", FluentIcons.Add, tag: "pratiche.new"));
            pratiche.Children.Add(NavItem.Create("Archivio", FluentIcons.Folder, tag: "pratiche.archive"));
            pratiche.Children.Add(NavItem.Create("In Lavorazione", FluentIcons.Edit, tag: "pratiche.wip"));
            _navView.MenuItems.Add(pratiche);

            var anagrafica = NavItem.Create("Anagrafica", FluentIcons.People, tag: "anagrafica");
            anagrafica.Children.Add(NavItem.Create("Clienti", FluentIcons.Person, tag: "anagrafica.clienti"));
            anagrafica.Children.Add(NavItem.Create("Fornitori", FluentIcons.ContactCard, tag: "anagrafica.fornitori"));
            _navView.MenuItems.Add(anagrafica);

            _navView.MenuItems.Add(NavItem.Separator());

            _navView.MenuItems.Add(NavItem.GroupHeader("Strumenti"));

            _navView.MenuItems.Add(NavItem.Create(
                "Mappa", FluentIcons.Map, tag: "mappa"));

            _navView.MenuItems.Add(NavItem.Create(
                "Report", FluentIcons.Report, tag: "report"));

            // Voce disabilitata
            var disabled = NavItem.Create("Import (N/D)", FluentIcons.Upload, tag: "import");
            disabled.IsEnabled = false;
            _navView.MenuItems.Add(disabled);

            // --- Footer ------------------------------------------------------

            _navView.FooterMenuItems.Add(NavItem.Create(
                "Impostazioni", FluentIcons.Settings, tag: "settings"));

            _navView.FooterMenuItems.Add(NavItem.Create(
                "Profilo", FluentIcons.Person, tag: "profile"));
        }

        // -------------------------------------------------------------------------
        // Navigazione iniziale
        // -------------------------------------------------------------------------

        private void NavigateToFirst()
        {
            // Seleziona Home all'avvio
            if (_navView.MenuItems.Count > 0)
                _navView.Navigate(_navView.MenuItems[0]);
        }

        // -------------------------------------------------------------------------
        // Handler selezione
        // -------------------------------------------------------------------------

        private void OnSelectionChanged(object? sender, NavSelectionChangedEventArgs e)
        {
            _statusLabel.Text = $"Selezionato: {e.Item.Label}  |  Tag: {e.Item.Tag}";

            // Crea e mostra la pagina corrispondente al tag
            var page = CreatePage(e.Item.Tag?.ToString() ?? string.Empty, e.Item.Label);
            _navView.SetContent(page);
            _currentPage = page;
        }

        // -------------------------------------------------------------------------
        // Factory pagine demo
        // -------------------------------------------------------------------------

        private static UserControl CreatePage(string tag, string title)
        {
            var page = new UserControl
            {
                BackColor = Color.White
                //,Dock = DockStyle.Fill
            };

            // Contenuto demo: pannello centrato con titolo e tag
            var panel = new Panel
            {
                AutoSize = true,
                Location = new Point(32, 32)
            };

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = Color.FromArgb(26, 26, 26),
                AutoSize = true,
                Location = new Point(0, 0)
            };

            var lblTag = new Label
            {
                Text = $"tag: {tag}",
                Font = new Font("Segoe UI", 10f),
                ForeColor = Color.FromArgb(130, 130, 130),
                AutoSize = true,
                Location = new Point(2, lblTitle.PreferredHeight + 8)
            };

            var lblHint = new Label
            {
                Text = "Sostituisci questo UserControl con il contenuto reale.",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(160, 160, 160),
                AutoSize = true,
                Location = new Point(2, lblTitle.PreferredHeight + lblTag.PreferredHeight + 20)
            };

            panel.Controls.AddRange(new Control[] { lblTitle, lblTag, lblHint });
            page.Controls.Add(panel);

            return page;
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // MainForm
            // 
            ClientSize = new Size(284, 261);
            Name = "MainForm";
            ResumeLayout(false);

        }



        // -------------------------------------------------------------------------
        // Menu contestuale tema (tasto destro sulla form per demo)
        // -------------------------------------------------------------------------

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (e.Button == MouseButtons.Right)
            {
                var menu = new ContextMenuStrip();

                var lightItem = new ToolStripMenuItem("Tema Chiaro");
                lightItem.Click += (s, _) => _navView.Theme = NavViewTheme.Light;

                var darkItem = new ToolStripMenuItem("Tema Scuro");
                darkItem.Click += (s, _) => _navView.Theme = NavViewTheme.Dark;

                var modeLeft = new ToolStripMenuItem("Modalità Left");
                modeLeft.Click += (s, _) => _navView.PaneDisplayMode = PaneDisplayMode.Left;

                var modeCompact = new ToolStripMenuItem("Modalità LeftCompact");
                modeCompact.Click += (s, _) => _navView.PaneDisplayMode = PaneDisplayMode.LeftCompact;

                var collapseAll = new ToolStripMenuItem("Comprimi tutto");
                collapseAll.Click += (s, _) => _navView.CollapseAll();

                menu.Items.AddRange(new ToolStripItem[]
                {
                    lightItem, darkItem,
                    new ToolStripSeparator(),
                    modeLeft, modeCompact,
                    new ToolStripSeparator(),
                    collapseAll
                });

                menu.Show(this, e.Location);
            }
        }
    }
}