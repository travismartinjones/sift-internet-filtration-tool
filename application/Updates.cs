using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Sift
{
    public partial class Updates : SiftForm
    {
        public Updates()
        {            
            InitializeComponent();
            CustomInitializeComponent();
        }

        private void CustomInitializeComponent()
        {
            lblCurrentVersion.Text = UpdateManager.CurrentVersion.ToString();
            lblAvailableVersion.Text = UpdateManager.AvailableVersion.ToString();
            lblDescription.Text = UpdateManager.AvailableDescription;

            System.Text.StringBuilder changes = new StringBuilder();

            foreach(string change in UpdateManager.Changes)
            {
                changes.Append(change);
                changes.Append(System.Environment.NewLine);
            }

            txtChangeLog.Text = changes.ToString();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}