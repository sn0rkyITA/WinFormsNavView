// NavViewRenderer.cs
// Contiene: INavViewRenderer, NavViewRenderer (implementazione GDI+)
//
// Responsabilità: tutto il disegno del pannello laterale.
// Il controllo NavigationView chiama questi metodi passando il Graphics
// corrente; non conosce i dettagli del rendering.

using System;
using System.Collections.Generic;
using System.Drawing;
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
        public const float LabelFontSize = 13f;

        /// <summary>Dimensione testo AppTitle in pt.</summary>
        public const float AppTitleFontSize = 13f;

        /// <summary>Dimensione testo ContentHeader in pt.</summary>
        public const float ContentHeaderFontSize = 20f;

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
                      int compactWidth);

        /// <summary>
        /// Disegna l'header del contenuto (titolo sezione corrente).
        /// Chiamato da NavigationView.OnPaint sull'area content header.
        /// </summary>
        void DrawContentHeader(Graphics g, Rectangle bounds, string headerText);

        /// <summary>
        /// Calcola l'altezza totale occupata da una lista di item
        /// (usata per determinare se i footer item vanno in fondo o subito dopo i menu item).
        /// </summary>
        int MeasureItemsHeight(IReadOnlyList<RendererItemInfo> items);
    }

    // -------------------------------------------------------------------------
    // RendererItemInfo
    // -------------------------------------------------------------------------

    /// <summary>
    /// Struttura dati passata al renderer per ogni voce visibile.
    /// Il renderer non conosce NavItem direttamente: riceve solo
    /// le informazioni necessarie al disegno.
    /// Questo disaccoppia il modello dal rendering.
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

        /// <summary>
        /// Rectangle calcolato dal controllo per questa voce (coordinate nel pane).
        /// Il renderer lo usa per disegnare e il controllo lo usa per hit testing.
        /// </summary>
        public Rectangle Bounds { get; set; }

        /// <summary>Riferimento al NavItem originale. Usato dal controllo per hit testing.</summary>
        public NavItem Source { get; set; } = null!;
    }

    // -------------------------------------------------------------------------
    // NavViewRenderer — implementazione GDI+
    // -------------------------------------------------------------------------

    /// <summary>
    /// Implementazione GDI+ del renderer.
    /// Disegna tutto il pannello laterale con sfondo, voci, icone, label,
    /// separatori, group header, frecce di espansione, accento selezione.
    /// </summary>
    public class NavViewRenderer : INavViewRenderer
    {
        public NavViewColors Colors { get; set; } = NavViewColors.Light();

        // Cache font — ricreati solo se le dimensioni cambiano
        private Font? _iconFont;
        private Font? _labelFont;
        private Font? _appTitleFont;
        private Font? _contentHeaderFont;
        private Font? _groupHeaderFont;

        private const string IconFontFamily = "Segoe Fluent Icons";
        private const string LabelFontFamily = "Segoe UI";

        // -------------------------------------------------------------------------
        // Font helpers
        // -------------------------------------------------------------------------

        private Font IconFont => _iconFont
            ??= TryCreateFont(IconFontFamily, NavViewMetrics.IconFontSize)
             ?? new Font("Arial", NavViewMetrics.IconFontSize);

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

        private static Font? TryCreateFont(string family, float size)
        {
            try { return new Font(family, size, FontStyle.Regular, GraphicsUnit.Point); }
            catch { return null; }
        }

        // -------------------------------------------------------------------------
        // DrawPane — entry point principale
        // -------------------------------------------------------------------------

        public void DrawPane(Graphics g, Rectangle paneBounds,
                             string appTitle,
                             IReadOnlyList<RendererItemInfo> items,
                             bool isPaneOpen,
                             int compactWidth)
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

            // 3. Header (hamburger + AppTitle)
            var headerBounds = new Rectangle(
                paneBounds.Left, paneBounds.Top,
                paneBounds.Width, NavViewMetrics.HeaderHeight);
            DrawPaneHeader(g, headerBounds, appTitle, isPaneOpen);

            // 4. Voci
            foreach (var item in items)
                DrawItem(g, item, isPaneOpen, paneBounds.Width);
        }

        // -------------------------------------------------------------------------
        // DrawPaneHeader
        // -------------------------------------------------------------------------

        private void DrawPaneHeader(Graphics g, Rectangle bounds,
                                    string appTitle, bool isPaneOpen)
        {
            // Sfondo header
            using var bg = new SolidBrush(Colors.PaneHeaderBackground);
            g.FillRectangle(bg, bounds);

            // Bottone hamburger ≡ — centrato verticalmente nell'header
            int hambX = bounds.Left + NavViewMetrics.HamburgerPadding;
            int hambY = bounds.Top + (bounds.Height - NavViewMetrics.HamburgerSize) / 2;
            var hambBounds = new Rectangle(hambX, hambY,
                NavViewMetrics.HamburgerSize, NavViewMetrics.HamburgerSize);

            DrawIconGlyph(g, FluentIcons.Menu, hambBounds,
                Colors.HamburgerForeground, IconFont);

            // AppTitle — solo se pane aperto e testo non vuoto
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
            if (item.IsSelected || item.IsHovered)
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
            int iconSize = NavViewMetrics.HamburgerSize; // stessa altezza del hamburger
            var iconBounds = new Rectangle(iconLeft,
                bounds.Top + (bounds.Height - iconSize) / 2,
                iconSize, iconSize);

            if (!string.IsNullOrEmpty(item.IconGlyph))
                DrawIconGlyph(g, item.IconGlyph, iconBounds, iconColor, IconFont);

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
            if (!isPaneOpen) return; // in modalità compatta non si mostra

            var bounds = item.Bounds;
            int indent = item.Depth * NavViewMetrics.DepthIndent;
            int x = bounds.Left + NavViewMetrics.IconPaddingLeft + indent;
            var textBounds = new Rectangle(x, bounds.Top,
                bounds.Width - x - 8, bounds.Height);

            using var brush = new SolidBrush(Colors.GroupHeaderForeground);
            var sf = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Far, // allineato in basso
                Trimming = StringTrimming.EllipsisCharacter,
                FormatFlags = StringFormatFlags.NoWrap
            };
            g.DrawString(item.Label.ToUpperInvariant(),
                GroupHeaderFont, brush, textBounds, sf);
        }

        // -------------------------------------------------------------------------
        // DrawContentHeader
        // -------------------------------------------------------------------------

        public void DrawContentHeader(Graphics g, Rectangle bounds, string headerText)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            // Sfondo
            using var bg = new SolidBrush(Colors.ContentHeaderBackground);
            g.FillRectangle(bg, bounds);

            if (string.IsNullOrWhiteSpace(headerText)) return;

            // Testo
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

        /// <summary>
        /// Disegna un glyph Segoe Fluent centrato nel rettangolo dato.
        /// </summary>
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

        /// <summary>
        /// Crea un GraphicsPath con angoli arrotondati.
        /// </summary>
        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();

            // Assicura che il raggio non superi la metà del lato minore
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

        /// <summary>Rilascia i font in cache.</summary>
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