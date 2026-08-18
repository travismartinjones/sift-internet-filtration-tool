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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Sift
{
    public partial class EditList : SiftForm
    {
        public enum ListType { IP, Domain, URL };
               
        ListType _listType;
        Guid _listId;

        #region CONTRUCTOR_DESTRUCTOR

        public EditList()
        {
            InitializeComponent();
        }

        public EditList(Guid listId, ListType type)
        {
            InitializeComponent();
            switch (type)
            {
                case ListType.IP:
                    this.Text = Sift.Strings.EditList.ListEditIPTitle;
                    break;
                case ListType.Domain:
                    this.Text = Sift.Strings.EditList.ListEditDomainTitle;
                    break;
                case ListType.URL:
                    this.Text = Sift.Strings.EditList.ListEditURLTitle;
                    break;
            }

            _listType = type;
            _listId = listId;

            LoadList();
        }

        #endregion CONTRUCTOR_DESTRUCTOR

        #region PROPERTIES

        Resources.Settings.ListUpdate _listUpdate = null;
        private Resources.Settings.ListUpdate listUpdate
        {
            get
            {
                if(_listUpdate == null)
                    _listUpdate = Resources.Settings.ListUpdateSettings.Settings.ListsUpdates.GetByListId(_listId);

                return _listUpdate;
            }
        }

        #endregion

        private void LoadList()
        {
            System.IO.StreamReader fileReader = null;
            Resources.Impersonator impersonation = null;

            if (Program.mainForm.IsConfigurationFileLocal)
            {
                Resources.Settings.List list = Resources.Settings.ListSettings.Settings.GetByListId(_listId);
                if (list != null && System.IO.File.Exists(list.Path))
                    fileReader = new System.IO.StreamReader(list.Path);
            }
            else if (Program.mainForm.IsRemotingAvailable)
            {
                // start impersonating
                impersonation = new Sift.Resources.Impersonator(Program.mainForm.RemotingUsername, Program.mainForm.RemotingDomain, Program.mainForm.RemotingPassword);
                fileReader = RemotingClient.GetListStream(_listId, Program.mainForm.HostName, Program.mainForm.HostPort, Program.mainForm.RemotingDomain, Program.mainForm.RemotingUsername, Program.mainForm.RemotingPassword);
            }
            else // we should never get here, gracefully do nothing
                return;

            if (fileReader == null)
            {
                // read off an empty file if no list file exists
                string tempFile = System.IO.Path.GetTempFileName();
                System.IO.File.CreateText(tempFile).Close();
                fileReader = new System.IO.StreamReader(tempFile);                
            }

            bool tempSorted;

            string line = fileReader.ReadLine();

            // store the original sorted value
            // and make sure that the list is 
            // not sorted while we add the listBox items
            tempSorted = listBox1.Sorted;
            listBox1.Sorted = false;

            while (line != null)
            {
                Resources.Settings.ListEntryUpdate mostRecentUpdate = null;

                if (listUpdate != null)
                    mostRecentUpdate = listUpdate.Updates.GetMostRecentByValue(line);

                if (mostRecentUpdate == null || mostRecentUpdate.Action != Sift.Resources.Types.ListUpdateType.Remove)
                {
                    // the line has not been marked for removal, so show it
                    listBox1.Items.Add(line);
                }

                line = fileReader.ReadLine();
            }

            // add any lines that have been added, but not committed to file
            if (listUpdate != null)
            {
                foreach (string value in listUpdate.Updates.Values)
                {
                    Resources.Settings.ListEntryUpdate mostRecentUpdate = listUpdate.Updates.GetMostRecentByValue(value);
                    if (mostRecentUpdate.Action == Sift.Resources.Types.ListUpdateType.Add)
                        listBox1.Items.Add(value);
                }
            }

            // restore the original sorted value
            listBox1.Sorted = tempSorted;
            fileReader.Close();
            
            if(impersonation != null) // stop impersonating
                impersonation.Dispose();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            for (int i = 0; i < listBox1.Items.Count; i++)
                if (listBox1.GetSelected(i))
                {
                    Sift.Program.mainForm.UpdateListValue(_listId, listBox1.Items[i].ToString(), Sift.Resources.Types.ListUpdateType.Remove);
                    listBox1.Items.RemoveAt(i--);
                }
        }

        private void button2_Click(object sender, EventArgs e)
        {                                   
            string value = this.textBox1.Text;

            //value = Regex.Replace(value, @"^.*://", "");
            //value = Regex.Replace(value, @"/$", "");
            Match result;

            switch (_listType)
            {
                case ListType.IP:
                    result = Regex.Match(value, Sift.Program.regexIP);
                    if (result.Success)
                    {
                        if (listBox1.Items.Contains(result.Groups[0].Value))
                            MessageBox.Show(Sift.Strings.EditList.AlreadyAddedIP.Replace(Sift.Strings.EditList.IPReplaceString,result.Groups[0].Value));
                        else
                        {
                            Sift.Program.mainForm.UpdateListValue(_listId, result.Groups[0].Value, Sift.Resources.Types.ListUpdateType.Add);
                            listBox1.Items.Add(result.Groups[0].Value);
                            textBox1.Text = string.Empty;
                        }
                    }
                    else
                        MessageBox.Show(Sift.Strings.EditList.InvalidIP.Replace(Sift.Strings.EditList.IPReplaceString,value));
                    break;
                case ListType.Domain:
                    result = Regex.Match(value, Sift.Program.regexURL);
                    if (result.Success)
                    {
                        if (listBox1.Items.Contains(result.Groups[2].Value))
                            MessageBox.Show(Sift.Strings.EditList.AlreadyAddedDomain.Replace(Sift.Strings.EditList.DomainReplaceString,result.Groups[2].Value));
                        else
                        {
                            Sift.Program.mainForm.UpdateListValue(_listId, result.Groups[2].Value, Sift.Resources.Types.ListUpdateType.Add);
                            listBox1.Items.Add(result.Groups[2].Value);
                            textBox1.Text = string.Empty;
                        }
                    }
                    else
                        MessageBox.Show(Sift.Strings.EditList.InvalidDomain.Replace(Sift.Strings.EditList.DomainReplaceString,value));
                    break;
                case ListType.URL:
                    result = Regex.Match(value, Sift.Program.regexURL);
                    if (result.Groups[4].Length > 0)
                    {
                        if (listBox1.Items.Contains(result.Groups[1].Value))
                            MessageBox.Show(Sift.Strings.EditList.AlreadyAddedURL.Replace(Sift.Strings.EditList.URLReplaceString,result.Groups[1].Value));
                        else
                        {
                            Sift.Program.mainForm.UpdateListValue(_listId, result.Groups[1].Value, Sift.Resources.Types.ListUpdateType.Add);
                            listBox1.Items.Add(result.Groups[1].Value);
                            textBox1.Text = string.Empty;
                        }
                    }
                    else
                        MessageBox.Show(Sift.Strings.EditList.InvalidURL.Replace(Sift.Strings.EditList.URLReplaceString,value));
                    break;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            button2.Enabled = textBox1.Text != "";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //// save the list entries to file
            //System.IO.StreamWriter streamWriter = new System.IO.StreamWriter(_filename, false);

            //foreach (object item in listBox1.Items)
            //    streamWriter.WriteLine((string)item);

            //streamWriter.Close();

            //// done editing, close this window
            //this.Close();
        }
    }
}