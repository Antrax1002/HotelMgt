using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Diagnostics.CodeAnalysis;

namespace HotelMgt.Custom
{
    public class RoundedTextBox : UserControl
    {
        // Fields
        private Color borderColor = Color.FromArgb(203, 213, 225);
        private Color borderFocusColor = Color.FromArgb(59, 130, 246);
        private int borderSize = 1;
        private bool underlinedStyle = false;
        private bool isFocused = false;
        private int borderRadius = 6;
        private Color placeholderColor = Color.Gray;
        private string placeholderText = "";
        private bool isPasswordChar = false;

        // Controls
        private TextBox textBox1;
        private Label placeholderLabel;

        // Properties
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Category("Appearance")]
        [Description("The color of the border.")]
        public Color BorderColor
        {
            get => borderColor;
            set { borderColor = value; Invalidate(); }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Category("Appearance")]
        [Description("The color of the border when focused.")]
        public Color BorderFocusColor
        {
            get => borderFocusColor;
            set { borderFocusColor = value; }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Category("Appearance")]
        [Description("The size of the border.")]
        public int BorderSize
        {
            get => borderSize;
            set { borderSize = value; Invalidate(); }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Category("Appearance")]
        [Description("Whether the textbox uses an underlined style.")]
        public bool UnderlinedStyle
        {
            get => underlinedStyle;
            set { underlinedStyle = value; Invalidate(); }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Category("Behavior")]
        [Description("Whether the textbox uses password characters.")]
        public bool PasswordChar
        {
            get => isPasswordChar;
            set
            {
                isPasswordChar = value;
                textBox1.UseSystemPasswordChar = value;
                Invalidate();
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Category("Appearance")]
        [Description("The text associated with the control.")]
        [AllowNull]
        public override string Text
        {
            get => textBox1.Text;
            set
            {
                textBox1.Text = value ?? string.Empty;
                Invalidate();
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Category("Behavior")]
        [Description("Indicates whether this is a multiline text box.")]
        public bool Multiline
        {
            get => textBox1.Multiline;
            set { textBox1.Multiline = value; }
        }

        public override Color BackColor
        {
            get => base.BackColor;
            set
            {
                base.BackColor = value;
                textBox1.BackColor = value;
            }
        }

        public override Color ForeColor
        {
            get => base.ForeColor;
            set
            {
                base.ForeColor = value;
                textBox1.ForeColor = value;
            }
        }

        [AllowNull]
        public override Font Font
        {
            get => base.Font;
            set
            {
                base.Font = value ?? base.Font;
                textBox1.Font = value ?? textBox1.Font;
                placeholderLabel.Font = value ?? placeholderLabel.Font;
                if (DesignMode)
                    UpdateControlHeight();
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Category("Appearance")]
        [Description("The border radius for rounded corners.")]
        public int BorderRadius
        {
            get => borderRadius;
            set
            {
                if (value >= 0)
                {
                    borderRadius = value;
                    Invalidate();
                }
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Category("Appearance")]
        [Description("The color of the placeholder text.")]
        public Color PlaceholderColor
        {
            get => placeholderColor;
            set
            {
                placeholderColor = value;
                UpdatePlaceholder();
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Category("Appearance")]
        [Description("The placeholder text.")]
        public string PlaceholderText
        {
            get => placeholderText;
            set
            {
                placeholderText = value;
                UpdatePlaceholder();
            }
        }

        // Constructor
        public RoundedTextBox()
        {
            textBox1 = new TextBox();
            placeholderLabel = new Label();

            SuspendLayout();

            // TextBox setup
            textBox1.BorderStyle = BorderStyle.None;
            textBox1.Dock = DockStyle.Fill;
            textBox1.Location = new Point(10, 7);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(230, 18);
            textBox1.TabIndex = 0;
            textBox1.BackColor = Color.White;
            textBox1.ForeColor = Color.Black;
            textBox1.TextChanged += (s, e) => UpdatePlaceholder();
            textBox1.Enter += (s, e) => UpdatePlaceholder();
            textBox1.Leave += (s, e) => UpdatePlaceholder();

            // Placeholder Label setup
            placeholderLabel.Text = placeholderText;
            placeholderLabel.ForeColor = placeholderColor;
            placeholderLabel.BackColor = Color.Transparent;
            placeholderLabel.AutoSize = false;
            placeholderLabel.Dock = DockStyle.Fill;
            placeholderLabel.TextAlign = ContentAlignment.MiddleLeft;
            placeholderLabel.Enabled = false; // So it doesn't block input

            // Add controls in this order
            Controls.Add(textBox1);
            Controls.Add(placeholderLabel);
            placeholderLabel.BringToFront();

            AutoScaleMode = AutoScaleMode.None;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 10F);
            Padding = new Padding(10, 7, 10, 7);
            Size = new Size(250, 35);

            ResumeLayout();
            PerformLayout();

            UpdatePlaceholder();
        }

        private void UpdatePlaceholder()
        {
            placeholderLabel.Visible = string.IsNullOrEmpty(textBox1.Text) && !string.IsNullOrEmpty(placeholderText);
            placeholderLabel.Text = placeholderText;
            placeholderLabel.ForeColor = placeholderColor;
        }

        // Draw placeholder text if needed
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics graph = e.Graphics;

            if (borderRadius > 1)
            {
                // Rounded textbox
                var rectBorderSmooth = ClientRectangle;
                var rectBorder = Rectangle.Inflate(rectBorderSmooth, -borderSize, -borderSize);
                int smoothSize = borderSize > 0 ? borderSize : 1;

                using (GraphicsPath pathBorderSmooth = GetFigurePath(rectBorderSmooth, borderRadius))
                using (GraphicsPath pathBorder = GetFigurePath(rectBorder, borderRadius - borderSize))
                using (Pen penBorderSmooth = new Pen(Parent?.BackColor ?? BackColor, smoothSize))
                using (Pen penBorder = new Pen(isFocused ? borderFocusColor : borderColor, borderSize))
                {
                    Region = new Region(pathBorderSmooth);
                    graph.SmoothingMode = SmoothingMode.AntiAlias;
                    penBorder.Alignment = PenAlignment.Center;

                    if (isFocused)
                        penBorder.Color = borderFocusColor;

                    // Draw border
                    graph.DrawPath(penBorderSmooth, pathBorderSmooth);
                    graph.DrawPath(penBorder, pathBorder);
                }
            }
            else
            {
                // Flat textbox
                using (Pen penBorder = new Pen(isFocused ? borderFocusColor : borderColor, borderSize))
                {
                    Region = new Region(ClientRectangle);
                    penBorder.Alignment = PenAlignment.Inset;

                    if (underlinedStyle)
                        graph.DrawLine(penBorder, 0, Height - 1, Width, Height - 1);
                    else
                        graph.DrawRectangle(penBorder, 0, 0, Width - 0.5F, Height - 0.5F);
                }
            }
        }

        private GraphicsPath GetFigurePath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            float curveSize = radius * 2F;

            path.StartFigure();
            path.AddArc(rect.X, rect.Y, curveSize, curveSize, 180, 90);
            path.AddArc(rect.Right - curveSize, rect.Y, curveSize, curveSize, 270, 90);
            path.AddArc(rect.Right - curveSize, rect.Bottom - curveSize, curveSize, curveSize, 0, 90);
            path.AddArc(rect.X, rect.Bottom - curveSize, curveSize, curveSize, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void UpdateControlHeight()
        {
            if (textBox1.Multiline == false)
            {
                int txtHeight = TextRenderer.MeasureText("Text", Font).Height + 1;
                textBox1.Multiline = true;
                textBox1.MinimumSize = new Size(0, txtHeight);
                textBox1.Multiline = false;

                Height = textBox1.Height + Padding.Top + Padding.Bottom;
            }
        }
    }
}
