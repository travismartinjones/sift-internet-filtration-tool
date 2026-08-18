using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Sift
{
    public partial class ListUpdater : Form
    {        
        public ListUpdater()
        {            
            InitializeComponent();            
        }

        public DialogResult ShowDialog(Sift.Resources.Settings.ListGroupCollection requests)
        {
            base.Show();
            ProcessRequests(Sift.Resources.Settings.ListSettings.Settings.FlattenListGroups(requests));
            this.Close();
            return DialogResult.OK;
        }

        private void ProcessListNewDownload(Sift.Resources.Settings.List list)
        {
            lblListName.Text = list.Description;

            downloadStatusList.Value = 0;

            long contentLength = 0;

            System.IO.StreamReader reader = new System.IO.StreamReader(Sift.Resources.WebServiceClient.GetListUpdateFileByList(list, out contentLength));

            System.Text.StringBuilder stringBuilder = null;

            if (contentLength > 0)
                stringBuilder = new System.Text.StringBuilder(Convert.ToInt32(contentLength));
            else
                stringBuilder = new System.Text.StringBuilder();

            #region Update the progress

            int bufferSize = 2048;
            char[] buffer = new char[bufferSize];

            int totalBytesRead = 0;
            int bytesRead = 1;

            while (bytesRead > 0)
            {
                // read a chunk of data into the buffer from the download stream
                bytesRead = reader.Read(buffer, 0, bufferSize);

                // write the received data to a string
                stringBuilder.Append(buffer, 0, bytesRead);

                totalBytesRead += bytesRead;

                if (contentLength == 0)
                    downloadStatusList.Value = 100;
                else
                    downloadStatusList.Value = Convert.ToInt32(((double)totalBytesRead/contentLength)*100);

                this.Refresh();
            }
            #endregion

            if ((Sift.Program.mainForm != null && Sift.Program.mainForm.IsAdministeringLocal) ||
                Sift.Application.ApplicationSettings.Settings.RemotingHostName == Sift.Strings.MainForm.NetworkLocalhost)
            {
                // if the sift service is local, save to the lists path                
                string listFilename = list.Path;

                if (!System.IO.File.Exists(listFilename))
                {
                    System.IO.StreamWriter file = new System.IO.StreamWriter(listFilename);
                    file.Write(stringBuilder.ToString());                    
                    file.Close();
                }                    
            }
            else
            {                                   
                // save the list to the remote sift instance
                Sift.RemotingClient.SaveListFile(Sift.Program.mainForm.HostName, Sift.Program.mainForm.HostPort, Sift.Program.mainForm.RemotingDomain, Sift.Program.mainForm.RemotingUsername, Sift.Program.mainForm.RemotingPassword, list.Id, stringBuilder.ToString());
            }
            
            reader.Close();
        }

        private void ProcessListUpdate(Sift.Resources.Settings.List list)
        {
            Sift.Resources.Settings.ListCollection lists = new Sift.Resources.Settings.ListCollection();
            lists.Add(list);

            XmlDocument document = Sift.Resources.WebServiceClient.GetListUpdatesXMLByLists(lists);

            lblProcessing.Text = "Processing";
            this.Refresh();

            XmlNodeList listsUpdateNodes = document.DocumentElement.SelectNodes("/configuration/ListUpdateSettings/listUpdates/listUpdate");

            foreach (XmlNode listUpdateNode in listsUpdateNodes)
            {
                Guid listID = new Guid(listUpdateNode.Attributes["ListId"].InnerText);

                Sift.Resources.Settings.ListUpdate listUpdate = Sift.Resources.Settings.ListUpdateSettings.Settings.ListsUpdates.GetByListId(listID);

                #region Add the list update if it doesn't exist already
                if (listUpdate == null)
                {
                    listUpdate = new Sift.Resources.Settings.ListUpdate();
                    listUpdate.Action = Sift.Resources.Types.ListUpdateType.Add;
                    listUpdate.ListId = listID;
                    Sift.Resources.Settings.ListUpdateSettings.Settings.ListsUpdates.Add(listUpdate);
                }
                #endregion

                XmlNodeList listEntryUpdateNodes = listUpdateNode.SelectNodes("./listEntryUpdates/listEntryUpdate");

                foreach (XmlNode listEntryUpdateNode in listEntryUpdateNodes)
                {
                    Sift.Resources.Settings.ListEntryUpdate listEntryUpdate = new Sift.Resources.Settings.ListEntryUpdate();
                    listEntryUpdate.Value = listEntryUpdateNode.Attributes["Value"].InnerText;
                    listEntryUpdate.Action = (Sift.Resources.Types.ListUpdateType)Enum.Parse(typeof(Sift.Resources.Types.ListUpdateType), listEntryUpdateNode.Attributes["Action"].InnerText);
                    listEntryUpdate.DateCreated = Convert.ToDateTime(listEntryUpdateNode.Attributes["DateCreated"].InnerText);
                    listUpdate.Updates.Add(listEntryUpdate);
                }
            }
        }

        /// <summary>
        /// Pulls down the list updates from the update server.         
        /// This method currently will not be reached. Any list that is already
        /// downloaded is not shown again as an option in the WizardCategorySelection.
        /// All list update processing is handled in the sift service.
        /// </summary>        
        /// <param name="request">The list group to process the updates for.</param>
        private void ProcessRequest(Sift.Resources.Settings.ListGroup request, int totalListGroupCount)
        {
            double downloadStatusProgressPercent = downloadStatus.Value;

            // add each new list, or update them if they already exist
            foreach (Sift.Resources.Settings.List list in request.Lists)
            {
                ProcessListNewDownload(list);

                // update the progress   
                downloadStatusProgressPercent += Convert.ToInt32((double)((double)1/totalListGroupCount / request.Lists.Count) * 100);
                downloadStatus.Value = Math.Min(Convert.ToInt32(downloadStatusProgressPercent),100);
                this.Refresh();

                list.LastUpdated = DateTime.Now;
            }

            // add the entries to the list groups in the configuration file if they are new
            if (Sift.Resources.Settings.ListSettings.Settings.ListGroups.GetByListGroupId(request.Id) == null)
            {                
                Sift.Resources.Settings.ListSettings.Settings.ListGroups.Add(request);
                Program.HasListSettingsChanged = true;                
            }
        }

        private void ProcessRequests(Sift.Resources.Settings.ListGroupCollection requests)
        {
            if (requests.Count > 0)
            {                
                lblTotalListCount.Text = requests.Count.ToString();                

                for (int i = 0; i < requests.Count; i++)
                {                    
                    lblCurrentListNumber.Text = ((int)i + 1).ToString();
                    lblCategoryName.Text = requests[i].Description;
                    lblProcessing.Text = "Downloading";
                    this.Refresh();

                    ProcessRequest(requests[i], requests.Count);
                    
                    // update the progress
                    downloadStatus.Value = Convert.ToInt32(((double)(i + 1) / requests.Count) * 100);
                }

                downloadStatus.Value = 100;
            }
        }        
    }
}
