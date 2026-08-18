namespace Sift
{
    partial class WizardCategorySelection
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WizardCategorySelection));
            this.treeViewCategories = new System.Windows.Forms.TreeView();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.txtListNotes = new System.Windows.Forms.TextBox();
            this.pictureBox4 = new System.Windows.Forms.PictureBox();
            this.gbDescription = new System.Windows.Forms.GroupBox();
            this.lblCategoryDescription = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
            this.gbDescription.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeViewCategories
            // 
            this.treeViewCategories.CheckBoxes = true;
            this.treeViewCategories.ImageIndex = 0;
            this.treeViewCategories.ImageList = this.imageList;
            this.treeViewCategories.LabelEdit = true;
            this.treeViewCategories.Location = new System.Drawing.Point(3, 60);
            this.treeViewCategories.Name = "treeViewCategories";
            this.treeViewCategories.SelectedImageIndex = 0;
            this.treeViewCategories.Size = new System.Drawing.Size(203, 226);
            this.treeViewCategories.TabIndex = 20;
            this.treeViewCategories.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treeViewCategories_AfterCheck);
            this.treeViewCategories.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeViewCategories_NodeMouseClick);
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "folder.png");
            // 
            // txtListNotes
            // 
            this.txtListNotes.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtListNotes.Location = new System.Drawing.Point(44, 11);
            this.txtListNotes.Multiline = true;
            this.txtListNotes.Name = "txtListNotes";
            this.txtListNotes.ReadOnly = true;
            this.txtListNotes.Size = new System.Drawing.Size(369, 35);
            this.txtListNotes.TabIndex = 40;
            this.txtListNotes.Text = "Select the content that you would like to block.\r\nThe most common blocked content" +
                " has been pre-selected for you.";
            // 
            // pictureBox4
            // 
            this.pictureBox4.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox4.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pictureBox4.BackgroundImage")));
            this.pictureBox4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.pictureBox4.Location = new System.Drawing.Point(6, 12);
            this.pictureBox4.Name = "pictureBox4";
            this.pictureBox4.Size = new System.Drawing.Size(32, 32);
            this.pictureBox4.TabIndex = 39;
            this.pictureBox4.TabStop = false;
            // 
            // gbDescription
            // 
            this.gbDescription.Controls.Add(this.lblCategoryDescription);
            this.gbDescription.Location = new System.Drawing.Point(212, 60);
            this.gbDescription.Name = "gbDescription";
            this.gbDescription.Size = new System.Drawing.Size(210, 226);
            this.gbDescription.TabIndex = 41;
            this.gbDescription.TabStop = false;
            this.gbDescription.Text = "Description";
            // 
            // lblCategoryDescription
            // 
            this.lblCategoryDescription.BackColor = System.Drawing.SystemColors.Control;
            this.lblCategoryDescription.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lblCategoryDescription.Location = new System.Drawing.Point(9, 29);
            this.lblCategoryDescription.Multiline = true;
            this.lblCategoryDescription.Name = "lblCategoryDescription";
            this.lblCategoryDescription.Size = new System.Drawing.Size(192, 191);
            this.lblCategoryDescription.TabIndex = 3;
            // 
            // WizardCategorySelection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbDescription);
            this.Controls.Add(this.txtListNotes);
            this.Controls.Add(this.pictureBox4);
            this.Controls.Add(this.treeViewCategories);
            this.Name = "WizardCategorySelection";
            this.Size = new System.Drawing.Size(430, 289);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
            this.gbDescription.ResumeLayout(false);
            this.gbDescription.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView treeViewCategories;
        private System.Windows.Forms.TextBox txtListNotes;
        private System.Windows.Forms.PictureBox pictureBox4;
        private System.Windows.Forms.GroupBox gbDescription;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.TextBox lblCategoryDescription;
    }
}
