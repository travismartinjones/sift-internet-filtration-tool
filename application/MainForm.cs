/*
 *    Copyright 2007 Travis Jones
 * 
 *    This file is part of SIFT.
 *
 *    SIFT is free software: you can redistribute it and/or modify
 *    it under the terms of the GNU General Public License as published by
 *    the Free Software Foundation, either version 3 of the License, or
 *    (at your option) any later version.
 *
 *    SIFT is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 *    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *    GNU General Public License for more details.
 *
 *    You should have received a copy of the GNU General Public License
 *    along with SIFT.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Security.Permissions;
using Microsoft.Win32;
using System.Management;

// Globalization includes
using System.Reflection;
using System.Resources;
using System.Globalization;

// wizard control (Genghis toolkit 0.8)
using Genghis.Windows.Forms;

namespace Sift
{    
    public partial class MainForm : SiftForm
    {
        private System.ServiceProcess.ServiceController filterService = null;                                 

        public MainForm()
        {

            bool isOnlyInstance;

            Mutex mutex = new Mutex(true, "SiftAppication", out isOnlyInstance);
            // keep the mutex reference alive until the normal termination of the program
            GC.KeepAlive(mutex);

            if (!isOnlyInstance)
            {
                this.Close();
            }
            else
            {
                InitializeComponent();
                CustomInitializeComponents();

                if (IsServiceAvailable)
                {
                    if (Application.ApplicationSettings.Settings.IsFirstStartup)
                    {
                        // if the application is starting for the first time, show the configuration wizard
                        ShowWizard();
                    }
                    else
                    {
                        CheckForUpdates();
                    }
                }

                // startup the server statistics updater
                threadServerStatisticsUpdater = new Thread(new ThreadStart(ServiceStatisticsUpdater));
                threadServerStatisticsUpdater.Start();

            }
        }

        #region PROPERTIES

        ResourceManager resourceManager = new ResourceManager("MainForm",Assembly.GetExecutingAssembly());
        Thread threadServerStatisticsUpdater = null;

        private Resources.Settings.ListGroup _listGroupQuickAdd = null;
        private Resources.Settings.ListGroup ListGroupQuickAdd
        {
            get
            {
                if (_listGroupQuickAdd == null)
                {                    
                    Resources.Settings.ListGroup customGroup = Resources.Settings.ListSettings.Settings.ListGroups.GetByListGroupId(Resources.Constants.ListGroupCustomId);

                    if (customGroup == null) // the custom group is missing, so add it
                    {
                        customGroup = new Sift.Resources.Settings.ListGroup();

                        customGroup.Id = Resources.Constants.ListGroupCustomId;
                        customGroup.Description = Sift.Strings.MainForm.ListGroupCustom;
                        customGroup.Enabled = true;
                        customGroup.Log = true;

                        Resources.Settings.ListSettings.Settings.ListGroups.Add(customGroup);                        
                    }

                    Resources.Settings.ListGroup quickAddGroup = customGroup.ListGroups.GetByListGroupId(Resources.Constants.ListGroupCustomQuickAddId);

                    if (quickAddGroup == null) // the quick add group is missing, so add it
                    {
                        quickAddGroup = new Sift.Resources.Settings.ListGroup();

                        quickAddGroup.Id = Resources.Constants.ListGroupCustomQuickAddId;
                        quickAddGroup.Description = Sift.Strings.MainForm.ListGroupQuickAdd;
                        quickAddGroup.Enabled = true;
                        quickAddGroup.Log = true;

                        customGroup.ListGroups.Add(quickAddGroup);                        
                    }

                    _listGroupQuickAdd = quickAddGroup;                    
                        
                }

                return _listGroupQuickAdd;
            }
        }

        public bool IsConfigurationFileLocal
        {
            get
            {                
                // the service is installed and the target of the administration is the current machine
                if(IsServiceInstalled && (HostName == Sift.Strings.MainForm.NetworkLocalhost || HostName == "127.0.0.1"))
                    return true;
                else
                    return false;
            }
        }

        #region Remoting Properties

        public string HostName
        {
            get
            {
                if (rbRemotingLocalMachine.Checked || txtServerName.Text == string.Empty)
                    return "127.0.0.1";
                else                
                    return txtServerName.Text;                
            }
        }

        public int HostPort
        {
            get
            {
                int port = 8080;

                Int32.TryParse(txtRemotingPort.Text, out port);

                return port;
            }
        }

        public string RemotingUsername
        {
            get
            {
                if (txtRemotingUsername.Text.Contains(@"\"))
                {
                    string[] usernameParts = txtRemotingUsername.Text.Split('\\');
                    return usernameParts[1];
                }
                else
                    return txtRemotingUsername.Text;
            }
        }

        public string RemotingDomain
        {
            get
            {
                if (txtRemotingUsername.Text.Contains(@"\"))
                {
                    string[] usernameParts = txtRemotingUsername.Text.Split('\\');
                    return usernameParts[0];
                }
                else
                    return string.Empty;
            }
        }

        public string RemotingPassword
        {
            get
            {
                return txtRemotingPassword.Text;
            }
        }

        #region IsRemotingAvailable
        
        private string _previousHostName = null;
        private int? _previousHostPort = null;
        private bool? _previousIsRemotingAvailable = null;

        /// <summary>
        /// Returns true if the remoting server is available. This operation is somewhat slow and requires an exception
        /// to be thrown. Therefore we cache the returned value, until the host name or host port changes.
        /// </summary>
        public bool IsRemotingAvailable
        {
            get
            {
                // if the service is not available, remoting is not available
                if (!IsServiceAvailable)
                    return false;

                if (_previousIsRemotingAvailable.HasValue && _previousHostName != null && _previousHostPort.HasValue &&
                    _previousHostName == HostName && _previousHostPort.Value == HostPort)
                {
                    return _previousIsRemotingAvailable.Value;
                }

                _previousHostName = HostName;
                _previousHostPort = HostPort;
                _previousIsRemotingAvailable = RemotingClient.IsRemotingAvailable(HostName, HostPort, RemotingDomain, RemotingUsername, RemotingPassword);
                return _previousIsRemotingAvailable.Value;
            }
        }
        #endregion

        public bool IsAdministeringLocal
        {
            get
            {
                return rbRemotingLocalMachine.Checked;
            }
        }

        #endregion

        private bool IsFilterEnabled
        {
            get
            {
                return RemotingClient.GetServiceStatistics(HostName, HostPort, RemotingDomain, RemotingUsername, RemotingPassword).IsEnabled;
            }
        }

        private bool IsServiceAvailable
        {
            get
            {
                if (IsServiceInstalled)
                {
                    try
                    {
                        using (new Resources.Impersonator(RemotingUsername, RemotingDomain, RemotingPassword))
                        {
                            filterService.Refresh();
                            if (filterService.Status == System.ServiceProcess.ServiceControllerStatus.Running)
                                return true;
                            else
                                return false;
                        }
                    }
                    catch
                    {                        
                        return false;
                    }
                }
                else
                    return false;
            }
        }

        private bool IsServiceInstalled
        {
            get
            {
                if (filterService == null)
                {
                    // check to see if the service is installed on the sytem
                    System.ServiceProcess.ServiceController[] services;

                    using (new Resources.Impersonator(RemotingUsername, RemotingDomain, RemotingPassword))
                    {   // run this block of code using an impersonated security level

                        string serviceHostName = HostName;
                        if (HostName == "127.0.0.1") // '127.0.0.1' doesn't work, needs to be '.'
                            serviceHostName = ".";

                        services = System.ServiceProcess.ServiceController.GetServices(serviceHostName);
                    }


                    foreach (System.ServiceProcess.ServiceController service in services)
                        if (service.ServiceName == Resources.Constants.ServiceName)
                        {
                            filterService = service;
                            return true;
                        }

                    return false;
                }

                return true;
            }
        }

        #endregion PROPERTIES

        #region METHODS

        private void ShowWizard()
        {
            WizardSheet wizard = new WizardSheet("SIFT Configuration Wizard");
            wizard.ShowHelpButton = false;

            WizardStart wizardStart = new WizardStart();
            WizardCategorySelection wizardCategorySelection = null;

            try
            {
                wizardCategorySelection = new WizardCategorySelection();
            }
            catch (System.Net.WebException ex)
            {
                MessageBox.Show(ex.Message + System.Environment.NewLine +
                                System.Environment.NewLine + "You will be able to select the content you wish to block" +
                                System.Environment.NewLine + "after restoring your internet connection by selecting the" +
                                System.Environment.NewLine + "Lists tab and clicking 'Add New Lists'");
            }

            wizard.AddPage(wizardStart);

            if(wizardCategorySelection != null)
                wizard.AddPage(wizardCategorySelection);

            if (wizard.ShowDialog(this) == DialogResult.OK)
            {
                // process wizard results if finish if clicked
                wizardCategorySelection.ApplyChanges();
                Sift.Resources.Settings.ListSettings.Refresh();
                Sift.Resources.Settings.ListUpdateSettings.Refresh();
                this.ApplyChanges();

                // store that we have started up for the first time
                Application.ApplicationSettings.Settings.IsFirstStartup = false;
                Application.ApplicationSettings.Save();
            }            
        }

        private void CheckForUpdates()
        {
            if (((TimeSpan)DateTime.Now.Subtract(Sift.Application.ApplicationSettings.Settings.LastUpdatePoll)).Days > 7)
            {
                if (UpdateManager.IsNewVersionAvailable)
                {
                    Updates updateForm = new Updates();
                    updateForm.ShowDialog(this);
                    Sift.Application.ApplicationSettings.Settings.LastUpdatePoll = DateTime.Now;
                    Sift.Application.ApplicationSettings.Save();
                }
            }
        }

        private void CustomInitializeComponents()
        {
            // load any remoting settings
            LoadRemotingSettings();

            EstablishRemotingConnection();

            if (IsServiceAvailable)
            {
                // load and update adapter tab            
                LoadAdapterSettings();

                // load and update lists tab
                LoadLists();
            }            

            UpdateServiceFormElements();

            //UpdateServiceStatisticsFormElements();

            // set the filter enabled status                        
            UpdateServiceStatus();            

            btnApply.Enabled = false;
        }

        #region Update Form Elements

        private void UpdateServiceFormElements()
        {            
            if(IsServiceAvailable)
                UpdateServiceFormInformation(Sift.Strings.MainForm.ServiceStartedDescription, Sift.Strings.MainForm.ServiceStopAction, true, true);
            else
                UpdateServiceFormInformation(Sift.Strings.MainForm.ServiceStoppedDescription, Sift.Strings.MainForm.ServiceStartAction, true, false);
        }

        private void ServiceStatisticsUpdater()
        {                        
            while (true)
            {                                        
                UpdateServiceStatisticsFormElements();          
                
                // update every 3 seconds if normal
                Thread.Sleep(3000);
            }
        }

        #region Thread Safe Service Statistics Update Calls

        delegate void btnEnableDisable_UpdateEnabledCallback(bool enabled);
        private void btnEnableDisable_UpdateEnabled(bool enabled)
        {
            if (btnEnableDisable.InvokeRequired)
            {
                btnEnableDisable_UpdateEnabledCallback callback = new btnEnableDisable_UpdateEnabledCallback(btnEnableDisable_UpdateEnabled);
                this.Invoke(callback, new object[] { enabled });
            }
            else
                btnEnableDisable.Enabled = enabled;
        }

        delegate void btnEnableDisable_UpdateTextCallback(string text);
        private void btnEnableDisable_UpdateText(string text)
        {
            if (btnEnableDisable.InvokeRequired)
            {
                btnEnableDisable_UpdateTextCallback callback = new btnEnableDisable_UpdateTextCallback(btnEnableDisable_UpdateText);
                this.Invoke(callback, new object[] { text });
            }
            else
                btnEnableDisable.Text = text;
        }

        delegate void lblIPAllowCount_UpdateTextCallback(string text);
        private void lblIPAllowCount_UpdateText(string text)
        {
            if (lblIPAllowCount.InvokeRequired)
            {
                lblIPAllowCount_UpdateTextCallback callback = new lblIPAllowCount_UpdateTextCallback(lblIPAllowCount_UpdateText);
                this.Invoke(callback, new object[] { text });
            }
            else
                lblIPAllowCount.Text = text;
        }

        delegate void lblIPBlockCount_UpdateTextCallback(string text);
        private void lblIPBlockCount_UpdateText(string text)
        {
            if (lblIPBlockCount.InvokeRequired)
            {
                lblIPBlockCount_UpdateTextCallback callback = new lblIPBlockCount_UpdateTextCallback(lblIPBlockCount_UpdateText);
                this.Invoke(callback, new object[] { text });
            }
            else
                lblIPBlockCount.Text = text;
        }

        delegate void lblDomainAllowCount_UpdateTextCallback(string text);
        private void lblDomainAllowCount_UpdateText(string text)
        {
            if (lblDomainAllowCount.InvokeRequired)
            {
                lblDomainAllowCount_UpdateTextCallback callback = new lblDomainAllowCount_UpdateTextCallback(lblDomainAllowCount_UpdateText);
                this.Invoke(callback, new object[] { text });
            }
            else
                lblDomainAllowCount.Text = text;
        }

        delegate void lblDomainBlockCount_UpdateTextCallback(string text);
        private void lblDomainBlockCount_UpdateText(string text)
        {
            if (lblDomainBlockCount.InvokeRequired)
            {
                lblDomainBlockCount_UpdateTextCallback callback = new lblDomainBlockCount_UpdateTextCallback(lblDomainBlockCount_UpdateText);
                this.Invoke(callback, new object[] { text });
            }
            else
                lblDomainBlockCount.Text = text;
        }

        delegate void lblURLAllowCount_UpdateTextCallback(string text);
        private void lblURLAllowCount_UpdateText(string text)
        {
            if (lblURLAllowCount.InvokeRequired)
            {
                lblURLAllowCount_UpdateTextCallback callback = new lblURLAllowCount_UpdateTextCallback(lblURLAllowCount_UpdateText);
                this.Invoke(callback, new object[] { text });
            }
            else
                lblURLAllowCount.Text = text;
        }

        delegate void lblURLBlockCount_UpdateTextCallback(string text);
        private void lblURLBlockCount_UpdateText(string text)
        {
            if (lblURLBlockCount.InvokeRequired)
            {
                lblURLBlockCount_UpdateTextCallback callback = new lblURLBlockCount_UpdateTextCallback(lblURLBlockCount_UpdateText);
                this.Invoke(callback, new object[] { text });
            }
            else
                lblURLBlockCount.Text = text;
        }

        delegate void lblLastListUpdateDate_UpdateTextCallback(string text);
        private void lblLastListUpdateDate_UpdateText(string text)
        {
            if (lblLastListUpdateDate.InvokeRequired)
            {
                lblLastListUpdateDate_UpdateTextCallback callback = new lblLastListUpdateDate_UpdateTextCallback(lblLastListUpdateDate_UpdateText);
                this.Invoke(callback, new object[] { text });
            }
            else
                lblLastListUpdateDate.Text = text;
        }

        delegate void lblRemotingSettingsError_UpdateVisibleCallback(bool visible);
        private void lblRemotingSettingsError_UpdateVisible(bool visible)
        {
            if (lblRemotingSettingsError.InvokeRequired)
            {
                lblRemotingSettingsError_UpdateVisibleCallback callback = new lblRemotingSettingsError_UpdateVisibleCallback(lblRemotingSettingsError_UpdateVisible);
                this.Invoke(callback, new object[] { visible });
            }
            else
                lblRemotingSettingsError.Visible = visible;
        }
       
        delegate void panelFilterStatusAvailable_UpdateVisibleCallback(bool visible);
        private void panelFilterStatusAvailable_UpdateVisible(bool visible)
        {
            if (panelFilterStatusAvailable.InvokeRequired)
            {
                panelFilterStatusAvailable_UpdateVisibleCallback callback = new panelFilterStatusAvailable_UpdateVisibleCallback(panelFilterStatusAvailable_UpdateVisible);
                this.Invoke(callback, new object[] { visible });
            }
            else
                panelFilterStatusAvailable.Visible = visible;
        }

        delegate void panelFilterStatusUnavailable_UpdateVisibleCallback(bool visible);
        private void panelFilterStatusUnavailable_UpdateVisible(bool visible)
        {
            if (panelFilterStatusUnavailable.InvokeRequired)
            {
                panelFilterStatusUnavailable_UpdateVisibleCallback callback = new panelFilterStatusUnavailable_UpdateVisibleCallback(panelFilterStatusUnavailable_UpdateVisible);
                this.Invoke(callback, new object[] { visible });
            }
            else
                panelFilterStatusUnavailable.Visible = visible;
        }

        #endregion

        private void UpdateServiceStatisticsFormElements()
        {
            Resources.ServiceStatistics statistics = RemotingClient.GetServiceStatistics(HostName, HostPort, RemotingDomain, RemotingUsername, RemotingPassword);

            if (IsRemotingAvailable)
            {
                if (statistics == null)
                {
                    btnEnableDisable_UpdateEnabled(false);
                    lblIPAllowCount_UpdateText(Sift.Strings.MainForm.ServiceStatisticsUnknownCharacter);
                    lblIPBlockCount_UpdateText(Sift.Strings.MainForm.ServiceStatisticsUnknownCharacter);
                    lblDomainAllowCount_UpdateText(Sift.Strings.MainForm.ServiceStatisticsUnknownCharacter);
                    lblDomainBlockCount_UpdateText(Sift.Strings.MainForm.ServiceStatisticsUnknownCharacter);
                    lblURLAllowCount_UpdateText(Sift.Strings.MainForm.ServiceStatisticsUnknownCharacter);
                    lblURLBlockCount_UpdateText(Sift.Strings.MainForm.ServiceStatisticsUnknownCharacter);
                    lblLastListUpdateDate_UpdateText(Sift.Strings.MainForm.ServiceStatisticsUnknownCharacter);
                }
                else
                {
                    btnEnableDisable_UpdateEnabled(true);

                    if (statistics.IsEnabled)
                        btnEnableDisable_UpdateText(Sift.Strings.MainForm.ServiceDisableAction);
                    else
                        btnEnableDisable_UpdateText(Sift.Strings.MainForm.ServiceEnableAction);

                    lblIPAllowCount_UpdateText(statistics.IpAllowCount.ToString());
                    lblIPBlockCount_UpdateText(statistics.IpBlockCount.ToString());
                    lblDomainAllowCount_UpdateText(statistics.DomainAllowCount.ToString());
                    lblDomainBlockCount_UpdateText(statistics.DomainBlockCount.ToString());
                    lblURLAllowCount_UpdateText(statistics.UrlAllowCount.ToString());
                    lblURLBlockCount_UpdateText(statistics.UrlBlockCount.ToString());
                    lblLastListUpdateDate_UpdateText(statistics.LastListUpdate.ToShortDateString());
                }
            }

            panelFilterStatusAvailable_UpdateVisible(IsRemotingAvailable);
            panelFilterStatusUnavailable_UpdateVisible(!IsRemotingAvailable);
            lblRemotingSettingsError_UpdateVisible(!IsRemotingAvailable);
        }

        private void UpdateListGroupDetails(Resources.Settings.ListGroup listGroup)
        {
            // hide the list panel
            groupListGroup.Visible = true;
            groupListFile.Visible = false;

            if (listGroup.Enabled)
                lblListGroupStatus.Text = Sift.Strings.MainForm.ServiceEnabled;
            else
                lblListGroupStatus.Text = Sift.Strings.MainForm.ServiceDisabled;

        }

        private void UpdateListDetails(Resources.Settings.List list)
        {
            // hide the list group panel
            groupListGroup.Visible = false;
            groupListFile.Visible = true;
            
            ddlContentType.SelectedIndex = ddlContentType.FindStringExact(list.Content.ToString());
            rbListBlock.Checked = list.MatchAction == Sift.Resources.Types.MatchActionType.Block;
            rbListAllow.Checked = list.MatchAction == Sift.Resources.Types.MatchActionType.Allow;
            rbListLog.Checked = list.MatchAction == Sift.Resources.Types.MatchActionType.Log;

            if (list.Enabled)
                lblListStatus.Text = Sift.Strings.MainForm.ServiceEnabled;
            else
                lblListStatus.Text = Sift.Strings.MainForm.ServiceDisabled;
        }

        private void UpdateListGroupContextMenu(TreeNode node)
        {
            Resources.Settings.ListGroup listGroup = (Resources.Settings.ListGroup)node.Tag;

            if (listGroup.Enabled)
                contextMenuListGroup.Items[0].Text = Sift.Strings.MainForm.ListGroupDisable;
            else
                contextMenuListGroup.Items[0].Text = Sift.Strings.MainForm.ListGroupEnable;
        }

        private void UpdateListContextMenu(TreeNode node)
        {
            Resources.Settings.List list = (Resources.Settings.List)node.Tag;

            if (list.Enabled)
                contextMenuList.Items[0].Text = Sift.Strings.MainForm.ListDisable;
            else
                contextMenuList.Items[0].Text = Sift.Strings.MainForm.ListEnable;
        }

        #endregion

        #region Tree Node Methods

        private TreeNode[] GetTreeNodesFromLists(Resources.Settings.ListCollection lists)
        {
            System.Collections.Generic.List<TreeNode> treeNodes = new System.Collections.Generic.List<TreeNode>();

            foreach (Resources.Settings.List list in lists)
            {
                TreeNode newNode = new TreeNode();

                // set tree node properties
                newNode.Text = list.Description;
                newNode.Checked = list.Enabled;
                newNode.ImageIndex = 1;
                newNode.SelectedImageIndex = 1;
                newNode.ContextMenuStrip = contextMenuList;
                newNode.Tag = list;

                treeNodes.Add(newNode);
            }

            return treeNodes.ToArray();
        }

        private TreeNode GetTreeNodeFromListGroup(Resources.Settings.ListGroup listGroup)
        {
            TreeNode newNode = new TreeNode();

            newNode.Text = listGroup.Description;
            newNode.Checked = listGroup.Enabled;
            newNode.Tag = listGroup;
            newNode.ImageIndex = 0;
            newNode.SelectedImageIndex = 0;
            newNode.ContextMenuStrip = contextMenuListGroup;
            newNode.ContextMenuStrip.Tag = newNode;

            foreach (Resources.Settings.ListGroup childListGroup in listGroup.ListGroups)
                newNode.Nodes.Add(GetTreeNodeFromListGroup(childListGroup));

            newNode.Nodes.AddRange(GetTreeNodesFromLists(listGroup.Lists));

            return newNode;
        }

        private TreeNode[] GetTreeNodesFromListSettings()
        {           
            System.Collections.Generic.List<TreeNode> treeNodes = new System.Collections.Generic.List<TreeNode>();            

            foreach(Resources.Settings.ListGroup listGroup in Resources.Settings.ListSettings.Settings.ListGroups)
                treeNodes.Add(GetTreeNodeFromListGroup(listGroup));

            return treeNodes.ToArray();
        }

        private TreeNode FindListNodeByTag(TreeNodeCollection nodes, Object tag)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Tag != null)
                {
                    if (node.Tag == tag)
                        return node;
                }

                TreeNode matchingNode = FindListNodeByTag(node.Nodes, tag);

                if (matchingNode != null)
                    return matchingNode;
            }

            return null;
        }

        private void SelectListNode(Resources.Settings.ListGroup listGroup)
        {
            if (listGroup != null)
            {
                TreeNode matchingNode = FindListNodeByTag(treeViewLists.Nodes, listGroup);
                if (matchingNode != null)
                {
                    treeViewLists.SelectedNode = matchingNode;
                    matchingNode.Expand();
                    UpdateSelectedNode();
                }
            }
        }

        private void SelectListNode(Resources.Settings.List list)
        {
            if (list != null)
            {
                TreeNode matchingNode = FindListNodeByTag(treeViewLists.Nodes, list);
                if (matchingNode != null)
                {
                    treeViewLists.SelectedNode = matchingNode;
                    matchingNode.Expand();
                    UpdateSelectedNode();
                }
            }
        }

        private void UpdateSelectedNode()
        {
            if (treeViewLists.SelectedNode != null)
            {
                // updating the selected node should not trigger a settings change, so store away the values for restoring later
                bool previousApplyEnabled = btnApply.Enabled;
                bool previousHasListSettingsChanged = Program.HasListSettingsChanged;                

                // set the appropriate context menu
                if (treeViewLists.SelectedNode.Tag.GetType() == typeof(Resources.Settings.ListGroup))
                    UpdateListGroupContextMenu(treeViewLists.SelectedNode);
                else if (treeViewLists.SelectedNode.Tag.GetType() == typeof(Resources.Settings.List))
                    UpdateListContextMenu(treeViewLists.SelectedNode);

                // update the detail pane based on the selected tree node type
                if (treeViewLists.SelectedNode.Tag.GetType() == typeof(Resources.Settings.ListGroup))
                    UpdateListGroupDetails((Resources.Settings.ListGroup)treeViewLists.SelectedNode.Tag);
                else if (treeViewLists.SelectedNode.Tag.GetType() == typeof(Resources.Settings.List))
                    UpdateListDetails((Resources.Settings.List)treeViewLists.SelectedNode.Tag);

                // restore the values
                btnApply.Enabled = previousApplyEnabled;
                Program.HasListSettingsChanged = previousHasListSettingsChanged;
            }
        }

        #endregion

        #region List Updates

        public void UpdateListValue(Guid listId, string value, Resources.Types.ListUpdateType action)
        {
            Resources.Settings.ListUpdate listUpdate = Resources.Settings.ListUpdateSettings.Settings.ListsUpdates.GetByListId(listId);

            if (listUpdate == null)
            {
                // add the list update section if it doesn't exist already
                listUpdate = new Resources.Settings.ListUpdate();
                listUpdate.ListId = listId;
                Resources.Settings.ListUpdateSettings.Settings.ListsUpdates.Add(listUpdate);
            }

            // track the add/removal for processing later
            Resources.Settings.ListEntryUpdate listEntryUpdate = new Resources.Settings.ListEntryUpdate();            
            listEntryUpdate.Action = action;
            listEntryUpdate.Value = value;
            listEntryUpdate.DateCreated = DateTime.Now;
            listUpdate.Updates.Add(listEntryUpdate);

            Program.HasListUpdateSettingsChanged = true;
            btnApply.Enabled = true;
        }

        public void UpdateList(Resources.Settings.List list, Resources.Types.ListUpdateType action)
        {
            Resources.Settings.ListUpdate listUpdate = Resources.Settings.ListUpdateSettings.Settings.ListsUpdates.GetByListId(list.Id);

            if (listUpdate == null)
            {
                // add the list update section if it doesn't exist already
                listUpdate = new Resources.Settings.ListUpdate();
                listUpdate.ListId = list.Id;
                Resources.Settings.ListUpdateSettings.Settings.ListsUpdates.Add(listUpdate);
            }

            listUpdate.Action = action;

            Program.HasListUpdateSettingsChanged = true;
            btnApply.Enabled = true;
        }

        public void RemoveListGroup(Resources.Settings.ListGroup listGroup)
        {
            foreach (Resources.Settings.List list in listGroup.Lists)
                UpdateList(list, Sift.Resources.Types.ListUpdateType.Remove);

            foreach (Resources.Settings.ListGroup childListGroup in listGroup.ListGroups)
                RemoveListGroup(childListGroup);
        }

        private void SaveListUpdateSettings()
        {
            // commit any changes to file            
            Resources.Settings.ListUpdateSettings.Save();            

            if (!IsConfigurationFileLocal && IsRemotingAvailable)
                RemotingClient.SaveListUpdates(HostName, HostPort, RemotingDomain, RemotingUsername, RemotingPassword);

            filterService.ExecuteCommand(Resources.Constants.CustomCommandLoadListUpdates);

            Resources.Settings.ListUpdateSettings.Settings.ListsUpdates.Clear();
        }

        private void ReloadListUpdateSettings()
        {
            Resources.Settings.ListUpdateSettings.RejectChanges();
        }

        #endregion

        #region Adapter Settings

        private void LoadAdapterSettings()
        {
            // the changing of an adapter should not be seen as a settings change, so store away the values to restore later
            bool previousApplyEnabled = btnApply.Enabled;
            bool previousHasAdapterSettingsChanged = Program.HasAdapterSettingsChanged;

            ddlAdapters.DataBindings.Clear();

            if (IsAdministeringLocal)
                Resources.Settings.AdapterSettings.Open(Resources.Constants.InstallPath + Resources.Constants.ConfigurationFilename);           
            else
                Resources.Settings.AdapterSettings.Open(Resources.Constants.InstallPath + Resources.Constants.ConfigurationRemotingFilename); 
                

            Resources.Settings.AdapterSetting defaultSetting = Resources.Settings.AdapterSettings.Settings.Adapters.DefaultSettings;

            if (defaultSetting == null) // the default settings are missing, abort loading
                return;
           
            chkDefaultFilterHttp.Checked = defaultSetting.FilterHTTP;
            chkDefaultFilterTcp.Checked = defaultSetting.FilterTCP;
            chkDefaultFilterUdp.Checked = defaultSetting.FilterUDP;
            chkOverrideFilterAll.Checked = defaultSetting.FilterAll;

            ddlAdapters.DisplayMember = "Description";
            ddlAdapters.ValueMember = "Id";
            ddlAdapters.DataSource = Resources.Settings.AdapterSettings.Settings.Adapters.SystemAdapters;

            if (ddlAdapters.Items.Count > 0)
                ddlAdapters.SelectedIndex = 0;

            // restore the previous values
            btnApply.Enabled = previousApplyEnabled;
            Program.HasAdapterSettingsChanged = previousHasAdapterSettingsChanged;
        }

        private void ReloadServiceAdapterSettings()
        {
            if (IsServiceAvailable)
            {
                using (new Resources.Impersonator(RemotingUsername, RemotingDomain, RemotingPassword))
                {
                    filterService.Refresh();

                    if (filterService.Status == System.ServiceProcess.ServiceControllerStatus.StartPending)
                        filterService.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Running);

                    filterService.ExecuteCommand(Resources.Constants.CustomCommandLoadAdapters);
                }
            }
        }

        private void SaveAdapterSettings()
        {           
            Resources.Settings.AdapterSettings.Save();

            if (!IsConfigurationFileLocal && IsRemotingAvailable) // send the changes to the remoting server
                RemotingClient.SaveConfiguration(HostName, HostPort, RemotingDomain, RemotingUsername, RemotingPassword);

            ReloadServiceAdapterSettings();
        }

        #endregion

        #region List Settings

        private void LoadLists()
        {            
            bool IsSelectedNodeListGroup = false;
            Guid SelectedNodeId = Guid.Empty;

            if (IsAdministeringLocal)
                Resources.Settings.ListSettings.Open(Resources.Constants.InstallPath + Resources.Constants.ConfigurationFilename);
            else
                Resources.Settings.ListSettings.Open(Resources.Constants.InstallPath + Resources.Constants.ConfigurationRemotingFilename);

            if (treeViewLists.SelectedNode != null)
            {
                if (treeViewLists.SelectedNode.Tag.GetType() == typeof(Resources.Settings.ListGroup))
                {
                    IsSelectedNodeListGroup = true;
                    SelectedNodeId = ((Resources.Settings.ListGroup)treeViewLists.SelectedNode.Tag).Id;
                }
                else if (treeViewLists.SelectedNode.Tag.GetType() == typeof(Resources.Settings.List))
                {
                    IsSelectedNodeListGroup = false;
                    SelectedNodeId = ((Resources.Settings.List)treeViewLists.SelectedNode.Tag).Id;
                }
            }

            treeViewLists.Nodes.Clear();
            treeViewLists.Nodes.AddRange(GetTreeNodesFromListSettings());

            if (treeViewLists.Nodes.Count == 0)
            {
                // there are no nodes, add the custom node list and select it
                Resources.Settings.ListGroup listGroup = new Sift.Resources.Settings.ListGroup();
                listGroup.Description = Sift.Strings.MainForm.ListGroupCustom;
                listGroup.Id = Resources.Constants.ListGroupCustomId;
                listGroup.Enabled = true;                
                Resources.Settings.ListSettings.Settings.ListGroups.Add(listGroup);
                treeViewLists.Nodes.AddRange(GetTreeNodesFromListSettings());
            }

            // select the appropriate node, the previous node if there was one, or the first node if not
            if (SelectedNodeId == Guid.Empty)
            {
                treeViewLists.SelectedNode = treeViewLists.Nodes[0];
            }
            else
            {
                if (IsSelectedNodeListGroup)
                {
                    Resources.Settings.ListGroup listGroup = Resources.Settings.ListSettings.Settings.GetByListGroupId(SelectedNodeId);
                    SelectListNode(listGroup);
                }
                else
                {
                    Resources.Settings.List list = Resources.Settings.ListSettings.Settings.GetByListId(SelectedNodeId);
                    SelectListNode(list);
                }
            }

            UpdateSelectedNode();
        }       

        private void SaveListSettings()
        {            
            Resources.Settings.ListSettings.Save();            

            if (!IsConfigurationFileLocal && IsRemotingAvailable)
                RemotingClient.SaveConfiguration(HostName, HostPort, RemotingDomain, RemotingUsername, RemotingPassword);

            filterService.ExecuteCommand(Resources.Constants.CustomCommandLoadLists);

            //ReloadServiceListSettings();            
            Resources.Settings.ListSettings.Close();
        }

        #endregion

        #region Log Settings

        private void ReloadServiceLogSettings()
        {
            if (IsServiceAvailable)
            {
                using (new Resources.Impersonator(RemotingUsername, RemotingDomain, RemotingPassword))
                {
                    filterService.Refresh();

                    if (filterService.Status == System.ServiceProcess.ServiceControllerStatus.StartPending)
                        filterService.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Running);
                }
                // TODO: Reload the log settings
            }
        }

        #endregion                

        #region Remoting

        private void LoadRemotingSettings()
        {
            Sift.Application.ApplicationSettings.RejectChanges(); // undo any changes           

            if (Sift.Application.ApplicationSettings.Settings.RemotingHostName == Sift.Strings.MainForm.NetworkLocalhost)
            {
                rbRemotingLocalMachine.Checked = true;
                rbRemotingRemoteMachine.Checked = false;
                UpdateRemotingFormFields();
                txtServerName.Text = string.Empty;
                txtRemotingPort.Text = Sift.Application.ApplicationSettings.Settings.RemotingPort.ToString();
            }
            else
            {
                rbRemotingLocalMachine.Checked = false;
                rbRemotingRemoteMachine.Checked = true;
                UpdateRemotingFormFields();
                txtServerName.Text = Sift.Application.ApplicationSettings.Settings.RemotingHostName;
                txtRemotingPort.Text = Sift.Application.ApplicationSettings.Settings.RemotingPort.ToString();
            }

            if (Sift.Application.ApplicationSettings.Settings.RemotingUsername == string.Empty)
            {
                rbRemotingCurrentUser.Checked = true;
                rbRemotingRemoteUser.Checked = false;
                UpdateRemotingFormFields();
            }
            else
            {
                rbRemotingCurrentUser.Checked = false;
                rbRemotingRemoteUser.Checked = true;
                UpdateRemotingFormFields();
                txtRemotingUsername.Text = Sift.Application.ApplicationSettings.Settings.RemotingUsername;
                txtRemotingPassword.Text = Sift.Application.ApplicationSettings.Settings.RemotingPassword;
                txtRemotingConfirmPassword.Text = Sift.Application.ApplicationSettings.Settings.RemotingPassword;
            }

            if (Sift.Application.ApplicationSettings.Settings.RemotingDomain != string.Empty)
                txtRemotingUsername.Text = Sift.Application.ApplicationSettings.Settings.RemotingDomain + @"\" + Sift.Application.ApplicationSettings.Settings.RemotingUsername;
            else
                txtRemotingUsername.Text = Sift.Application.ApplicationSettings.Settings.RemotingUsername;

            txtRemotingPassword.Text = Sift.Application.ApplicationSettings.Settings.RemotingPassword;
            txtRemotingConfirmPassword.Text = Sift.Application.ApplicationSettings.Settings.RemotingPassword;

            ToggleSettingTabs(IsConfigurationFileLocal || IsRemotingAvailable);
        }

        private void SaveRemotingSettings()
        {
            if (IsRemotingAvailable)
            {
                if (txtServerName.Text != string.Empty)
                    Sift.Application.ApplicationSettings.Settings.RemotingHostName = txtServerName.Text;
                else
                    Sift.Application.ApplicationSettings.Settings.RemotingHostName = Sift.Strings.MainForm.NetworkLocalhost;

                Sift.Application.ApplicationSettings.Settings.RemotingPort = Convert.ToInt32(txtRemotingPort.Text);              

                string username = string.Empty;
                string domain = string.Empty;

                if (txtRemotingUsername.Text.Contains(@"\"))
                {
                    string[] usernameParts = txtRemotingUsername.Text.Split('\\');
                    domain = usernameParts[0];
                    username = usernameParts[1];
                }
                else
                    username = txtRemotingUsername.Text;

                Sift.Application.ApplicationSettings.Settings.RemotingUsername = username;
                Sift.Application.ApplicationSettings.Settings.RemotingDomain = domain;
                Sift.Application.ApplicationSettings.Settings.RemotingPassword = txtRemotingPassword.Text;
                Sift.Application.ApplicationSettings.Save();            
                EstablishRemotingConnection();

                // the target computer has changed and we have pulled down the configuration, now load it
                if (IsAdministeringLocal)
                {
                    Resources.Settings.AdapterSettings.Open(Resources.Constants.InstallPath + Resources.Constants.ConfigurationFilename);
                    Resources.Settings.ListSettings.Open(Resources.Constants.InstallPath + Resources.Constants.ConfigurationFilename);
                }
                else
                {
                    Resources.Settings.AdapterSettings.Open(Resources.Constants.InstallPath + Resources.Constants.ConfigurationRemotingFilename);
                    Resources.Settings.ListSettings.Open(Resources.Constants.InstallPath + Resources.Constants.ConfigurationRemotingFilename);
                }

                Resources.Settings.ListUpdateSettings.RejectChanges();
                Resources.Settings.ListSettings.Refresh();
                Resources.Settings.AdapterSettings.Refresh();
                UpdateServiceStatus();
            }            

            ToggleSettingTabs(IsRemotingAvailable);
            UpdateServiceStatisticsFormElements();
        }

        private void EstablishRemotingConnection()
        {
            // reset any remoting settings            
            _listGroupQuickAdd = null;

            //if (!IsConfigurationFileLocal && IsServiceAvailable)
            //    Resources.Settings.ListUpdateSettings.Open(System.IO.Directory.GetCurrentDirectory() + @"\" + Resources.Constants.ConfigurationFilename);

            if (!IsConfigurationFileLocal && IsRemotingAvailable)
            {
                // pull the configuration file from the remoting server
                RemotingClient.GetConfiguration(HostName, HostPort, RemotingDomain, RemotingUsername, RemotingPassword);
                //Resources.Settings.AdapterSettings.Open(System.IO.Directory.GetCurrentDirectory() + @"\" + Resources.Constants.ConfigurationFilename);
            }

            LoadAdapterSettings();
            LoadLists();
        }

        private void UpdateRemotingFormFields()
        {
            if (rbRemotingLocalMachine.Checked)
            {
                txtServerName.Text = string.Empty;
                txtServerName.Enabled = false;
                txtRemotingPort.Enabled = false;
                rbRemotingRemoteMachine.Checked = false;

                rbRemotingCurrentUser.Checked = true;
                rbRemotingRemoteUser.Checked = false;
                groupRemotingConnectAs.Enabled = false;
            }

            if (rbRemotingRemoteMachine.Checked)
            {
                rbRemotingLocalMachine.Checked = false;
                txtServerName.Enabled = true;
                txtRemotingPort.Enabled = true;
                groupRemotingConnectAs.Enabled = true;
            }

            if (rbRemotingRemoteUser.Checked)
            {
                rbRemotingCurrentUser.Checked = false;
                txtRemotingUsername.Enabled = true;
                txtRemotingPassword.Enabled = true;
                txtRemotingConfirmPassword.Enabled = true;
            }

            if (rbRemotingCurrentUser.Checked)
            {
                rbRemotingRemoteUser.Checked = false;
                txtRemotingUsername.Text = string.Empty;
                txtRemotingUsername.Enabled = false;
                txtRemotingPassword.Enabled = false;
                txtRemotingPassword.Text = string.Empty;
                txtRemotingConfirmPassword.Text = string.Empty;
                txtRemotingConfirmPassword.Enabled = false;
            }
        }

        #endregion

        #region Service Status

        private void UpdateServiceFormInformation(string serviceStatus, string serviceButtonText, bool serviceButtonEnabled, bool isServiceStarted)
        {
            lblServiceStatus.Text = serviceStatus;
            btnStartStop.Text = serviceButtonText;
            btnStartStop.Enabled = serviceButtonEnabled;
            btnEnableDisable.Enabled = serviceButtonEnabled;
            toolStripMenuItemEnableDisable.Enabled = serviceButtonEnabled;
            ToggleSettingTabs(isServiceStarted);
        }

        private void UpdateServiceStatus()
        {
            if (IsServiceInstalled)
            {
                using (new Resources.Impersonator(RemotingUsername, RemotingDomain, RemotingPassword))
                {
                    filterService.Refresh();

                    switch (filterService.Status)
                    {
                        case System.ServiceProcess.ServiceControllerStatus.StartPending:
                            filterService.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Running);
                            UpdateServiceFormInformation(Sift.Strings.MainForm.ServiceStartedDescription, Sift.Strings.MainForm.ServiceStopAction, true, true);
                            break;
                        case System.ServiceProcess.ServiceControllerStatus.Running:
                            UpdateServiceFormInformation(Sift.Strings.MainForm.ServiceStartedDescription, Sift.Strings.MainForm.ServiceStopAction, true, true);
                            break;
                        case System.ServiceProcess.ServiceControllerStatus.Stopped:
                            filterService.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Stopped);
                            UpdateServiceFormInformation(Sift.Strings.MainForm.ServiceStoppedDescription, Sift.Strings.MainForm.ServiceStartAction, true, false);
                            break;
                        case System.ServiceProcess.ServiceControllerStatus.StopPending:
                            UpdateServiceFormInformation(Sift.Strings.MainForm.ServiceStoppedDescription, Sift.Strings.MainForm.ServiceStartAction, true, false);
                            break;
                    }
                }

                btnEnableDisable.Text = Sift.Strings.MainForm.ServiceDisableAction;
            }
            else
                UpdateServiceFormInformation(Sift.Strings.MainForm.ServiceNotFound, Sift.Strings.MainForm.ServiceStartAction, false, false);                
        }

        #endregion

        #region Quick Add

        private void QuickAddURL(string url)
        {
            Resources.Settings.List urlList = null;

            foreach (Resources.Settings.List list in ListGroupQuickAdd.Lists)
            {
                if (list.Content == Sift.Resources.Types.ContentType.URL)
                {
                    if (list.Description == Sift.Strings.MainForm.ListTypeURL)
                    {
                        // this is the default added list, instead of adding to a user created list, add it to the default
                        urlList = list;
                        break;
                    }
                    else if (urlList == null)
                        urlList = list;
                }
            }

            if (urlList == null) // no quick add url list file exists, add one
            {
                urlList = new Sift.Resources.Settings.List();
                urlList.Id = Guid.NewGuid();
                urlList.Content = Sift.Resources.Types.ContentType.URL;
                urlList.Enabled = true;
                urlList.Description = Sift.Strings.MainForm.ListTypeURL;
                ListGroupQuickAdd.Lists.Add(urlList);
                LoadLists();
                SelectListNode(urlList);
            }

            #region old non-remoting aware list adding
            //// create the directory if it doesn't exist
            //if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(urlList.Path)))
            //    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(urlList.Path));

            //bool IsUrlNew = true;

            //if (System.IO.File.Exists(urlList.Path))
            //{
            //    // check to see if the list already contains the entry
            //    System.IO.TextReader fileInput = new System.IO.StreamReader(urlList.Path);                

            //    string line = string.Empty;

            //    while (line != null)
            //    {
            //        line = fileInput.ReadLine();

            //        if (line == url)
            //            IsUrlNew = false;
            //    }
            //}

            //if (IsUrlNew)
            //{
            //    // only write the line if it doesn't already exist
            //    System.IO.TextWriter fileOutput = new System.IO.StreamWriter(urlList.Path, true);
            //    fileOutput.WriteLine(url);
            //    fileOutput.Close();
            //}
            #endregion

            UpdateListValue(urlList.Id, url, Sift.Resources.Types.ListUpdateType.Add);
        }

        private void QuickAddDomain(string domain)
        {
            Resources.Settings.List domainList = null;

            foreach (Resources.Settings.List list in ListGroupQuickAdd.Lists)
            {
                if (list.Content == Sift.Resources.Types.ContentType.Domain)
                {
                    if (list.Description == Sift.Strings.MainForm.ListTypeDomain)
                    {
                        // this is the default added list, instead of adding to a user created list, add it to the default
                        domainList = list;
                        break;
                    }
                    else if (domainList == null)
                        domainList = list;
                }
            }

            if (domainList == null) // no quick add url list file exists, add one
            {
                domainList = new Sift.Resources.Settings.List();
                domainList.Id = Guid.NewGuid();
                domainList.Content = Sift.Resources.Types.ContentType.Domain;
                domainList.Enabled = true;
                domainList.Description = Sift.Strings.MainForm.ListTypeDomain;
                ListGroupQuickAdd.Lists.Add(domainList);
                LoadLists();
                SelectListNode(domainList);
            }

            UpdateListValue(domainList.Id, domain, Sift.Resources.Types.ListUpdateType.Add);
        }

        private void QuickAddIP(string ip)
        {
            Resources.Settings.List ipList = null;

            foreach (Resources.Settings.List list in ListGroupQuickAdd.Lists)
            {
                if (list.Content == Sift.Resources.Types.ContentType.IP)
                {
                    if (list.Description == Sift.Strings.MainForm.ListTypeIP)
                    {
                        // this is the default added list, instead of adding to a user created list, add it to the default
                        ipList = list;
                        break;
                    }
                    else if (ipList == null)
                        ipList = list;
                }
            }

            if (ipList == null) // no quick add url list file exists, add one
            {
                ipList = new Sift.Resources.Settings.List();
                ipList.Id = Guid.NewGuid();
                ipList.Content = Sift.Resources.Types.ContentType.IP;
                ipList.Enabled = true;
                ipList.Description = Sift.Strings.MainForm.ListTypeIP;
                ListGroupQuickAdd.Lists.Add(ipList);
                LoadLists();
                SelectListNode(ipList);
            }

            UpdateListValue(ipList.Id, ip, Sift.Resources.Types.ListUpdateType.Add);
        }

        #endregion            
       
        private void ToggleSettingTabs(bool visible)
        {
            TabPage selectedTab = tabMain.SelectedTab;

            tabMain.Visible = false;

            tabMain.TabPages.Clear();

            tabMain.TabPages.Add(tabInformation);

            if (visible)
                tabMain.TabPages.Add(tabLists);

            tabMain.TabPages.Add(tabRemotingSetup);

            if (visible)
                tabMain.TabPages.Add(tabSettings);

            tabMain.SelectedTab = selectedTab;

            tabMain.Visible = true;
        }

        private void ReloadServiceListSettings()
        {
            if (IsServiceAvailable)
            {
                using (new Resources.Impersonator(RemotingUsername, RemotingDomain, RemotingPassword))
                {
                    filterService.Refresh();

                    if (filterService.Status == System.ServiceProcess.ServiceControllerStatus.StartPending)
                    {
                        filterService.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Running, new TimeSpan(0,0,10));
                    }

                    if (filterService.Status == System.ServiceProcess.ServiceControllerStatus.Running)
                    {
                        filterService.ExecuteCommand(Resources.Constants.CustomCommandLoadLists);
                    }
                }
            }
        }                           

        private void ApplyChanges()
        {
            //bool hasConfigurationFileChanged = false;
            bool reloadTreeViewLists = false;

            #region Adapter and List Settings Saving
            /// The adapter and list settings are saved to the same configuration file
            /// In order to prevent file changed exceptions, we must refresh the list/adapter
            /// if the adapter/list is changed.

            if (Program.HasAdapterSettingsChanged)
            //{
                SaveAdapterSettings();
                //hasConfigurationFileChanged = true;
            //}

            //if (hasConfigurationFileChanged)
            //{
            //    Resources.Settings.ListSettings.Refresh();
            //    reloadTreeViewLists = true;
            //}

            if (Program.HasListSettingsChanged)
            {
                SaveListSettings();
                reloadTreeViewLists = true;
                //hasConfigurationFileChanged = true;
            }

            //if (hasConfigurationFileChanged)
            //{
            //    Resources.Settings.AdapterSettings.Refresh();
            //}

            #endregion

            if (Program.HasListUpdateSettingsChanged)
                SaveListUpdateSettings();

            if (Program.HasRemotingSettingsChanged)            
                SaveRemotingSettings();                                            

            if (reloadTreeViewLists)
                LoadLists();

            Program.HasListSettingsChanged = false;
            Program.HasListUpdateSettingsChanged = false;
            Program.HasRemotingSettingsChanged = false;
            Program.HasAdapterSettingsChanged = false;

            btnApply.Enabled = false;
        }

        private void CancelChanges()
        {
            if (Program.HasRemotingSettingsChanged)
                LoadRemotingSettings();

            if (Program.HasListSettingsChanged)
            {
                Resources.Settings.ListSettings.RejectChanges();                               
                LoadLists();
            }

            if (Program.HasListUpdateSettingsChanged)
            {
                Resources.Settings.ListUpdateSettings.RejectChanges();
                ReloadListUpdateSettings();
            }

            if (Program.HasAdapterSettingsChanged)
            {                
                Resources.Settings.AdapterSettings.RejectChanges();
                LoadAdapterSettings();
            }

            Program.HasListSettingsChanged = false;
            Program.HasListUpdateSettingsChanged = false;
            Program.HasRemotingSettingsChanged = false;
            Program.HasAdapterSettingsChanged = false;

            btnApply.Enabled = false;
        }

        private void Restore()
        {
            Show();
            WindowState = FormWindowState.Normal;            
            threadServerStatisticsUpdater.Resume();
        }

        private void Minimize()
        {
            threadServerStatisticsUpdater.Suspend();
            Hide();
            WindowState = FormWindowState.Normal;
        }

        #endregion

        #region CONTROL EVENTS

        private void btnStartStop_Click(object sender, EventArgs e)
        {
            // run the start and stop operation in the background
            // since it can take the filter service a long length
            // of time to fully start
            btnEnableDisable.Enabled = false;
            backgroundWorkerServiceControl.RunWorkerAsync();
        }   

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnEnableDisable_Click(object sender, EventArgs e)
        {
            if (IsFilterEnabled)
            {
                // disable the service
                filterService.ExecuteCommand(Resources.Constants.CustomCommandDisable);
                btnEnableDisable.Text = Sift.Strings.MainForm.ServiceEnableAction;
                toolStripMenuItemEnableDisable.Text = Sift.Strings.MainForm.ServiceEnableAction;
            }
            else
            {
                // reload the adapter settings, thus enabling
                // the service
                filterService.ExecuteCommand(Resources.Constants.CustomCommandEnable);
                btnEnableDisable.Text = Sift.Strings.MainForm.ServiceDisableAction;
                toolStripMenuItemEnableDisable.Text = Sift.Strings.MainForm.ServiceDisableAction;
            }
        }

        private void backgroundWorkerServiceControl_DoWork(object sender, DoWorkEventArgs e)
        {
            backgroundWorkerServiceControl.ReportProgress(0);

            using (new Resources.Impersonator(RemotingUsername, RemotingDomain, RemotingPassword))
            {
                filterService.Refresh();
                switch (filterService.Status)
                {
                    case System.ServiceProcess.ServiceControllerStatus.StartPending:
                        filterService.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Running);
                        filterService.Stop();
                        filterService.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Stopped, new TimeSpan(0, 0, 10));
                        break;
                    case System.ServiceProcess.ServiceControllerStatus.Running:
                        filterService.Stop();
                        filterService.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Stopped, new TimeSpan(0, 0, 10));
                        break;
                    case System.ServiceProcess.ServiceControllerStatus.Stopped:
                        filterService.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Stopped);
                        filterService.Start();
                        filterService.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Running, new TimeSpan(0, 0, 10));
                        break;
                    case System.ServiceProcess.ServiceControllerStatus.StopPending:
                        filterService.Start();
                        filterService.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Running, new TimeSpan(0, 0, 10));
                        break;
                }
            }
        }

        private void backgroundWorkerServiceControl_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            switch (e.ProgressPercentage)
            {
                case 0:
                    using (new Resources.Impersonator(RemotingUsername, RemotingDomain, RemotingPassword))
                    {
                        filterService.Refresh();
                        switch (filterService.Status)
                        {
                            case System.ServiceProcess.ServiceControllerStatus.StartPending:
                                UpdateServiceFormInformation(Sift.Strings.MainForm.ServiceStartingDescription, Sift.Strings.MainForm.ServiceStartAction, false, false);
                                break;
                            case System.ServiceProcess.ServiceControllerStatus.Running:
                                UpdateServiceFormInformation(Sift.Strings.MainForm.ServiceStoppingDescription, Sift.Strings.MainForm.ServiceStopAction, false, false);
                                break;
                            case System.ServiceProcess.ServiceControllerStatus.Stopped:
                                UpdateServiceFormInformation(Sift.Strings.MainForm.ServiceStartingDescription, Sift.Strings.MainForm.ServiceStartAction, false, false);
                                break;
                            case System.ServiceProcess.ServiceControllerStatus.StopPending:
                                UpdateServiceFormInformation(Sift.Strings.MainForm.ServiceStoppingDescription, Sift.Strings.MainForm.ServiceStopAction, false, false);
                                break;
                        }
                    }
                    break;
            }
        }

        private void backgroundWorkerServiceControl_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            using (new Resources.Impersonator(RemotingUsername, RemotingDomain, RemotingPassword))
            {
                filterService.Refresh();
                switch (filterService.Status)
                {
                    case System.ServiceProcess.ServiceControllerStatus.StartPending:
                        filterService.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Running);
                        break;
                    case System.ServiceProcess.ServiceControllerStatus.StopPending:
                        filterService.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Stopped);
                        break;
                }

                /// The configuration file could have potentially changed from the service starting, 
                /// to prevent errors, we need to refresh the configuration
                if (filterService.Status == System.ServiceProcess.ServiceControllerStatus.Running)
                {
                    Resources.Settings.ListSettings.Refresh();
                    Resources.Settings.AdapterSettings.Refresh();
                }
            }

            UpdateServiceFormElements();

            btnStartStop.Enabled = true;
            toolStripMenuItemEnableDisable.Enabled = true;
            UpdateServiceStatisticsFormElements(); // remoting and statistic information could have changed, poll for the changes
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == WindowState)
            {
                Hide();
            }
        }

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void toolStripMenuItemExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void toolStripMenuItemEnableDisable_Click(object sender, EventArgs e)
        {
            btnEnableDisable_Click(sender, e);
        }

        private void toolStripMenuItemAbout_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.Show();
        }

        private void toolStripMenuItemOpen_Click(object sender, EventArgs e)
        {
            Restore();
        }

        private void chkOverrideDefault_CheckedChanged(object sender, EventArgs e)
        {
            Resources.Settings.AdapterSetting selectedAdapter = (Resources.Settings.AdapterSetting)ddlAdapters.Items[ddlAdapters.SelectedIndex];
            selectedAdapter.UseDefaults = !chkOverrideDefault.Checked;           

            if (chkOverrideDefault.Checked)
            {
                chkOverrideFilterHttp.Enabled = true;
                chkOverrideFilterTcp.Enabled = true;
                chkOverrideFilterAll.Enabled = true;
                chkOverrideFilterUdp.Enabled = true;
            }
            else
            {
                chkOverrideFilterHttp.Enabled = false;
                chkOverrideFilterTcp.Enabled = false;
                chkOverrideFilterAll.Enabled = false;
                chkOverrideFilterUdp.Enabled = false;
            }

            Program.HasAdapterSettingsChanged = true;
            btnApply.Enabled = true;
        }

        private void chkDefaultFilterHttp_CheckedChanged(object sender, EventArgs e)
        {
            Resources.Settings.AdapterSetting defaultAdapter = Resources.Settings.AdapterSettings.Settings.Adapters.GetByAdapterId(Resources.Constants.AdapterDefaultSettingId);
            defaultAdapter.FilterHTTP = chkDefaultFilterHttp.Checked;

            Program.HasAdapterSettingsChanged = true;
            btnApply.Enabled = true;
        }

        private void chkDefaultFilterTcp_CheckedChanged(object sender, EventArgs e)
        {
            Resources.Settings.AdapterSetting defaultAdapter = Resources.Settings.AdapterSettings.Settings.Adapters.GetByAdapterId(Resources.Constants.AdapterDefaultSettingId);
            defaultAdapter.FilterTCP = chkDefaultFilterTcp.Checked;

            Program.HasAdapterSettingsChanged = true;
            btnApply.Enabled = true;
        }

        private void chkDefaultFilterUdp_CheckedChanged(object sender, EventArgs e)
        {
            Resources.Settings.AdapterSetting defaultAdapter = Resources.Settings.AdapterSettings.Settings.Adapters.GetByAdapterId(Resources.Constants.AdapterDefaultSettingId);
            defaultAdapter.FilterUDP = chkDefaultFilterUdp.Checked;

            Program.HasAdapterSettingsChanged = true;
            btnApply.Enabled = true;
        }

        private void chkDefaultFilterAll_CheckedChanged(object sender, EventArgs e)
        {
            Resources.Settings.AdapterSetting defaultAdapter = Resources.Settings.AdapterSettings.Settings.Adapters.GetByAdapterId(Resources.Constants.AdapterDefaultSettingId);
            defaultAdapter.FilterAll =chkDefaultFilterAll.Checked;

            Program.HasAdapterSettingsChanged = true;
            btnApply.Enabled = true;
        }

        private void chkOverrideFilterHttp_CheckedChanged(object sender, EventArgs e)
        {
            Resources.Settings.AdapterSetting selectedAdapter = (Resources.Settings.AdapterSetting)ddlAdapters.Items[ddlAdapters.SelectedIndex];
            selectedAdapter.FilterHTTP = chkOverrideFilterHttp.Checked;            

            Program.HasAdapterSettingsChanged = true;
            btnApply.Enabled = true;
        }

        private void chkOverrideFilterTcp_CheckedChanged(object sender, EventArgs e)
        {
            Resources.Settings.AdapterSetting selectedAdapter = (Resources.Settings.AdapterSetting)ddlAdapters.Items[ddlAdapters.SelectedIndex];
            selectedAdapter.FilterTCP = chkOverrideFilterTcp.Checked;

            Program.HasAdapterSettingsChanged = true;
            btnApply.Enabled = true;
        }

        private void chkOverrideFilterUdp_CheckedChanged(object sender, EventArgs e)
        {
            Resources.Settings.AdapterSetting selectedAdapter = (Resources.Settings.AdapterSetting)ddlAdapters.Items[ddlAdapters.SelectedIndex];
            selectedAdapter.FilterUDP = chkOverrideFilterUdp.Checked;

            Program.HasAdapterSettingsChanged = true;
            btnApply.Enabled = true;
        }

        private void chkOverrideFilterAll_CheckedChanged(object sender, EventArgs e)
        {
            Resources.Settings.AdapterSetting selectedAdapter = (Resources.Settings.AdapterSetting)ddlAdapters.Items[ddlAdapters.SelectedIndex];
            selectedAdapter.FilterAll = chkOverrideFilterAll.Checked;

            Program.HasAdapterSettingsChanged = true;
            btnApply.Enabled = true;
        }

        private void btnQuickAdd_Click(object sender, EventArgs e)
        {
            Match result;

            // try to match a url first
            result = Regex.Match(txtQuickAdd.Text, Sift.Program.regexURL);
            if (result.Groups[4].Length > 0)
            {
                QuickAddURL(result.Groups[4].Value);

                Program.HasListSettingsChanged = true;
                btnApply.Enabled = true;

                txtQuickAdd.Text = string.Empty;
            }
            else
            {
                // then match a domain
                result = Regex.Match(txtQuickAdd.Text, Sift.Program.regexURL);
                if (result.Success)
                {
                    QuickAddDomain(result.Groups[1].Value);

                    Program.HasListSettingsChanged = true;
                    btnApply.Enabled = true;

                    txtQuickAdd.Text = string.Empty;
                }
                else
                {
                    // finally match an ip
                    result = Regex.Match(txtQuickAdd.Text, Sift.Program.regexIP);
                    if (result.Success)
                    {
                        QuickAddIP(result.Groups[0].Value);

                        Program.HasListSettingsChanged = true;
                        btnApply.Enabled = true;

                        txtQuickAdd.Text = string.Empty;
                    }
                    else
                        MessageBox.Show(Sift.Strings.MainForm.QuickAddInvalid);
                }
            }                        
        }

        private void ddlAdapters_SelectedIndexChanged(object sender, EventArgs e)
        {
            Resources.Settings.AdapterSetting selectedAdapter = (Resources.Settings.AdapterSetting)ddlAdapters.Items[ddlAdapters.SelectedIndex];

            if (selectedAdapter != null)
            {
                // the changing of an adapter should not be seen as a settings change, so store away the values to restore later
                bool previousApplyEnabled = btnApply.Enabled;
                bool previousHasAdapterSettingsChanged = Program.HasAdapterSettingsChanged;

                chkOverrideDefault.Checked = !selectedAdapter.UseDefaults;
                chkOverrideFilterHttp.Checked = selectedAdapter.FilterHTTP;
                chkOverrideFilterTcp.Checked = selectedAdapter.FilterTCP;
                chkOverrideFilterAll.Checked = selectedAdapter.FilterAll;
                chkOverrideFilterUdp.Checked = selectedAdapter.FilterUDP;                

                chkOverrideFilterHttp.Enabled = !selectedAdapter.UseDefaults;
                chkOverrideFilterTcp.Enabled = !selectedAdapter.UseDefaults;
                chkOverrideFilterAll.Enabled = !selectedAdapter.UseDefaults;
                chkOverrideFilterUdp.Enabled = !selectedAdapter.UseDefaults;                

                // restore the previous values
                btnApply.Enabled = previousApplyEnabled;
                Program.HasAdapterSettingsChanged = previousHasAdapterSettingsChanged;
            }
        }

        private void treeViewLists_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag.GetType() == typeof(Resources.Settings.ListGroup))
            {
                Resources.Settings.ListGroup listGroup = (Resources.Settings.ListGroup)e.Node.Tag;
                listGroup.Enabled = e.Node.Checked;
            }
            else if (e.Node.Tag.GetType() == typeof(Resources.Settings.List))
            {
                Resources.Settings.List list = (Resources.Settings.List)e.Node.Tag;
                list.Enabled = e.Node.Checked;
            }

            Program.HasListSettingsChanged = true;
            btnApply.Enabled = true;
        }

        private void treeViewLists_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Node.Tag.GetType() == typeof(Resources.Settings.ListGroup))
            {
                Resources.Settings.ListGroup listGroup = (Resources.Settings.ListGroup)e.Node.Tag;
                listGroup.Description = e.Label;
            }
            else if (e.Node.Tag.GetType() == typeof(Resources.Settings.List))
            {
                Resources.Settings.List list = (Resources.Settings.List)e.Node.Tag;
                list.Description = e.Label;
            }

            Program.HasListSettingsChanged = true;
            btnApply.Enabled = true;
        }

        private void toolStripListGroupEnabled_Click(object sender, EventArgs e)
        {
            treeViewLists.SelectedNode.Checked = !treeViewLists.SelectedNode.Checked;
        }

        private void toolStripListEnabled_Click(object sender, EventArgs e)
        {
            treeViewLists.SelectedNode.Checked = !treeViewLists.SelectedNode.Checked;
        }

        private void toolStripListGroupRename_Click(object sender, EventArgs e)
        {
            treeViewLists.LabelEdit = true;
            if (!treeViewLists.SelectedNode.IsEditing)
                treeViewLists.SelectedNode.BeginEdit();
        }

        private void toolStripListRename_Click(object sender, EventArgs e)
        {
            treeViewLists.LabelEdit = true;
            if (!treeViewLists.SelectedNode.IsEditing)
                treeViewLists.SelectedNode.BeginEdit();
        }

        private void btnCancelListSettings_Click(object sender, EventArgs e)
        {

        }

        private void txtQuickAdd_TextChanged(object sender, EventArgs e)
        {
            if (txtQuickAdd.Text != string.Empty)
                btnQuickAdd.Enabled = true;
            else
                btnQuickAdd.Enabled = false;
        }

        private void treeViewLists_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            treeViewLists.SelectedNode = e.Node;
            UpdateSelectedNode();
        }

        private void rbListBlock_CheckedChanged(object sender, EventArgs e)
        {
            if (rbListBlock.Checked)
                if (treeViewLists.SelectedNode.Tag.GetType() == typeof(Resources.Settings.List))
                {
                    Resources.Settings.List list = (Resources.Settings.List)treeViewLists.SelectedNode.Tag;

                    list.MatchAction = Sift.Resources.Types.MatchActionType.Block;

                    Program.HasListSettingsChanged = true;
                    btnApply.Enabled = true;
                }
        }

        private void rbListAllow_CheckedChanged(object sender, EventArgs e)
        {
            if (rbListAllow.Checked)
                if (treeViewLists.SelectedNode.Tag.GetType() == typeof(Resources.Settings.List))
                {
                    Resources.Settings.List list = (Resources.Settings.List)treeViewLists.SelectedNode.Tag;

                    list.MatchAction = Sift.Resources.Types.MatchActionType.Allow;

                    Program.HasListSettingsChanged = true;
                    btnApply.Enabled = true;
                }
        }

        private void rbListLog_CheckedChanged(object sender, EventArgs e)
        {
            if (rbListLog.Checked)
                if (treeViewLists.SelectedNode.Tag.GetType() == typeof(Resources.Settings.List))
                {
                    Resources.Settings.List list = (Resources.Settings.List)treeViewLists.SelectedNode.Tag;

                    list.MatchAction = Sift.Resources.Types.MatchActionType.Log;

                    Program.HasListSettingsChanged = true;
                    btnApply.Enabled = true;
                }
        }

        private void ddlContentType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (treeViewLists.SelectedNode.Tag.GetType() == typeof(Resources.Settings.List))
            {
                Resources.Settings.List list = (Resources.Settings.List)treeViewLists.SelectedNode.Tag;

                list.Content = (Sift.Resources.Types.ContentType)Enum.Parse(typeof(Sift.Resources.Types.ContentType), ddlContentType.SelectedItem.ToString());

                Program.HasListSettingsChanged = true;
                btnApply.Enabled = true;
            }
        }

        private void btnEditListFile_Click(object sender, EventArgs e)
        {            
            // determine the selected list type
            EditList.ListType listType = (EditList.ListType)Enum.Parse(typeof(EditList.ListType), ddlContentType.Items[ddlContentType.SelectedIndex].ToString());

            Resources.Settings.List list = (Resources.Settings.List)treeViewLists.SelectedNode.Tag;
            EditList editList = new EditList(list.Id, listType);
            editList.ShowDialog();
        }

        private void toolStripListGroupNewSubgroup_Click(object sender, EventArgs e)
        {
            // add a new subgroup
            Resources.Settings.ListGroup listGroup = (Resources.Settings.ListGroup)treeViewLists.SelectedNode.Tag;

            Resources.Settings.ListGroup newListGroup = new Sift.Resources.Settings.ListGroup();
            newListGroup.Id = Guid.NewGuid();
            newListGroup.Description = Sift.Strings.MainForm.ListGroupNew;
            newListGroup.Enabled = true;

            listGroup.ListGroups.Add(newListGroup);

            LoadLists();
            SelectListNode(newListGroup);

            Program.HasListSettingsChanged = true;
            btnApply.Enabled = true;
        }

        private void toolStripListGroupNewList_Click(object sender, EventArgs e)
        {
            // add a new list
            Resources.Settings.ListGroup listGroup = (Resources.Settings.ListGroup)treeViewLists.SelectedNode.Tag;

            Sift.Resources.Types.MatchActionType matchActionType = Sift.Resources.Types.MatchActionType.Log;

            // default the match action to the first entry, as list group are likely to all be the same type
            if (listGroup.Lists.Count > 0)
                matchActionType = listGroup.Lists[0].MatchAction;

            Resources.Settings.List newList = new Sift.Resources.Settings.List();
            newList.Id = Guid.NewGuid();
            newList.MatchAction = matchActionType;
            newList.Enabled = true;
            newList.Description = Sift.Strings.MainForm.ListNew;
            newList.Content = Sift.Resources.Types.ContentType.URL;
            
            listGroup.Lists.Add(newList);
            UpdateList(newList, Sift.Resources.Types.ListUpdateType.Add);
            LoadLists();
            SelectListNode(newList);

            Program.HasListSettingsChanged = true;
            btnApply.Enabled = true;
        }

        private void toolStripListDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(Sift.Strings.MainForm.ListGroupDeleteWarning, Sift.Strings.MainForm.ListGroupDeleteConfirmation, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                // delete the list
                Resources.Settings.List list = (Resources.Settings.List)treeViewLists.SelectedNode.Tag;
                Resources.Settings.ListGroup listGroup = (Resources.Settings.ListGroup)((TreeNode)treeViewLists.SelectedNode.Parent).Tag;

                UpdateList(list, Sift.Resources.Types.ListUpdateType.Remove);
                listGroup.Lists.Remove(list);
                             
                LoadLists();

                Program.HasListSettingsChanged = true;
                btnApply.Enabled = true;
            }
        }

        private void toolStripListGroupDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(Sift.Strings.MainForm.ListDeleteWarning, Sift.Strings.MainForm.ListDeleteConfirmation, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                // delete the list group and all it's subgroups and sublists
                Resources.Settings.ListGroup listGroup = (Resources.Settings.ListGroup)treeViewLists.SelectedNode.Tag;

                Resources.Settings.ListSettings.Settings.ListGroups.DeepRemove(listGroup);
                RemoveListGroup(listGroup);

                LoadLists();

                Program.HasListSettingsChanged = true;
                btnApply.Enabled = true;
            }
        }

        private void btnRemotingTest_Click(object sender, EventArgs e)
        {
            if (txtRemotingPassword.Text != txtRemotingConfirmPassword.Text)
                MessageBox.Show(Sift.Strings.MainForm.PasswordMismatch);
            else
            {
                try
                {
                    if (IsRemotingAvailable)
                        MessageBox.Show(Sift.Strings.MainForm.RemotingConnectionSuccess);
                    else
                        MessageBox.Show(Sift.Strings.MainForm.RemotingConnectionFailure);
                }

                catch (System.Net.WebException ex)
                {
                    MessageBox.Show(ex.Message);                    
                }
            }
        }

        private void txtServerName_TextChanged(object sender, EventArgs e)
        {
            Program.HasRemotingSettingsChanged = true;
            btnApply.Enabled = true;
        }

        private void txtRemotingPort_TextChanged(object sender, EventArgs e)
        {
            Program.HasRemotingSettingsChanged = true;
            btnApply.Enabled = true;
        }

        private void btnServiceStatisticsRefresh_Click(object sender, EventArgs e)
        {
            UpdateServiceStatisticsFormElements();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (txtRemotingPassword.Text != txtRemotingConfirmPassword.Text)
            {
                tabRemotingSetup.Select();
                MessageBox.Show(Sift.Strings.MainForm.PasswordMismatch);
            }
            else
            {
                try
                {
                    ApplyChanges();
                }
                catch (Win32Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
                Minimize();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            CancelChanges();
            Minimize();
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            if (txtRemotingPassword.Text != txtRemotingConfirmPassword.Text)
            {
                tabRemotingSetup.Select();
                MessageBox.Show(Sift.Strings.MainForm.PasswordMismatch);
            }
            else
            {
                try
                {
                    ApplyChanges();
                }
                catch (Win32Exception ex)
                {
                    MessageBox.Show(ex.Message);                    
                }
            }
        }

        private void rbRemotingLocalMachine_CheckedChanged(object sender, EventArgs e)
        {
            if (rbRemotingLocalMachine.Checked)
            {
                rbRemotingRemoteMachine.Checked = false;
                UpdateRemotingFormFields();
                ToggleSettingTabs(false);
            }

            Program.HasRemotingSettingsChanged = true;
            btnApply.Enabled = true;
        }

        private void rbRemotingRemoteMachine_CheckedChanged(object sender, EventArgs e)
        {
            if (rbRemotingRemoteMachine.Checked)
            {
                rbRemotingLocalMachine.Checked = false;
                UpdateRemotingFormFields();
                ToggleSettingTabs(false);
            }

            Program.HasRemotingSettingsChanged = true;
            btnApply.Enabled = true;
        }

        private void rbRemotingRemoteUser_CheckedChanged(object sender, EventArgs e)
        {
            if (rbRemotingRemoteUser.Checked)
            {
                rbRemotingCurrentUser.Checked = false;
                UpdateRemotingFormFields();
            }

            Program.HasRemotingSettingsChanged = true;
            btnApply.Enabled = true;
        }

        private void rbRemotingCurrentUser_CheckedChanged(object sender, EventArgs e)
        {
            if (rbRemotingCurrentUser.Checked)
            {
                rbRemotingRemoteUser.Checked = false;
                UpdateRemotingFormFields();
            }

            Program.HasRemotingSettingsChanged = true;
            btnApply.Enabled = true;
        }

        private void txtRemotingUsername_TextChanged(object sender, EventArgs e)
        {
            Program.HasRemotingSettingsChanged = true;
            btnApply.Enabled = true;
        }

        private void txtRemotingPassword_TextChanged(object sender, EventArgs e)
        {
            Program.HasRemotingSettingsChanged = true;
            btnApply.Enabled = true;
        }

        private void txtRemotingConfirmPassword_TextChanged(object sender, EventArgs e)
        {
            Program.HasRemotingSettingsChanged = true;
            btnApply.Enabled = true;
        }

        private void systemTrayIcon_DoubleClick(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void treeViewLists_ItemDrag(object sender, ItemDragEventArgs e)
        {
            treeViewLists.AllowDrop = true;
            treeViewLists.DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void treeViewLists_DragDrop(object sender, DragEventArgs e)
        {
            Point point = treeViewLists.PointToClient(new Point(e.X, e.Y));
            TreeNode dropNode = treeViewLists.GetNodeAt(point);
            TreeNode selectedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));


            if (dropNode != null)
            {
                if (selectedNode.Tag.GetType() == typeof(Resources.Settings.List) && dropNode.Tag.GetType() == typeof(Resources.Settings.ListGroup))
                {
                    // dragging a list into another list group
                    Resources.Settings.List list = (Resources.Settings.List)selectedNode.Tag;

                    // add the list to the drop point                    
                    Resources.Settings.ListGroup newListGroup = (Resources.Settings.ListGroup)dropNode.Tag;
                    newListGroup.Lists.Add(list);

                    // remove the list from it's parent                    
                    Resources.Settings.ListGroup oldListGroup = (Resources.Settings.ListGroup)selectedNode.Parent.Tag;
                    oldListGroup.Lists.Remove(list.Id);

                    LoadLists();
                }
                else if (selectedNode.Tag.GetType() == typeof(Resources.Settings.ListGroup) && dropNode.Tag.GetType() == typeof(Resources.Settings.ListGroup))
                {
                    // dragging a list group into another list group
                    Resources.Settings.ListGroup listGroup = (Resources.Settings.ListGroup)selectedNode.Tag;
                    Resources.Settings.ListGroup oldListGroup = null;

                    if (selectedNode.Parent != null)
                        oldListGroup = (Resources.Settings.ListGroup)selectedNode.Parent.Tag;

                    // add the list group to the drop point                    
                    Resources.Settings.ListGroup newListGroup = (Resources.Settings.ListGroup)dropNode.Tag;
                    newListGroup.ListGroups.Add(listGroup);

                    // remove the list group from it's parent, if it has one                    
                    if (oldListGroup != null)
                        oldListGroup.ListGroups.Remove(listGroup.Id);
                    else
                        Resources.Settings.ListSettings.Settings.ListGroups.Remove(listGroup.Id);

                    LoadLists();
                }

                Program.HasListSettingsChanged = true;
                btnApply.Enabled = true;
            }
            else if (selectedNode.Tag.GetType() == typeof(Resources.Settings.ListGroup))
            {
                // we're dropping a list group from another list group to the main group
                Resources.Settings.ListGroup listGroup = (Resources.Settings.ListGroup)selectedNode.Tag;
                Resources.Settings.ListGroup oldListGroup = null;

                if (selectedNode.Parent != null)
                    oldListGroup = (Resources.Settings.ListGroup)selectedNode.Parent.Tag;

                // remove the list group from it's parent, if it has one                    
                if (oldListGroup != null)
                {
                    Resources.Settings.ListSettings.Settings.ListGroups.Add(listGroup);
                    oldListGroup.ListGroups.Remove(listGroup.Id);

                    LoadLists();
                }

                Program.HasListSettingsChanged = true;
                btnApply.Enabled = true;
            }
        }

        private void treeViewLists_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.Show();
        }

        private void btnAddNewLists_Click(object sender, EventArgs e)
        {
            WizardSheet wizard = new WizardSheet("Add New Lists");
            wizard.ShowHelpButton = false;

            WizardCategorySelection wizardCategorySelection = null;

            try
            {
                wizardCategorySelection = new WizardCategorySelection();
            }
            catch (System.Net.WebException ex)
            {
                MessageBox.Show(ex.Message + System.Environment.NewLine + System.Environment.NewLine + "Check your internet connection and try again.");
            }

            if (wizardCategorySelection != null)
            {
                wizard.AddPage(wizardCategorySelection);

                if (wizard.ShowDialog(this) == DialogResult.OK)
                {
                    // process wizard results if finish if clicked
                    wizardCategorySelection.ApplyChanges();
                    LoadLists();
                    btnApply.Enabled = true;
                }
            }
        }
        #endregion                                                                       
    }
}