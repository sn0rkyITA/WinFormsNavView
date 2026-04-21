namespace WinFormsFluentTabView
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Text;
    using System.Linq;
    using System.Windows.Forms;

    public class WFTabItem
    {
        /// Identificatore univoco. Corrisponde tipicamente all'Id della VoceMenu che ha aperto la tab.
        public string Id { get; set; }
        /// Titolo visualizzato nella strip. Modificabile a runtime via AggiornaTitolo().
        public string Titolo { get; set; } = " ";
        /// Glyph Unicode opzionale (stesso font di WFNavigationView).
        public string Icona { get; set; } = " ";
        /// Controllo contenuto della tab. Già istanziato dall'host o dal plugin.
        public Control Contenuto { get; set; }
        /// Se True: nessuna X, nessun drag, tab sempre presente.
        public bool IsFixed { get; set; } = false;
        /// Nome del plugin proprietario. Informativo.
        public string NomePlugin { get; set; } = " ";
    }

    public class WFTabClosingEventArgs : EventArgs
    {
        public string Id { get; set; }
        public string Titolo { get; set; }
        public bool Cancel { get; set; } = false;
    }

    public class TabTheme
    {
        public Color StripBackColor { get; set; } = Color.FromArgb(243, 243, 243);
        public Color ContentBackColor { get; set; } = Color.White;
        public Color TabActiveBackColor { get; set; } = Color.White;
        public Color TabInactiveBackColor { get; set; } = Color.FromArgb(243, 243, 243);
        public Color TabHoverBackColor { get; set; } = Color.FromArgb(229, 229, 229);
        public Color TabActiveForeColor { get; set; } = Color.FromArgb(26, 26, 26);
        public Color TabInactiveForeColor { get; set; } = Color.FromArgb(96, 96, 96);
        public Color AccentColor { get; set; } = Color.FromArgb(0, 90, 158);
        public Color CloseButtonColor { get; set; } = Color.FromArgb(96, 96, 96);
        public Color CloseButtonHoverBackColor { get; set; } = Color.FromArgb(196, 43, 28);
        public Color CloseButtonHoverForeColor { get; set; } = Color.White;
        public Color ScrollArrowColor { get; set; } = Color.FromArgb(96, 96, 96);
        public Color DragIndicatorColor { get; set; } = Color.FromArgb(0, 90, 158);
        public Color SeparatorColor { get; set; } = Color.FromArgb(210, 210, 210);

        // ── temi predefiniti ─────────────────────────────────────────────────────

        public static TabTheme Light => new TabTheme();

        public static TabTheme Dark => new TabTheme
        {
            StripBackColor = Color.FromArgb(32, 32, 32),
            ContentBackColor = Color.FromArgb(24, 24, 24),
            TabActiveBackColor = Color.FromArgb(24, 24, 24),
            TabInactiveBackColor = Color.FromArgb(32, 32, 32),
            TabHoverBackColor = Color.FromArgb(50, 50, 50),
            TabActiveForeColor = Color.FromArgb(242, 242, 242),
            TabInactiveForeColor = Color.FromArgb(160, 160, 160),
            AccentColor = Color.FromArgb(0, 120, 212),
            CloseButtonColor = Color.FromArgb(160, 160, 160),
            CloseButtonHoverBackColor = Color.FromArgb(196, 43, 28),
            CloseButtonHoverForeColor = Color.White,
            ScrollArrowColor = Color.FromArgb(160, 160, 160),
            DragIndicatorColor = Color.FromArgb(0, 120, 212),
            SeparatorColor = Color.FromArgb(60, 60, 60)
        };

        /// <summary>
        /// Costruisce un tema chiaro con accent personalizzato.
        /// </summary>
        public static TabTheme FromAccent(Color accent) => new TabTheme
        {
            AccentColor = accent,
            DragIndicatorColor = accent
        };
    }

    public enum TabStyleEnum
    {
        /// Tab con sfondo pieno e bordi arrotondati in alto. Stile Chrome/Edge.
        Browser = 0,
        /// Tab piatte con indicatore sottolineato sull'attiva. Stile WinUI3.
        Fluent = 1
    }

    public enum TabWidthModeEnum
    {
        /// Tutte le tab hanno la stessa larghezza fissa (proprietà TabWidth).
        Fixed = 0,
        /// Larghezza adattiva al contenuto, con min e max configurabili.
        Adaptive = 1
    }

    public enum OverflowModeEnum
    {
        /// Frecce laterali per scorrere la strip.
        Scroll = 0
    }

    // Delegati per gli eventi (mappatura esatta delle firme VB.NET)
    public delegate void TabAttivataHandler(object sender, string id);
    public delegate void TabChiusaHandler(object sender, string id);
    public delegate void TabClosingHandler(object sender, WFTabClosingEventArgs e);
    public delegate void TabRiordinataHandler(object sender, int oldIndex, int newIndex);

    internal class WFTabStrip : Control
    {
        private readonly WFFTabView _owner;

        internal WFTabStrip(WFFTabView owner)
        {
            _owner = owner;
            this.SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.ResizeRedraw, true);
            this.DoubleBuffered = true;
            this.UpdateStyles();
            this.Dock = DockStyle.Top;
            this.Height = 54;
            this.Cursor = Cursors.Default;
        }

        protected override void OnPaint(PaintEventArgs e) => _owner.PaintStrip(e.Graphics, this.ClientRectangle);

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            _owner.StripMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            _owner.StripMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _owner.StripMouseUp(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _owner.StripMouseLeave();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            _owner.StripMouseWheel(e);
        }

        internal void Ridisegna() => Invalidate();
    }

    public class WFFTabView : UserControl
    {
        private readonly List<WFTabItem> _tabs = new List<WFTabItem>();
        private string _activeId = null;
        private readonly WFTabStrip _strip;
        private readonly Panel _contentPanel;
        private TabTheme _theme = TabTheme.Light;
        private Font _tabFont;
        private Font _iconFont;
        private string _iconFontFamily = "Segoe Fluent Icons";
        private float _fontSize = 9.0F;
        private float _iconFontSize = 14.0F;
        private TabStyleEnum _tabStyle = TabStyleEnum.Browser;
        private TabWidthModeEnum _tabWidthMode = TabWidthModeEnum.Adaptive;
        private int _tabWidth = 160;
        private int _tabMinWidth = 80;
        private int _tabMaxWidth = 240;
        private int _stripHeight = 54;
        private int _tabPaddingX = 12;
        private int _cornerRadius = 6;
        private int _closeButtonSize = 16;
        private int _scrollOffset = 0;
        private const int ARROW_WIDTH = 28;
        private bool _showScrollArrows = false;
        private string _hoverId = null;
        private string _hoverClose = null;
        private bool _isDragging = false;
        private string _dragId = null;
        private int _dragStartX = 0;
        private int _dragInsertIndex = -1;
        private const int DRAG_THRESHOLD = 6;
        private readonly System.Windows.Forms.Timer _autoScrollTimer = new System.Windows.Forms.Timer { Interval = 50 };
        private int _autoScrollDirection = 0;
        private readonly ToolTip _tt = new ToolTip { ShowAlways = true, InitialDelay = 600 };
        private bool _enableTooltips = true;

        public event TabAttivataHandler TabAttivata;
        public event TabChiusaHandler TabChiusa;
        public event TabClosingHandler TabClosing;
        public event TabRiordinataHandler TabRiordinata;

        public WFFTabView()
        {
            this.SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.ResizeRedraw, true);
            this.DoubleBuffered = true;
            this.UpdateStyles();
            this.AutoScaleMode = AutoScaleMode.Dpi;

            _contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = _theme.ContentBackColor,
                Padding = new Padding(0)
            };
            _strip = new WFTabStrip(this) { Height = _stripHeight };

            this.Controls.Add(_contentPanel);
            this.Controls.Add(_strip);

            RebuildFonts();
            ApplicaThemeInterno();
            _autoScrollTimer.Tick += OnAutoScrollTick;
        }

        [Category("Appearance"), Description("Stile visivo delle tab: Browser o Fluent."), DefaultValue(TabStyleEnum.Browser)]
        public TabStyleEnum TabStyle
        {
            get => _tabStyle;
            set { _tabStyle = value; _strip.Ridisegna(); }
        }

        [Category("Appearance"), Description("Modalità larghezza tab: Fixed o Adaptive."), DefaultValue(TabWidthModeEnum.Adaptive)]
        public TabWidthModeEnum TabWidthMode
        {
            get => _tabWidthMode;
            set { _tabWidthMode = value; _strip.Ridisegna(); }
        }

        [Category("Appearance"), Description("Larghezza fissa delle tab (solo modalità Fixed)."), DefaultValue(160)]
        public int TabWidth
        {
            get => _tabWidth;
            set { _tabWidth = Math.Max(40, Math.Min(400, value)); _strip.Ridisegna(); }
        }

        [Category("Appearance"), Description("Larghezza minima tab (solo modalità Adaptive)."), DefaultValue(80)]
        public int TabMinWidth
        {
            get => _tabMinWidth;
            set { _tabMinWidth = Math.Max(30, value); _strip.Ridisegna(); }
        }

        [Category("Appearance"), Description("Larghezza massima tab (solo modalità Adaptive)."), DefaultValue(240)]
        public int TabMaxWidth
        {
            get => _tabMaxWidth;
            set { _tabMaxWidth = Math.Max(_tabMinWidth, value); _strip.Ridisegna(); }
        }

        [Category("Appearance"), Description("Altezza della strip tab in pixel."), DefaultValue(40)]
        public int StripHeight
        {
            get => _stripHeight;
            set { _stripHeight = Math.Max(24, Math.Min(64, value)); _strip.Height = _stripHeight; _strip.Ridisegna(); }
        }

        [Category("Appearance"), Description("Padding orizzontale interno delle tab."), DefaultValue(12)]
        public int TabPaddingX
        {
            get => _tabPaddingX;
            set { _tabPaddingX = Math.Max(4, Math.Min(32, value)); _strip.Ridisegna(); }
        }

        [Category("Appearance"), Description("Raggio arrotondamento angoli (stile Browser)."), DefaultValue(4)]
        public int CornerRadius
        {
            get => _cornerRadius;
            set { _cornerRadius = Math.Max(0, Math.Min(12, value)); _strip.Ridisegna(); }
        }

        [Category("Appearance"), Description("Dimensione font testo tab (pt)."), DefaultValue(9.0F)]
        public float FontSize
        {
            get => _fontSize;
            set { _fontSize = Math.Max(7.0F, Math.Min(16.0F, value)); RebuildFonts(); _strip.Ridisegna(); }
        }

        [Category("Appearance"), Description("Dimensione font icone tab (pt)."), DefaultValue(14.0F)]
        public float IconFontSize
        {
            get => _iconFontSize;
            set { _iconFontSize = Math.Max(8.0F, Math.Min(24.0F, value)); RebuildFonts(); _strip.Ridisegna(); }
        }

        [Category("Appearance"), Description("Font family per i glyph icona. Default: Segoe Fluent Icons."), DefaultValue("Segoe Fluent Icons")]
        public string IconFontFamily
        {
            get => _iconFontFamily;
            set
            {
                _iconFontFamily = string.IsNullOrWhiteSpace(value) ? "Segoe Fluent Icons" : value.Trim();
                RebuildFonts();
                _strip.Ridisegna();
            }
        }

        [Category("Behavior"), Description("Mostra tooltip sul titolo troncato delle tab."), DefaultValue(true)]
        public bool EnableTooltips
        {
            get => _enableTooltips;
            set => _enableTooltips = value;
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IReadOnlyList<WFTabItem> TabAperte => _tabs.AsReadOnly();

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public WFTabItem TabAttiva => _tabs.FirstOrDefault(t => t.Id == _activeId);

        public void ApplyTheme(TabTheme tema)
        {
            if (tema == null) return;
            _theme = tema;
            ApplicaThemeInterno();
            _strip.Ridisegna();
        }

        public void ApplyTheme(object temaNavigation)
        {
            if (temaNavigation == null) return;
            try
            {
                var t = temaNavigation.GetType();
                var backColor = (Color)t.GetProperty("BackColor")?.GetValue(temaNavigation);
                var accent = (Color)t.GetProperty("AccentColor")?.GetValue(temaNavigation);
                var isDark = backColor.GetBrightness() < 0.4;
                var tema = isDark ? TabTheme.Dark : TabTheme.Light;
                tema.AccentColor = accent;
                tema.DragIndicatorColor = accent;
                ApplyTheme(tema);
            }
            catch { }
        }

        public void ApriTab(WFTabItem tab)
        {
            if (tab == null || string.IsNullOrEmpty(tab.Id)) return;
            var esistente = _tabs.FirstOrDefault(t => t.Id == tab.Id);
            if (esistente != null)
            {
                AttivaTab(tab.Id);
                return;
            }
            if (tab.Contenuto != null)
            {
                tab.Contenuto.Dock = DockStyle.Fill;
                tab.Contenuto.Visible = false;
                _contentPanel.Controls.Add(tab.Contenuto);
            }
            _tabs.Add(tab);
            AttivaTab(tab.Id);
            _strip.Ridisegna();
        }

        public void ChiudiTab(string id)
        {
            var tab = _tabs.FirstOrDefault(t => t.Id == id);
            if (tab == null || tab.IsFixed) return;

            var args = new WFTabClosingEventArgs { Id = id, Titolo = tab.Titolo };
            TabClosing?.Invoke(this, args);
            if (args.Cancel) return;

            var idx = _tabs.IndexOf(tab);
            _tabs.Remove(tab);
            if (tab.Contenuto != null)
            {
                _contentPanel.Controls.Remove(tab.Contenuto);
                tab.Contenuto.Dispose();
            }
            if (_activeId == id)
            {
                _activeId = null;
                if (_tabs.Count > 0)
                {
                    var nuovoIdx = Math.Min(idx, _tabs.Count - 1);
                    AttivaTab(_tabs[nuovoIdx].Id);
                }
            }
            CorreggiScroll();
            _strip.Ridisegna();
            TabChiusa?.Invoke(this, id);
        }

        public void AttivaTab(string id)
        {
            var tab = _tabs.FirstOrDefault(t => t.Id == id);
            if (tab == null) return;
            if (_activeId == id) return;

            _activeId = id;
            foreach (var t in _tabs)
            {
                if (t.Contenuto != null)
                    t.Contenuto.Visible = t.Id == id;
            }
            _strip.Ridisegna();
            TabAttivata?.Invoke(this, id);
        }

        public void AggiornaTitolo(string id, string titolo)
        {
            var tab = _tabs.FirstOrDefault(t => t.Id == id);
            if (tab == null) return;
            tab.Titolo = titolo;
            _strip.Ridisegna();
        }

        internal void PaintStrip(Graphics g, Rectangle bounds)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            g.CompositingQuality = CompositingQuality.HighQuality;

            using (var bg = new SolidBrush(_theme.StripBackColor))
                g.FillRectangle(bg, bounds);

            using (var sp = new Pen(_theme.SeparatorColor, 1))
                g.DrawLine(sp, bounds.Left, bounds.Bottom - 1, bounds.Right, bounds.Bottom - 1);

            var stripW = bounds.Width;
            var contentLeft = _showScrollArrows ? ARROW_WIDTH : 0;
            var contentRight = _showScrollArrows ? stripW - ARROW_WIDTH : stripW;
            var contentW = contentRight - contentLeft;

            g.SetClip(new Rectangle(contentLeft, bounds.Top, contentW, bounds.Height));
            var layouts = CalcolaLayoutTab(contentW);
            _showScrollArrows = CalcolaTotaleTabW(layouts) > contentW;

            if (_showScrollArrows)
            {
                contentLeft = ARROW_WIDTH;
                contentRight = stripW - ARROW_WIDTH;
                contentW = contentRight - contentLeft;
                layouts = CalcolaLayoutTab(contentW);
                g.SetClip(new Rectangle(contentLeft, bounds.Top, contentW, bounds.Height));
            }

            for (int i = 0; i < _tabs.Count; i++)
            {
                var tab = _tabs[i];
                if (tab.Id == _activeId) continue;
                if (i >= layouts.Count) continue;
                var layout = layouts[i];
                var x = contentLeft + layout.X - _scrollOffset;
                if (x + layout.W > contentLeft && x < contentRight)
                    DrawTab(g, tab, new Rectangle(x, bounds.Top, layout.W, bounds.Height), false, bounds);
            }

            for (int i = 0; i < _tabs.Count; i++)
            {
                var tab = _tabs[i];
                if (tab.Id != _activeId) continue;
                if (i >= layouts.Count) continue;
                var layout = layouts[i];
                var x = contentLeft + layout.X - _scrollOffset;
                DrawTab(g, tab, new Rectangle(x, bounds.Top, layout.W, bounds.Height), true, bounds);
                break;
            }

            if (_isDragging && _dragInsertIndex >= 0)
                DrawDragIndicator(g, layouts, contentLeft, bounds);

            g.ResetClip();

            if (_showScrollArrows)
            {
                DrawScrollArrow(g, bounds, true);
                DrawScrollArrow(g, bounds, false);
            }
        }

        private void DrawTab(Graphics g, WFTabItem tab, Rectangle rect, bool isActive, Rectangle stripBounds)
        {
            var isHover = tab.Id == _hoverId;
            var isDragged = _isDragging && tab.Id == _dragId;

            switch (_tabStyle)
            {
                case TabStyleEnum.Browser:
                    DrawTabBrowser(g, tab, rect, isActive, isHover, isDragged, stripBounds);
                    break;
                case TabStyleEnum.Fluent:
                    DrawTabFluent(g, tab, rect, isActive, isHover, isDragged, stripBounds);
                    break;
            }
        }

        private void DrawTabBrowser(Graphics g, WFTabItem tab, Rectangle rect, bool isActive, bool isHover, bool isDragged, Rectangle stripBounds)
        {
            var alpha = isDragged ? 120 : 255;
            var tabH = rect.Height;
            var tabRect = new Rectangle(rect.X, rect.Y + 4, rect.Width - 4, tabH - 2);

            var backColor = isActive ? _theme.TabActiveBackColor :
                            isHover ? _theme.TabHoverBackColor :
                            _theme.TabInactiveBackColor;

            if (alpha < 255)
                backColor = Color.FromArgb(alpha, backColor);

            using (var path = CreateTabPath(tabRect, _cornerRadius))
            {
                using (var br = new SolidBrush(backColor))
                    g.FillPath(br, path);

                if (!isActive)
                {
                    using (var pen = new Pen(Color.FromArgb(40, 0, 0, 0), 1))
                        g.DrawPath(pen, path);
                }
            }
            DrawTabContent(g, tab, tabRect, isActive, alpha);
        }

        private void DrawTabFluent(Graphics g, WFTabItem tab, Rectangle rect, bool isActive, bool isHover, bool isDragged, Rectangle stripBounds)
        {
            var alpha = isDragged ? 120 : 255;

            if (isHover && !isActive)
            {
                var hoverAlpha = alpha < 255 ? alpha : 255;
                var hoverColor = Color.FromArgb(hoverAlpha, _theme.TabHoverBackColor);
                using (var br = new SolidBrush(hoverColor))
                using (var path = CreateRoundedRect(new Rectangle(rect.X + 2, rect.Y + 2, rect.Width - 4, rect.Height - 4), 4))
                    g.FillPath(br, path);
            }

            DrawTabContent(g, tab, rect, isActive, alpha);

            if (isActive)
            {
                var lineY = rect.Bottom - 2;
                var lineX1 = rect.X + 8;
                var lineX2 = rect.Right - 8;
                using (var pen = new Pen(_theme.AccentColor, 2) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                    g.DrawLine(pen, lineX1, lineY, lineX2, lineY);
            }
        }

        private void DrawTabContent(Graphics g, WFTabItem tab, Rectangle rect, bool isActive, int alpha)
        {
            var foreColor = isActive ? _theme.TabActiveForeColor : _theme.TabInactiveForeColor;
            if (alpha < 255) foreColor = Color.FromArgb(alpha, foreColor);

            var curX = rect.X + _tabPaddingX;
            if (!string.IsNullOrEmpty(tab.Icona) && _iconFont != null)
            {
                var iconSize = (int)(_iconFontSize * 1.2);
                var iconBox = new RectangleF(curX, rect.Top, iconSize, rect.Height);
                using (var ib = new SolidBrush(foreColor))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.NoClip, Trimming = StringTrimming.None })
                    g.DrawString(tab.Icona, _iconFont, ib, iconBox, sf);
                curX += iconSize + 4;
            }

            var closeRect = Rectangle.Empty;
            if (!tab.IsFixed)
            {
                var cx = rect.Right - _tabPaddingX - _closeButtonSize;
                var cy = rect.Top + (rect.Height - _closeButtonSize) / 2;
                closeRect = new Rectangle(cx, cy, _closeButtonSize, _closeButtonSize);
            }

            var textRight = closeRect == Rectangle.Empty ? rect.Right - _tabPaddingX : closeRect.Left - 4;
            var textRect = new Rectangle(curX, rect.Top, Math.Max(0, textRight - curX), rect.Height);

            if (textRect.Width > 0)
            {
                if (_enableTooltips && _tabFont != null)
                {
                    var measW = (int)Math.Ceiling(g.MeasureString(tab.Titolo, _tabFont).Width);
                    if (measW > textRect.Width)
                        _tt.SetToolTip(_strip, tab.Titolo);
                }
                TextRenderer.DrawText(g, tab.Titolo, _tabFont, textRect, foreColor,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine);
            }

            if (!tab.IsFixed)
                DrawCloseButton(g, closeRect, tab.Id, alpha);
        }

        private void DrawCloseButton(Graphics g, Rectangle rect, string tabId, int alpha)
        {
            var isHoverClose = tabId == _hoverClose;
            if (isHoverClose)
            {
                using (var br = new SolidBrush(Color.FromArgb(alpha, _theme.CloseButtonHoverBackColor)))
                using (var path = CreateRoundedRect(rect, 3))
                    g.FillPath(br, path);
            }

            var xColor = isHoverClose ? Color.FromArgb(alpha, _theme.CloseButtonHoverForeColor) : Color.FromArgb(alpha, _theme.CloseButtonColor);
            var showX = tabId == _hoverId || tabId == _activeId || isHoverClose;
            if (!showX) return;

            var cx = rect.X + rect.Width / 2;
            var cy = rect.Y + rect.Height / 2;
            const int s = 4;
            using (var pen = new Pen(xColor, 1.5F) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                g.DrawLine(pen, cx - s, cy - s, cx + s, cy + s);
                g.DrawLine(pen, cx + s, cy - s, cx - s, cy + s);
            }
        }

        private void DrawScrollArrow(Graphics g, Rectangle bounds, bool isLeft)
        {
            var arrowRect = isLeft ? new Rectangle(0, 0, ARROW_WIDTH, bounds.Height)
                                   : new Rectangle(bounds.Width - ARROW_WIDTH, 0, ARROW_WIDTH, bounds.Height);

            using (var bg = new SolidBrush(_theme.StripBackColor))
                g.FillRectangle(bg, arrowRect);

            var cx = arrowRect.X + arrowRect.Width / 2;
            var cy = arrowRect.Y + arrowRect.Height / 2;
            const int s = 5;
            var isDisabled = isLeft ? _scrollOffset == 0 : !PuoScrollareDestra();
            var arrowColor = isDisabled ? Color.FromArgb(80, _theme.ScrollArrowColor) : _theme.ScrollArrowColor;

            using (var pen = new Pen(arrowColor, 1.5F) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                if (isLeft)
                {
                    g.DrawLine(pen, cx + 3, cy - s, cx - 3, cy);
                    g.DrawLine(pen, cx - 3, cy, cx + 3, cy + s);
                }
                else
                {
                    g.DrawLine(pen, cx - 3, cy - s, cx + 3, cy);
                    g.DrawLine(pen, cx + 3, cy, cx - 3, cy + s);
                }
            }
        }

        private void DrawDragIndicator(Graphics g, List<TabLayout> layouts, int contentLeft, Rectangle bounds)
        {
            if (_dragInsertIndex < 0 || _dragInsertIndex > layouts.Count) return;

            int x;
            if (_dragInsertIndex == 0)
                x = contentLeft - _scrollOffset;
            else if (_dragInsertIndex >= layouts.Count)
            {
                var last = layouts[layouts.Count - 1];
                x = contentLeft + last.X + last.W - _scrollOffset;
            }
            else
            {
                x = contentLeft + layouts[_dragInsertIndex].X - _scrollOffset;
            }

            const int triH = 5;
            using (var br = new SolidBrush(_theme.DragIndicatorColor))
            {
                var pts1 = new[] { new Point(x - triH, bounds.Top + 2), new Point(x + triH, bounds.Top + 2), new Point(x, bounds.Top + 2 + triH) };
                g.FillPolygon(br, pts1);
                using (var pen = new Pen(_theme.DragIndicatorColor, 2))
                    g.DrawLine(pen, x, bounds.Top + 2 + triH, x, bounds.Bottom - 2 - triH);
                var pts2 = new[] { new Point(x - triH, bounds.Bottom - 2), new Point(x + triH, bounds.Bottom - 2), new Point(x, bounds.Bottom - 2 - triH) };
                g.FillPolygon(br, pts2);
            }
        }

        internal void StripMouseDown(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            if (_showScrollArrows)
            {
                if (e.X < ARROW_WIDTH) { ScorriSinistra(); return; }
                if (e.X > _strip.Width - ARROW_WIDTH) { ScorriDestra(); return; }
            }

            var hit = HitTestTab(e.X, e.Y);
            if (hit == null) return;

            if (!hit.IsFixed)
            {
                var closeRect = GetCloseRect(hit, e.X, e.Y);
                if (closeRect.Contains(e.X, e.Y))
                {
                    ChiudiTab(hit.Id);
                    return;
                }
            }

            _dragId = hit.Id;
            _dragStartX = e.X;
            AttivaTab(hit.Id);
        }

        internal void StripMouseMove(MouseEventArgs e)
        {
            if (_dragId != null && !_isDragging)
            {
                if (Math.Abs(e.X - _dragStartX) > DRAG_THRESHOLD)
                {
                    var tab = _tabs.FirstOrDefault(t => t.Id == _dragId);
                    if (tab != null && !tab.IsFixed)
                    {
                        _isDragging = true;
                        _strip.Cursor = Cursors.SizeWE;
                    }
                }
            }

            if (_isDragging)
            {
                AggiornaDragInsertIndex(e.X);
                GestisciAutoScroll(e.X);
                _strip.Ridisegna();
                return;
            }

            var hit = HitTestTab(e.X, e.Y);
            var newHoverId = hit?.Id;
            string newHoverClose = null;
            if (hit != null && !hit.IsFixed)
            {
                var cr = GetCloseRect(hit, e.X, e.Y);
                if (cr.Contains(e.X, e.Y)) newHoverClose = hit.Id;
            }

            var changed = newHoverId != _hoverId || newHoverClose != _hoverClose;
            _hoverId = newHoverId;
            _hoverClose = newHoverClose;

            if (changed)
            {
                _strip.Cursor = newHoverClose != null ? Cursors.Hand : Cursors.Default;
                _strip.Ridisegna();
            }
        }

        internal void StripMouseUp(MouseEventArgs e)
        {
            _autoScrollTimer.Stop();
            _autoScrollDirection = 0;

            if (_isDragging && _dragInsertIndex >= 0)
                EseguiReorder();

            _isDragging = false;
            _dragId = null;
            _dragInsertIndex = -1;
            _strip.Cursor = Cursors.Default;
            _strip.Ridisegna();
        }

        internal void StripMouseLeave()
        {
            var changed = _hoverId != null || _hoverClose != null;
            _hoverId = null;
            _hoverClose = null;
            _autoScrollTimer.Stop();
            if (changed) _strip.Ridisegna();
        }

        internal void StripMouseWheel(MouseEventArgs e)
        {
            if (e.Delta > 0) ScorriSinistra(); else ScorriDestra();
        }

        private void AggiornaDragInsertIndex(int mouseX)
        {
            var contentLeft = _showScrollArrows ? ARROW_WIDTH : 0;
            var contentW = _strip.Width - (_showScrollArrows ? ARROW_WIDTH * 2 : 0);
            var layouts = CalcolaLayoutTab(contentW);
            _dragInsertIndex = _tabs.Count;

            for (int i = 0; i < layouts.Count; i++)
            {
                var x = contentLeft + layouts[i].X - _scrollOffset;
                var midX = x + layouts[i].W / 2;
                if (mouseX < midX)
                {
                    _dragInsertIndex = i;
                    break;
                }
            }
        }

        private void GestisciAutoScroll(int mouseX)
        {
            var contentLeft = _showScrollArrows ? ARROW_WIDTH : 0;
            var contentRight = _strip.Width - (_showScrollArrows ? ARROW_WIDTH : 0);

            if (mouseX < contentLeft + 20)
            {
                _autoScrollDirection = -1;
                if (!_autoScrollTimer.Enabled) _autoScrollTimer.Start();
            }
            else if (mouseX > contentRight - 20)
            {
                _autoScrollDirection = 1;
                if (!_autoScrollTimer.Enabled) _autoScrollTimer.Start();
            }
            else
            {
                _autoScrollDirection = 0;
                _autoScrollTimer.Stop();
            }
        }

        private void OnAutoScrollTick(object sender, EventArgs e)
        {
            if (_autoScrollDirection < 0) ScorriSinistra();
            else if (_autoScrollDirection > 0) ScorriDestra();
        }

        private void EseguiReorder()
        {
            var tab = _tabs.FirstOrDefault(t => t.Id == _dragId);
            if (tab == null) return;

            var oldIndex = _tabs.IndexOf(tab);
            var newIndex = _dragInsertIndex;
            if (newIndex > oldIndex) newIndex--;
            newIndex = Math.Max(0, Math.Min(_tabs.Count - 1, newIndex));

            if (oldIndex == newIndex) return;

            _tabs.RemoveAt(oldIndex);
            _tabs.Insert(newIndex, tab);
            TabRiordinata?.Invoke(this, oldIndex, newIndex);
        }

        private void ScorriSinistra()
        {
            if (_scrollOffset == 0) return;
            _scrollOffset = Math.Max(0, _scrollOffset - 40);
            _strip.Ridisegna();
        }

        private void ScorriDestra()
        {
            if (!PuoScrollareDestra()) return;
            _scrollOffset += 40;
            CorreggiScroll();
            _strip.Ridisegna();
        }

        private bool PuoScrollareDestra()
        {
            var contentW = _strip.Width - (_showScrollArrows ? ARROW_WIDTH * 2 : 0);
            var layouts = CalcolaLayoutTab(contentW);
            var totale = CalcolaTotaleTabW(layouts);
            return _scrollOffset < Math.Max(0, totale - contentW);
        }

        private void CorreggiScroll()
        {
            var contentW = _strip.Width - (_showScrollArrows ? ARROW_WIDTH * 2 : 0);
            var layouts = CalcolaLayoutTab(contentW);
            var maxOffset = Math.Max(0, CalcolaTotaleTabW(layouts) - contentW);
            _scrollOffset = Math.Max(0, Math.Min(_scrollOffset, maxOffset));
        }

        private struct TabLayout
        {
            internal int X;
            internal int W;
        }

        private List<TabLayout> CalcolaLayoutTab(int contentW)
        {
            var layouts = new List<TabLayout>();
            var x = 0;
            foreach (var t in _tabs)
            {
                var w = CalcolaLarghezzaTab(t, contentW);
                layouts.Add(new TabLayout { X = x, W = w });
                x += w;
            }
            return layouts;
        }

        private int CalcolaLarghezzaTab(WFTabItem tab, int contentW)
        {
            if (_tabWidthMode == TabWidthModeEnum.Fixed) return _tabWidth;
            if (_tabFont == null) return _tabMinWidth;

            var iconW = !string.IsNullOrEmpty(tab.Icona) ? (int)(_iconFontSize * 1.4) + 4 : 0;
            var closeW = !tab.IsFixed ? _closeButtonSize + 4 : 0;
            var textW = TextRenderer.MeasureText(tab.Titolo + "  ", _tabFont).Width;
            var totale = _tabPaddingX + iconW + textW + closeW + _tabPaddingX;
            return Math.Max(_tabMinWidth, Math.Min(_tabMaxWidth, totale));
        }

        private int CalcolaTotaleTabW(List<TabLayout> layouts)
        {
            if (layouts.Count == 0) return 0;
            var last = layouts[layouts.Count - 1];
            return last.X + last.W;
        }

        private WFTabItem HitTestTab(int mouseX, int mouseY)
        {
            var contentLeft = _showScrollArrows ? ARROW_WIDTH : 0;
            var contentRight = _strip.Width - (_showScrollArrows ? ARROW_WIDTH : 0);
            if (mouseX < contentLeft || mouseX > contentRight) return null;

            var contentW = contentRight - contentLeft;
            var layouts = CalcolaLayoutTab(contentW);

            for (int i = 0; i < _tabs.Count; i++)
            {
                if (i >= layouts.Count) continue;
                var x = contentLeft + layouts[i].X - _scrollOffset;
                if (mouseX >= x && mouseX < x + layouts[i].W) return _tabs[i];
            }
            return null;
        }

        private Rectangle GetCloseRect(WFTabItem tab, int mouseX, int mouseY)
        {
            var contentLeft = _showScrollArrows ? ARROW_WIDTH : 0;
            var contentW = _strip.Width - (_showScrollArrows ? ARROW_WIDTH * 2 : 0);
            var layouts = CalcolaLayoutTab(contentW);
            var idx = _tabs.IndexOf(tab);
            if (idx < 0 || idx >= layouts.Count) return Rectangle.Empty;

            var x = contentLeft + layouts[idx].X - _scrollOffset;
            var tabRect = new Rectangle(x, 0, layouts[idx].W, _stripHeight);
            var cx = tabRect.Right - _tabPaddingX - _closeButtonSize;
            var cy = tabRect.Top + (tabRect.Height - _closeButtonSize) / 2;
            return new Rectangle(cx, cy, _closeButtonSize, _closeButtonSize);
        }

        private GraphicsPath CreateTabPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            if (radius == 0)
            {
                path.AddRectangle(rect);
                return path;
            }
            var d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddLine(rect.Right, rect.Bottom, rect.X, rect.Bottom);
            path.CloseFigure();
            return path;
        }

        private GraphicsPath CreateRoundedRect(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            if (radius == 0)
            {
                path.AddRectangle(rect);
                return path;
            }
            var d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void RebuildFonts()
        {
            _tabFont?.Dispose();
            _iconFont?.Dispose();
            _tabFont = new Font("Segoe UI", _fontSize, FontStyle.Regular, GraphicsUnit.Point);

            var families = new[] { _iconFontFamily, "Segoe Fluent Icons", "Segoe MDL2 Assets" };
            _iconFont = null;
            foreach (var fam in families)
            {
                try
                {
                    var f = new Font(fam, _iconFontSize, FontStyle.Regular, GraphicsUnit.Point);
                    if (f.Name.Equals(fam, StringComparison.OrdinalIgnoreCase))
                    {
                        _iconFont = f;
                        break;
                    }
                    else
                    {
                        f.Dispose();
                    }
                }
                catch { }
            }
            if (_iconFont == null)
                _iconFont = new Font("Segoe UI", _iconFontSize, FontStyle.Regular, GraphicsUnit.Point);
        }

        private void ApplicaThemeInterno()
        {
            this.BackColor = _theme.StripBackColor;
            _contentPanel.BackColor = _theme.ContentBackColor;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _autoScrollTimer.Stop();
                _autoScrollTimer.Dispose();
                _tabFont?.Dispose();
                _iconFont?.Dispose();
                _tt.Dispose();
                foreach (var t in _tabs)
                {
                    if (t.Contenuto != null && !t.Contenuto.IsDisposed)
                        t.Contenuto.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}