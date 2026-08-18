namespace Sift
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.btnEnableDisable = new System.Windows.Forms.Button();
            this.groupFilterStatus = new System.Windows.Forms.GroupBox();
            this.panelFilterStatusAvailable = new System.Windows.Forms.Panel();
            this.lblLastListUpdateDate = new System.Windows.Forms.Label();
            this.lblLastListUpdateDateLabel = new System.Windows.Forms.Label();
            this.btnServiceStatisticsRefresh = new System.Windows.Forms.Button();
            this.lblDomainCountDescription = new System.Windows.Forms.Label();
            this.lblBlockingDescription = new System.Windows.Forms.Label();
            this.lblURLCountDescription = new System.Windows.Forms.Label();
            this.lblAllowingDescription = new System.Windows.Forms.Label();
            this.lblIPCountDescription = new System.Windows.Forms.Label();
            this.lblIPBlockCount = new System.Windows.Forms.Label();
            this.lblDomainAllowCount = new System.Windows.Forms.Label();
            this.lblIPAllowCount = new System.Windows.Forms.Label();
            this.lblDomainBlockCount = new System.Windows.Forms.Label();
            this.lblURLBlockCount = new System.Windows.Forms.Label();
            this.lblURLAllowCount = new System.Windows.Forms.Label();
            this.panelFilterStatusUnavailable = new System.Windows.Forms.Panel();
            this.lblFilterStatusUnavailable = new System.Windows.Forms.Label();
            this.lblServiceStatus = new System.Windows.Forms.Label();
            this.btnStartStop = new System.Windows.Forms.Button();
            this.backgroundWorkerServiceControl = new System.ComponentModel.BackgroundWorker();
            this.tabMain = new System.Windows.Forms.TabControl();
            this.tabInformation = new System.Windows.Forms.TabPage();
            this.btnAbout = new System.Windows.Forms.Button();
            this.groupServiceStatus = new System.Windows.Forms.GroupBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.tabLists = new System.Windows.Forms.TabPage();
            this.btnAddNewLists = new System.Windows.Forms.Button();
            this.txtListNotes = new System.Windows.Forms.TextBox();
            this.treeViewLists = new System.Windows.Forms.TreeView();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.txtQuickAdd = new System.Windows.Forms.TextBox();
            this.btnQuickAdd = new System.Windows.Forms.Button();
            this.groupListFile = new System.Windows.Forms.GroupBox();
            this.lblListStatus = new System.Windows.Forms.Label();
            this.lblListStatusDescription = new System.Windows.Forms.Label();
            this.rbListLog = new System.Windows.Forms.RadioButton();
            this.rbListBlock = new System.Windows.Forms.RadioButton();
            this.ddlContentType = new System.Windows.Forms.ComboBox();
            this.rbListAllow = new System.Windows.Forms.RadioButton();
            this.lblContentType = new System.Windows.Forms.Label();
            this.btnEditListFile = new System.Windows.Forms.Button();
            this.lblFilterType = new System.Windows.Forms.Label();
            this.lblListFile = new System.Windows.Forms.Label();
            this.groupListGroup = new System.Windows.Forms.GroupBox();
            this.lblListGroupStatus = new System.Windows.Forms.Label();
            this.lblListGroupStatusDescription = new System.Windows.Forms.Label();
            this.pictureBox4 = new System.Windows.Forms.PictureBox();
            this.tabRemotingSetup = new System.Windows.Forms.TabPage();
            this.btnRemotingTest = new System.Windows.Forms.Button();
            this.groupRemotingConnectAs = new System.Windows.Forms.GroupBox();
            this.txtRemotingConfirmPassword = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtRemotingPassword = new System.Windows.Forms.TextBox();
            this.lblRemotingPassword = new System.Windows.Forms.Label();
            this.txtRemotingUsername = new System.Windows.Forms.TextBox();
            this.rbRemotingRemoteUser = new System.Windows.Forms.RadioButton();
            this.rbRemotingCurrentUser = new System.Windows.Forms.RadioButton();
            this.lblRemotingSettingsError = new System.Windows.Forms.Label();
            this.groupRemotingConnectTo = new System.Windows.Forms.GroupBox();
            this.rbRemotingRemoteMachine = new System.Windows.Forms.RadioButton();
            this.rbRemotingLocalMachine = new System.Windows.Forms.RadioButton();
            this.txtServerName = new System.Windows.Forms.TextBox();
            this.txtRemotingPort = new System.Windows.Forms.TextBox();
            this.lblPort = new System.Windows.Forms.Label();
            this.lblRemotingSettingsNotes = new System.Windows.Forms.Label();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.tabSettings = new System.Windows.Forms.TabPage();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.groupAdapterSettings = new System.Windows.Forms.GroupBox();
            this.lblAdapter = new System.Windows.Forms.Label();
            this.ddlAdapters = new System.Windows.Forms.ComboBox();
            this.chkOverrideFilterAll = new System.Windows.Forms.CheckBox();
            this.chkOverrideFilterUdp = new System.Windows.Forms.CheckBox();
            this.chkOverrideFilterHttp = new System.Windows.Forms.CheckBox();
            this.chkOverrideFilterTcp = new System.Windows.Forms.CheckBox();
            this.chkOverrideDefault = new System.Windows.Forms.CheckBox();
            this.lblAdapterNotes = new System.Windows.Forms.TextBox();
            this.groupDefaultSettings = new System.Windows.Forms.GroupBox();
            this.lblDefaultDescription = new System.Windows.Forms.Label();
            this.chkDefaultFilterHttp = new System.Windows.Forms.CheckBox();
            this.chkDefaultFilterTcp = new System.Windows.Forms.CheckBox();
            this.chkDefaultFilterAll = new System.Windows.Forms.CheckBox();
            this.chkDefaultFilterUdp = new System.Windows.Forms.CheckBox();
            this.systemTrayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.systemTrayContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemEnableDisable = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemExit = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialogIP = new System.Windows.Forms.OpenFileDialog();
            this.openFileDialogDomain = new System.Windows.Forms.OpenFileDialog();
            this.openFileDialogURL = new System.Windows.Forms.OpenFileDialog();
            this.contextMenuListGroup = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripListGroupEnabled = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripListGroupRename = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripListGroupNew = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripListGroupNewSubgroup = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripListGroupNewList = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripListGroupDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuList = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripListEnabled = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripListRename = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripListDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnApply = new System.Windows.Forms.Button();
            this.groupFilterStatus.SuspendLayout();
            this.panelFilterStatusAvailable.SuspendLayout();
            this.panelFilterStatusUnavailable.SuspendLayout();
            this.tabMain.SuspendLayout();
            this.tabInformation.SuspendLayout();
            this.groupServiceStatus.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.tabLists.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupListFile.SuspendLayout();
            this.groupListGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
            this.tabRemotingSetup.SuspendLayout();
            this.groupRemotingConnectAs.SuspendLayout();
            this.groupRemotingConnectTo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.tabSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            this.groupAdapterSettings.SuspendLayout();
            this.groupDefaultSettings.SuspendLayout();
            this.systemTrayContextMenu.SuspendLayout();
            this.contextMenuListGroup.SuspendLayout();
            this.contextMenuList.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnEnableDisable
            // 
            resources.ApplyResources(this.btnEnableDisable, "btnEnableDisable");
            this.btnEnableDisable.Name = "btnEnableDisable";
            this.btnEnableDisable.Click += new System.EventHandler(this.btnEnableDisable_Click);
            // 
            // groupFilterStatus
            // 
            this.groupFilterStatus.Controls.Add(this.panelFilterStatusAvailable);
            this.groupFilterStatus.Controls.Add(this.panelFilterStatusUnavailable);
            resources.ApplyResources(this.groupFilterStatus, "groupFilterStatus");
            this.groupFilterStatus.Name = "groupFilterStatus";
            this.groupFilterStatus.TabStop = false;
            // 
            // panelFilterStatusAvailable
            // 
            this.panelFilterStatusAvailable.Controls.Add(this.lblLastListUpdateDate);
            this.panelFilterStatusAvailable.Controls.Add(this.lblLastListUpdateDateLabel);
            this.panelFilterStatusAvailable.Controls.Add(this.btnServiceStatisticsRefresh);
            this.panelFilterStatusAvailable.Controls.Add(this.btnEnableDisable);
            this.panelFilterStatusAvailable.Controls.Add(this.lblDomainCountDescription);
            this.panelFilterStatusAvailable.Controls.Add(this.lblBlockingDescription);
            this.panelFilterStatusAvailable.Controls.Add(this.lblURLCountDescription);
            this.panelFilterStatusAvailable.Controls.Add(this.lblAllowingDescription);
            this.panelFilterStatusAvailable.Controls.Add(this.lblIPCountDescription);
            this.panelFilterStatusAvailable.Controls.Add(this.lblIPBlockCount);
            this.panelFilterStatusAvailable.Controls.Add(this.lblDomainAllowCount);
            this.panelFilterStatusAvailable.Controls.Add(this.lblIPAllowCount);
            this.panelFilterStatusAvailable.Controls.Add(this.lblDomainBlockCount);
            this.panelFilterStatusAvailable.Controls.Add(this.lblURLBlockCount);
            this.panelFilterStatusAvailable.Controls.Add(this.lblURLAllowCount);
            resources.ApplyResources(this.panelFilterStatusAvailable, "panelFilterStatusAvailable");
            this.panelFilterStatusAvailable.Name = "panelFilterStatusAvailable";
            // 
            // lblLastListUpdateDate
            // 
            resources.ApplyResources(this.lblLastListUpdateDate, "lblLastListUpdateDate");
            this.lblLastListUpdateDate.Name = "lblLastListUpdateDate";
            // 
            // lblLastListUpdateDateLabel
            // 
            resources.ApplyResources(this.lblLastListUpdateDateLabel, "lblLastListUpdateDateLabel");
            this.lblLastListUpdateDateLabel.Name = "lblLastListUpdateDateLabel";
            // 
            // btnServiceStatisticsRefresh
            // 
            resources.ApplyResources(this.btnServiceStatisticsRefresh, "btnServiceStatisticsRefresh");
            this.btnServiceStatisticsRefresh.Name = "btnServiceStatisticsRefresh";
            this.btnServiceStatisticsRefresh.UseVisualStyleBackColor = true;
            this.btnServiceStatisticsRefresh.Click += new System.EventHandler(this.btnServiceStatisticsRefresh_Click);
            // 
            // lblDomainCountDescription
            // 
            resources.ApplyResources(this.lblDomainCountDescription, "lblDomainCountDescription");
            this.lblDomainCountDescription.Name = "lblDomainCountDescription";
            // 
            // lblBlockingDescription
            // 
            resources.ApplyResources(this.lblBlockingDescription, "lblBlockingDescription");
            this.lblBlockingDescription.Name = "lblBlockingDescription";
            // 
            // lblURLCountDescription
            // 
            resources.ApplyResources(this.lblURLCountDescription, "lblURLCountDescription");
            this.lblURLCountDescription.Name = "lblURLCountDescription";
            // 
            // lblAllowingDescription
            // 
            resources.ApplyResources(this.lblAllowingDescription, "lblAllowingDescription");
            this.lblAllowingDescription.Name = "lblAllowingDescription";
            // 
            // lblIPCountDescription
            // 
            resources.ApplyResources(this.lblIPCountDescription, "lblIPCountDescription");
            this.lblIPCountDescription.Name = "lblIPCountDescription";
            // 
            // lblIPBlockCount
            // 
            resources.ApplyResources(this.lblIPBlockCount, "lblIPBlockCount");
            this.lblIPBlockCount.Name = "lblIPBlockCount";
            // 
            // lblDomainAllowCount
            // 
            resources.ApplyResources(this.lblDomainAllowCount, "lblDomainAllowCount");
            this.lblDomainAllowCount.Name = "lblDomainAllowCount";
            // 
            // lblIPAllowCount
            // 
            resources.ApplyResources(this.lblIPAllowCount, "lblIPAllowCount");
            this.lblIPAllowCount.Name = "lblIPAllowCount";
            // 
            // lblDomainBlockCount
            // 
            resources.ApplyResources(this.lblDomainBlockCount, "lblDomainBlockCount");
            this.lblDomainBlockCount.Name = "lblDomainBlockCount";
            // 
            // lblURLBlockCount
            // 
            resources.ApplyResources(this.lblURLBlockCount, "lblURLBlockCount");
            this.lblURLBlockCount.Name = "lblURLBlockCount";
            // 
            // lblURLAllowCount
            // 
            resources.ApplyResources(this.lblURLAllowCount, "lblURLAllowCount");
            this.lblURLAllowCount.Name = "lblURLAllowCount";
            // 
            // panelFilterStatusUnavailable
            // 
            this.panelFilterStatusUnavailable.Controls.Add(this.lblFilterStatusUnavailable);
            resources.ApplyResources(this.panelFilterStatusUnavailable, "panelFilterStatusUnavailable");
            this.panelFilterStatusUnavailable.Name = "panelFilterStatusUnavailable";
            // 
            // lblFilterStatusUnavailable
            // 
            resources.ApplyResources(this.lblFilterStatusUnavailable, "lblFilterStatusUnavailable");
            this.lblFilterStatusUnavailable.Name = "lblFilterStatusUnavailable";
            // 
            // lblServiceStatus
            // 
            resources.ApplyResources(this.lblServiceStatus, "lblServiceStatus");
            this.lblServiceStatus.MaximumSize = new System.Drawing.Size(350, 100);
            this.lblServiceStatus.Name = "lblServiceStatus";
            // 
            // btnStartStop
            // 
            resources.ApplyResources(this.btnStartStop, "btnStartStop");
            this.btnStartStop.Name = "btnStartStop";
            this.btnStartStop.Click += new System.EventHandler(this.btnStartStop_Click);
            // 
            // backgroundWorkerServiceControl
            // 
            this.backgroundWorkerServiceControl.WorkerReportsProgress = true;
            this.backgroundWorkerServiceControl.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerServiceControl_DoWork);
            this.backgroundWorkerServiceControl.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerServiceControl_RunWorkerCompleted);
            this.backgroundWorkerServiceControl.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorkerServiceControl_ProgressChanged);
            // 
            // tabMain
            // 
            this.tabMain.Controls.Add(this.tabInformation);
            this.tabMain.Controls.Add(this.tabLists);
            this.tabMain.Controls.Add(this.tabRemotingSetup);
            this.tabMain.Controls.Add(this.tabSettings);
            resources.ApplyResources(this.tabMain, "tabMain");
            this.tabMain.Name = "tabMain";
            this.tabMain.SelectedIndex = 0;
            // 
            // tabInformation
            // 
            this.tabInformation.BackColor = System.Drawing.Color.Transparent;
            this.tabInformation.Controls.Add(this.btnAbout);
            this.tabInformation.Controls.Add(this.groupServiceStatus);
            this.tabInformation.Controls.Add(this.pictureBox1);
            this.tabInformation.Controls.Add(this.groupFilterStatus);
            resources.ApplyResources(this.tabInformation, "tabInformation");
            this.tabInformation.Name = "tabInformation";
            this.tabInformation.UseVisualStyleBackColor = true;
            // 
            // btnAbout
            // 
            resources.ApplyResources(this.btnAbout, "btnAbout");
            this.btnAbout.Name = "btnAbout";
            this.btnAbout.UseVisualStyleBackColor = true;
            this.btnAbout.Click += new System.EventHandler(this.btnAbout_Click);
            // 
            // groupServiceStatus
            // 
            this.groupServiceStatus.Controls.Add(this.lblServiceStatus);
            this.groupServiceStatus.Controls.Add(this.btnStartStop);
            resources.ApplyResources(this.groupServiceStatus, "groupServiceStatus");
            this.groupServiceStatus.Name = "groupServiceStatus";
            this.groupServiceStatus.TabStop = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.BackgroundImage = global::Sift.Properties.Resources.information_bar;
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.ErrorImage = null;
            this.pictureBox1.InitialImage = null;
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            // 
            // tabLists
            // 
            this.tabLists.BackColor = System.Drawing.Color.Transparent;
            this.tabLists.Controls.Add(this.btnAddNewLists);
            this.tabLists.Controls.Add(this.txtListNotes);
            this.tabLists.Controls.Add(this.treeViewLists);
            this.tabLists.Controls.Add(this.groupBox5);
            this.tabLists.Controls.Add(this.groupListFile);
            this.tabLists.Controls.Add(this.groupListGroup);
            this.tabLists.Controls.Add(this.pictureBox4);
            resources.ApplyResources(this.tabLists, "tabLists");
            this.tabLists.Name = "tabLists";
            this.tabLists.UseVisualStyleBackColor = true;
            // 
            // btnAddNewLists
            // 
            resources.ApplyResources(this.btnAddNewLists, "btnAddNewLists");
            this.btnAddNewLists.Name = "btnAddNewLists";
            this.btnAddNewLists.UseVisualStyleBackColor = true;
            this.btnAddNewLists.Click += new System.EventHandler(this.btnAddNewLists_Click);
            // 
            // txtListNotes
            // 
            this.txtListNotes.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.txtListNotes.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.txtListNotes, "txtListNotes");
            this.txtListNotes.Name = "txtListNotes";
            this.txtListNotes.ReadOnly = true;
            // 
            // treeViewLists
            // 
            this.treeViewLists.CheckBoxes = true;
            resources.ApplyResources(this.treeViewLists, "treeViewLists");
            this.treeViewLists.ImageList = this.imageList;
            this.treeViewLists.LabelEdit = true;
            this.treeViewLists.Name = "treeViewLists";
            this.treeViewLists.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treeViewLists_AfterCheck);
            this.treeViewLists.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.treeViewLists_AfterLabelEdit);
            this.treeViewLists.DragDrop += new System.Windows.Forms.DragEventHandler(this.treeViewLists_DragDrop);
            this.treeViewLists.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeViewLists_NodeMouseClick);
            this.treeViewLists.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.treeViewLists_ItemDrag);
            this.treeViewLists.DragOver += new System.Windows.Forms.DragEventHandler(this.treeViewLists_DragOver);
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "folder.png");
            this.imageList.Images.SetKeyName(1, "file_doc.png");
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.txtQuickAdd);
            this.groupBox5.Controls.Add(this.btnQuickAdd);
            resources.ApplyResources(this.groupBox5, "groupBox5");
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.TabStop = false;
            // 
            // txtQuickAdd
            // 
            resources.ApplyResources(this.txtQuickAdd, "txtQuickAdd");
            this.txtQuickAdd.Name = "txtQuickAdd";
            this.txtQuickAdd.TextChanged += new System.EventHandler(this.txtQuickAdd_TextChanged);
            // 
            // btnQuickAdd
            // 
            resources.ApplyResources(this.btnQuickAdd, "btnQuickAdd");
            this.btnQuickAdd.Name = "btnQuickAdd";
            this.btnQuickAdd.Click += new System.EventHandler(this.btnQuickAdd_Click);
            // 
            // groupListFile
            // 
            this.groupListFile.Controls.Add(this.lblListStatus);
            this.groupListFile.Controls.Add(this.lblListStatusDescription);
            this.groupListFile.Controls.Add(this.rbListLog);
            this.groupListFile.Controls.Add(this.rbListBlock);
            this.groupListFile.Controls.Add(this.ddlContentType);
            this.groupListFile.Controls.Add(this.rbListAllow);
            this.groupListFile.Controls.Add(this.lblContentType);
            this.groupListFile.Controls.Add(this.btnEditListFile);
            this.groupListFile.Controls.Add(this.lblFilterType);
            this.groupListFile.Controls.Add(this.lblListFile);
            resources.ApplyResources(this.groupListFile, "groupListFile");
            this.groupListFile.Name = "groupListFile";
            this.groupListFile.TabStop = false;
            // 
            // lblListStatus
            // 
            resources.ApplyResources(this.lblListStatus, "lblListStatus");
            this.lblListStatus.Name = "lblListStatus";
            // 
            // lblListStatusDescription
            // 
            resources.ApplyResources(this.lblListStatusDescription, "lblListStatusDescription");
            this.lblListStatusDescription.Name = "lblListStatusDescription";
            // 
            // rbListLog
            // 
            resources.ApplyResources(this.rbListLog, "rbListLog");
            this.rbListLog.Name = "rbListLog";
            this.rbListLog.TabStop = true;
            this.rbListLog.UseVisualStyleBackColor = true;
            this.rbListLog.CheckedChanged += new System.EventHandler(this.rbListLog_CheckedChanged);
            // 
            // rbListBlock
            // 
            resources.ApplyResources(this.rbListBlock, "rbListBlock");
            this.rbListBlock.Name = "rbListBlock";
            this.rbListBlock.CheckedChanged += new System.EventHandler(this.rbListBlock_CheckedChanged);
            // 
            // ddlContentType
            // 
            this.ddlContentType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddlContentType.FormattingEnabled = true;
            this.ddlContentType.Items.AddRange(new object[] {
            resources.GetString("ddlContentType.Items"),
            resources.GetString("ddlContentType.Items1"),
            resources.GetString("ddlContentType.Items2")});
            resources.ApplyResources(this.ddlContentType, "ddlContentType");
            this.ddlContentType.Name = "ddlContentType";
            this.ddlContentType.SelectedIndexChanged += new System.EventHandler(this.ddlContentType_SelectedIndexChanged);
            // 
            // rbListAllow
            // 
            resources.ApplyResources(this.rbListAllow, "rbListAllow");
            this.rbListAllow.Name = "rbListAllow";
            this.rbListAllow.CheckedChanged += new System.EventHandler(this.rbListAllow_CheckedChanged);
            // 
            // lblContentType
            // 
            resources.ApplyResources(this.lblContentType, "lblContentType");
            this.lblContentType.Name = "lblContentType";
            // 
            // btnEditListFile
            // 
            resources.ApplyResources(this.btnEditListFile, "btnEditListFile");
            this.btnEditListFile.Name = "btnEditListFile";
            this.btnEditListFile.Click += new System.EventHandler(this.btnEditListFile_Click);
            // 
            // lblFilterType
            // 
            resources.ApplyResources(this.lblFilterType, "lblFilterType");
            this.lblFilterType.Name = "lblFilterType";
            // 
            // lblListFile
            // 
            resources.ApplyResources(this.lblListFile, "lblListFile");
            this.lblListFile.Name = "lblListFile";
            // 
            // groupListGroup
            // 
            this.groupListGroup.Controls.Add(this.lblListGroupStatus);
            this.groupListGroup.Controls.Add(this.lblListGroupStatusDescription);
            resources.ApplyResources(this.groupListGroup, "groupListGroup");
            this.groupListGroup.Name = "groupListGroup";
            this.groupListGroup.TabStop = false;
            // 
            // lblListGroupStatus
            // 
            resources.ApplyResources(this.lblListGroupStatus, "lblListGroupStatus");
            this.lblListGroupStatus.Name = "lblListGroupStatus";
            // 
            // lblListGroupStatusDescription
            // 
            resources.ApplyResources(this.lblListGroupStatusDescription, "lblListGroupStatusDescription");
            this.lblListGroupStatusDescription.Name = "lblListGroupStatusDescription";
            // 
            // pictureBox4
            // 
            this.pictureBox4.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.pictureBox4, "pictureBox4");
            this.pictureBox4.Name = "pictureBox4";
            this.pictureBox4.TabStop = false;
            // 
            // tabRemotingSetup
            // 
            this.tabRemotingSetup.Controls.Add(this.btnRemotingTest);
            this.tabRemotingSetup.Controls.Add(this.groupRemotingConnectAs);
            this.tabRemotingSetup.Controls.Add(this.lblRemotingSettingsError);
            this.tabRemotingSetup.Controls.Add(this.groupRemotingConnectTo);
            this.tabRemotingSetup.Controls.Add(this.lblRemotingSettingsNotes);
            this.tabRemotingSetup.Controls.Add(this.pictureBox2);
            resources.ApplyResources(this.tabRemotingSetup, "tabRemotingSetup");
            this.tabRemotingSetup.Name = "tabRemotingSetup";
            this.tabRemotingSetup.UseVisualStyleBackColor = true;
            // 
            // btnRemotingTest
            // 
            resources.ApplyResources(this.btnRemotingTest, "btnRemotingTest");
            this.btnRemotingTest.Name = "btnRemotingTest";
            this.btnRemotingTest.UseVisualStyleBackColor = true;
            this.btnRemotingTest.Click += new System.EventHandler(this.btnRemotingTest_Click);
            // 
            // groupRemotingConnectAs
            // 
            this.groupRemotingConnectAs.Controls.Add(this.txtRemotingConfirmPassword);
            this.groupRemotingConnectAs.Controls.Add(this.label1);
            this.groupRemotingConnectAs.Controls.Add(this.txtRemotingPassword);
            this.groupRemotingConnectAs.Controls.Add(this.lblRemotingPassword);
            this.groupRemotingConnectAs.Controls.Add(this.txtRemotingUsername);
            this.groupRemotingConnectAs.Controls.Add(this.rbRemotingRemoteUser);
            this.groupRemotingConnectAs.Controls.Add(this.rbRemotingCurrentUser);
            resources.ApplyResources(this.groupRemotingConnectAs, "groupRemotingConnectAs");
            this.groupRemotingConnectAs.Name = "groupRemotingConnectAs";
            this.groupRemotingConnectAs.TabStop = false;
            // 
            // txtRemotingConfirmPassword
            // 
            resources.ApplyResources(this.txtRemotingConfirmPassword, "txtRemotingConfirmPassword");
            this.txtRemotingConfirmPassword.Name = "txtRemotingConfirmPassword";
            this.txtRemotingConfirmPassword.UseSystemPasswordChar = true;
            this.txtRemotingConfirmPassword.TextChanged += new System.EventHandler(this.txtRemotingConfirmPassword_TextChanged);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // txtRemotingPassword
            // 
            resources.ApplyResources(this.txtRemotingPassword, "txtRemotingPassword");
            this.txtRemotingPassword.Name = "txtRemotingPassword";
            this.txtRemotingPassword.UseSystemPasswordChar = true;
            this.txtRemotingPassword.TextChanged += new System.EventHandler(this.txtRemotingPassword_TextChanged);
            // 
            // lblRemotingPassword
            // 
            resources.ApplyResources(this.lblRemotingPassword, "lblRemotingPassword");
            this.lblRemotingPassword.Name = "lblRemotingPassword";
            // 
            // txtRemotingUsername
            // 
            resources.ApplyResources(this.txtRemotingUsername, "txtRemotingUsername");
            this.txtRemotingUsername.Name = "txtRemotingUsername";
            this.txtRemotingUsername.TextChanged += new System.EventHandler(this.txtRemotingUsername_TextChanged);
            // 
            // rbRemotingRemoteUser
            // 
            resources.ApplyResources(this.rbRemotingRemoteUser, "rbRemotingRemoteUser");
            this.rbRemotingRemoteUser.Name = "rbRemotingRemoteUser";
            this.rbRemotingRemoteUser.TabStop = true;
            this.rbRemotingRemoteUser.UseVisualStyleBackColor = true;
            this.rbRemotingRemoteUser.CheckedChanged += new System.EventHandler(this.rbRemotingRemoteUser_CheckedChanged);
            // 
            // rbRemotingCurrentUser
            // 
            resources.ApplyResources(this.rbRemotingCurrentUser, "rbRemotingCurrentUser");
            this.rbRemotingCurrentUser.Name = "rbRemotingCurrentUser";
            this.rbRemotingCurrentUser.TabStop = true;
            this.rbRemotingCurrentUser.UseVisualStyleBackColor = true;
            this.rbRemotingCurrentUser.CheckedChanged += new System.EventHandler(this.rbRemotingCurrentUser_CheckedChanged);
            // 
            // lblRemotingSettingsError
            // 
            resources.ApplyResources(this.lblRemotingSettingsError, "lblRemotingSettingsError");
            this.lblRemotingSettingsError.ForeColor = System.Drawing.Color.Red;
            this.lblRemotingSettingsError.Name = "lblRemotingSettingsError";
            // 
            // groupRemotingConnectTo
            // 
            this.groupRemotingConnectTo.Controls.Add(this.rbRemotingRemoteMachine);
            this.groupRemotingConnectTo.Controls.Add(this.rbRemotingLocalMachine);
            this.groupRemotingConnectTo.Controls.Add(this.txtServerName);
            this.groupRemotingConnectTo.Controls.Add(this.txtRemotingPort);
            this.groupRemotingConnectTo.Controls.Add(this.lblPort);
            resources.ApplyResources(this.groupRemotingConnectTo, "groupRemotingConnectTo");
            this.groupRemotingConnectTo.Name = "groupRemotingConnectTo";
            this.groupRemotingConnectTo.TabStop = false;
            // 
            // rbRemotingRemoteMachine
            // 
            resources.ApplyResources(this.rbRemotingRemoteMachine, "rbRemotingRemoteMachine");
            this.rbRemotingRemoteMachine.Name = "rbRemotingRemoteMachine";
            this.rbRemotingRemoteMachine.TabStop = true;
            this.rbRemotingRemoteMachine.UseVisualStyleBackColor = true;
            this.rbRemotingRemoteMachine.CheckedChanged += new System.EventHandler(this.rbRemotingRemoteMachine_CheckedChanged);
            // 
            // rbRemotingLocalMachine
            // 
            resources.ApplyResources(this.rbRemotingLocalMachine, "rbRemotingLocalMachine");
            this.rbRemotingLocalMachine.Name = "rbRemotingLocalMachine";
            this.rbRemotingLocalMachine.TabStop = true;
            this.rbRemotingLocalMachine.UseVisualStyleBackColor = true;
            this.rbRemotingLocalMachine.CheckedChanged += new System.EventHandler(this.rbRemotingLocalMachine_CheckedChanged);
            // 
            // txtServerName
            // 
            resources.ApplyResources(this.txtServerName, "txtServerName");
            this.txtServerName.Name = "txtServerName";
            this.txtServerName.TextChanged += new System.EventHandler(this.txtServerName_TextChanged);
            // 
            // txtRemotingPort
            // 
            resources.ApplyResources(this.txtRemotingPort, "txtRemotingPort");
            this.txtRemotingPort.Name = "txtRemotingPort";
            this.txtRemotingPort.TextChanged += new System.EventHandler(this.txtRemotingPort_TextChanged);
            // 
            // lblPort
            // 
            resources.ApplyResources(this.lblPort, "lblPort");
            this.lblPort.Name = "lblPort";
            // 
            // lblRemotingSettingsNotes
            // 
            resources.ApplyResources(this.lblRemotingSettingsNotes, "lblRemotingSettingsNotes");
            this.lblRemotingSettingsNotes.Name = "lblRemotingSettingsNotes";
            // 
            // pictureBox2
            // 
            this.pictureBox2.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.pictureBox2, "pictureBox2");
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.TabStop = false;
            // 
            // tabSettings
            // 
            this.tabSettings.BackColor = System.Drawing.Color.Transparent;
            this.tabSettings.Controls.Add(this.pictureBox3);
            this.tabSettings.Controls.Add(this.groupAdapterSettings);
            this.tabSettings.Controls.Add(this.lblAdapterNotes);
            this.tabSettings.Controls.Add(this.groupDefaultSettings);
            resources.ApplyResources(this.tabSettings, "tabSettings");
            this.tabSettings.Name = "tabSettings";
            this.tabSettings.UseVisualStyleBackColor = true;
            // 
            // pictureBox3
            // 
            this.pictureBox3.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.pictureBox3, "pictureBox3");
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.TabStop = false;
            // 
            // groupAdapterSettings
            // 
            this.groupAdapterSettings.Controls.Add(this.lblAdapter);
            this.groupAdapterSettings.Controls.Add(this.ddlAdapters);
            this.groupAdapterSettings.Controls.Add(this.chkOverrideFilterAll);
            this.groupAdapterSettings.Controls.Add(this.chkOverrideFilterUdp);
            this.groupAdapterSettings.Controls.Add(this.chkOverrideFilterHttp);
            this.groupAdapterSettings.Controls.Add(this.chkOverrideFilterTcp);
            this.groupAdapterSettings.Controls.Add(this.chkOverrideDefault);
            resources.ApplyResources(this.groupAdapterSettings, "groupAdapterSettings");
            this.groupAdapterSettings.Name = "groupAdapterSettings";
            this.groupAdapterSettings.TabStop = false;
            // 
            // lblAdapter
            // 
            resources.ApplyResources(this.lblAdapter, "lblAdapter");
            this.lblAdapter.Name = "lblAdapter";
            // 
            // ddlAdapters
            // 
            this.ddlAdapters.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.ddlAdapters, "ddlAdapters");
            this.ddlAdapters.Name = "ddlAdapters";
            this.ddlAdapters.SelectedIndexChanged += new System.EventHandler(this.ddlAdapters_SelectedIndexChanged);
            // 
            // chkOverrideFilterAll
            // 
            resources.ApplyResources(this.chkOverrideFilterAll, "chkOverrideFilterAll");
            this.chkOverrideFilterAll.Name = "chkOverrideFilterAll";
            this.chkOverrideFilterAll.CheckedChanged += new System.EventHandler(this.chkOverrideFilterAll_CheckedChanged);
            // 
            // chkOverrideFilterUdp
            // 
            resources.ApplyResources(this.chkOverrideFilterUdp, "chkOverrideFilterUdp");
            this.chkOverrideFilterUdp.Name = "chkOverrideFilterUdp";
            this.chkOverrideFilterUdp.CheckedChanged += new System.EventHandler(this.chkOverrideFilterUdp_CheckedChanged);
            // 
            // chkOverrideFilterHttp
            // 
            resources.ApplyResources(this.chkOverrideFilterHttp, "chkOverrideFilterHttp");
            this.chkOverrideFilterHttp.Name = "chkOverrideFilterHttp";
            this.chkOverrideFilterHttp.CheckedChanged += new System.EventHandler(this.chkOverrideFilterHttp_CheckedChanged);
            // 
            // chkOverrideFilterTcp
            // 
            resources.ApplyResources(this.chkOverrideFilterTcp, "chkOverrideFilterTcp");
            this.chkOverrideFilterTcp.Name = "chkOverrideFilterTcp";
            this.chkOverrideFilterTcp.CheckedChanged += new System.EventHandler(this.chkOverrideFilterTcp_CheckedChanged);
            // 
            // chkOverrideDefault
            // 
            resources.ApplyResources(this.chkOverrideDefault, "chkOverrideDefault");
            this.chkOverrideDefault.Name = "chkOverrideDefault";
            this.chkOverrideDefault.CheckedChanged += new System.EventHandler(this.chkOverrideDefault_CheckedChanged);
            // 
            // lblAdapterNotes
            // 
            this.lblAdapterNotes.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblAdapterNotes.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.lblAdapterNotes, "lblAdapterNotes");
            this.lblAdapterNotes.Name = "lblAdapterNotes";
            this.lblAdapterNotes.ReadOnly = true;
            // 
            // groupDefaultSettings
            // 
            this.groupDefaultSettings.Controls.Add(this.lblDefaultDescription);
            this.groupDefaultSettings.Controls.Add(this.chkDefaultFilterHttp);
            this.groupDefaultSettings.Controls.Add(this.chkDefaultFilterTcp);
            this.groupDefaultSettings.Controls.Add(this.chkDefaultFilterAll);
            this.groupDefaultSettings.Controls.Add(this.chkDefaultFilterUdp);
            resources.ApplyResources(this.groupDefaultSettings, "groupDefaultSettings");
            this.groupDefaultSettings.Name = "groupDefaultSettings";
            this.groupDefaultSettings.TabStop = false;
            // 
            // lblDefaultDescription
            // 
            resources.ApplyResources(this.lblDefaultDescription, "lblDefaultDescription");
            this.lblDefaultDescription.Name = "lblDefaultDescription";
            // 
            // chkDefaultFilterHttp
            // 
            resources.ApplyResources(this.chkDefaultFilterHttp, "chkDefaultFilterHttp");
            this.chkDefaultFilterHttp.Name = "chkDefaultFilterHttp";
            this.chkDefaultFilterHttp.CheckedChanged += new System.EventHandler(this.chkDefaultFilterHttp_CheckedChanged);
            // 
            // chkDefaultFilterTcp
            // 
            resources.ApplyResources(this.chkDefaultFilterTcp, "chkDefaultFilterTcp");
            this.chkDefaultFilterTcp.Name = "chkDefaultFilterTcp";
            this.chkDefaultFilterTcp.CheckedChanged += new System.EventHandler(this.chkDefaultFilterTcp_CheckedChanged);
            // 
            // chkDefaultFilterAll
            // 
            resources.ApplyResources(this.chkDefaultFilterAll, "chkDefaultFilterAll");
            this.chkDefaultFilterAll.Name = "chkDefaultFilterAll";
            this.chkDefaultFilterAll.CheckedChanged += new System.EventHandler(this.chkDefaultFilterAll_CheckedChanged);
            // 
            // chkDefaultFilterUdp
            // 
            resources.ApplyResources(this.chkDefaultFilterUdp, "chkDefaultFilterUdp");
            this.chkDefaultFilterUdp.Name = "chkDefaultFilterUdp";
            this.chkDefaultFilterUdp.CheckedChanged += new System.EventHandler(this.chkDefaultFilterUdp_CheckedChanged);
            // 
            // systemTrayIcon
            // 
            this.systemTrayIcon.ContextMenuStrip = this.systemTrayContextMenu;
            resources.ApplyResources(this.systemTrayIcon, "systemTrayIcon");
            this.systemTrayIcon.DoubleClick += new System.EventHandler(this.systemTrayIcon_DoubleClick);
            // 
            // systemTrayContextMenu
            // 
            this.systemTrayContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemOpen,
            this.toolStripSeparator2,
            this.toolStripMenuItemEnableDisable,
            this.toolStripMenuItemAbout,
            this.toolStripSeparator1,
            this.toolStripMenuItemExit});
            this.systemTrayContextMenu.Name = "systemTrayContextMenu";
            this.systemTrayContextMenu.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            resources.ApplyResources(this.systemTrayContextMenu, "systemTrayContextMenu");
            // 
            // toolStripMenuItemOpen
            // 
            this.toolStripMenuItemOpen.Name = "toolStripMenuItemOpen";
            resources.ApplyResources(this.toolStripMenuItemOpen, "toolStripMenuItemOpen");
            this.toolStripMenuItemOpen.Click += new System.EventHandler(this.toolStripMenuItemOpen_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            // 
            // toolStripMenuItemEnableDisable
            // 
            this.toolStripMenuItemEnableDisable.Name = "toolStripMenuItemEnableDisable";
            resources.ApplyResources(this.toolStripMenuItemEnableDisable, "toolStripMenuItemEnableDisable");
            this.toolStripMenuItemEnableDisable.Click += new System.EventHandler(this.toolStripMenuItemEnableDisable_Click);
            // 
            // toolStripMenuItemAbout
            // 
            this.toolStripMenuItemAbout.Name = "toolStripMenuItemAbout";
            resources.ApplyResources(this.toolStripMenuItemAbout, "toolStripMenuItemAbout");
            this.toolStripMenuItemAbout.Click += new System.EventHandler(this.toolStripMenuItemAbout_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // toolStripMenuItemExit
            // 
            this.toolStripMenuItemExit.Name = "toolStripMenuItemExit";
            resources.ApplyResources(this.toolStripMenuItemExit, "toolStripMenuItemExit");
            this.toolStripMenuItemExit.Click += new System.EventHandler(this.toolStripMenuItemExit_Click);
            // 
            // openFileDialogIP
            // 
            this.openFileDialogIP.DefaultExt = "lis";
            resources.ApplyResources(this.openFileDialogIP, "openFileDialogIP");
            // 
            // openFileDialogDomain
            // 
            this.openFileDialogDomain.DefaultExt = "lis";
            this.openFileDialogDomain.FileName = "openFileDialogDomain";
            resources.ApplyResources(this.openFileDialogDomain, "openFileDialogDomain");
            // 
            // openFileDialogURL
            // 
            this.openFileDialogURL.DefaultExt = "lis";
            this.openFileDialogURL.FileName = "openFileDialogURL";
            resources.ApplyResources(this.openFileDialogURL, "openFileDialogURL");
            // 
            // contextMenuListGroup
            // 
            this.contextMenuListGroup.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripListGroupEnabled,
            this.toolStripListGroupRename,
            this.toolStripListGroupNew,
            this.toolStripListGroupDelete});
            this.contextMenuListGroup.Name = "contextMenuListGroup";
            resources.ApplyResources(this.contextMenuListGroup, "contextMenuListGroup");
            // 
            // toolStripListGroupEnabled
            // 
            this.toolStripListGroupEnabled.Name = "toolStripListGroupEnabled";
            resources.ApplyResources(this.toolStripListGroupEnabled, "toolStripListGroupEnabled");
            this.toolStripListGroupEnabled.Click += new System.EventHandler(this.toolStripListGroupEnabled_Click);
            // 
            // toolStripListGroupRename
            // 
            this.toolStripListGroupRename.Name = "toolStripListGroupRename";
            resources.ApplyResources(this.toolStripListGroupRename, "toolStripListGroupRename");
            this.toolStripListGroupRename.Click += new System.EventHandler(this.toolStripListGroupRename_Click);
            // 
            // toolStripListGroupNew
            // 
            this.toolStripListGroupNew.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripListGroupNewSubgroup,
            this.toolStripListGroupNewList});
            this.toolStripListGroupNew.Name = "toolStripListGroupNew";
            resources.ApplyResources(this.toolStripListGroupNew, "toolStripListGroupNew");
            // 
            // toolStripListGroupNewSubgroup
            // 
            this.toolStripListGroupNewSubgroup.Name = "toolStripListGroupNewSubgroup";
            resources.ApplyResources(this.toolStripListGroupNewSubgroup, "toolStripListGroupNewSubgroup");
            this.toolStripListGroupNewSubgroup.Click += new System.EventHandler(this.toolStripListGroupNewSubgroup_Click);
            // 
            // toolStripListGroupNewList
            // 
            this.toolStripListGroupNewList.Name = "toolStripListGroupNewList";
            resources.ApplyResources(this.toolStripListGroupNewList, "toolStripListGroupNewList");
            this.toolStripListGroupNewList.Click += new System.EventHandler(this.toolStripListGroupNewList_Click);
            // 
            // toolStripListGroupDelete
            // 
            this.toolStripListGroupDelete.Name = "toolStripListGroupDelete";
            resources.ApplyResources(this.toolStripListGroupDelete, "toolStripListGroupDelete");
            this.toolStripListGroupDelete.Click += new System.EventHandler(this.toolStripListGroupDelete_Click);
            // 
            // contextMenuList
            // 
            this.contextMenuList.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripListEnabled,
            this.toolStripListRename,
            this.toolStripListDelete});
            this.contextMenuList.Name = "contextMenuList";
            resources.ApplyResources(this.contextMenuList, "contextMenuList");
            // 
            // toolStripListEnabled
            // 
            this.toolStripListEnabled.Name = "toolStripListEnabled";
            resources.ApplyResources(this.toolStripListEnabled, "toolStripListEnabled");
            this.toolStripListEnabled.Click += new System.EventHandler(this.toolStripListEnabled_Click);
            // 
            // toolStripListRename
            // 
            this.toolStripListRename.Name = "toolStripListRename";
            resources.ApplyResources(this.toolStripListRename, "toolStripListRename");
            this.toolStripListRename.Click += new System.EventHandler(this.toolStripListRename_Click);
            // 
            // toolStripListDelete
            // 
            this.toolStripListDelete.Name = "toolStripListDelete";
            resources.ApplyResources(this.toolStripListDelete, "toolStripListDelete");
            this.toolStripListDelete.Click += new System.EventHandler(this.toolStripListDelete_Click);
            // 
            // btnOK
            // 
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.Name = "btnOK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnApply
            // 
            resources.ApplyResources(this.btnApply, "btnApply");
            this.btnApply.Name = "btnApply";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // MainForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ControlBox = false;
            this.Controls.Add(this.tabMain);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.groupFilterStatus.ResumeLayout(false);
            this.panelFilterStatusAvailable.ResumeLayout(false);
            this.panelFilterStatusAvailable.PerformLayout();
            this.panelFilterStatusUnavailable.ResumeLayout(false);
            this.panelFilterStatusUnavailable.PerformLayout();
            this.tabMain.ResumeLayout(false);
            this.tabInformation.ResumeLayout(false);
            this.groupServiceStatus.ResumeLayout(false);
            this.groupServiceStatus.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.tabLists.ResumeLayout(false);
            this.tabLists.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupListFile.ResumeLayout(false);
            this.groupListFile.PerformLayout();
            this.groupListGroup.ResumeLayout(false);
            this.groupListGroup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
            this.tabRemotingSetup.ResumeLayout(false);
            this.tabRemotingSetup.PerformLayout();
            this.groupRemotingConnectAs.ResumeLayout(false);
            this.groupRemotingConnectAs.PerformLayout();
            this.groupRemotingConnectTo.ResumeLayout(false);
            this.groupRemotingConnectTo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.tabSettings.ResumeLayout(false);
            this.tabSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            this.groupAdapterSettings.ResumeLayout(false);
            this.groupAdapterSettings.PerformLayout();
            this.groupDefaultSettings.ResumeLayout(false);
            this.groupDefaultSettings.PerformLayout();
            this.systemTrayContextMenu.ResumeLayout(false);
            this.contextMenuListGroup.ResumeLayout(false);
            this.contextMenuList.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblServiceStatus;
        private System.Windows.Forms.Button btnStartStop;
        private System.Windows.Forms.Button btnEnableDisable;
        private System.Windows.Forms.GroupBox groupFilterStatus;
        private System.ComponentModel.BackgroundWorker backgroundWorkerServiceControl;
        private System.Windows.Forms.TabControl tabMain;
        private System.Windows.Forms.TabPage tabInformation;
        private System.Windows.Forms.TabPage tabSettings;
        private System.Windows.Forms.TabPage tabLists;
        private System.Windows.Forms.NotifyIcon systemTrayIcon;
        private System.Windows.Forms.ContextMenuStrip systemTrayContextMenu;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemEnableDisable;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemAbout;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemExit;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemOpen;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.GroupBox groupDefaultSettings;
        private System.Windows.Forms.CheckBox chkDefaultFilterHttp;
        private System.Windows.Forms.CheckBox chkDefaultFilterTcp;
        private System.Windows.Forms.CheckBox chkDefaultFilterAll;
        private System.Windows.Forms.CheckBox chkDefaultFilterUdp;
        private System.Windows.Forms.GroupBox groupAdapterSettings;
        private System.Windows.Forms.CheckBox chkOverrideDefault;
        private System.Windows.Forms.CheckBox chkOverrideFilterHttp;
        private System.Windows.Forms.CheckBox chkOverrideFilterTcp;
        private System.Windows.Forms.CheckBox chkOverrideFilterAll;
        private System.Windows.Forms.CheckBox chkOverrideFilterUdp;
        private System.Windows.Forms.Label lblDomainCountDescription;
        private System.Windows.Forms.Label lblURLCountDescription;
        private System.Windows.Forms.Label lblIPCountDescription;
        private System.Windows.Forms.Label lblDomainAllowCount;
        private System.Windows.Forms.Label lblDomainBlockCount;
        private System.Windows.Forms.Label lblURLAllowCount;
        private System.Windows.Forms.Label lblURLBlockCount;
        private System.Windows.Forms.Label lblIPAllowCount;
        private System.Windows.Forms.Label lblIPBlockCount;
        private System.Windows.Forms.Label lblAllowingDescription;
        private System.Windows.Forms.Label lblBlockingDescription;
        private System.Windows.Forms.RadioButton rbListBlock;
        private System.Windows.Forms.RadioButton rbListAllow;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.TextBox txtQuickAdd;
        private System.Windows.Forms.Button btnQuickAdd;
        private System.Windows.Forms.Button btnEditListFile;
        private System.Windows.Forms.Label lblListFile;
        private System.Windows.Forms.Label lblFilterType;
        private System.Windows.Forms.OpenFileDialog openFileDialogIP;
        private System.Windows.Forms.OpenFileDialog openFileDialogDomain;
        private System.Windows.Forms.OpenFileDialog openFileDialogURL;
        private System.Windows.Forms.Label lblAdapter;
        private System.Windows.Forms.ComboBox ddlAdapters;
        private System.Windows.Forms.Label lblDefaultDescription;
        private System.Windows.Forms.TextBox lblAdapterNotes;
        private System.Windows.Forms.TreeView treeViewLists;
        private System.Windows.Forms.ComboBox ddlContentType;
        private System.Windows.Forms.Label lblContentType;
        private System.Windows.Forms.GroupBox groupListFile;
        private System.Windows.Forms.ContextMenuStrip contextMenuListGroup;
        private System.Windows.Forms.ToolStripMenuItem toolStripListGroupEnabled;
        private System.Windows.Forms.ToolStripMenuItem toolStripListGroupRename;
        private System.Windows.Forms.ContextMenuStrip contextMenuList;
        private System.Windows.Forms.ToolStripMenuItem toolStripListEnabled;
        private System.Windows.Forms.ToolStripMenuItem toolStripListRename;
        private System.Windows.Forms.GroupBox groupListGroup;
        private System.Windows.Forms.Label lblListGroupStatusDescription;
        private System.Windows.Forms.RadioButton rbListLog;
        private System.Windows.Forms.Label lblListStatusDescription;
        private System.Windows.Forms.Label lblListGroupStatus;
        private System.Windows.Forms.Label lblListStatus;
        private System.Windows.Forms.TabPage tabRemotingSetup;
        private System.Windows.Forms.TextBox txtServerName;
        private System.Windows.Forms.TextBox txtRemotingPort;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.ToolStripMenuItem toolStripListGroupNew;
        private System.Windows.Forms.ToolStripMenuItem toolStripListGroupNewSubgroup;
        private System.Windows.Forms.ToolStripMenuItem toolStripListGroupNewList;
        private System.Windows.Forms.ToolStripMenuItem toolStripListGroupDelete;
        private System.Windows.Forms.ToolStripMenuItem toolStripListDelete;
        private System.Windows.Forms.Button btnRemotingTest;
        private System.Windows.Forms.Panel panelFilterStatusAvailable;
        private System.Windows.Forms.Panel panelFilterStatusUnavailable;
        private System.Windows.Forms.Label lblFilterStatusUnavailable;
        private System.Windows.Forms.Label lblRemotingSettingsNotes;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Label lblRemotingSettingsError;
        private System.Windows.Forms.Button btnServiceStatisticsRefresh;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.GroupBox groupServiceStatus;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.TextBox txtListNotes;
        private System.Windows.Forms.PictureBox pictureBox4;
        private System.Windows.Forms.GroupBox groupRemotingConnectTo;
        private System.Windows.Forms.RadioButton rbRemotingRemoteMachine;
        private System.Windows.Forms.RadioButton rbRemotingLocalMachine;
        private System.Windows.Forms.GroupBox groupRemotingConnectAs;
        private System.Windows.Forms.RadioButton rbRemotingCurrentUser;
        private System.Windows.Forms.TextBox txtRemotingUsername;
        private System.Windows.Forms.RadioButton rbRemotingRemoteUser;
        private System.Windows.Forms.TextBox txtRemotingConfirmPassword;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtRemotingPassword;
        private System.Windows.Forms.Label lblRemotingPassword;
        private System.Windows.Forms.Button btnAbout;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.Button btnAddNewLists;
        private System.Windows.Forms.Label lblLastListUpdateDate;
        private System.Windows.Forms.Label lblLastListUpdateDateLabel;
    }
}

