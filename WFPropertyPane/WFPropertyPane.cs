// STD.Controls/WFPropertyPane.cs
// C# / .NET 9+ / WinForms

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace STD.Controls
{
    // ---------------------------------------------------------------------------
    // Enums
    // ---------------------------------------------------------------------------

    public enum PropertyPaneCollapseMode
    {
        /// <summary>
        /// Larghezza 0. In hover sul bordo sinistro appare una linguetta per espandere.
        /// </summary>
        Minimal,

        /// <summary>
        /// Barra fissa 36px con solo pulsante toggle visibile.
        /// </summary>
        Compact
    }

    // ---------------------------------------------------------------------------
    // Modello sezione
    // ---------------------------------------------------------------------------

    internal sealed class PaneSezione
    {
        public string Id { get; }
        public string Titolo { get; set; }
        public Control? Contenuto { get; set; }
        public bool IsExpanded { get; set; } = true;

        // header bar
        public Panel Header { get; } = new Panel();
        // wrapper che contiene Contenuto, hidden quando collassato
        public Panel Body { get; } = new Panel();

        public PaneSezione(string id, string titolo)
        {
            Id = id;
            Titolo = titolo;
        }
    }

    // ---------------------------------------------------------------------------
    // WFPropertyPane
    // ---------------------------------------------------------------------------

    public sealed class WFPropertyPane : Control
    {
        // -----------------------------------------------------------------------
        // Costanti
        // -----------------------------------------------------------------------

        private const int CompactWidth = 36;
        private const int HoverGripWidth = 6;
        private const int HeaderHeight = 26;
        private const int DefaultPaneWidth = 240;
        private const int CollapseAnimMs = 160;

        private static readonly Color AccentDefault = Color.FromArgb(0xCE, 0xA2, 0x41);
        private static readonly Color HeaderBg = Color.FromArgb(245, 245, 243);
        private static readonly Color HeaderBgDark = Color.FromArgb(42, 42, 40);
        private static readonly Color BorderColor = Color.FromArgb(220, 218, 212);
        private static readonly Color BorderDark = Color.FromArgb(60, 60, 58);
        private static readonly Color SectionBg = Color.FromArgb(252, 252, 250);
        private static readonly Color SectionBgDark = Color.FromArgb(30, 30, 28);

        // -----------------------------------------------------------------------
        // Campi privati
        // -----------------------------------------------------------------------

        private Color _accentColor = AccentDefault;
        private bool _isDark = false;
        private bool _isExpanded = true;
        private int _expandedWidth;
        private PropertyPaneCollapseMode _collapseMode = PropertyPaneCollapseMode.Minimal;
        private string? _activePluginId;

        // Persistenza stato collasso sezioni: chiave = "pluginId.sectionId"
        private readonly Dictionary<string, bool> _sectionState = new();

        // Sezioni (in ordine di visualizzazione, escluso Comandi)
        private readonly List<PaneSezione> _sezioni = new();
        private PaneSezione? _sezioneProprietà;
        private PaneSezione? _sezioneComandi;

        // Layout panels
        private readonly Panel _gripPanel;           // Minimal: hover grip
        private readonly Panel _compactBar;          // Compact: barra toggle
        private readonly Panel _contentPanel;        // area principale
        private readonly Panel _scrollWrapper;       // contiene _flowSezioni + padding
        private readonly FlowLayoutPanel _flowSezioni;
        private readonly Panel _footerPanel;         // Comandi fisso in fondo
        private readonly Button _btnToggleCompact;   // toggle in modalità Compact

        // Grip hover (Minimal)
        private bool _gripHover = false;
        private System.Windows.Forms.Timer? _animTimer;
        private int _animTarget;
        private int _animStep;

        // -----------------------------------------------------------------------
        // Costruttore
        // -----------------------------------------------------------------------

        public WFPropertyPane()
        {
            _expandedWidth = DefaultPaneWidth;

            DoubleBuffered = true;
            ResizeRedraw = true;
            Width = _expandedWidth;

            // --- Grip (Minimal) ------------------------------------------------
            _gripPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = HoverGripWidth,
                Visible = false,
                Cursor = Cursors.Hand,
                BackColor = Color.Transparent
            };
            _gripPanel.MouseEnter += (s, e) => { _gripHover = true; _gripPanel.Invalidate(); };
            _gripPanel.MouseLeave += (s, e) => { _gripHover = false; _gripPanel.Invalidate(); };
            _gripPanel.MouseClick += (s, e) => Expand();
            _gripPanel.Paint += GripPanel_Paint;

            // --- Compact bar ---------------------------------------------------
            _btnToggleCompact = new Button
            {
                FlatStyle = FlatStyle.Flat,
                Text = "▶",
                Font = new Font("Segoe UI", 9f),
                Dock = DockStyle.Top,
                Height = 36,
                Cursor = Cursors.Hand,
                ForeColor = _accentColor,
                BackColor = Color.Transparent,
                TabStop = false
            };
            _btnToggleCompact.FlatAppearance.BorderSize = 0;
            _btnToggleCompact.Click += (s, e) => Expand();

            _compactBar = new Panel
            {
                Dock = DockStyle.Fill,
                Width = CompactWidth,
                Visible = false
            };
            _compactBar.Controls.Add(_btnToggleCompact);

            // --- Content panel -------------------------------------------------
            _contentPanel = new Panel { Dock = DockStyle.Fill };

            // --- Flow sezioni (scroll area) ------------------------------------
            _flowSezioni = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            _scrollWrapper = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(0, 0, 0, 4)
            };
            _scrollWrapper.Controls.Add(_flowSezioni);

            // --- Footer (Comandi) ----------------------------------------------
            _footerPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 0,
                Visible = false
            };

            _contentPanel.Controls.Add(_scrollWrapper);
            _contentPanel.Controls.Add(_footerPanel);

            // --- Pulsante collapse nel content (header del pannello) -----------
            var topBar = BuildPanelTopBar();
            _contentPanel.Controls.Add(topBar);

            Controls.Add(_contentPanel);
            Controls.Add(_compactBar);
            Controls.Add(_gripPanel);

            // Sezioni default
            _sezioneProprietà = CreaSezioneInterna("__props__", "Proprietà");
            _sezioneComandi = CreaSezioneInterna("__cmds__", "Comandi");
            CollegaFooter(_sezioneComandi);

            // Aggiunge sezione Proprietà al flow
            AggiornaSectionFlow();
        }

        // -----------------------------------------------------------------------
        // Proprietà pubbliche
        // -----------------------------------------------------------------------

        public bool IsExpanded => _isExpanded;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]

        public int MinPaneWidth
        {
            get => _expandedWidth;
            set { _expandedWidth = Math.Max(120, value); }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public Color AccentColor
        {
            get => _accentColor;
            set
            {
                _accentColor = value;
                _btnToggleCompact.ForeColor = value;
                RefreshHeaderColors();
                Invalidate(true);
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public bool IsDark
        {
            get => _isDark;
            set
            {
                _isDark = value;
                RefreshTheme();
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public PropertyPaneCollapseMode CollapseMode
        {
            get => _collapseMode;
            set
            {
                _collapseMode = value;
                ApplyCollapseModeUI();
            }
        }

        /// <summary>
        /// Id del plugin attivo. Usato come prefisso per la persistenza dello stato sezioni.
        /// Va impostato da IPluginHost prima di iniettare contenuto.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public string? ActivePluginId
        {
            get => _activePluginId;
            set { _activePluginId = value; }
        }

        // -----------------------------------------------------------------------
        // API pubblica — iniezione contenuto
        // -----------------------------------------------------------------------

        /// <summary>Inietta il controllo nella sezione Proprietà.</summary>
        public void SetProprietà(Control? control)
        {
            if (_sezioneProprietà == null) return;
            SetSezioneContenuto(_sezioneProprietà, control);
        }

        /// <summary>Inietta il controllo nella sezione Comandi (footer).</summary>
        public void SetComandi(Control? control)
        {
            if (_sezioneComandi == null) return;
            SetSezioneContenuto(_sezioneComandi, control);
            AggiornaFooterHeight();
        }

        /// <summary>
        /// Aggiunge una sezione aggiuntiva tra Proprietà e Comandi.
        /// Se esiste già un id identico, sostituisce il contenuto.
        /// </summary>
        public void AddSezione(string id, string titolo, Control controllo)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException(nameof(id));

            var existing = _sezioni.FirstOrDefault(s => s.Id == id);
            if (existing != null)
            {
                SetSezioneContenuto(existing, controllo);
                return;
            }

            var sezione = CreaSezioneInterna(id, titolo);
            SetSezioneContenuto(sezione, controllo);
            _sezioni.Add(sezione);
            AggiornaSectionFlow();
        }

        /// <summary>Rimuove una sezione aggiuntiva per id.</summary>
        public void RemoveSezione(string id)
        {
            var sezione = _sezioni.FirstOrDefault(s => s.Id == id);
            if (sezione == null) return;
            _sezioni.Remove(sezione);
            _flowSezioni.Controls.Remove(sezione.Header);
            _flowSezioni.Controls.Remove(sezione.Body);
            sezione.Contenuto = null;
            AggiornaSectionFlow();
        }

        /// <summary>Svuota tutte le sezioni e rimuove quelle aggiuntive.</summary>
        public void ClearAll()
        {
            SetProprietà(null);
            SetComandi(null);
            foreach (var s in _sezioni.ToList())
                RemoveSezione(s.Id);
        }

        // -----------------------------------------------------------------------
        // API pubblica — stato pannello
        // -----------------------------------------------------------------------

        public void Toggle()
        {
            if (_isExpanded) Collapse();
            else Expand();
        }

        public void Expand()
        {
            if (_isExpanded) return;
            _isExpanded = true;
            AnimateTo(_expandedWidth);
            ApplyCollapseModeUI();
        }

        public void Collapse()
        {
            if (!_isExpanded) return;
            _isExpanded = false;
            int target = _collapseMode == PropertyPaneCollapseMode.Compact ? CompactWidth : 0;
            AnimateTo(target);
            ApplyCollapseModeUI();
        }

        /// <summary>
        /// Salva lo stato corrente delle sezioni per il plugin attivo.
        /// Chiamare da IPluginHost.OnDisattivato.
        /// </summary>
        public void SalvaStatoSezioni()
        {
            if (_activePluginId == null) return;
            foreach (var s in TutteLeSezioni())
                _sectionState[$"{_activePluginId}.{s.Id}"] = s.IsExpanded;
        }

        /// <summary>
        /// Ripristina lo stato delle sezioni per il plugin attivo.
        /// Chiamare da IPluginHost.OnAttivato dopo aver iniettato il contenuto.
        /// </summary>
        public void RipristinaStatoSezioni()
        {
            if (_activePluginId == null) return;
            foreach (var s in TutteLeSezioni())
            {
                string key = $"{_activePluginId}.{s.Id}";
                bool expanded = _sectionState.TryGetValue(key, out bool v) ? v : true;
                SetSezioneExpanded(s, expanded, animate: false);
            }
        }

        // -----------------------------------------------------------------------
        // Costruzione UI sezioni
        // -----------------------------------------------------------------------

        private PaneSezione CreaSezioneInterna(string id, string titolo)
        {
            var s = new PaneSezione(id, titolo);

            // Header
            s.Header.Height = HeaderHeight;
            s.Header.Cursor = Cursors.Hand;
            s.Header.Padding = new Padding(8, 0, 4, 0);
            s.Header.BackColor = _isDark ? HeaderBgDark : HeaderBg;
            s.Header.Width = _expandedWidth;
            s.Header.Anchor = AnchorStyles.Left | AnchorStyles.Right;

            var lbl = new Label
            {
                Text = titolo,
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = _accentColor,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };

            var chevron = new Label
            {
                Text = "▾",
                AutoSize = false,
                Width = 20,
                Dock = DockStyle.Right,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 8f),
                ForeColor = _isDark ? Color.FromArgb(160, 160, 155) : Color.FromArgb(140, 140, 135),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };

            s.Header.Controls.Add(lbl);
            s.Header.Controls.Add(chevron);

            // Body
            s.Body.BackColor = _isDark ? SectionBgDark : SectionBg;
            s.Body.Width = _expandedWidth;
            s.Body.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            s.Body.Height = 0;
            s.Body.Visible = true;

            // Click header → toggle
            EventHandler toggleClick = (sender, e) =>
            {
                s.IsExpanded = !s.IsExpanded;
                chevron.Text = s.IsExpanded ? "▾" : "▸";
                SetSezioneExpanded(s, s.IsExpanded, animate: true);
            };
            s.Header.Click += toggleClick;
            lbl.Click += toggleClick;
            chevron.Click += toggleClick;

            // Separatore bottom sull'header
            s.Header.Paint += (sender, e) =>
            {
                var c = _isDark ? BorderDark : BorderColor;
                using var pen = new Pen(c, 1f);
                e.Graphics.DrawLine(pen, 0, s.Header.Height - 1,
                                         s.Header.Width, s.Header.Height - 1);
            };

            return s;
        }

        private void SetSezioneContenuto(PaneSezione s, Control? control)
        {
            s.Body.Controls.Clear();
            s.Contenuto = control;

            bool visible = control != null;
            s.Header.Visible = visible;
            s.Body.Visible = visible && s.IsExpanded;

            if (control != null)
            {
                control.Dock = DockStyle.Fill;
                s.Body.Controls.Add(control);
                if (s.IsExpanded)
                    s.Body.Height = control.PreferredSize.Height > 0
                        ? control.PreferredSize.Height
                        : 120;
            }
            else
            {
                s.Body.Height = 0;
            }
        }

        private void SetSezioneExpanded(PaneSezione s, bool expanded, bool animate)
        {
            s.IsExpanded = expanded;

            // Aggiorna chevron
            foreach (Control c in s.Header.Controls)
                if (c is Label lbl && (lbl.Text == "▾" || lbl.Text == "▸"))
                    lbl.Text = expanded ? "▾" : "▸";

            int targetH = expanded
                ? (s.Contenuto?.PreferredSize.Height > 0 ? s.Contenuto.PreferredSize.Height : 120)
                : 0;

            s.Body.Visible = expanded || s.Body.Height > 0;

            if (!animate || !_isExpanded)
            {
                s.Body.Height = targetH;
                s.Body.Visible = expanded && s.Contenuto != null;
                return;
            }

            // Animazione semplicissima via Timer
            var timer = new System.Windows.Forms.Timer { Interval = 12 };
            timer.Tick += (sender, e) =>
            {
                int delta = (targetH - s.Body.Height);
                if (Math.Abs(delta) <= 4)
                {
                    s.Body.Height = targetH;
                    s.Body.Visible = expanded && s.Contenuto != null;
                    timer.Stop();
                    timer.Dispose();
                }
                else
                {
                    s.Body.Height += delta / 3;
                }
            };
            timer.Start();
        }

        // -----------------------------------------------------------------------
        // Flow layout sezioni
        // -----------------------------------------------------------------------

        private void AggiornaSectionFlow()
        {
            _flowSezioni.SuspendLayout();
            _flowSezioni.Controls.Clear();

            // Proprietà sempre prima
            if (_sezioneProprietà != null)
            {
                _flowSezioni.Controls.Add(_sezioneProprietà.Header);
                _flowSezioni.Controls.Add(_sezioneProprietà.Body);
            }

            // Sezioni aggiuntive
            foreach (var s in _sezioni)
            {
                _flowSezioni.Controls.Add(s.Header);
                _flowSezioni.Controls.Add(s.Body);
            }

            _flowSezioni.ResumeLayout(true);
        }

        private void CollegaFooter(PaneSezione s)
        {
            _footerPanel.Controls.Clear();
            _footerPanel.Controls.Add(s.Body);
            _footerPanel.Controls.Add(s.Header);
            s.Header.Dock = DockStyle.Top;
            s.Body.Dock = DockStyle.Fill;
        }

        private void AggiornaFooterHeight()
        {
            if (_sezioneComandi == null) return;
            bool visible = _sezioneComandi.Contenuto != null;
            _footerPanel.Visible = visible;
            if (!visible) { _footerPanel.Height = 0; return; }

            int bodyH = _sezioneComandi.IsExpanded
                ? (_sezioneComandi.Contenuto?.PreferredSize.Height > 0
                    ? _sezioneComandi.Contenuto.PreferredSize.Height : 80)
                : 0;
            _footerPanel.Height = HeaderHeight + bodyH;
        }

        // -----------------------------------------------------------------------
        // Top bar del pannello (pulsante collapse)
        // -----------------------------------------------------------------------

        private Panel BuildPanelTopBar()
        {
            var bar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 36,
                BackColor = _isDark ? HeaderBgDark : HeaderBg
            };

            bar.Paint += (s, e) =>
            {
                using var pen = new Pen(_isDark ? BorderDark : BorderColor, 1f);
                e.Graphics.DrawLine(pen, 0, bar.Height - 1, bar.Width, bar.Height - 1);
            };

            var btn = new Button
            {
                FlatStyle = FlatStyle.Flat,
                Text = "◀",
                Font = new Font("Segoe UI", 9f),
                Dock = DockStyle.Right,
                Width = 36,
                Cursor = Cursors.Hand,
                ForeColor = _accentColor,
                BackColor = Color.Transparent,
                TabStop = false
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) => Collapse();
            var tip = new ToolTip();
            tip.SetToolTip(btn, "Chiudi pannello");
            bar.Controls.Add(btn);
            return bar;
        }

        // -----------------------------------------------------------------------
        // Grip (Minimal mode)
        // -----------------------------------------------------------------------

        private void GripPanel_Paint(object? sender, PaintEventArgs e)
        {
            if (!_gripHover) return;
            using var brush = new SolidBrush(Color.FromArgb(60, _accentColor));
            e.Graphics.FillRectangle(brush, e.ClipRectangle);
            using var pen = new Pen(_accentColor, 1.5f);
            int cx = e.ClipRectangle.Width / 2;
            int cy = e.ClipRectangle.Height / 2;
            e.Graphics.DrawLine(pen, cx - 1, cy - 6, cx - 1, cy + 6);
        }

        // -----------------------------------------------------------------------
        // Animazione larghezza pannello
        // -----------------------------------------------------------------------

        private void AnimateTo(int targetWidth)
        {
            _animTimer?.Stop();
            _animTimer?.Dispose();

            _animTarget = targetWidth;

            _animTimer = new System.Windows.Forms.Timer { Interval = 8 };
            _animTimer.Tick += (s, e) =>
            {
                int delta = _animTarget - Width;
                if (Math.Abs(delta) <= 3)
                {
                    Width = _animTarget;
                    _animTimer!.Stop();
                    _animTimer.Dispose();
                    _animTimer = null;
                }
                else
                {
                    Width += delta / 3;
                }
            };
            _animTimer.Start();
        }

        // -----------------------------------------------------------------------
        // Tema
        // -----------------------------------------------------------------------

        private void RefreshTheme()
        {
            BackColor = _isDark ? SectionBgDark : SectionBg;

            foreach (var s in TutteLeSezioni())
            {
                s.Header.BackColor = _isDark ? HeaderBgDark : HeaderBg;
                s.Body.BackColor = _isDark ? SectionBgDark : SectionBg;
                s.Header.Invalidate();
            }

            Invalidate(true);
        }

        private void RefreshHeaderColors()
        {
            foreach (var s in TutteLeSezioni())
                foreach (Control c in s.Header.Controls)
                    if (c is Label lbl && (lbl.Text != "▾" && lbl.Text != "▸"))
                        lbl.ForeColor = _accentColor;
        }

        // -----------------------------------------------------------------------
        // Collapse mode UI
        // -----------------------------------------------------------------------

        private void ApplyCollapseModeUI()
        {
            if (_isExpanded)
            {
                _gripPanel.Visible = false;
                _compactBar.Visible = false;
                _contentPanel.Visible = true;
                return;
            }

            _contentPanel.Visible = false;

            if (_collapseMode == PropertyPaneCollapseMode.Minimal)
            {
                _compactBar.Visible = false;
                _gripPanel.Visible = true;
                _gripPanel.Width = HoverGripWidth;
            }
            else
            {
                _gripPanel.Visible = false;
                _compactBar.Visible = true;
                _btnToggleCompact.Text = "◀";
            }
        }

        // -----------------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------------

        private IEnumerable<PaneSezione> TutteLeSezioni()
        {
            if (_sezioneProprietà != null) yield return _sezioneProprietà;
            foreach (var s in _sezioni) yield return s;
            if (_sezioneComandi != null) yield return _sezioneComandi;
        }

        // -----------------------------------------------------------------------
        // Override WinForms
        // -----------------------------------------------------------------------

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            foreach (var s in TutteLeSezioni())
            {
                s.Header.Width = ClientSize.Width;
                s.Body.Width = ClientSize.Width;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _animTimer?.Stop();
                _animTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}