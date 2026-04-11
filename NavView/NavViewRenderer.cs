// NavViewRenderer.cs
// Contiene: INavViewRenderer, NavViewRenderer (implementazione GDI+)
//
// CHANGELOG:
// - INavViewRenderer.DrawPane aggiornato: hamburgerHovered, scrollbar params, clipY
// - DrawPane applica clip region sull'area menu item (evita sconfinamento su header/footer)
// - DrawScrollBar: scrollbar visuale sottile sul bordo destro del pane
// - Fix hover su item disabilitato: guard in DrawNavItem
// - Fix hamburger hover: DrawPaneHeader usa il colore HamburgerHoverBackground

using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace NavView
{
    // -------------------------------------------------------------------------
    // Costanti di layout
    // -------------------------------------------------------------------------

    /// <summary>
    /// Misure fisse del layout. Centralizzate per facilitare future modifiche.
    /// </summary>
    internal static class NavViewMetrics
    {
        /// <summary>Altezza di ogni voce di menu in pixel.</summary>
        public const int ItemHeight = 40;

        /// <summary>Altezza dell'area header (hamburger + AppTitle).</summary>
        public const int HeaderHeight = 48;

        /// <summary>Altezza dell'header del contenuto (titolo sezione).</summary>
        public const int ContentHeaderHeight = 48;

        /// <summary>Altezza della linea separatrice.</summary>
        public const int SeparatorHeight = 9;

        /// <summary>Altezza del group header (label di gruppo).</summary>
        public const int GroupHeaderHeight = 34;

        /// <summary>Dimensione icona Segoe Fluent in pt.</summary>
        public const float IconFontSize = 16f;

        /// <summary>Dimensione testo label voce in pt.</summary>
        public const float LabelFontSize = 10f;

        /// <summary>Dimensione testo AppTitle in pt.</summary>
        public const float AppTitleFontSize = 13f;

        /// <summary>Dimensione testo ContentHeader in pt.</summary>
        public const float ContentHeaderFontSize = 14f;

        /// <summary>Dimensione testo group header in pt.</summary>
        public const float GroupHeaderFontSize = 11f;

        /// <summary>Larghezza dell'accento laterale (barra sinistra voce selezionata).</summary>
        public const int AccentBarWidth = 3;

        /// <summary>Raggio angoli arrotondati delle voci.</summary>
        public const int ItemCornerRadius = 4;

        /// <summary>Margine orizzontale esterno delle voci.</summary>
        public const int ItemMarginH = 4;

        /// <summary>Margine verticale tra le voci.</summary>
        public const int ItemMarginV = 2;

        /// <summary>Padding orizzontale sinistro dell'icona rispetto al bordo pane.</summary>
        public const int IconPaddingLeft = 12;

        /// <summary>Padding tra icona e label.</summary>
        public const int IconLabelGap = 12;

        /// <summary>Indentazione per ogni livello di profondità figlio.</summary>
        public const int DepthIndent = 16;

        /// <summary>Larghezza area chevron (freccia espansione).</summary>
        public const int ChevronWidth = 24;

        /// <summary>Padding destro del bottone hamburger.</summary>
        public const int HamburgerPadding = 12;

        /// <summary>Dimensione quadrata clickable del bottone hamburger.</summary>
        public const int HamburgerSize = 40;

    }

    // -------------------------------------------------------------------------
    // INavViewRenderer
    // -------------------------------------------------------------------------

    /// <summary>
    /// Interfaccia del renderer. Implementazioni alternative (es. tema ad alto
    /// contrasto, renderer custom) devono implementare questa interfaccia.
    /// </summary>
    public interface INavViewRenderer
    {
        /// <summary>Palette colori corrente.</summary>
        NavViewColors Colors { get; set; }

        /// <summary>
        /// Disegna l'intero pannello laterale (pane).
        /// Chiamato da NavigationView.OnPaint.
        /// </summary>
        void DrawPane(Graphics g, Rectangle paneBounds,
                      string appTitle,
                      IReadOnlyList<RendererItemInfo> items,
                      bool isPaneOpen,
                      int compactWidth,
                      bool hamburgerHovered,
                      bool scrollBarVisible,
                      Rectangle scrollBarBounds,
                      Rectangle scrollThumbBounds,
                      int menuClipTop,
                      int menuClipBottom);

        /// <summary>
        /// Disegna l'header del contenuto (titolo sezione corrente).
        /// </summary>
        void DrawContentHeader(Graphics g, Rectangle bounds, string headerText);

        /// <summary>
        /// Calcola l'altezza totale occupata da una lista di item.
        /// </summary>
        int MeasureItemsHeight(IReadOnlyList<RendererItemInfo> items);
    }

    // -------------------------------------------------------------------------
    // RendererItemInfo
    // -------------------------------------------------------------------------

    /// <summary>
    /// Struttura dati passata al renderer per ogni voce visibile.
    /// </summary>
    public class RendererItemInfo
    {
        public string Label { get; set; } = string.Empty;
        public string IconGlyph { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
        public bool IsHovered { get; set; }
        public bool IsEnabled { get; set; } = true;
        public bool IsSeparator { get; set; }
        public bool IsGroupHeader { get; set; }
        public bool HasChildren { get; set; }
        public bool IsExpanded { get; set; }
        public int Depth { get; set; }
        public Image? CustomIcon { get; set; }
        public bool HasNotification { get; set; }

        /// <summary>
        /// Rectangle calcolato dal controllo per questa voce (coordinate nel pane).
        /// Per i menu item le coordinate includono l'offset di scroll.
        /// </summary>
        public Rectangle Bounds { get; set; }

        /// <summary>Riferimento al NavItem originale.</summary>
        public NavItem Source { get; set; } = null!;

        /// <summary>
        /// True se la voce appartiene al footer (coordinate assolute, non scrollabile).
        /// False per menu item e separatore strutturale.
        /// </summary>
        public bool IsFooterItem { get; set; }
    }

    // -------------------------------------------------------------------------
    // NavViewRenderer — implementazione GDI+
    // -------------------------------------------------------------------------

    /// <summary>
    /// Implementazione GDI+ del renderer.
    /// </summary>
    public class NavViewRenderer : INavViewRenderer
    {
        public NavViewColors Colors { get; set; } = NavViewColors.Light();

        // Cache font
        private Font? _iconFont;
        private Font? _labelFont;
        private Font? _appTitleFont;
        private Font? _contentHeaderFont;
        private Font? _groupHeaderFont;

        private const string LabelFontFamily = "Segoe UI";

        // -------------------------------------------------------------------------
        // Font helpers
        // -------------------------------------------------------------------------

        private Font IconFont => _iconFont
            ??= NavViewFontResolver.CreateIconFont(NavViewMetrics.IconFontSize);

        private Font LabelFont => _labelFont
            ??= new Font(LabelFontFamily, NavViewMetrics.LabelFontSize,
                         FontStyle.Regular, GraphicsUnit.Point);

        private Font AppTitleFont => _appTitleFont
            ??= new Font(LabelFontFamily, NavViewMetrics.AppTitleFontSize,
                         FontStyle.Bold, GraphicsUnit.Point);

        private Font ContentHeaderFont => _contentHeaderFont
            ??= new Font(LabelFontFamily, NavViewMetrics.ContentHeaderFontSize,
                         FontStyle.Bold, GraphicsUnit.Point);

        private Font GroupHeaderFont => _groupHeaderFont
            ??= new Font(LabelFontFamily, NavViewMetrics.GroupHeaderFontSize,
                         FontStyle.Regular, GraphicsUnit.Point);

        // -------------------------------------------------------------------------
        // DrawPane — entry point principale
        // -------------------------------------------------------------------------

        public void DrawPane(Graphics g, Rectangle paneBounds,
                             string appTitle,
                             IReadOnlyList<RendererItemInfo> items,
                             bool isPaneOpen,
                             int compactWidth,
                             bool hamburgerHovered,
                             bool scrollBarVisible,
                             Rectangle scrollBarBounds,
                             Rectangle scrollThumbBounds,
                             int menuClipTop,
                             int menuClipBottom)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            // 1. Sfondo pane
            using var bgBrush = new SolidBrush(Colors.PaneBackground);
            g.FillRectangle(bgBrush, paneBounds);

            // 2. Bordo destro
            using var borderPen = new Pen(Colors.PaneBorder, 1);
            g.DrawLine(borderPen,
                paneBounds.Right - 1, paneBounds.Top,
                paneBounds.Right - 1, paneBounds.Bottom);

            // 3. Header
            var headerBounds = new Rectangle(
                paneBounds.Left, paneBounds.Top,
                paneBounds.Width, NavViewMetrics.HeaderHeight);
            DrawPaneHeader(g, headerBounds, appTitle, isPaneOpen, hamburgerHovered);

            // 4. Voci con clip region per i menu item
            var clipRegion = new Region(paneBounds);

            // Clip region attiva: esclude l'header e la zona footer
            var menuClipRect = new Rectangle(
                paneBounds.Left, menuClipTop,
                paneBounds.Width, menuClipBottom - menuClipTop);

            // Salva lo stato grafico corrente
            var originalClip = g.Clip;

            // Disegna prima i footer item e i separatori strutturali (fuori dalla clip)
            foreach (var item in items)
            {
                if (item.IsFooterItem || (!item.IsFooterItem && item.IsSeparator && item.Source == null!))
                    DrawItem(g, item, isPaneOpen, paneBounds.Width);
            }

            // Applica la clip region per i menu item
            g.SetClip(menuClipRect);

            foreach (var item in items)
            {
                if (!item.IsFooterItem && !(item.IsSeparator && item.Source == null!))
                    DrawItem(g, item, isPaneOpen, paneBounds.Width);
            }

            // Ripristina la clip originale
            g.Clip = originalClip;

            // 5. Scrollbar (fuori dalla clip)
            if (scrollBarVisible)
                DrawScrollBar(g, scrollBarBounds, scrollThumbBounds);
        }

        // -------------------------------------------------------------------------
        // DrawPaneHeader
        // -------------------------------------------------------------------------

        private void DrawPaneHeader(Graphics g, Rectangle bounds,
                                    string appTitle, bool isPaneOpen,
                                    bool hamburgerHovered)
        {
            using var bg = new SolidBrush(Colors.PaneHeaderBackground);
            g.FillRectangle(bg, bounds);

            int hambX = bounds.Left + NavViewMetrics.HamburgerPadding;
            int hambY = bounds.Top + (bounds.Height - NavViewMetrics.HamburgerSize) / 2;
            var hambBounds = new Rectangle(hambX, hambY,
                NavViewMetrics.HamburgerSize, NavViewMetrics.HamburgerSize);

            // FIX: sfondo hover hamburger
            if (hamburgerHovered)
            {
                using var hoverBrush = new SolidBrush(Colors.HamburgerHoverBackground);
                using var hoverPath = RoundedRect(hambBounds, NavViewMetrics.ItemCornerRadius);
                g.FillPath(hoverBrush, hoverPath);
            }

            DrawIconGlyph(g, FluentIcons.Menu, hambBounds,
                Colors.HamburgerForeground, IconFont);

            if (isPaneOpen && !string.IsNullOrWhiteSpace(appTitle))
            {
                int titleX = hambBounds.Right + 8;
                int titleW = bounds.Width - titleX - 8;
                if (titleW > 0)
                {
                    var titleBounds = new Rectangle(titleX, bounds.Top, titleW, bounds.Height);
                    using var titleBrush = new SolidBrush(Colors.PaneHeaderForeground);
                    var sf = new StringFormat
                    {
                        Alignment = StringAlignment.Near,
                        LineAlignment = StringAlignment.Center,
                        Trimming = StringTrimming.EllipsisCharacter,
                        FormatFlags = StringFormatFlags.NoWrap
                    };
                    g.DrawString(appTitle, AppTitleFont, titleBrush, titleBounds, sf);
                }
            }
        }

        // -------------------------------------------------------------------------
        // DrawItem — dispatcher per tipo
        // -------------------------------------------------------------------------

        private void DrawItem(Graphics g, RendererItemInfo item,
                              bool isPaneOpen, int paneWidth)
        {
            if (item.IsSeparator)
            {
                DrawSeparator(g, item.Bounds, paneWidth);
                return;
            }

            if (item.IsGroupHeader)
            {
                DrawGroupHeader(g, item, isPaneOpen);
                return;
            }

            DrawNavItem(g, item, isPaneOpen, paneWidth);
        }

        // -------------------------------------------------------------------------
        // DrawNavItem — voce normale
        // -------------------------------------------------------------------------

        private void DrawNavItem(Graphics g, RendererItemInfo item,
                                 bool isPaneOpen, int paneWidth)
        {
            var bounds = item.Bounds;

            // --- Sfondo arrotondato ------------------------------------------
            // FIX: no sfondo hover se item disabilitato
            if (item.IsSelected || (item.IsHovered && item.IsEnabled))
            {
                var bgColor = item.IsSelected
                    ? Colors.ItemSelectedBackground
                    : Colors.ItemHoverBackground;

                var bgRect = new Rectangle(
                    bounds.Left + NavViewMetrics.ItemMarginH,
                    bounds.Top + NavViewMetrics.ItemMarginV,
                    bounds.Width - NavViewMetrics.ItemMarginH * 2,
                    bounds.Height - NavViewMetrics.ItemMarginV * 2);

                using var bgBrush = new SolidBrush(bgColor);
                using var path = RoundedRect(bgRect, NavViewMetrics.ItemCornerRadius);
                g.FillPath(bgBrush, path);
            }

            // --- Accento laterale (voce selezionata) -------------------------
            if (item.IsSelected)
            {
                int accentX = bounds.Left + NavViewMetrics.ItemMarginH;
                int accentH = bounds.Height - NavViewMetrics.ItemMarginV * 4;
                int accentY = bounds.Top + (bounds.Height - accentH) / 2;

                using var accentBrush = new SolidBrush(Colors.ItemSelectedAccent);
                using var accentPath = RoundedRect(
                    new Rectangle(accentX, accentY,
                                  NavViewMetrics.AccentBarWidth, accentH), 2);
                g.FillPath(accentBrush, accentPath);
            }

            // --- Icona -------------------------------------------------------
            var iconColor = item.IsSelected ? Colors.IconSelectedForeground
                          : !item.IsEnabled ? Colors.IconDisabledForeground
                                            : Colors.IconForeground;

            int indent = item.Depth * NavViewMetrics.DepthIndent;
            int iconLeft = bounds.Left + NavViewMetrics.IconPaddingLeft + indent;
            int iconSize = NavViewMetrics.HamburgerSize;
            var iconBounds = new Rectangle(iconLeft,
                bounds.Top + (bounds.Height - iconSize) / 2,
                iconSize, iconSize);

            // CustomIcon ha priorità su IconGlyph
            if (item.CustomIcon != null)
            {
                g.DrawImage(item.CustomIcon, iconBounds);
            }
            else if (!string.IsNullOrEmpty(item.IconGlyph))
            {
                DrawIconGlyph(g, item.IconGlyph, iconBounds, iconColor, IconFont);
            }

            // Dot badge — cerchio sovrapposto in alto a destra dell'icona
            if (item.HasNotification)
            {
                const int dotSize = 8;
                var dotBounds = new Rectangle(
                    iconBounds.Right - dotSize,
                    iconBounds.Top + 2,
                    dotSize, dotSize);

                using var dotBrush = new SolidBrush(Colors.NotificationDotColor);
                g.FillEllipse(dotBrush, dotBounds);
            }

            // --- Label (solo pane aperto) ------------------------------------
            if (isPaneOpen)
            {
                var labelColor = item.IsSelected ? Colors.ItemSelectedForeground
                               : !item.IsEnabled ? Colors.ItemDisabledForeground
                                                 : Colors.ItemForeground;

                int labelLeft = iconBounds.Right + NavViewMetrics.IconLabelGap;
                int chevronW = item.HasChildren ? NavViewMetrics.ChevronWidth : 0;
                int labelWidth = bounds.Right - labelLeft - chevronW - 8;

                if (labelWidth > 0)
                {
                    var labelBounds = new Rectangle(labelLeft, bounds.Top,
                                                    labelWidth, bounds.Height);
                    using var labelBrush = new SolidBrush(labelColor);
                    var sf = new StringFormat
                    {
                        Alignment = StringAlignment.Near,
                        LineAlignment = StringAlignment.Center,
                        Trimming = StringTrimming.EllipsisCharacter,
                        FormatFlags = StringFormatFlags.NoWrap
                    };
                    g.DrawString(item.Label, LabelFont, labelBrush, labelBounds, sf);
                }

                // --- Chevron espansione --------------------------------------
                if (item.HasChildren)
                {
                    var chevGlyph = item.IsExpanded
                        ? FluentIcons.ChevronDown
                        : FluentIcons.ChevronRight;

                    var chevBounds = new Rectangle(
                        bounds.Right - NavViewMetrics.ChevronWidth - 4,
                        bounds.Top,
                        NavViewMetrics.ChevronWidth,
                        bounds.Height);

                    using var chevBrush = new SolidBrush(Colors.ChevronForeground);
                    var sfChev = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    g.DrawString(chevGlyph, IconFont, chevBrush, chevBounds, sfChev);
                }
            }
        }

        // -------------------------------------------------------------------------
        // DrawSeparator
        // -------------------------------------------------------------------------

        private void DrawSeparator(Graphics g, Rectangle bounds, int paneWidth)
        {
            int y = bounds.Top + bounds.Height / 2;
            int x1 = NavViewMetrics.ItemMarginH + 8;
            int x2 = paneWidth - NavViewMetrics.ItemMarginH - 8;

            using var pen = new Pen(Colors.SeparatorColor, 1);
            g.DrawLine(pen, x1, y, x2, y);
        }

        // -------------------------------------------------------------------------
        // DrawGroupHeader
        // -------------------------------------------------------------------------

        private void DrawGroupHeader(Graphics g, RendererItemInfo item, bool isPaneOpen)
        {
            if (!isPaneOpen) return;

            var bounds = item.Bounds;
            int indent = item.Depth * NavViewMetrics.DepthIndent;
            int x = bounds.Left + NavViewMetrics.IconPaddingLeft + indent;
            var textBounds = new Rectangle(x, bounds.Top,
                bounds.Width - x - 8, bounds.Height);

            using var brush = new SolidBrush(Colors.GroupHeaderForeground);
            var sf = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Far,
                Trimming = StringTrimming.EllipsisCharacter,
                FormatFlags = StringFormatFlags.NoWrap
            };
            g.DrawString(item.Label.ToUpperInvariant(),
                GroupHeaderFont, brush, textBounds, sf);
        }

        // -------------------------------------------------------------------------
        // DrawScrollBar
        // -------------------------------------------------------------------------

        private void DrawScrollBar(Graphics g, Rectangle trackBounds, Rectangle thumbBounds)
        {
            // Track: quasi trasparente
            using var trackBrush = new SolidBrush(
                Color.FromArgb(30, Colors.ItemForeground));
            using var trackPath = RoundedRect(trackBounds, 3);
            g.FillPath(trackBrush, trackPath);

            // Thumb
            using var thumbBrush = new SolidBrush(
                Color.FromArgb(120, Colors.ItemForeground));
            using var thumbPath = RoundedRect(thumbBounds, 3);
            g.FillPath(thumbBrush, thumbPath);
        }

        // -------------------------------------------------------------------------
        // DrawContentHeader
        // -------------------------------------------------------------------------

        public void DrawContentHeader(Graphics g, Rectangle bounds, string headerText)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            using var bg = new SolidBrush(Colors.ContentHeaderBackground);
            g.FillRectangle(bg, bounds);

            if (string.IsNullOrWhiteSpace(headerText)) return;

            var textBounds = new Rectangle(
                bounds.Left + 24, bounds.Top,
                bounds.Width - 32, bounds.Height);

            using var brush = new SolidBrush(Colors.ContentHeaderForeground);
            var sf = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter,
                FormatFlags = StringFormatFlags.NoWrap
            };
            g.DrawString(headerText, ContentHeaderFont, brush, textBounds, sf);
        }

        // -------------------------------------------------------------------------
        // MeasureItemsHeight
        // -------------------------------------------------------------------------

        public int MeasureItemsHeight(IReadOnlyList<RendererItemInfo> items)
        {
            int total = 0;
            foreach (var item in items)
            {
                if (item.IsSeparator) total += NavViewMetrics.SeparatorHeight;
                else if (item.IsGroupHeader) total += NavViewMetrics.GroupHeaderHeight;
                else total += NavViewMetrics.ItemHeight;
            }
            return total;
        }

        // -------------------------------------------------------------------------
        // Helpers grafici
        // -------------------------------------------------------------------------

        private static void DrawIconGlyph(Graphics g, string glyph,
                                          Rectangle bounds, Color color, Font font)
        {
            if (string.IsNullOrEmpty(glyph)) return;

            using var brush = new SolidBrush(color);
            var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            g.DrawString(glyph, font, brush, bounds, sf);
        }

        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();

            if (d > bounds.Width) d = bounds.Width;
            if (d > bounds.Height) d = bounds.Height;

            path.AddArc(bounds.Left, bounds.Top, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Top, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        // -------------------------------------------------------------------------
        // Dispose
        // -------------------------------------------------------------------------

        public void Dispose()
        {
            _iconFont?.Dispose();
            _labelFont?.Dispose();
            _appTitleFont?.Dispose();
            _contentHeaderFont?.Dispose();
            _groupHeaderFont?.Dispose();
        }
    }
}