using System.ComponentModel;

namespace NavView
{
    // -------------------------------------------------------------------------
    // Enums
    // -------------------------------------------------------------------------

    /// <summary>
    /// Determina se il NavigationView gestisce internamente l'area contenuto
    /// oppure delega la gestione all'host esterno.
    /// </summary>
    public enum ContentMode
    {
        /// <summary>Il NavView disegna l'area contenuto e posiziona il control interno.</summary>
        InternalHost,
        /// <summary>Nessuna area contenuto: il pane occupa tutta la larghezza del controllo.
        /// L'host intercetta SelectionChanged e gestisce tab/property pane da solo.</summary>
        ExternalHost
    }

    /// <summary>
    /// Tipo di voce nella lista dei RendererItemInfo.
    /// Sostituisce i flag booleani IsSeparator / IsGroupHeader / IsFooterItem.
    /// </summary>
    public enum RendererItemKind
    {
        MenuItem,
        FooterItem,
        GroupHeader,
        Separator,
        /// <summary>Separatore strutturale fisso sopra il footer — non scrollabile, Source è null.</summary>
        StructuralSeparator
    }

    // -------------------------------------------------------------------------
    // NavigationView
    // -------------------------------------------------------------------------

    /// <summary>
    /// NavigationView per WinForms.
    /// Pannello laterale collassabile con voci di menu, footer, gerarchia,
    /// header contenuto e area contenuto opzionale (solo in InternalHost).
    /// </summary>
    public class NavigationView : Control
    {
        // -------------------------------------------------------------------------
        // Campi privati
        // -------------------------------------------------------------------------
        private INavViewRenderer _renderer;
        private NavViewTheme _theme = NavViewTheme.Light;
        private PaneDisplayMode _displayMode = PaneDisplayMode.LeftCompact;
        private ContentMode _contentMode = ContentMode.InternalHost;
        private bool _isPaneOpen = false;
        private bool _paneOpenedByHamburger = false;
        private int _paneWidth = 240;
        private bool _autoSizePaneWidth = false;
        private int _compactWidth = NavViewMetrics.CompactPaneWidthMin;
        private string _appTitle = string.Empty;
        private string _contentHeader = string.Empty;
        private bool _autoContentHeader = false;
        private NavItem? _selectedItem;
        private NavItem? _hoveredItem;
        private Control? _content;
        private readonly ToolTip _toolTip = new ToolTip();
        private NavItem? _tooltipItem;

        // Larghezza corrente del pane
        private int _currentPaneWidth;

        // Lista piatta delle voci visibili con Bounds calcolati.
        // I Bounds dei menu item sono in coordinate "virtuali" (prima dello scroll offset).
        // I Bounds dei footer item sono in coordinate assolute (non scrollano).
        private readonly List<RendererItemInfo> _visibleItems = new();

        // Traccia se il mouse è sull'hamburger
        private bool _hamburgerHovered = false;
        private Rectangle _hamburgerBounds;

        // -------------------------------------------------------------------------
        // Scroll
        // -------------------------------------------------------------------------

        /// <summary>Offset verticale corrente dello scroll (px, >= 0).</summary>
        private int _scrollOffset = 0;

        /// <summary>Altezza virtuale totale di tutti i menu item (senza scroll).</summary>
        private int _menuVirtualHeight = 0;

        /// <summary>Altezza dell'area visibile dedicata ai menu item.</summary>
        private int _menuViewportHeight = 0;

        /// <summary>Bounds della scrollbar (thumb incluso) — aggiornato in BuildVisibleItems.</summary>
        private Rectangle _scrollBarBounds;
        private Rectangle _scrollThumbBounds;
        private bool _scrollBarVisible = false;

        // Costanti scrollbar
        private const int ScrollBarWidth = 6;
        private const int ScrollBarMinThumbHeight = 20;
        private const int ScrollStep = NavViewMetrics.ItemHeight + NavViewMetrics.ItemMarginV;

        // -------------------------------------------------------------------------
        // Collezioni pubbliche
        // -------------------------------------------------------------------------
        public NavItemCollection MenuItems { get; } = new NavItemCollection();
        public NavItemCollection FooterMenuItems { get; } = new NavItemCollection();

        // -------------------------------------------------------------------------
        // Proprietà pubbliche
        // -------------------------------------------------------------------------
        [Category("NavView")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string AppTitle
        {
            get => _appTitle;
            set { _appTitle = value ?? string.Empty; Invalidate(); }
        }

        /// <summary>
        /// Header mostrato nell'area contenuto (solo InternalHost).
        /// In ExternalHost ignorato visivamente; se AutoContentHeader è true,
        /// viene aggiornato automaticamente dalla selezione ma non disegnato.
        /// </summary>
        [Category("NavView")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string ContentHeader
        {
            get => _contentHeader;
            set { _contentHeader = value ?? string.Empty; Invalidate(); }
        }

        /// <summary>
        /// Se true, ContentHeader viene aggiornato automaticamente al label
        /// della voce selezionata (e svuotato alla deselezione).
        /// Default: false — il chiamante gestisce ContentHeader manualmente.
        /// </summary>
        [Category("NavView")]
        [DefaultValue(false)]
        public bool AutoContentHeader
        {
            get => _autoContentHeader;
            set { _autoContentHeader = value; }
        }

        /// <summary>
        /// Modalità di gestione dell'area contenuto.
        /// ExternalHost: nessuna area contenuto disegnata; SetContent non ha effetto.
        /// </summary>
        [Category("NavView")]
        [DefaultValue(ContentMode.InternalHost)]
        public ContentMode ContentMode
        {
            get => _contentMode;
            set { _contentMode = value; RecalcLayout(); }
        }

        [Category("NavView")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public PaneDisplayMode PaneDisplayMode
        {
            get => _displayMode;
            set { _displayMode = value; UpdatePaneState(); }
        }

        [Category("NavView")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool IsPaneOpen
        {
            get => _isPaneOpen;
            set { if (_isPaneOpen != value) TogglePane(); }
        }

        [Category("NavView")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int PaneWidth
        {
            get => _paneWidth;
            set
            {
                if (_autoSizePaneWidth) return;
                _paneWidth = Math.Max(100, value);
                RecalcLayout();
            }
        }

        [Category("NavView")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool AutoSizePaneWidth
        {
            get => _autoSizePaneWidth;
            set
            {
                _autoSizePaneWidth = value;
                if (value) RecalcPaneWidth();
            }
        }

        [Category("NavView")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int CompactPaneWidth
        {
            get => _compactWidth;
            set { _compactWidth = Math.Max(NavViewMetrics.CompactPaneWidthMin, value); RecalcLayout(); }
        }

        [Category("NavView")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public NavViewTheme Theme
        {
            get => _theme;
            set
            {
                _theme = value;
                _renderer.Colors = value == NavViewTheme.Dark
                    ? NavViewColors.Dark()
                    : NavViewColors.Light();
                Invalidate();
            }
        }

        [Browsable(false)]
        public NavItem? SelectedItem => _selectedItem;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public INavViewRenderer Renderer
        {
            get => _renderer;
            set
            {
                if (_renderer is IDisposable d) d.Dispose();
                _renderer = value;
                Invalidate();
            }
        }

        // -------------------------------------------------------------------------
        // Eventi pubblici
        // -------------------------------------------------------------------------
        public event EventHandler<NavSelectionChangedEventArgs>? SelectionChanged;
        public event EventHandler? PaneOpened;
        public event EventHandler? PaneClosed;

        protected virtual void OnKeyboardNavigate(Keys key) { }

        // -------------------------------------------------------------------------
        // Costruttore
        // -------------------------------------------------------------------------
        public NavigationView()
        {
            _renderer = new NavViewRenderer();
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);

            UpdateCursor();
            MenuItems.CollectionChanged += OnCollectionChanged;
            FooterMenuItems.CollectionChanged += OnCollectionChanged;
            _currentPaneWidth = _compactWidth;
        }

        // -------------------------------------------------------------------------
        // API pubblica
        // -------------------------------------------------------------------------

        private void RecalcPaneWidth()
        {
            using var g = CreateGraphics();
            using var font = new Font("Segoe UI", NavViewMetrics.LabelFontSize,
                                      FontStyle.Regular, GraphicsUnit.Point);
            int maxWidth = 0;
            MeasureCollection(MenuItems, g, font, ref maxWidth);
            MeasureCollection(FooterMenuItems, g, font, ref maxWidth);
            _paneWidth = Math.Max(100, maxWidth + NavViewMetrics.PaneWidthPadding);
            RecalcLayout();
        }

        public void FitPaneWidth()
        {
            RecalcPaneWidth();
        }

        private static void MeasureCollection(NavItemCollection collection,
                                               Graphics g, Font font, ref int maxWidth)
        {
            foreach (var item in collection.Flatten())
            {
                if (!item.IsVisible || item.IsSeparator || item.IsGroupHeader) continue;

                int iconArea = NavViewMetrics.IconPaddingLeft
                             + NavViewMetrics.HamburgerSize
                             + NavViewMetrics.IconLabelGap
                             + item.Depth * NavViewMetrics.DepthIndent;

                int chevron = item.HasChildren ? NavViewMetrics.ChevronWidth : 0;

                int labelW = (int)Math.Ceiling(
                    g.MeasureString(item.Label, font).Width);

                int total = iconArea + labelW + chevron + NavViewMetrics.ItemMarginH;

                if (total > maxWidth) maxWidth = total;
            }
        }

        /// <summary>
        /// Imposta il controllo contenuto interno. Solo efficace in ContentMode.InternalHost.
        /// In ExternalHost la chiamata è ignorata.
        /// </summary>
        public void SetContent(Control? control)
        {
            if (_contentMode == ContentMode.ExternalHost) return;

            if (_content != null)
            {
                Controls.Remove(_content);
                _content = null;
            }
            _content = control;
            if (_content != null)
            {
                _content.TabStop = true;
                Controls.Add(_content);
                RecalcLayout();
            }
        }

        public void Navigate(NavItem item)
        {
            if (item == null || !item.IsSelectable) return;
            SelectItem(item);
        }

        public void Navigate(string itemId)
        {
            var item = FindById(itemId, MenuItems.Flatten())
                    ?? FindById(itemId, FooterMenuItems.Flatten());
            if (item != null) Navigate(item);
        }

        public void CollapseAll()
        {
            CollapseCollection(MenuItems);
            CollapseCollection(FooterMenuItems);
            BuildVisibleItems();
            Invalidate();
        }

        // -------------------------------------------------------------------------
        // Toggle pane
        // -------------------------------------------------------------------------
        private void TogglePane()
        {
            if (_displayMode == PaneDisplayMode.Left) return;

            if (_isPaneOpen)
            {
                CollapseCollection(MenuItems);
                CollapseCollection(FooterMenuItems);

                if (_selectedItem != null)
                {
                    var parent = _selectedItem.Parent;
                    while (parent != null && !parent.IsSelectable)
                        parent = parent.Parent;
                    if (parent != null)
                    {
                        var prev = _selectedItem;
                        _selectedItem = parent;
                        if (_autoContentHeader)
                            _contentHeader = parent.Label;
                        SelectionChanged?.Invoke(this,
                            new NavSelectionChangedEventArgs(_selectedItem, prev));
                    }
                }

                _isPaneOpen = false;
                _paneOpenedByHamburger = false;
                _scrollOffset = 0;
                RecalcLayout();
                PaneClosed?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                _isPaneOpen = true;
                _paneOpenedByHamburger = true;
                RecalcLayout();
                PaneOpened?.Invoke(this, EventArgs.Empty);
            }
        }

        private void UpdatePaneState()
        {
            if (_displayMode == PaneDisplayMode.Left)
            {
                _isPaneOpen = true;
                _paneOpenedByHamburger = true;
                _currentPaneWidth = _paneWidth;
            }
            else
            {
                _isPaneOpen = false;
                _paneOpenedByHamburger = false;
                _currentPaneWidth = _compactWidth;
                _scrollOffset = 0;
            }
            RecalcLayout();
        }

        // -------------------------------------------------------------------------
        // Layout
        // -------------------------------------------------------------------------
        private void RecalcLayout()
        {
            BuildVisibleItems();

            // In ExternalHost non c'è area contenuto da gestire
            if (_contentMode == ContentMode.InternalHost && _content != null)
            {
                var ca = ContentAreaBounds;
                int headerH = string.IsNullOrWhiteSpace(_contentHeader)
                    ? 0
                    : NavViewMetrics.ContentHeaderHeight;

                _content.SetBounds(
                    ca.Left, ca.Top + headerH,
                    ca.Width, ca.Height - headerH);
            }

            Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ClampScrollOffset();
            RecalcLayout();
        }

        private Rectangle PaneBounds => new Rectangle(0, 0, _currentPaneWidth, Height);

        /// <summary>
        /// In ExternalHost il pane occupa tutta la larghezza — ContentAreaBounds è vuoto.
        /// </summary>
        private Rectangle ContentAreaBounds => _contentMode == ContentMode.ExternalHost
            ? Rectangle.Empty
            : new Rectangle(
                _currentPaneWidth, 0,
                Math.Max(0, Width - _currentPaneWidth), Height);

        // -------------------------------------------------------------------------
        // Costruzione lista voci visibili
        // -------------------------------------------------------------------------
        private void BuildVisibleItems()
        {
            _visibleItems.Clear();

            bool hasExpandedInCompact = _displayMode == PaneDisplayMode.LeftCompact
                                      && !_isPaneOpen
                                      && HasAnyExpanded(MenuItems, FooterMenuItems);

            _currentPaneWidth = _displayMode == PaneDisplayMode.Left
                ? _paneWidth
                : (_isPaneOpen || hasExpandedInCompact) ? _paneWidth : _compactWidth;

            int paneW = _currentPaneWidth;
            bool showLabels = _isPaneOpen
                           || _displayMode == PaneDisplayMode.Left
                           || hasExpandedInCompact;

            bool scrollActive = showLabels;

            int paneHeight = Math.Max(0, Height);

            // ---- Footer: sempre fisso in basso ---------------------------------
            int footerH = 0;
            int footerStartY = paneHeight;

            if (FooterMenuItems.Count > 0)
            {
                footerH = _renderer.MeasureItemsHeight(
                    BuildRendererList(FooterMenuItems, showLabels));
                footerStartY = paneHeight - footerH - 8;
                footerStartY = Math.Max(NavViewMetrics.HeaderHeight + 8, footerStartY);
            }

            // ---- Separatore strutturale sopra il footer ------------------------
            if (FooterMenuItems.Count > 0)
            {
                _visibleItems.Add(new RendererItemInfo
                {
                    Kind = RendererItemKind.StructuralSeparator,
                    Bounds = new Rectangle(
                        0,
                        footerStartY - NavViewMetrics.SeparatorHeight - 2,
                        paneW,
                        NavViewMetrics.SeparatorHeight),
                    Source = null
                });
            }

            // ---- Footer item: coordinate assolute, non scrollano ---------------
            if (FooterMenuItems.Count > 0)
            {
                int fy = footerStartY;
                AddItemsToVisible(FooterMenuItems, ref fy, paneW, showLabels, isFooter: true);
            }

            // ---- Area viewport menu --------------------------------------------
            int menuAreaTop = NavViewMetrics.HeaderHeight;
            int menuAreaBottom = FooterMenuItems.Count > 0
                ? footerStartY - NavViewMetrics.SeparatorHeight - 4
                : paneHeight - 4;

            _menuViewportHeight = Math.Max(0, menuAreaBottom - menuAreaTop);

            // ---- Menu item: coordinate virtuali --------------------------------
            _menuVirtualHeight = MeasureVirtualHeight(MenuItems, showLabels);

            if (scrollActive)
                ClampScrollOffset();
            else
                _scrollOffset = 0;

            int y = menuAreaTop - _scrollOffset;
            AddItemsToVisible(MenuItems, ref y, paneW, showLabels, isFooter: false);

            // ---- Scrollbar -----------------------------------------------------
            _scrollBarVisible = scrollActive && _menuVirtualHeight > _menuViewportHeight;

            if (_scrollBarVisible)
            {
                int sbX = paneW - ScrollBarWidth - 2;
                int sbY = menuAreaTop;
                int sbH = _menuViewportHeight;
                _scrollBarBounds = new Rectangle(sbX, sbY, ScrollBarWidth, sbH);

                int thumbH = Math.Max(ScrollBarMinThumbHeight,
                    (int)((double)_menuViewportHeight / _menuVirtualHeight * sbH));
                int maxOffset = _menuVirtualHeight - _menuViewportHeight;
                int thumbY = maxOffset > 0
                    ? sbY + (int)((double)_scrollOffset / maxOffset * (sbH - thumbH))
                    : sbY;
                _scrollThumbBounds = new Rectangle(sbX, thumbY, ScrollBarWidth, thumbH);
            }
            else
            {
                _scrollBarBounds = Rectangle.Empty;
                _scrollThumbBounds = Rectangle.Empty;
            }

            // ---- Hamburger bounds ----------------------------------------------
            int hambX = (_isPaneOpen || _displayMode == PaneDisplayMode.Left)
                ? NavViewMetrics.HamburgerPadding
                : (_currentPaneWidth - NavViewMetrics.HamburgerSize) / 2;

            _hamburgerBounds = new Rectangle(
                hambX,
                (NavViewMetrics.HeaderHeight - NavViewMetrics.HamburgerSize) / 2,
                NavViewMetrics.HamburgerSize,
                NavViewMetrics.HamburgerSize);
        }

        private int MeasureVirtualHeight(NavItemCollection collection, bool showLabels)
        {
            int total = 0;
            foreach (var item in collection)
                total += MeasureItemVirtualHeight(item, showLabels);
            return total;
        }

        private int MeasureItemVirtualHeight(NavItem item, bool showLabels)
        {
            if (item.IsGroupHeader && !showLabels) return 0;

            int h = item.IsSeparator ? NavViewMetrics.SeparatorHeight
                  : item.IsGroupHeader ? NavViewMetrics.GroupHeaderHeight
                                       : NavViewMetrics.ItemHeight;

            int total = h + (item.IsSeparator || item.IsGroupHeader ? 0 : NavViewMetrics.ItemMarginV);

            if (item.HasChildren && item.IsExpanded)
                foreach (var child in item.Children)
                    total += MeasureItemVirtualHeight(child, showLabels);

            return total;
        }

        private void AddItemsToVisible(NavItemCollection collection,
                                       ref int y, int paneW, bool showLabels,
                                       bool isFooter)
        {
            foreach (var item in collection)
                AddItemRecursive(item, ref y, paneW, showLabels, isFooter);
        }

        private void AddItemRecursive(NavItem item, ref int y, int paneW,
                                      bool showLabels, bool isFooter)
        {
            if (!item.IsVisible) return;
            if (item.IsGroupHeader && !showLabels) return;

            int h = item.IsSeparator ? NavViewMetrics.SeparatorHeight
                  : item.IsGroupHeader ? NavViewMetrics.GroupHeaderHeight
                                       : NavViewMetrics.ItemHeight;

            // Determina il Kind corretto
            RendererItemKind kind;
            if (item.IsSeparator)
                kind = RendererItemKind.Separator;
            else if (item.IsGroupHeader)
                kind = RendererItemKind.GroupHeader;
            else if (isFooter)
                kind = RendererItemKind.FooterItem;
            else
                kind = RendererItemKind.MenuItem;

            bool include;
            if (isFooter)
            {
                // Footer item: sempre inclusi, coordinate assolute
                include = true;
            }
            else
            {
                // Menu item: includi solo se (almeno parzialmente) nel viewport
                int menuAreaTop = NavViewMetrics.HeaderHeight;
                int menuAreaBottom = menuAreaTop + _menuViewportHeight;
                bool completelyAbove = y + h <= menuAreaTop;
                bool completelyBelow = y >= menuAreaBottom;
                include = !completelyAbove && !completelyBelow;
            }

            if (include)
            {
                _visibleItems.Add(new RendererItemInfo
                {
                    Kind = kind,
                    Label = item.Label,
                    IconGlyph = item.IconGlyph,
                    IsSelected = item == _selectedItem,
                    IsHovered = item == _hoveredItem && item.IsEnabled,
                    IsEnabled = item.IsEnabled,
                    HasChildren = item.HasChildren,
                    IsExpanded = item.IsExpanded,
                    Depth = item.Depth,
                    Bounds = new Rectangle(0, y, paneW, h),
                    Source = item,
                    CustomIcon = item.CustomIcon,
                    HasNotification = item.HasNotification
                });
            }

            y += h + (item.IsSeparator || item.IsGroupHeader ? 0 : NavViewMetrics.ItemMarginV);

            if (item.HasChildren && item.IsExpanded)
                foreach (var child in item.Children)
                    AddItemRecursive(child, ref y, paneW, showLabels, isFooter);
        }

        private static List<RendererItemInfo> BuildRendererList(
            NavItemCollection collection, bool showLabels)
        {
            var list = new List<RendererItemInfo>();
            int y = 0;
            foreach (var item in collection)
            {
                if (item.IsGroupHeader && !showLabels) continue;

                RendererItemKind kind;
                if (item.IsSeparator) kind = RendererItemKind.Separator;
                else if (item.IsGroupHeader) kind = RendererItemKind.GroupHeader;
                else kind = RendererItemKind.FooterItem;

                int h = item.IsSeparator ? NavViewMetrics.SeparatorHeight
                      : item.IsGroupHeader ? NavViewMetrics.GroupHeaderHeight
                                           : NavViewMetrics.ItemHeight;
                list.Add(new RendererItemInfo
                {
                    Kind = kind,
                    Bounds = new Rectangle(0, y, 200, h),
                    CustomIcon = item.CustomIcon,
                    HasNotification = item.HasNotification,
                    Source = item
                });
                y += h;
            }
            return list;
        }

        private static bool HasAnyExpanded(params NavItemCollection[] collections)
        {
            foreach (var col in collections)
                foreach (var item in col.Flatten())
                    if (item.IsExpanded && item.HasChildren) return true;
            return false;
        }

        private static void CollapseCollection(NavItemCollection col)
        {
            foreach (var item in col.Flatten())
                item.IsExpanded = false;
        }

        // -------------------------------------------------------------------------
        // Scroll helpers
        // -------------------------------------------------------------------------

        private void ClampScrollOffset()
        {
            int maxOffset = Math.Max(0, _menuVirtualHeight - _menuViewportHeight);
            _scrollOffset = Math.Max(0, Math.Min(_scrollOffset, maxOffset));
        }

        private void ScrollBy(int delta)
        {
            int before = _scrollOffset;
            _scrollOffset += delta;
            ClampScrollOffset();
            if (_scrollOffset != before)
            {
                BuildVisibleItems();
                Invalidate(PaneBounds);
            }
        }

        // -------------------------------------------------------------------------
        // Paint
        // -------------------------------------------------------------------------
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;

            // In ExternalHost non si disegna nulla fuori dal pane
            if (_contentMode == ContentMode.InternalHost)
            {
                var ca = ContentAreaBounds;
                if (!ca.IsEmpty)
                {
                    using var contentBg = new SolidBrush(_renderer.Colors.ContentBackground);
                    g.FillRectangle(contentBg, ca);
                }

                if (_autoContentHeader && !string.IsNullOrWhiteSpace(_contentHeader))
                {
                    var chBounds = new Rectangle(
                        _currentPaneWidth, 0,
                        Width - _currentPaneWidth,
                        NavViewMetrics.ContentHeaderHeight);
                    _renderer.DrawContentHeader(g, chBounds, _contentHeader);
                }
            }

            _renderer.DrawPane(g, PaneBounds, _appTitle, _visibleItems,
                               _isPaneOpen || _displayMode == PaneDisplayMode.Left,
                               _compactWidth,
                               _hamburgerHovered,
                               _scrollBarVisible,
                               _scrollBarBounds,
                               _scrollThumbBounds,
                               NavViewMetrics.HeaderHeight,
                               NavViewMetrics.HeaderHeight + _menuViewportHeight);
        }

        // -------------------------------------------------------------------------
        // Mouse
        // -------------------------------------------------------------------------
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            bool hambHov = _hamburgerBounds.Contains(e.Location);
            if (hambHov != _hamburgerHovered)
            {
                _hamburgerHovered = hambHov;
                Invalidate(_hamburgerBounds);
            }

            var hit = HitTest(e.Location);
            if (hit != _hoveredItem)
            {
                _hoveredItem = hit;
                BuildVisibleItems();
                Invalidate(PaneBounds);
            }

            Cursor = (hit != null && hit.IsSelectable) || hambHov
                ? Cursors.Hand
                : Cursors.Default;

            if (hit != _tooltipItem)
            {
                _tooltipItem = hit;
                _toolTip.SetToolTip(this,
                    hit != null && !string.IsNullOrEmpty(hit.ToolTipText)
                        ? hit.ToolTipText
                        : string.Empty);
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hoveredItem = null;
            _hamburgerHovered = false;
            BuildVisibleItems();
            Invalidate(PaneBounds);
            Cursor = Cursors.Default;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button == MouseButtons.Right)
            {
                var item = HitTest(e.Location);
                if (item?.ContextMenuStrip != null)
                    item.ContextMenuStrip.Show(this, e.Location);
                return;
            }

            if (e.Button != MouseButtons.Left) return;

            if (_hamburgerBounds.Contains(e.Location))
            {
                TogglePane();
                return;
            }

            var clicked = HitTest(e.Location);
            if (clicked == null || !clicked.IsSelectable) return;

            if (clicked.HasChildren)
            {
                clicked.IsExpanded = !clicked.IsExpanded;

                if (_displayMode == PaneDisplayMode.LeftCompact)
                {
                    if (!_paneOpenedByHamburger)
                    {
                        _isPaneOpen = clicked.IsExpanded;
                        if (!_isPaneOpen) _scrollOffset = 0;
                    }
                }

                BuildVisibleItems();
                RecalcLayout();
                return;
            }

            SelectItem(clicked);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (e.X > _currentPaneWidth) return;
            if (!_scrollBarVisible) return;

            int steps = e.Delta / SystemInformation.MouseWheelScrollDelta;
            ScrollBy(-steps * ScrollStep);
        }

        protected override void OnDoubleClick(EventArgs e) { }
        protected override void OnMouseDoubleClick(MouseEventArgs e) { }

        // -------------------------------------------------------------------------
        // Selezione
        // -------------------------------------------------------------------------
        private void SelectItem(NavItem item)
        {
            if (item == _selectedItem)
            {
                if (!item.DeselectOnClick) return;
                var prev = _selectedItem;
                _selectedItem = null;
                if (_autoContentHeader) _contentHeader = string.Empty;

                if (_displayMode == PaneDisplayMode.LeftCompact && _isPaneOpen && !_paneOpenedByHamburger)
                {
                    CollapseCollection(MenuItems);
                    CollapseCollection(FooterMenuItems);
                    _isPaneOpen = false;
                    _scrollOffset = 0;

                    var ancestor = item.Parent;
                    while (ancestor != null && !ancestor.IsSelectable)
                        ancestor = ancestor.Parent;
                    if (ancestor != null)
                        _selectedItem = ancestor;
                }

                BuildVisibleItems();
                RecalcLayout();
                SelectionChanged?.Invoke(this,
                    new NavSelectionChangedEventArgs(null, prev));
                return;
            }

            var previous = _selectedItem;
            _selectedItem = item;
            if (_autoContentHeader) _contentHeader = item.Label;

            if (_displayMode == PaneDisplayMode.LeftCompact && _isPaneOpen && !_paneOpenedByHamburger)
            {
                CollapseCollection(MenuItems);
                CollapseCollection(FooterMenuItems);
                _isPaneOpen = false;
                _scrollOffset = 0;

                var ancestor = item.Parent;
                while (ancestor != null && !ancestor.IsSelectable)
                    ancestor = ancestor.Parent;
                if (ancestor != null)
                    _selectedItem = ancestor;
            }

            BuildVisibleItems();
            RecalcLayout();
            item.ExecuteAction?.Invoke(item);
            SelectionChanged?.Invoke(this,
                new NavSelectionChangedEventArgs(item, previous));
        }

        // -------------------------------------------------------------------------
        // Hit testing
        // -------------------------------------------------------------------------
        private NavItem? HitTest(Point p)
        {
            if (p.X > _currentPaneWidth) return null;
            if (_hamburgerBounds.Contains(p)) return null;
            if (_scrollBarVisible && _scrollBarBounds.Contains(p)) return null;

            int menuAreaTop = NavViewMetrics.HeaderHeight;
            int menuAreaBottom = menuAreaTop + _menuViewportHeight;

            foreach (var info in _visibleItems)
            {
                // StructuralSeparator: Source è null, non è cliccabile
                if (info.Kind == RendererItemKind.StructuralSeparator) continue;

                if (info.Kind == RendererItemKind.MenuItem || info.Kind == RendererItemKind.Separator || info.Kind == RendererItemKind.GroupHeader)
                {
                    if (p.Y < menuAreaTop || p.Y > menuAreaBottom) continue;
                }

                if (info.Bounds.Contains(p)) return info.Source;
            }

            return null;
        }

        // -------------------------------------------------------------------------
        // Helpers
        // -------------------------------------------------------------------------
        private static NavItem? FindById(string id, IEnumerable<NavItem> items)
        {
            foreach (var item in items)
                if (item.Id == id) return item;
            return null;
        }

        private void OnCollectionChanged(object? sender, EventArgs e)
        {
            _scrollOffset = 0;
            if (_autoSizePaneWidth) RecalcPaneWidth();
            BuildVisibleItems();
            Invalidate();
        }

        private void UpdateCursor() => Cursor = Cursors.Default;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_renderer is IDisposable d) d.Dispose();
                _toolTip.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}