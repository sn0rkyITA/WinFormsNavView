using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System;
using System.Drawing;
using System.Windows.Forms;
using NavView;

namespace NavView.Demo
{
    public partial class Form1 : Form
    {
        private NavigationView navView;
        private TableLayoutPanel mainLayout;
        private Label contentLabel;

        public Form1()
        {
            InitializeComponent();
            SetupUI();
            PopulateNavView();
        }

        private void SetupUI()
        {
            this.Text = "NavigationView - Test AutoSize";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Layout principale: 2 colonne (nav + contenuto)
            mainLayout = new TableLayoutPanel
            {
                ColumnCount = 2,
                RowCount = 1,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));  // ← NavView si adatta
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // ← Contenuto riempie il resto

            // NavigationView
            navView = new NavigationView
            {
                Dock = DockStyle.Fill,
                AppTitle = "My App",
                PaneDisplayMode = PaneDisplayMode.LeftCompact,
                CompactPaneWidth = 60,                   // Larghezza in modalità compatta
                PaneWidth = 60,                          // Larghezza in modalità normale
                ContentMode = ContentMode.ExternalHost, // ← Importante: delega il layout all'host
                AutoSizePaneWidth = true,               // ← Attiva il calcolo automatico larghezza
                AutoSize = true,                        // ← Permette al controllo di ridimensionarsi
                Theme = NavViewTheme.Light
            };
            navView.SelectionChanged += (s, e) =>
            {
                contentLabel.Text = e.Item?.Label ?? "Nessuna selezione";

            };

            // Area contenuto (semplice label per test)
            contentLabel = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Seleziona una voce dal menu",
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 12F),
                BackColor = Color.White
            };

            // Aggiungi al layout
            mainLayout.Controls.Add(navView, 0, 0);
            mainLayout.Controls.Add(contentLabel, 1, 0);
            this.Controls.Add(mainLayout);

            // Bottone di test per toggle manuale
            var toggleBtn = new Button
            {
                Text = "Toggle Pane (H)",
                Top = 10,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            toggleBtn.Click += (s, e) => navView.IsPaneOpen = !navView.IsPaneOpen;
            this.Controls.Add(toggleBtn);

            // Label per debug: mostra larghezza corrente
            var debugLabel = new Label
            {
                Text = $"NavView.Width: {navView.Width}px",
                Top = 40,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                AutoSize = true,
                BackColor = Color.LightYellow,
                Padding = new Padding(4)
            };
            navView.ParentChanged += (s, e) =>
                debugLabel.Text = $"NavView.Width: {navView.Width}px | Preferred: {navView.GetPreferredSize(Size.Empty).Width}px";
            this.Controls.Add(debugLabel);

            // Timer per aggiornamento debug in tempo reale
            var timer = new System.Windows.Forms.Timer { Interval = 100, Enabled = true };
            timer.Tick += (s, e) => debugLabel.Text =
                $"W: {navView.Width}px | Pref: {navView.GetPreferredSize(Size.Empty).Width}px | Open: {navView.IsPaneOpen}";
        }

        private void PopulateNavView()
        {
            // Voci con etichette di lunghezza variabile per testare l'auto-sizing
            var home = new NavItem { Id = "home", Label = "Home", IconGlyph = "🏠" };
            var dashboard = new NavItem { Id = "dash", Label = "Dashboard", IconGlyph = "📊" };

            var settings = new NavItem { Id = "settings", Label = "Impostazioni", IconGlyph = "⚙️", IsExpanded = true };
            settings.Children.Add(new NavItem { Id = "settings.general", Label = "Generali", IconGlyph = "🔧" });
            settings.Children.Add(new NavItem { Id = "settings.advanced", Label = "Configurazioni Avanzate", IconGlyph = "🎛️" }); // ← Etichetta lunga!
            settings.Children.Add(new NavItem { Id = "settings.security", Label = "Sicurezza e Privacy", IconGlyph = "🔐" });

            var reports = new NavItem { Id = "reports", Label = "Reportistica Dettagliata", IconGlyph = "📈" }; // ← Etichetta lunga!
            reports.Children.Add(new NavItem { Id = "reports.monthly", Label = "Mensili", IconGlyph = "🗓️" });
            reports.Children.Add(new NavItem { Id = "reports.yearly", Label = "Annuali con Grafici Comparativi", IconGlyph = "📅" }); // ← Molto lunga!

            var help = new NavItem { Id = "help", Label = "?", IconGlyph = "❓", ToolTipText = "Aiuto" };

            navView.MenuItems.Add(home);
            navView.MenuItems.Add(dashboard);
            navView.MenuItems.Add(settings);
            navView.MenuItems.Add(reports);
            navView.MenuItems.Add(new NavItem { IsSeparator = true });
            navView.MenuItems.Add(help);

            // Footer items (anche questi influenzano la larghezza se AutoSizePaneWidth = true)
            navView.FooterMenuItems.Add(new NavItem { Id = "logout", Label = "Esci", IconGlyph = "🚪" });
            navView.FooterMenuItems.Add(new NavItem { Id = "profile", Label = "Profilo Utente", IconGlyph = "👤" });

            // Seleziona una voce di default per testare ContentHeader
            navView.Navigate("home");
        }

        // Supporto tastiera per toggle rapido
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.H) // Premi 'H' per toggle
            {
                navView.IsPaneOpen = !navView.IsPaneOpen;
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}