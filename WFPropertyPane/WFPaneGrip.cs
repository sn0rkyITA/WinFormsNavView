using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.ConstrainedExecution;
using System.Windows.Forms;

namespace Pane
{
    /// <summary>
    /// Grip overlay per la modalità Minimal. 
    /// </summary>
    public sealed class WFPaneGrip : Control
    {
        private const int GripWidth = 18;
        private const int MinExpandWidth = 120;
        private const int CollapseThreshold = 80;

        private static readonly Color ColIdle = Color.FromArgb(0xCE, 0xA2, 0x41);
        private static readonly Color ColHover = Color.FromArgb(0x99, 0x7A, 0x30);
        private static readonly Color ColDrag = Color.FromArgb(0x99, 0x7A, 0x30);

        private WFPropertyPane? _targetPane;
        private bool _isDragging;
        private bool _isHovered;
        private int _dragStartScreenX;
        private int _dragStartPaneWidth;

        public WFPaneGrip()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable | ControlStyles.SupportsTransparentBackColor, true);
            Width = GripWidth; // LARGHEZZA FISSA: mai cambiata a runtime
            Dock = DockStyle.None;
            Cursor = Cursors.Hand;
            BackColor = Color.Transparent;
            Visible = false;
        }

        [Category("Comportamento")]
        [Description("Riferimento al WFPropertyPane da sincronizzare.")]
        [DefaultValue(null)]
        [TypeConverter(typeof(ReferenceConverter))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public WFPropertyPane? TargetPane
        {
            get => _targetPane;
            set
            {
                if (_targetPane == value) return;
                if (_targetPane != null) _targetPane.CollapseStateChanged -= OnPaneStateChanged;
                _targetPane = value;
                if (_targetPane != null)
                {
                    _targetPane.CollapseStateChanged += OnPaneStateChanged;
                    SyncVisibility();
                }
            }
        }

        private void OnPaneStateChanged(object? sender, EventArgs e) => SyncVisibility();

        private void SyncVisibility()
        {
            if (_targetPane == null) { Visible = false; return; }
            bool shouldShow = !_targetPane.IsExpanded && _targetPane.CollapseMode == PaneCollapseMode.Minimal;
            if (Visible != shouldShow)
            {
                Visible = shouldShow;
                if (Visible) UpdatePosition();
            }
        }

        private void UpdatePosition()
        {
            if (Parent is null) return;
            // Si ancora al bordo destro del container padre
            Location = new Point(Parent.ClientSize.Width - Width, 0);
            Height = 48;
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            if (Parent != null)
            {
                Parent.Resize += (s, a) => { if (Visible && !_isDragging) UpdatePosition(); };
                UpdatePosition();
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            if (!_isDragging) { _isHovered = true; Invalidate(); }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (!_isDragging) { _isHovered = false; Invalidate(); }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left && Visible)
            {
                _isDragging = true;
                _dragStartScreenX = Control.MousePosition.X;
                _dragStartPaneWidth = _targetPane?.Width ?? 0;
                Capture = true; // Cattura mouse: il drag continua anche se il cursore esce dai 4px
                Invalidate();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (_isDragging)
            {
                _isDragging = false;
                Capture = false;
                Invalidate();

                // Click/Release espande immediatamente il pannello
                if (_targetPane != null && !_targetPane.IsExpanded)
                    _targetPane.Expand();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Color lineColor = _isDragging ? ColDrag : (_isHovered ? ColHover : ColIdle);
            int lineWidth = _isDragging ? 3 : 2;
            int x = (Width - lineWidth) / 2;

            using var pen = new Pen(lineColor, lineWidth);
            // Linea centrata, leggermente rientrante in alto/basso per eleganza
            e.Graphics.DrawLine(pen, x, Height * 0.15f, x, Height * 0.95f);
            e.Graphics.DrawLine(pen, x * 1.05f, Height * 0.15f, x * 1.05f, Height * 0.95f);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _targetPane != null)
                _targetPane.CollapseStateChanged -= OnPaneStateChanged;
            base.Dispose(disposing);
        }
    }
}