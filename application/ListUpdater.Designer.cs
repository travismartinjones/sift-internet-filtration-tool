namespace Sift
{
    partial class ListUpdater
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblTotalListCount = new System.Windows.Forms.Label();
            this.lblOf = new System.Windows.Forms.Label();
            this.lblCurrentListNumber = new System.Windows.Forms.Label();
            this.lblCategoryName = new System.Windows.Forms.Label();
            this.lblProcessing = new System.Windows.Forms.Label();
            this.downloadStatus = new System.Windows.Forms.ProgressBar();
            this.downloadStatusList = new System.Windows.Forms.ProgressBar();
            this.lblDownloadProgressLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblListName = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblTotalListCount
            // 
            this.lblTotalListCount.AutoSize = true;
            this.lblTotalListCount.Location = new System.Drawing.Point(315, 11);
            this.lblTotalListCount.Name = "lblTotalListCount";
            this.lblTotalListCount.Size = new System.Drawing.Size(19, 13);
            this.lblTotalListCount.TabIndex = 11;
            this.lblTotalListCount.Text = "10";
            // 
            // lblOf
            // 
            this.lblOf.AutoSize = true;
            this.lblOf.Location = new System.Drawing.Point(301, 11);
            this.lblOf.Name = "lblOf";
            this.lblOf.Size = new System.Drawing.Size(16, 13);
            this.lblOf.TabIndex = 10;
            this.lblOf.Text = "of";
            // 
            // lblCurrentListNumber
            // 
            this.lblCurrentListNumber.AutoSize = true;
            this.lblCurrentListNumber.Location = new System.Drawing.Point(286, 11);
            this.lblCurrentListNumber.Name = "lblCurrentListNumber";
            this.lblCurrentListNumber.Size = new System.Drawing.Size(13, 13);
            this.lblCurrentListNumber.TabIndex = 9;
            this.lblCurrentListNumber.Text = "1";
            // 
            // lblCategoryName
            // 
            this.lblCategoryName.AutoSize = true;
            this.lblCategoryName.Location = new System.Drawing.Point(174, 11);
            this.lblCategoryName.Name = "lblCategoryName";
            this.lblCategoryName.Size = new System.Drawing.Size(80, 13);
            this.lblCategoryName.TabIndex = 8;
            this.lblCategoryName.Text = "Category Name";
            // 
            // lblProcessing
            // 
            this.lblProcessing.AutoSize = true;
            this.lblProcessing.Location = new System.Drawing.Point(12, 11);
            this.lblProcessing.Name = "lblProcessing";
            this.lblProcessing.Size = new System.Drawing.Size(114, 13);
            this.lblProcessing.TabIndex = 7;
            this.lblProcessing.Text = "Downloading Category";
            // 
            // downloadStatus
            // 
            this.downloadStatus.Location = new System.Drawing.Point(11, 36);
            this.downloadStatus.Name = "downloadStatus";
            this.downloadStatus.Size = new System.Drawing.Size(336, 23);
            this.downloadStatus.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.downloadStatus.TabIndex = 6;
            // 
            // downloadStatusList
            // 
            this.downloadStatusList.Location = new System.Drawing.Point(10, 87);
            this.downloadStatusList.Name = "downloadStatusList";
            this.downloadStatusList.Size = new System.Drawing.Size(336, 23);
            this.downloadStatusList.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.downloadStatusList.TabIndex = 12;
            // 
            // lblDownloadProgressLabel
            // 
            this.lblDownloadProgressLabel.AutoSize = true;
            this.lblDownloadProgressLabel.Location = new System.Drawing.Point(12, 67);
            this.lblDownloadProgressLabel.Name = "lblDownloadProgressLabel";
            this.lblDownloadProgressLabel.Size = new System.Drawing.Size(69, 13);
            this.lblDownloadProgressLabel.TabIndex = 13;
            this.lblDownloadProgressLabel.Text = "Downloading";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(272, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(10, 13);
            this.label1.TabIndex = 14;
            this.label1.Text = "(";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(331, 11);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(10, 13);
            this.label2.TabIndex = 15;
            this.label2.Text = ")";
            // 
            // lblListName
            // 
            this.lblListName.AutoSize = true;
            this.lblListName.Location = new System.Drawing.Point(174, 67);
            this.lblListName.Name = "lblListName";
            this.lblListName.Size = new System.Drawing.Size(36, 13);
            this.lblListName.TabIndex = 16;
            this.lblListName.Text = "IP List";
            // 
            // ListUpdater
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(358, 125);
            this.ControlBox = false;
            this.Controls.Add(this.lblListName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblDownloadProgressLabel);
            this.Controls.Add(this.downloadStatusList);
            this.Controls.Add(this.lblTotalListCount);
            this.Controls.Add(this.lblOf);
            this.Controls.Add(this.lblCurrentListNumber);
            this.Controls.Add(this.lblCategoryName);
            this.Controls.Add(this.lblProcessing);
            this.Controls.Add(this.downloadStatus);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ListUpdater";
            this.Text = "Downloading Categories";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTotalListCount;
        private System.Windows.Forms.Label lblOf;
        private System.Windows.Forms.Label lblCurrentListNumber;
        private System.Windows.Forms.Label lblCategoryName;
        private System.Windows.Forms.Label lblProcessing;
        private System.Windows.Forms.ProgressBar downloadStatus;
        private System.Windows.Forms.ProgressBar downloadStatusList;
        private System.Windows.Forms.Label lblDownloadProgressLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblListName;
    }
}