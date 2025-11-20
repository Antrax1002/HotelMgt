using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;

namespace HotelMgt.Custom
{
    // Borderless TabControl: masks the themed 1px body border (left/right) to match the host background
    public class BorderlessTabControl : TabControl
    {
        private const int TCM_ADJUSTRECT = 0x1328;

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int Left, Top, Right, Bottom; }

        public BorderlessTabControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            HookParentBackColorChanged();
            Invalidate();
        }

        private void HookParentBackColorChanged()
        {
            if (_parentRef != null)
                _parentRef.BackColorChanged -= Parent_BackColorChanged;

            _parentRef = Parent;

            if (_parentRef != null)
                _parentRef.BackColorChanged += Parent_BackColorChanged;
        }

        private Control? _parentRef;
        private void Parent_BackColorChanged(object? sender, EventArgs e) => Invalidate();

        protected override void WndProc(ref Message m)
        {
            // Expand the display rect so the system's border falls outside our visible client area
            if (m.Msg == TCM_ADJUSTRECT && m.LParam != IntPtr.Zero)
            {
                base.WndProc(ref m);

                if (m.WParam == IntPtr.Zero)
                {
                    var rc = Marshal.PtrToStructure<RECT>(m.LParam);
                    // Push borders out a bit more than 1px to be safe on various DPIs
                    rc.Left  -= 3;
                    rc.Right += 3;
                    rc.Top   -= 2;
                    rc.Bottom+= 2;

                    Marshal.StructureToPtr(rc, m.LParam, true);
                }
                return;
            }

            base.WndProc(ref m);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            EraseBodyBorder(e.Graphics);
        }

        private void EraseBodyBorder(Graphics g)
        {
            // Use the host/parent background to "hide" the body border lines
            var eraseColor = Parent?.BackColor ?? BackColor;
            using var br = new SolidBrush(eraseColor);

            var body = DisplayRectangle;
            if (body.Width <= 0 || body.Height <= 0) return;

            // DPI-aware strip thickness (3px @ 100% scaling)
            int edge = Math.Max(3, (int)Math.Round(3f * (DeviceDpi / 96f)));

            // 1) Absolute control-edge strips (guarantees the extreme left/right edges are covered)
            var cr = ClientRectangle;
            g.FillRectangle(br, new Rectangle(cr.Left, cr.Top, edge, cr.Height));                 // left-most
            g.FillRectangle(br, new Rectangle(cr.Right - edge, cr.Top, edge, cr.Height));         // right-most

            // 2) Safety strip aligned to the page body’s right edge (covers themed line near body.Right)
            int top = Math.Max(0, body.Top - edge);
            int height = Math.Min(Height - top, body.Height + edge * 2);
            var rightSafety = new Rectangle(Math.Max(body.Right - edge, 0), top, Math.Max(edge + 2, cr.Right - (body.Right - edge)), height);
            g.FillRectangle(br, rightSafety);
        }
    }

    public static class TabControlBorderless
    {
        // Replaces an existing TabControl instance with a BorderlessTabControl, preserving pages and layout
        public static TabControl Replace(TabControl oldTc)
        {
            if (oldTc is BorderlessTabControl) return oldTc;
            if (oldTc.Parent == null) return oldTc;

            var parent = oldTc.Parent;
            int z = parent.Controls.GetChildIndex(oldTc);

            // Snapshot pages for compatibility
            var pages = new List<TabPage>(oldTc.TabPages.Count);
            foreach (TabPage p in oldTc.TabPages)
                pages.Add(p);

            int selectedIndex = oldTc.SelectedIndex;
            var imageList = oldTc.ImageList;

            var newTc = new BorderlessTabControl
            {
                Name = oldTc.Name,
                Dock = oldTc.Dock,
                Location = oldTc.Location,
                Size = oldTc.Size,
                Anchor = oldTc.Anchor,
                Font = oldTc.Font,
                RightToLeft = oldTc.RightToLeft,
                Alignment = oldTc.Alignment,
                Padding = oldTc.Padding,
                Margin = oldTc.Margin,
                DrawMode = oldTc.DrawMode,
                SizeMode = oldTc.SizeMode,
                HotTrack = oldTc.HotTrack,
                Appearance = oldTc.Appearance,
                Multiline = oldTc.Multiline,
                ShowToolTips = oldTc.ShowToolTips,
                ImageList = imageList,
                BackColor = oldTc.BackColor
            };

            foreach (var p in pages)
                newTc.TabPages.Add(p);

            if (selectedIndex >= 0 && selectedIndex < newTc.TabPages.Count)
                newTc.SelectedIndex = selectedIndex;

            parent.Controls.Remove(oldTc);
            parent.Controls.Add(newTc);
            parent.Controls.SetChildIndex(newTc, z);

            oldTc.Dispose();
            return newTc;
        }
    }
}
