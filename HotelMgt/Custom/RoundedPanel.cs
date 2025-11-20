using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HotelMgt.Custom
{
    [DefaultProperty("BorderRadius")]
    public class RoundedPanel : Panel
    {
        private int borderRadius = 12;

        [Browsable(true)]
        [Category("Appearance")]
        [Description("The radius of the panel's rounded corners.")]
        [DefaultValue(12)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int BorderRadius
        {
            get => borderRadius;
            set
            {
                if (value < 0) value = 0;
                if (borderRadius == value) return;
                borderRadius = value;
                Invalidate();
                UpdateRegion();
            }
        }

        public RoundedPanel()
        {
            BackColor = Color.White;
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);
        }

        // Designer serialization helpers
        public bool ShouldSerializeBorderRadius() => borderRadius != 12;
        public void ResetBorderRadius() => BorderRadius = 12;

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            UpdateRegion();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (var path = GetRoundedRectPath(new Rectangle(0, 0, Width, Height), borderRadius))
            using (var brush = new SolidBrush(BackColor))
            {
                e.Graphics.FillPath(brush, path);
            }
        }

        private void UpdateRegion()
        {
            using (var path = GetRoundedRectPath(new Rectangle(0, 0, Width, Height), borderRadius))
            {
                Region = new Region(path);
            }
        }

        private static GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            int d = radius * 2;
            path.StartFigure();
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}