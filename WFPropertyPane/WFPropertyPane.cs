using System.ComponentModel;
namespace Pane
{
    public enum PaneCollapseMode
    {
        Minimal,
        Compact
    }
    internal sealed class PaneSection
    {
        public string Id { get; }
        public string Titolo { get; }
        public Control? Contenuto { get; set; }
        public bool IsExpanded { get; set; } = true;
        public int NaturalHeight { get; set; } = 120;
        public int CurrentHeight { get; set; }
        public Panel HeaderPanel { get; } = new Panel();
        public Panel BodyPanel { get; } = new Panel();
        public PaneSection(string id, string titolo)
        {
            Id = id;
            Titolo = titolo;
        }
        public bool IsVisible => Contenuto is not null;
    }
    public sealed class WFPropertyPane : Control
    {
        private const int TopBarHeight = 48;
        private const int SectionHeaderH = 32;
        private const int CompactBarWidth = 36;
        private const int DefaultWidth = 240;
        private const int MinExpandedWidth = 120;
        private const int AnimIntervalMs = 10;
        private const int AnimStepDivisor = 3;
        private const int AnimSnapThreshold = 3;
        private static readonly Color ColTopBarBg = Color.FromArgb(248, 247, 244);
        private static readonly Color ColSectionHeaderBg = Color.FromArgb(243, 242, 239);
        private static readonly Color ColSectionHover = Color.FromArgb(235, 234, 230);
        private static readonly Color ColBodyBg = Color.FromArgb(252, 252, 250);
        private static readonly Color ColBorder = Color.FromArgb(220, 218, 212);
        private static readonly Color ColChevron = Color.FromArgb(150, 148, 143);
        private static readonly Color ColHeaderText = Color.FromArgb(100, 98, 94);
        private static readonly Color ColAccentDefault = Color.FromArgb(0xCE, 0xA2, 0x41);
        private Color _accentColor = ColAccentDefault;
        private bool _isExpanded = true;
        private int _expandedWidth = DefaultWidth;
        private PaneCollapseMode _collapseMode = PaneCollapseMode.Minimal;
        private string? _activePluginId;
        private readonly Dictionary<string, bool> _sectionState = new();
        private readonly PaneSection _sezioneProps;
        private readonly List<PaneSection> _sezioniExtra = new();
        private readonly PaneSection _sezioneComandi;
        private readonly Panel _topBar;
        private readonly Label _topBarTitle;
        private readonly Button _btnCollapse;
        private readonly Panel _scrollPanel;
        private readonly Panel _footerPanel;
        private readonly Panel _compactBar;
        private readonly Button _btnCompactExpand;
        private System.Windows.Forms.Timer? _paneAnimTimer;
        private int _paneAnimTarget;
        private System.Windows.Forms.Timer? _sectionAnimTimer;
        private PaneSection? _animatingSection;
        private int _sectionAnimTarget;
        public WFPropertyPane()
        {
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.ResizeRedraw,
                true);
            Width = _expandedWidth;
            Dock = DockStyle.Right;
            _topBarTitle = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI Variable Text", 12f, FontStyle.Bold),
                ForeColor = ColHeaderText,
                BackColor = Color.Transparent,
                Padding = new Padding(10, 0, 0, 0),
                Text = "Pannello"
            };
            _btnCollapse = BuildIconButton("\uE76C", "Chiudi pannello");
            _btnCollapse.Click += (_, _) => Collapse();
            _topBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = TopBarHeight,
                BackColor = ColTopBarBg
            };
            _topBar.Controls.Add(_topBarTitle);
            _topBar.Controls.Add(_btnCollapse);
            _topBar.Paint += (_, e) =>
            {
                using var pen = new Pen(ColBorder, 0.5f);
                e.Graphics.DrawLine(pen, 0, _topBar.Height - 1, _topBar.Width, _topBar.Height - 1);
            };
            _footerPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 0,
                BackColor = ColBodyBg,
                Visible = false
            };
            _footerPanel.Paint += (_, e) =>
            {
                using var pen = new Pen(ColBorder, 0.5f);
                e.Graphics.DrawLine(pen, 0, 0, _footerPanel.Width, 0);
            };
            _scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = ColBodyBg
            };
            _btnCompactExpand = BuildIconButton("\uE76B", "Espandi pannello");
            _btnCompactExpand.Click += (_, _) => Expand();
            _compactBar = new Panel
            {
                Dock = DockStyle.None,
                BackColor = ColTopBarBg,
                Visible = false
            };
            _compactBar.Controls.Add(_btnCompactExpand);
            Controls.Add(_compactBar);   // Dock=None — fuori dal flusso dock
            Controls.Add(_footerPanel);  // Dock=Bottom
            Controls.Add(_topBar);       // Dock=Top
            Controls.Add(_scrollPanel);  // Dock=Fill — ultimo, occupa il resto
            _sezioneProps = CreaSezione("__props__", "Proprietà");
            _sezioneComandi = CreaSezione("__comandi__", "Comandi");
            AggiungiSezioneAScrollPanel(_sezioneProps);
            AggiungiSezioneAFooter(_sezioneComandi);
        }
        public bool IsExpanded => _isExpanded;
        public event EventHandler<PaneCollapseEventArgs>? CollapseStateChanged;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ExpandedWidth
        {
            get => _expandedWidth;
            set
            {
                _expandedWidth = Math.Max(MinExpandedWidth, value);
                if (_isExpanded) Width = _expandedWidth;
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color AccentColor
        {
            get => _accentColor;
            set
            {
                _accentColor = value;
                _btnCollapse.ForeColor = value;
                _btnCompactExpand.ForeColor = value;
                Invalidate(true);
            }
        }
        [DefaultValue(PaneCollapseMode.Minimal)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public PaneCollapseMode CollapseMode
        {
            get => _collapseMode;
            set
            {
                if (_collapseMode == value) return;
                _collapseMode = value;
                ApplyCollapseMode();
            }
        }
        public sealed class PaneCollapseEventArgs : EventArgs
        {
            public bool IsExpanded { get; }
            public PaneCollapseMode CollapseMode { get; }
            public PaneCollapseEventArgs(bool isExpanded, PaneCollapseMode collapseMode)
            {
                IsExpanded = isExpanded;
                CollapseMode = collapseMode;
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string? ActivePluginId
        {
            get => _activePluginId;
            set { _activePluginId = value; }
        }
        public void SetProprietà(Control? controllo)
        {
            ImpostaContenutoSezione(_sezioneProps, controllo);
            RicalcolaLayoutScrollPanel();
        }
        public void SetComandi(Control? controllo)
        {
            ImpostaContenutoSezione(_sezioneComandi, controllo);
            AggiornaFooterHeight();
        }
        public void AddSezione(string id, string titolo, Control controllo)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Id sezione non può essere vuoto.", nameof(id));
            ArgumentNullException.ThrowIfNull(controllo);
            var existing = _sezioniExtra.FirstOrDefault(s => s.Id == id);
            if (existing is not null)
            {
                ImpostaContenutoSezione(existing, controllo);
                RicalcolaLayoutScrollPanel();
                return;
            }
            var sezione = CreaSezione(id, titolo);
            ImpostaContenutoSezione(sezione, controllo);
            _sezioniExtra.Add(sezione);
            AggiungiSezioneAScrollPanel(sezione);
            RicalcolaLayoutScrollPanel();
        }
        public void RemoveSezione(string id)
        {
            var sezione = _sezioniExtra.FirstOrDefault(s => s.Id == id);
            if (sezione is null) return;
            _sezioniExtra.Remove(sezione);
            _scrollPanel.Controls.Remove(sezione.HeaderPanel);
            _scrollPanel.Controls.Remove(sezione.BodyPanel);
            sezione.Contenuto = null;
            RicalcolaLayoutScrollPanel();
        }
        public void ClearAll()
        {
            foreach (var id in _sezioniExtra.Select(s => s.Id).ToList())
                RemoveSezione(id);
            SetProprietà(null);
            SetComandi(null);
        }
        public void Toggle()
        {
            if (_isExpanded) Collapse();
            else Expand();
        }
        public void Expand()
        {
            if (_isExpanded) return;
            _isExpanded = true;
            _compactBar.Visible = false;
            _topBar.Visible = true;
            _scrollPanel.Visible = true;
            _footerPanel.Visible = _sezioneComandi.IsVisible;
            AvviaAnimazionePannello(_expandedWidth, onComplete: null);
        }
        public void Collapse()
        {
            if (!_isExpanded) return;
            _isExpanded = false;
            _topBar.Visible = false;
            _scrollPanel.Visible = false;
            _footerPanel.Visible = false;
            if (_collapseMode == PaneCollapseMode.Compact)
            {
                AggiornaCompactBar();
                _compactBar.Visible = true;
                AvviaAnimazionePannello(CompactBarWidth, onComplete: null);
            }
            else
            {
                _compactBar.Visible = false;
                AvviaAnimazionePannello(0, onComplete: null);
            }
        }
        private void ApplyCollapseMode()
        {
            if (_isExpanded) return; // espanso: il modo non ha effetto visivo immediato

            // Il pannello è già collassato: riallinea la UI al nuovo modo
            if (_collapseMode == PaneCollapseMode.Compact)
            {
                AggiornaCompactBar();
                _compactBar.Visible = true;
                Width = CompactBarWidth;
            }
            else // Minimal
            {
                _compactBar.Visible = false;
                Width = 0;
            }
        }
        public void SalvaStatoSezioni()
        {
            if (_activePluginId is null) return;
            foreach (var s in TutteLeSezioni())
                _sectionState[$"{_activePluginId}.{s.Id}"] = s.IsExpanded;
        }
        public void RipristinaStatoSezioni()
        {
            if (_activePluginId is null) return;
            foreach (var s in TutteLeSezioni())
            {
                string key = $"{_activePluginId}.{s.Id}";
                bool expanded = !_sectionState.TryGetValue(key, out bool saved) || saved;
                ImpostaEspansioneSezione(s, expanded, animate: false);
            }
            RicalcolaLayoutScrollPanel();
            AggiornaFooterHeight();
        }
        private PaneSection CreaSezione(string id, string titolo)
        {
            var sezione = new PaneSection(id, titolo);
            var lblTitolo = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI Variable Text", 8f, FontStyle.Bold),
                ForeColor = ColHeaderText,
                BackColor = Color.Transparent,
                Padding = new Padding(10, 0, 0, 0),
                Text = titolo.ToUpperInvariant(),
                Cursor = Cursors.Hand
            };
            var lblChevron = new Label
            {
                AutoSize = false,
                Width = 24,
                Dock = DockStyle.Right,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe MDL2 Assets", 9f),
                Text = "\uE70D",
                ForeColor = ColChevron,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            sezione.HeaderPanel.Height = SectionHeaderH;
            sezione.HeaderPanel.BackColor = ColSectionHeaderBg;
            sezione.HeaderPanel.Cursor = Cursors.Hand;
            sezione.HeaderPanel.Visible = false;
            sezione.HeaderPanel.Controls.Add(lblTitolo);
            sezione.HeaderPanel.Controls.Add(lblChevron);
            sezione.HeaderPanel.Paint += (_, e) =>
            {
                using var pen = new Pen(ColBorder, 0.5f);
                e.Graphics.DrawLine(pen, 0, sezione.HeaderPanel.Height - 1,
                                         sezione.HeaderPanel.Width, sezione.HeaderPanel.Height - 1);
                using var barPen = new Pen(_accentColor, 2f);
                e.Graphics.DrawLine(barPen, 1, 4, 1, sezione.HeaderPanel.Height - 4);
            };
            void OnEnter(object? _, EventArgs __) => sezione.HeaderPanel.BackColor = ColSectionHover;
            void OnLeave(object? _, EventArgs __) => sezione.HeaderPanel.BackColor = ColSectionHeaderBg;
            sezione.HeaderPanel.MouseEnter += OnEnter;
            sezione.HeaderPanel.MouseLeave += OnLeave;
            lblTitolo.MouseEnter += OnEnter;
            lblTitolo.MouseLeave += OnLeave;
            lblChevron.MouseEnter += OnEnter;
            lblChevron.MouseLeave += OnLeave;
            EventHandler onToggle = (_, _) =>
                ImpostaEspansioneSezione(sezione, !sezione.IsExpanded, animate: true);
            sezione.HeaderPanel.Click += onToggle;
            lblTitolo.Click += onToggle;
            lblChevron.Click += onToggle;
            sezione.BodyPanel.BackColor = ColBodyBg;
            sezione.BodyPanel.Height = 0;
            sezione.BodyPanel.Visible = false;
            return sezione;
        }
        private void ImpostaContenutoSezione(PaneSection sezione, Control? controllo)
        {
            sezione.BodyPanel.Controls.Clear();
            sezione.Contenuto = controllo;
            if (controllo is not null)
            {
                controllo.Dock = DockStyle.Fill;
                sezione.BodyPanel.Controls.Add(controllo);
                int naturale = controllo.PreferredSize.Height;
                sezione.NaturalHeight = naturale > 0 ? naturale : 120;
            }
            sezione.HeaderPanel.Visible = sezione.IsVisible;
            if (sezione.IsVisible && sezione.IsExpanded)
            {
                sezione.CurrentHeight = sezione.NaturalHeight;
                sezione.BodyPanel.Height = sezione.NaturalHeight;
                sezione.BodyPanel.Visible = true;
            }
            else
            {
                sezione.CurrentHeight = 0;
                sezione.BodyPanel.Height = 0;
                sezione.BodyPanel.Visible = false;
            }
        }
        private void ImpostaEspansioneSezione(PaneSection sezione, bool espandi, bool animate)
        {
            if (!sezione.IsVisible) return;
            sezione.IsExpanded = espandi;
            AggiornaChevron(sezione, espandi);
            int target = espandi ? sezione.NaturalHeight : 0;
            if (!animate || !_isExpanded)
            {
                sezione.CurrentHeight = target;
                sezione.BodyPanel.Height = target;
                sezione.BodyPanel.Visible = espandi && target > 0;
                if (sezione == _sezioneComandi)
                    AggiornaFooterHeight();
                else
                    RicalcolaLayoutScrollPanel();
                return;
            }
            if (_animatingSection == sezione)
            {
                _sectionAnimTimer?.Stop();
                _sectionAnimTimer?.Dispose();
                _sectionAnimTimer = null;
            }
            sezione.BodyPanel.Visible = true;
            _animatingSection = sezione;
            _sectionAnimTarget = target;
            _sectionAnimTimer = new System.Windows.Forms.Timer { Interval = AnimIntervalMs };
            _sectionAnimTimer.Tick += (_, _) =>
            {
                int delta = _sectionAnimTarget - sezione.CurrentHeight;
                if (Math.Abs(delta) <= AnimSnapThreshold)
                {
                    sezione.CurrentHeight = _sectionAnimTarget;
                    sezione.BodyPanel.Height = _sectionAnimTarget;
                    sezione.BodyPanel.Visible = _sectionAnimTarget > 0;
                    _sectionAnimTimer!.Stop();
                    _sectionAnimTimer.Dispose();
                    _sectionAnimTimer = null;
                    _animatingSection = null;
                }
                else
                {
                    int step = delta / AnimStepDivisor;
                    sezione.CurrentHeight += step != 0 ? step : Math.Sign(delta);
                    sezione.BodyPanel.Height = sezione.CurrentHeight;
                }
                if (sezione == _sezioneComandi)
                    AggiornaFooterHeight();
                else
                    RicalcolaLayoutScrollPanel();
            };
            _sectionAnimTimer.Start();
        }
        private void RicalcolaLayoutScrollPanel()
        {
            if (_scrollPanel == null) return;
            
            _scrollPanel.SuspendLayout();
            int cursor = 0;
            int larghezza = _scrollPanel.ClientSize.Width;
            if (larghezza <= 0) larghezza = Width;
            PosizioneSezione(_sezioneProps, ref cursor, larghezza);
            foreach (var s in _sezioniExtra.ToList())  // Copia per evitare modifiche durante iterazione
                PosizioneSezione(s, ref cursor, larghezza);
            _scrollPanel.AutoScrollMinSize = new Size(0, cursor);
            _scrollPanel.ResumeLayout(false);
        }
        private static void PosizioneSezione(PaneSection? s, ref int cursor, int larghezza)
        {
            if (s == null) return;
            
            if (!s.IsVisible)
            {
                s.HeaderPanel.Visible = false;
                s.BodyPanel.Visible = false;
                return;
            }
            s.HeaderPanel.Visible = true;
            s.HeaderPanel.SetBounds(0, cursor, larghezza, SectionHeaderH);
            cursor += SectionHeaderH;
            if (s.IsExpanded && s.CurrentHeight > 0)
            {
                s.BodyPanel.Visible = true;
                s.BodyPanel.SetBounds(0, cursor, larghezza, s.CurrentHeight);
                cursor += s.CurrentHeight;
            }
            else
            {
                s.BodyPanel.Visible = false;
            }
        }
        private void AggiungiSezioneAFooter(PaneSection sezione)
        {
            sezione.HeaderPanel.Dock = DockStyle.Top;
            sezione.BodyPanel.Dock = DockStyle.Fill;
            _footerPanel.Controls.Add(sezione.BodyPanel);
            _footerPanel.Controls.Add(sezione.HeaderPanel);
        }
        private void AggiornaFooterHeight()
        {
            if (!_sezioneComandi.IsVisible)
            {
                _footerPanel.Visible = false;
                _footerPanel.Height = 0;
                return;
            }
            _footerPanel.Visible = _isExpanded;
            int bodyH = _sezioneComandi.IsExpanded
                ? _sezioneComandi.NaturalHeight
                : 0;
            _footerPanel.Height = SectionHeaderH + bodyH;
        }
        private void AggiungiSezioneAScrollPanel(PaneSection sezione)
        {
            _scrollPanel.Controls.Add(sezione.BodyPanel);
            _scrollPanel.Controls.Add(sezione.HeaderPanel);
        }
        private void AggiornaCompactBar()
        {
            _compactBar.SetBounds(0, 0, CompactBarWidth, CompactBarWidth);
            _btnCompactExpand.SetBounds(0, 0, CompactBarWidth, CompactBarWidth);
        }
        private void AvviaAnimazionePannello(int targetWidth, Action? onComplete)
        {
            _paneAnimTimer?.Stop();
            _paneAnimTimer?.Dispose();
            _paneAnimTarget = targetWidth;
            _paneAnimTimer = new System.Windows.Forms.Timer { Interval = AnimIntervalMs };
            _paneAnimTimer.Tick += (_, _) =>
            {
                int delta = _paneAnimTarget - Width;
                if (Math.Abs(delta) <= AnimSnapThreshold)
                {
                    Width = _paneAnimTarget;
                    _paneAnimTimer!.Stop();
                    _paneAnimTimer.Dispose();
                    _paneAnimTimer = null;
                    onComplete?.Invoke();
                    onComplete?.Invoke();
                    CollapseStateChanged?.Invoke(this, new PaneCollapseEventArgs(_isExpanded, _collapseMode));
                }
                else
                {
                    int step = delta / AnimStepDivisor;
                    Width += step != 0 ? step : Math.Sign(delta);
                }
            };
            _paneAnimTimer.Start();
        }
        private Button BuildIconButton(string glyph, string tooltip)
        {
            var btn = new Button
            {
                FlatStyle = FlatStyle.Flat,
                Text = glyph,
                Font = new Font("Segoe MDL2 Assets", 9f),
                Dock = DockStyle.Right,
                Width = TopBarHeight,
                Cursor = Cursors.Hand,
                ForeColor = _accentColor,
                BackColor = Color.Transparent,
                TabStop = false
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = ColSectionHover;
            btn.FlatAppearance.MouseDownBackColor = ColBorder;
            new ToolTip().SetToolTip(btn, tooltip);
            return btn;
        }
        private static void AggiornaChevron(PaneSection sezione, bool espanso)
        {
            foreach (Control c in sezione.HeaderPanel.Controls)
                if (c is Label lbl && (lbl.Text == "\uE70D" || lbl.Text == "\uE70E"))
                    lbl.Text = espanso ? "\uE70D" : "\uE70E";
        }
        private IEnumerable<PaneSection> TutteLeSezioni()
        {
            yield return _sezioneProps;
            foreach (var s in _sezioniExtra) yield return s;
            yield return _sezioneComandi;
        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (_isExpanded && Width >= MinExpandedWidth)
                _expandedWidth = Width;
            if (!_isExpanded && _collapseMode == PaneCollapseMode.Compact && _compactBar.Visible)
                AggiornaCompactBar();
            RicalcolaLayoutScrollPanel();
        }
        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);
            RicalcolaLayoutScrollPanel();
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _paneAnimTimer?.Stop();
                _paneAnimTimer?.Dispose();
                _sectionAnimTimer?.Stop();
                _sectionAnimTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}