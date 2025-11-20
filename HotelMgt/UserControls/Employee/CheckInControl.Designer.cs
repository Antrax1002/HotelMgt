namespace HotelMgt.UserControls.Employee
{
    partial class CheckInControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            panelMain = new Panel();
            tabCheckInType = new TabControl();
            panelMain.SuspendLayout();
            SuspendLayout();
            // 
            // panelMain
            // 
            panelMain.AutoScroll = true;
            panelMain.BackColor = Color.Gray;
            panelMain.Controls.Add(tabCheckInType);
            panelMain.Dock = DockStyle.Fill;
            panelMain.Location = new Point(0, 0);
            panelMain.Name = "panelMain";
            panelMain.Size = new Size(1300, 800);
            panelMain.TabIndex = 0;
            // 
            // tabCheckInType
            // 
            tabCheckInType.Location = new Point(20, 100);
            tabCheckInType.Name = "tabCheckInType";
            tabCheckInType.SelectedIndex = 0;
            tabCheckInType.Size = new Size(1200, 650);
            tabCheckInType.TabIndex = 0;
            // 
            // CheckInControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(240, 244, 248);
            Controls.Add(panelMain);
            Name = "CheckInControl";
            Size = new Size(1300, 800);
            panelMain.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panelMain;
        private TabControl tabCheckInType;
    }
}
