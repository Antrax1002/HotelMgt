namespace HotelMgt.otherUI
{
    partial class EditDescriptionForm
    {
        private System.ComponentModel.IContainer components = null;
        private Label lblHeader;
        private Label lblInfo;
        private TextBox txtDescription;
        private Button btnSave;
        private Button btnCancel;
        private Panel pnlButtons;
        private Panel content;
        private Panel spacer;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblHeader = new Label();
            lblInfo = new Label();
            txtDescription = new TextBox();
            btnSave = new Button();
            btnCancel = new Button();
            pnlButtons = new Panel();
            content = new Panel();
            spacer = new Panel();
            pnlButtons.SuspendLayout();
            content.SuspendLayout();
            SuspendLayout();
            // 
            // lblHeader
            // 
            lblHeader.Dock = DockStyle.Top;
            lblHeader.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblHeader.Location = new Point(0, 0);
            lblHeader.Name = "lblHeader";
            lblHeader.Padding = new Padding(8, 4, 8, 0);
            lblHeader.Size = new Size(420, 24);
            lblHeader.TabIndex = 0;
            lblHeader.Text = "Room - Guest";
            lblHeader.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblInfo
            // 
            lblInfo.Dock = DockStyle.Top;
            lblInfo.Location = new Point(8, 8);
            lblInfo.Name = "lblInfo";
            lblInfo.Size = new Size(404, 18);
            lblInfo.TabIndex = 0;
            lblInfo.Text = "Notes (max 500 chars):";
            // 
            // txtDescription
            // 
            txtDescription.AcceptsReturn = true;
            txtDescription.AcceptsTab = true;
            txtDescription.BorderStyle = BorderStyle.FixedSingle;
            txtDescription.Dock = DockStyle.Top;
            txtDescription.Font = new Font("Segoe UI", 9F);
            txtDescription.Location = new Point(8, 32);
            txtDescription.MaxLength = 500;
            txtDescription.Multiline = true;
            txtDescription.Name = "txtDescription";
            txtDescription.ScrollBars = ScrollBars.Vertical;
            txtDescription.Size = new Size(404, 122);
            txtDescription.TabIndex = 2;
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSave.Location = new Point(237, 0);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(84, 28);
            btnSave.TabIndex = 0;
            btnSave.Text = "Save";
            btnSave.Click += btnSave_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(324, 0);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(84, 28);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Cancel";
            btnCancel.Click += btnCancel_Click;
            // 
            // pnlButtons
            // 
            pnlButtons.Controls.Add(btnSave);
            pnlButtons.Controls.Add(btnCancel);
            pnlButtons.Dock = DockStyle.Bottom;
            pnlButtons.Location = new Point(0, 200);
            pnlButtons.Name = "pnlButtons";
            pnlButtons.Size = new Size(420, 40);
            pnlButtons.TabIndex = 1;
            pnlButtons.Resize += pnlButtons_Resize;
            // 
            // content
            // 
            content.Controls.Add(txtDescription);
            content.Controls.Add(spacer);
            content.Controls.Add(lblInfo);
            content.Dock = DockStyle.Fill;
            content.Location = new Point(0, 24);
            content.Name = "content";
            content.Padding = new Padding(8);
            content.Size = new Size(420, 176);
            content.TabIndex = 2;
            // 
            // spacer
            // 
            spacer.Dock = DockStyle.Top;
            spacer.Location = new Point(8, 26);
            spacer.Name = "spacer";
            spacer.Size = new Size(404, 6);
            spacer.TabIndex = 1;
            // 
            // EditDescriptionForm
            // 
            AcceptButton = btnSave;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            CancelButton = btnCancel;
            ClientSize = new Size(420, 240);
            Controls.Add(content);
            Controls.Add(pnlButtons);
            Controls.Add(lblHeader);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "EditDescriptionForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Edit Description";
            pnlButtons.ResumeLayout(false);
            content.ResumeLayout(false);
            content.PerformLayout();
            ResumeLayout(false);
        }

        private void pnlButtons_Resize(object sender, EventArgs e)
        {
            btnCancel.Left = pnlButtons.ClientSize.Width - btnCancel.Width - 8;
            btnCancel.Top = (pnlButtons.ClientSize.Height - btnCancel.Height) / 2;
            btnSave.Left = btnCancel.Left - btnSave.Width - 8;
            btnSave.Top = btnCancel.Top;
        }
    }
}