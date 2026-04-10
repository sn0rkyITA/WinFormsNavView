// NavigationView.cs
// Controllo principale. Gestisce layout, hit testing, eventi mouse,
// selezione, espansione accordion, cambio tema, SetContent.
using System.ComponentModel;
namespace NavView
{
    /// <summary>
    /// NavigationView per WinForms.
    /// Pannello laterale collassabile con voci di menu, footer, gerarchia,
    /// header contenuto e area contenuto neutrale.
    /// </summary>
    public class NavigationView : Control
    {
        // -------------------------------------------------------------------------
        // Campi privati
        // -------------------------------------------------------------------------
        private NavViewRenderer _renderer;
        private NavViewTheme _theme = NavViewTheme.Light;
        private PaneDisplayMode _displayMode = PaneDisplayMode.LeftCompact;
        private bool _isPaneOpen = false;
        private int _paneWidth = 240;
        private int _compactWidth = 48;
        private string _appTitle = string.Empty;
        private string _contentHeader = string.Empty;
        private NavItem? _selectedItem;
        private NavItem? _hoveredItem;
        private Control? _content;

        // Larghezza corrente del pane
        private int _currentPaneWidth;

        // Lista piatta delle voci visibili, con i Bounds calcolati.
        private readonly List<RendererItemInfo> _visibleItems = new();

        // Traccia se il mouse è sull'hamburger
        private bool _hamburgerHovered = false;
        private Rectangle _hamburgerBounds;

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

        [Category("NavView")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string ContentHeader
        {
            get => _contentHeader;
            set { _contentHeader = value ?? string.Empty; Invalidate(); }
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
            set { _paneWidth = Math.Max(100, value); RecalcLayout(); }
        }

        [Category("NavView")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int CompactPaneWidth
        {
            get => _compactWidth;
            set { _compactWidth = Math.Max(32, value); RecalcLayout(); }
        }

        [Category("NavView")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public NavViewTheme Theme
        {
            get => _theme;
            set
            {
                _theme = value;
                _renderer.Colors = value == NavViewTheme.Dark ? NavViewColors.Dark() : NavViewColors.Light();
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
                _renderer?.Dispose();
                _renderer = (NavViewRenderer)value;
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
        public void SetContent(Control? control)
        {
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
            var item = FindById(itemId, MenuItems.Flatten()) ?? FindById(itemId, FooterMenuItems.Flatten());
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
        // Toggle pane (FIX #1)
        // -------------------------------------------------------------------------
        private void TogglePane()
        {
            if (_displayMode == PaneDisplayMode.Left) return;

            bool wasOpen = _isPaneOpen;
            if (wasOpen)
            {
                // 1. Chiude tutti i nodi espansi
                CollapseCollection(MenuItems);
                CollapseCollection(FooterMenuItems);

                // 2. Se è selezionato un figlio, sposta la selezione sul padre
                if (_selectedItem != null)
                {
                    var parent = _selectedItem.Parent;
                    while (parent != null && !parent.IsSelectable)
                        parent = parent.Parent;

                    if (parent != null)
                    {
                        var prev = _selectedItem;
                        _selectedItem = parent;
                        _contentHeader = parent.Label;
                        SelectionChanged?.Invoke(this, new NavSelectionChangedEventArgs(_selectedItem, prev));
                    }
                }
            }

            _isPaneOpen = !_isPaneOpen;

            // Ricalcola tutto: BuildVisibleItems vedrà !isPaneOpen e HasAnyExpanded==false,
            // quindi imposterà correttamente _currentPaneWidth = _compactWidth.
            RecalcLayout();

            if (_isPaneOpen) PaneOpened?.Invoke(this, EventArgs.Empty);
            else PaneClosed?.Invoke(this, EventArgs.Empty);
        }

        private void UpdatePaneState()
        {
            if (_displayMode == PaneDisplayMode.Left)
            {
                _isPaneOpen = true;
                _currentPaneWidth = _paneWidth;
            }
            else
            {
                _isPaneOpen = false;
                _currentPaneWidth = _compactWidth;
            }
            RecalcLayout();
        }

        // -------------------------------------------------------------------------
        // Layout
        // -------------------------------------------------------------------------
        private void RecalcLayout()
        {
            BuildVisibleItems();

            if (_content != null)
            {
                var ca = ContentAreaBounds;
                int headerH = string.IsNullOrWhiteSpace(_contentHeader) ? 0 : NavViewMetrics.ContentHeaderHeight;

                _content.SetBounds(
                    ca.Left, ca.Top + headerH,
                    ca.Width, ca.Height - headerH);
            }

            Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            RecalcLayout();
        }

        private Rectangle PaneBounds => new Rectangle(0, 0, _currentPaneWidth, Height);
        private Rectangle ContentAreaBounds => new Rectangle(
            _currentPaneWidth, 0,
            Math.Max(0, Width - _currentPaneWidth), Height);

        // -------------------------------------------------------------------------
        // Costruzione lista voci visibili (FIX #1 & #2)
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

            int paneHeight = Math.Max(0, Height);
            int y = NavViewMetrics.HeaderHeight;

            // Calcola spazio riservato al footer e limite verticale per il menu
            int footerH = 0;
            int footerStartY = paneHeight;
            int menuLimitY = paneHeight;

            if (FooterMenuItems.Count > 0)
            {
                footerH = _renderer.MeasureItemsHeight(BuildRendererList(FooterMenuItems, showLabels));
                // Ancoraggio in basso con margine inferiore di 8px
                footerStartY = Math.Max(y + 16, paneHeight - footerH - 8);
                menuLimitY = footerStartY - 4; // 4px di gap tra menu e footer
            }

            // 1. Voci menu (con limite verticale per non sovrascrivere il footer)
            AddItemsToVisible(MenuItems, ref y, paneW, showLabels, menuLimitY);

            // 2. Voci footer (ancorate stabilmente in basso)
            if (FooterMenuItems.Count > 0)
            {
                // Separatore automatico se c'è spazio tra l'ultima voce menu e il footer
                if (footerStartY > y + 8)
                {
                    _visibleItems.Add(new RendererItemInfo
                    {
                        IsSeparator = true,
                        Bounds = new Rectangle(0, footerStartY - NavViewMetrics.SeparatorHeight - 2,
                                               paneW, NavViewMetrics.SeparatorHeight),
                        Source = null!
                    });
                }

                int fy = footerStartY;
                AddItemsToVisible(FooterMenuItems, ref fy, paneW, showLabels, int.MaxValue);
            }

            // Aggiorna bounds hamburger per hit testing
            _hamburgerBounds = new Rectangle(
                NavViewMetrics.HamburgerPadding,
                (NavViewMetrics.HeaderHeight - NavViewMetrics.HamburgerSize) / 2,
                NavViewMetrics.HamburgerSize,
                NavViewMetrics.HamburgerSize);
        }

        private void AddItemsToVisible(NavItemCollection collection,
                                       ref int y, int paneW, bool showLabels, int limitY = int.MaxValue)
        {
            foreach (var item in collection)
                AddItemRecursive(item, ref y, paneW, showLabels, limitY);
        }

        private void AddItemRecursive(NavItem item, ref int y, int paneW, bool showLabels, int limitY)
        {
            int h = item.IsSeparator ? NavViewMetrics.SeparatorHeight
                  : item.IsGroupHeader ? NavViewMetrics.GroupHeaderHeight
                                       : NavViewMetrics.ItemHeight;

            if (item.IsGroupHeader && !showLabels) return;
            if (y + h > limitY) return; // Blocca il layout se supera il limite calcolato

            var info = new RendererItemInfo
            {
                Label = item.Label,
                IconGlyph = item.IconGlyph,
                IsSelected = item == _selectedItem,
                IsHovered = item == _hoveredItem,
                IsEnabled = item.IsEnabled,
                IsSeparator = item.IsSeparator,
                IsGroupHeader = item.IsGroupHeader,
                HasChildren = item.HasChildren,
                IsExpanded = item.IsExpanded,
                Depth = item.Depth,
                Bounds = new Rectangle(0, y, paneW, h),
                Source = item
            };

            _visibleItems.Add(info);
            y += h + (item.IsSeparator || item.IsGroupHeader ? 0 : NavViewMetrics.ItemMarginV);

            if (item.HasChildren && item.IsExpanded)
            {
                foreach (var child in item.Children)
                    AddItemRecursive(child, ref y, paneW, showLabels, limitY);
            }
        }

        private static List<RendererItemInfo> BuildRendererList(NavItemCollection collection, bool showLabels)
        {
            var list = new List<RendererItemInfo>();
            int y = 0;
            foreach (var item in collection)
            {
                if (item.IsGroupHeader && !showLabels) continue;
                int h = item.IsSeparator ? NavViewMetrics.SeparatorHeight
                      : item.IsGroupHeader ? NavViewMetrics.GroupHeaderHeight
                                           : NavViewMetrics.ItemHeight;
                list.Add(new RendererItemInfo
                {
                    IsSeparator = item.IsSeparator,
                    IsGroupHeader = item.IsGroupHeader,
                    Bounds = new Rectangle(0, y, 200, h),
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
        // Paint
        // -------------------------------------------------------------------------
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            using var contentBg = new SolidBrush(_renderer.Colors.ContentBackground);
            g.FillRectangle(contentBg, ContentAreaBounds);

            if (!string.IsNullOrWhiteSpace(_contentHeader))
            {
                var chBounds = new Rectangle(_currentPaneWidth, 0, Width - _currentPaneWidth, NavViewMetrics.ContentHeaderHeight);
                _renderer.DrawContentHeader(g, chBounds, _contentHeader);
            }

            _renderer.DrawPane(g, PaneBounds, _appTitle, _visibleItems,
                               _isPaneOpen || _displayMode == PaneDisplayMode.Left,
                               _compactWidth);
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

            Cursor = (hit != null && hit.IsSelectable) || hambHov ? Cursors.Hand : Cursors.Default;
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

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (e.Button != MouseButtons.Left) return;

            if (_hamburgerBounds.Contains(e.Location))
            {
                TogglePane();
                return;
            }

            var item = HitTest(e.Location);
            if (item == null || !item.IsSelectable) return;

            if (item.HasChildren)
            {
                item.IsExpanded = !item.IsExpanded;
                BuildVisibleItems();
                RecalcLayout();
                return;
            }

            SelectItem(item);
        }

        // -------------------------------------------------------------------------
        // Selezione
        // -------------------------------------------------------------------------
        private void SelectItem(NavItem item)
        {
            if (item == _selectedItem) return;
            var prev = _selectedItem;
            _selectedItem = item;
            _contentHeader = item.Label;
            BuildVisibleItems();
            RecalcLayout();
            SelectionChanged?.Invoke(this, new NavSelectionChangedEventArgs(item, prev));
        }

        // -------------------------------------------------------------------------
        // Hit testing
        // -------------------------------------------------------------------------
        private NavItem? HitTest(Point p)
        {
            if (p.X > _currentPaneWidth) return null;
            if (_hamburgerBounds.Contains(p)) return null;

            foreach (var info in _visibleItems)
                if (info.Source != null && info.Bounds.Contains(p)) return info.Source;

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
            BuildVisibleItems();
            Invalidate();
        }

        private void UpdateCursor() => Cursor = Cursors.Default;

        protected override void Dispose(bool disposing)
        {
            if (disposing) _renderer?.Dispose();
            base.Dispose(disposing);
        }
    }
}