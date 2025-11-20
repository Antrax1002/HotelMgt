using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace HotelMgt.otherUI
{
    public partial class EditDescriptionForm : Form
    {
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string? Description
        {
            get => txtDescription.Text.Trim();
            set => txtDescription.Text = value ?? string.Empty;
        }

        public EditDescriptionForm(string room, string guest, string? currentNotes = null)
        {
            InitializeComponent();

            lblHeader.Text = $"Room {room} - {guest}";
            txtDescription.Text = currentNotes ?? string.Empty;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}