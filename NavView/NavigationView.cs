// NavigationView.cs
// Controllo principale. Gestisce layout, hit testing, eventi mouse,
// selezione, espansione accordion, cambio tema, SetContent.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

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

        // Larghezza corrente del pane (animazione futura: valore interpolato)
        private int _currentPaneWidth;

        // Lista piatta delle voci visibili, con i Bounds calcolati.
        // Ricalcolata a ogni layout change.
        private readonly List<RendererItemInfo> _visibleItems = new();

        // Traccia se il mouse è sull'hamburger
        private bool _hamburgerHovered = false;
        private Rectangle _hamburgerBounds;

        // -------------------------------------------------------------------------
        // Collezioni pubbliche
        // -------------------------------------------------------------------------

        /// <summary>Voci principali del pannello (parte superiore).</summary>
        public NavItemCollection MenuItems { get; } = new NavItemCollection();

        /// <summary>Voci footer del pannello (parte inferiore: Settings, ecc.).</summary>
        public NavItemCollection FooterMenuItems { get; } = new NavItemCollection();

        // -------------------------------------------------------------------------
        // Proprietà pubbliche
        // -------------------------------------------------------------------------

        /// <summary>Testo mostrato nell'header del pane accanto all'hamburger.</summary>
        [Category("NavView")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string AppTitle
        {
            get => _appTitle;
            set { _appTitle = value ?? string.Empty; Invalidate(); }
        }

        /// <summary>Titolo dell'area contenuto (mostrato sopra il ContentArea).</summary>
        [Category("NavView")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string ContentHeader
        {
            get => _contentHeader;
            set { _contentHeader = value ?? string.Empty; Invalidate(); }
        }

        /// <summary>Modalità di visualizzazione del pane.</summary>
        [Category("NavView")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public PaneDisplayMode PaneDisplayMode
        {
            get => _displayMode;
            set { _displayMode = value; UpdatePaneState(); }
        }

        /// <summary>True se il pane è espanso.</summary>
        [Category("NavView")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool IsPaneOpen
        {
            get => _isPaneOpen;
            set { if (_isPaneOpen != value) TogglePane(); }
        }

        /// <summary>Larghezza del pane quando espanso.</summary>
        [Category("NavView")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int PaneWidth
        {
            get => _paneWidth;
            set { _paneWidth = Math.Max(100, value); RecalcLayout(); }
        }

        /// <summary>Larghezza del pane in modalità compatta (solo icone).</summary>
        [Category("NavView")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int CompactPaneWidth
        {
            get => _compactWidth;
            set { _compactWidth = Math.Max(32, value); RecalcLayout(); }
        }

        /// <summary>Tema visivo corrente.</summary>
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

        /// <summary>Voce attualmente selezionata. Null se nessuna.</summary>
        [Browsable(false)]
        public NavItem? SelectedItem => _selectedItem;

        /// <summary>
        /// Renderer corrente. Sostituibile con implementazione custom.
        /// </summary>
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

        /// <summary>Sollevato quando la voce selezionata cambia.</summary>
        public event EventHandler<NavSelectionChangedEventArgs>? SelectionChanged;

        /// <summary>Sollevato quando il pane si apre.</summary>
        public event EventHandler? PaneOpened;

        /// <summary>Sollevato quando il pane si chiude.</summary>
        public event EventHandler? PaneClosed;

        // -------------------------------------------------------------------------
        // Interfaccia keyboard (predisposta, non implementata in v1)
        // -------------------------------------------------------------------------

        /// <summary>
        /// Predisposto per navigazione da tastiera futura.
        /// Sovrascrivere per implementare Tab/Frecce/Enter.
        /// </summary>
        protected virtual void OnKeyboardNavigate(Keys key) { }

        // -------------------------------------------------------------------------
        // Costruttore
        // -------------------------------------------------------------------------

        public NavigationView()
        {
            _renderer = new NavViewRenderer();

            // Stile controllo: doppio buffer per evitare flickering GDI+
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);

            UpdateCursor();

            // Collega gli eventi delle collezioni
            MenuItems.CollectionChanged += OnCollectionChanged;
            FooterMenuItems.CollectionChanged += OnCollectionChanged;

            // Larghezza pane iniziale
            _currentPaneWidth = _compactWidth;
        }

        // -------------------------------------------------------------------------
        // API pubblica
        // -------------------------------------------------------------------------

        /// <summary>
        /// Imposta il controllo da mostrare nell'area contenuto.
        /// Il controllo viene ridimensionato per occupare tutto il ContentArea.
        /// </summary>
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

        /// <summary>Seleziona una voce per riferimento.</summary>
        public void Navigate(NavItem item)
        {
            if (item == null || !item.IsSelectable) return;
            SelectItem(item);
        }

        /// <summary>Seleziona una voce per Id.</summary>
        public void Navigate(string itemId)
        {
            var item = FindById(itemId,
                MenuItems.Flatten()) ??
                FindById(itemId, FooterMenuItems.Flatten());
            if (item != null) Navigate(item);
        }

        /// <summary>Chiude tutti i nodi accordion aperti.</summary>
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
            if (_displayMode == PaneDisplayMode.Left)
                return; // in Left il pane è sempre aperto

            _isPaneOpen = !_isPaneOpen;
            _currentPaneWidth = _isPaneOpen ? _paneWidth : _compactWidth;
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

        /// <summary>
        /// Ricalcola la posizione del ContentArea e del controllo contenuto.
        /// Chiamato a ogni resize, toggle pane, aggiunta voci.
        /// </summary>
        private void RecalcLayout()
        {
            BuildVisibleItems();

            if (_content != null)
            {
                var ca = ContentAreaBounds;
                int headerH = string.IsNullOrWhiteSpace(_contentHeader)
                    ? 0 : NavViewMetrics.ContentHeaderHeight;

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

        // -------------------------------------------------------------------------
        // Bounds helpers
        // -------------------------------------------------------------------------

        private Rectangle PaneBounds => new Rectangle(0, 0, _currentPaneWidth, Height);

        private Rectangle ContentAreaBounds => new Rectangle(
            _currentPaneWidth, 0,
            Math.Max(0, Width - _currentPaneWidth), Height);

        // -------------------------------------------------------------------------
        // Costruzione lista voci visibili
        // -------------------------------------------------------------------------

        /// <summary>
        /// Costruisce _visibleItems: la lista piatta e ordinata di tutte le voci
        /// attualmente visibili, con i Bounds calcolati in coordinate pane.
        /// Gestisce la logica accordion: i figli appaiono solo se IsExpanded == true.
        /// Per LeftCompact con nodo espanso: allarga temporaneamente il pane.
        /// </summary>
        private void BuildVisibleItems()
        {
            _visibleItems.Clear();

            // Determina se c'è un nodo espanso in LeftCompact
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

            int y = NavViewMetrics.HeaderHeight;

            // Menu items principali
            AddItemsToVisible(MenuItems, ref y, paneW, showLabels);

            // Footer items: in fondo al pane se c'è spazio, altrimenti subito dopo i menu
            if (FooterMenuItems.Count > 0)
            {
                int footerH = _renderer.MeasureItemsHeight(BuildRendererList(FooterMenuItems, showLabels));
                int footerY = Math.Max(y + 8, Height - footerH - 8);

                // Separatore automatico prima del footer se non c'è già
                if (footerY > y + 8)
                {
                    _visibleItems.Add(new RendererItemInfo
                    {
                        IsSeparator = true,
                        Bounds = new Rectangle(0, footerY - NavViewMetrics.SeparatorHeight - 4,
                                               paneW, NavViewMetrics.SeparatorHeight),
                        Source = null!
                    });
                }

                AddItemsToVisible(FooterMenuItems, ref footerY, paneW, showLabels);
            }

            // Aggiorna bounds hamburger per hit testing
            _hamburgerBounds = new Rectangle(
                NavViewMetrics.HamburgerPadding,
                (NavViewMetrics.HeaderHeight - NavViewMetrics.HamburgerSize) / 2,
                NavViewMetrics.HamburgerSize,
                NavViewMetrics.HamburgerSize);
        }

        private void AddItemsToVisible(NavItemCollection collection,
                                       ref int y, int paneW, bool showLabels)
        {
            foreach (var item in collection)
            {
                AddItemRecursive(item, ref y, paneW, showLabels);
            }
        }

        private void AddItemRecursive(NavItem item, ref int y, int paneW, bool showLabels)
        {
            int h = item.IsSeparator ? NavViewMetrics.SeparatorHeight
                  : item.IsGroupHeader ? NavViewMetrics.GroupHeaderHeight
                                       : NavViewMetrics.ItemHeight;

            // Group header invisibile in modalità compatta senza label
            if (item.IsGroupHeader && !showLabels)
                return;

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

            // Figli: visibili solo se espanso
            if (item.HasChildren && item.IsExpanded)
            {
                foreach (var child in item.Children)
                    AddItemRecursive(child, ref y, paneW, showLabels);
            }
        }

        private static List<RendererItemInfo> BuildRendererList(
            NavItemCollection collection, bool showLabels)
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

        // -------------------------------------------------------------------------
        // Paint
        // -------------------------------------------------------------------------

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;

            // Sfondo generale (area contenuto)
            using var contentBg = new SolidBrush(_renderer.Colors.ContentBackground);
            g.FillRectangle(contentBg, ContentAreaBounds);

            // Content header
            if (!string.IsNullOrWhiteSpace(_contentHeader))
            {
                var chBounds = new Rectangle(
                    _currentPaneWidth, 0,
                    Width - _currentPaneWidth,
                    NavViewMetrics.ContentHeaderHeight);
                _renderer.DrawContentHeader(g, chBounds, _contentHeader);
            }

            // Pane
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
                BuildVisibleItems(); // aggiorna IsHovered nei RendererItemInfo
                Invalidate(PaneBounds);
            }

            Cursor = (hit != null && hit.IsSelectable) || hambHov
                ? Cursors.Hand
                : Cursors.Default;
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

            // Click sull'hamburger
            if (_hamburgerBounds.Contains(e.Location))
            {
                TogglePane();
                return;
            }

            // Click su una voce
            var item = HitTest(e.Location);
            if (item == null) return;

            if (!item.IsSelectable) return;

            // Nodo con figli: espandi/comprimi (accordion)
            if (item.HasChildren)
            {
                item.IsExpanded = !item.IsExpanded;

                // In LeftCompact con pane chiuso: se si espande,
                // il pane si allarga automaticamente tramite BuildVisibleItems
                BuildVisibleItems();
                RecalcLayout();
                return;
            }

            // Voce foglia: seleziona
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

            // Aggiorna ContentHeader con il label della voce selezionata
            _contentHeader = item.Label;

            BuildVisibleItems();
            RecalcLayout();

            SelectionChanged?.Invoke(this,
                new NavSelectionChangedEventArgs(item, prev));
        }

        // -------------------------------------------------------------------------
        // Hit testing
        // -------------------------------------------------------------------------

        /// <summary>
        /// Restituisce il NavItem sotto il punto dato (coordinate controllo),
        /// o null se il punto non è su nessuna voce del pane.
        /// </summary>
        private NavItem? HitTest(Point p)
        {
            if (p.X > _currentPaneWidth) return null; // fuori dal pane
            if (_hamburgerBounds.Contains(p)) return null; // sull'hamburger

            foreach (var info in _visibleItems)
            {
                if (info.Source == null) continue;
                if (info.Bounds.Contains(p))
                    return info.Source;
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

        private static void CollapseCollection(NavItemCollection col)
        {
            foreach (var item in col.Flatten())
                item.IsExpanded = false;
        }

        private void OnCollectionChanged(object? sender, EventArgs e)
        {
            BuildVisibleItems();
            Invalidate();
        }

        private void UpdateCursor() => Cursor = Cursors.Default;

        // -------------------------------------------------------------------------
        // Dispose
        // -------------------------------------------------------------------------

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _renderer?.Dispose();
            base.Dispose(disposing);
        }
    }
}