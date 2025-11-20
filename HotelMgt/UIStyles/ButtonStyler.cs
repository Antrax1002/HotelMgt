using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HotelMgt.UIStyles
{
    public static class ButtonStyler
    {
        /// <summary>
        /// Applies a rounded rectangle region to the given button.
        /// </summary>
        /// <param name="button">The button to style.</param>
        /// <param name="radius">The corner radius in pixels (e.g., 10 for Logout style).</param>
        public static void ApplyRoundedStyle(Button button, int radius = 10)
        {
            if (button == null) return;

            void Apply()
            {
                if (button.Width > 0 && button.Height > 0)
                {
                    using var path = GetRoundedRectPath(new Rectangle(Point.Empty, button.Size), radius);
                    button.Region = new Region(path);
                }
            }

            // Apply immediately if handle is created, else on handle created
            if (button.IsHandleCreated)
                Apply();
            else
                button.HandleCreated += (_, __) => Apply();

            // Re-apply on size change to keep corners correct
            button.SizeChanged -= Button_SizeChanged;
            button.SizeChanged += Button_SizeChanged;

            void Button_SizeChanged(object? sender, EventArgs e) => Apply();
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
            if (d > rect.Width) d = rect.Width;
            if (d > rect.Height) d = rect.Height;
            var arc = new Rectangle(rect.X, rect.Y, d, d);
            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - d;
            path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - d;
            path.AddArc(arc, 0, 90);
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}