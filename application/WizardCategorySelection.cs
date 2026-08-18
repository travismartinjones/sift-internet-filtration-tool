using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Sift.Resources;

namespace Sift
{
    public partial class WizardCategorySelection : UserControl
    {
        #region PROPERTIES
        TreeNode[] originalCategories;
        TreeNode[] currentCategories;
        #endregion

        public WizardCategorySelection()
        {
            InitializeComponent();
            CustomInitializeComponent();
        }

        #region PRIVATE METHODS
        private void CustomInitializeComponent()
        {
            //treeViewCategories.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawAll;
            //treeViewCategories.DrawNode += new System.Windows.Forms.DrawTreeNodeEventHandler(treeViewCategories_DrawNode);

            LoadCategoryTreeNodes();
        }

        #region Category Tree Nodes
        private void LoadCategoryTreeNodes()
        {
            XmlDocument document = Sift.Resources.WebServiceClient.GetAvailableListGroups();

            if (document.DocumentElement != null)
            {
                originalCategories = GetTreeNodesFromCategoryDocument(document);
                currentCategories = GetTreeNodesFromCategoryDocument(document);

                treeViewCategories.Nodes.AddRange(currentCategories);

                if (treeViewCategories.Nodes.Count > 0)
                {
                    treeViewCategories.SelectedNode = treeViewCategories.Nodes[0];
                    UpdateSelectedNode();
                }
            }
            else
            {
                throw new System.Net.WebException("Unable to connect to the SIFT server.");
            }
        }

        private TreeNode[] GetTreeNodesFromCategoryDocument(XmlDocument document)
        {
            System.Collections.Generic.List<TreeNode> treeNodes = new System.Collections.Generic.List<TreeNode>();

            XmlElement root = document.DocumentElement;
            XmlNodeList categoryNodes = root.SelectNodes("/configuration/ListSettings/listGroups/listGroup");

            foreach (XmlNode categoryNode in categoryNodes)
            {
                TreeNode treeNode = GetTreeNodeFromCategory(categoryNode);

                // only show the category to add if the user has not already added it
                if(Sift.Resources.Settings.ListSettings.Settings.GetByListGroupId(((Sift.Resources.Settings.ListGroup)treeNode.Tag).Id) == null)
                    treeNodes.Add(treeNode);
            }

            return treeNodes.ToArray();
        }

        private Sift.Resources.Settings.ListGroup GetListGroupFromCategory(XmlNode category)
        {
            Sift.Resources.Settings.ListGroup listGroup = new Sift.Resources.Settings.ListGroup();

            listGroup.Id = new Guid(category.Attributes["Id"].InnerText);
            listGroup.Description = category.Attributes["Description"].InnerText;
            listGroup.Details = category.Attributes["Details"].InnerText;
            listGroup.Enabled = Convert.ToBoolean(category.Attributes["Enabled"].InnerText);
            listGroup.Log = Convert.ToBoolean(category.Attributes["Log"].InnerText);
            
            // add the lists tied to this list group
            XmlNodeList listNodes = category.SelectNodes("./lists/list");
            foreach (XmlNode listNode in listNodes)
            {
                Sift.Resources.Settings.List list = new Sift.Resources.Settings.List();
                list.Id = new Guid(listNode.Attributes["Id"].InnerText);
                list.Content = (Sift.Resources.Types.ContentType)Enum.Parse(typeof(Sift.Resources.Types.ContentType), listNode.Attributes["Content"].InnerText);
                list.MatchAction = (Sift.Resources.Types.MatchActionType)Enum.Parse(typeof(Sift.Resources.Types.MatchActionType), listNode.Attributes["MatchAction"].InnerText);
                list.Description = listNode.Attributes["Description"].InnerText;
                list.Enabled = Convert.ToBoolean(listNode.Attributes["Enabled"].InnerText);  
                              
                listGroup.Lists.Add(list);
            }

            return listGroup;
        }

        private TreeNode GetTreeNodeFromCategory(XmlNode category)
        {
            TreeNode newNode = new TreeNode();            
            
            newNode.Text = category.Attributes["Description"].InnerText;
            newNode.Checked = Convert.ToBoolean(category.Attributes["IsRecommended"].InnerText);            
            newNode.ImageIndex = 0;
            newNode.SelectedImageIndex = 0;
            newNode.Tag = GetListGroupFromCategory(category);

            XmlNodeList categoryNodes = category.SelectNodes("./listGroups/listGroup");

            foreach (XmlNode categoryNode in categoryNodes)
            {
                TreeNode childNode = GetTreeNodeFromCategory(categoryNode);

                // only show the category to add if the user has not already added it
                if (Sift.Resources.Settings.ListSettings.Settings.GetByListGroupId(((Sift.Resources.Settings.ListGroup)childNode.Tag).Id) == null)
                    newNode.Nodes.Add(childNode);
            }

            return newNode;
        }
        #endregion

        private void UpdateSelectedNode()
        {
            if (treeViewCategories.SelectedNode != null)
            {
                Sift.Resources.Settings.ListGroup listGroup = (Sift.Resources.Settings.ListGroup)treeViewCategories.SelectedNode.Tag;

                lblCategoryDescription.Text = listGroup.Details;
            }
        }


        /// <summary>
        /// Determines if a category should be updated from the list updated server.
        /// </summary>
        /// <param name="originalNode">The original settings before user interaction.</param>
        /// <param name="currentNode">The potentially changed settings after user interaction.</param>        
        /// <returns>A list of categories to update from the list update server.</returns>
        private Sift.Resources.Settings.ListGroupCollection GetUpdatesFromCategoryComparison(TreeNode originalNode, TreeNode currentNode)
        {
            Sift.Resources.Settings.ListGroupCollection listGroups = new Sift.Resources.Settings.ListGroupCollection();

            if (currentNode.Checked)
            {
                Sift.Resources.Settings.ListGroup listGroup = (Sift.Resources.Settings.ListGroup)currentNode.Tag;

                DateTime lastUpdated = new DateTime(2000, 1, 1);

                // TODO: determine the last time the category has been updated if the user 
                //       has previously selected this list                

                listGroups.Add(listGroup);

                // update any children that are selected
                for (int i = 0; i < currentNode.Nodes.Count; i++)
                {
                    Sift.Resources.Settings.ListGroupCollection subListGroups = GetUpdatesFromCategoryComparison(originalNode.Nodes[i], currentNode.Nodes[i]);
                    foreach (Sift.Resources.Settings.ListGroup subListGroup in subListGroups)
                        listGroups.Add(subListGroup);
                }
                
            }

            return listGroups;
        }

        public void AddNewListGroups(TreeNode node)
        {
            AddNewListGroups(node, null);
        }

        public void AddNewListGroups( TreeNode node, Sift.Resources.Settings.ListGroup parentGroup)
        {            
            if (node.Checked)
            {
                Sift.Resources.Settings.ListGroup listGroup = (Sift.Resources.Settings.ListGroup)node.Tag;

                if(Sift.Resources.Settings.ListSettings.Settings.GetByListGroupId(listGroup.Id) == null)
                {
                    if(parentGroup == null)
                        Sift.Resources.Settings.ListSettings.Settings.ListGroups.Add(listGroup);
                    else
                        parentGroup.ListGroups.Add(listGroup);

                    Program.HasListSettingsChanged = true;
                }                
                             
                // update any children that are selected
                for (int i = 0; i < node.Nodes.Count; i++)
                    AddNewListGroups(node.Nodes[i], listGroup);
            }           
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Applies any changes made by the user to their requested category listings.
        /// </summary>
        public void ApplyChanges()
        {
            Sift.Resources.Settings.ListGroupCollection listGroups = new Sift.Resources.Settings.ListGroupCollection();

            // loop through each category and compare if anything has changed
            // if it has, get the list updates for that category
            for (int i = 0; i < originalCategories.Length; i++)
            {
                Sift.Resources.Settings.ListGroupCollection subListGroups = GetUpdatesFromCategoryComparison(originalCategories[i], currentCategories[i]);

                AddNewListGroups(currentCategories[i]);

                foreach(Sift.Resources.Settings.ListGroup subListGroup in subListGroups)
                    listGroups.Add(subListGroup);
            }                  

            ListUpdater listUpdater = new ListUpdater();
            //try
            {
                listUpdater.ShowDialog(listGroups);
            }
            //catch
            //{
            //    listUpdater.Close();
            //    MessageBox.Show("Internet connection lost.");
            //}
        }
        #endregion

        #region CONTROL EVENTS
        //private void treeViewCategories_DrawNode(object sender, DrawTreeNodeEventArgs e)
        //{
        //    // hide the checkboxes for any node with child nodes
        //    if (e.Node.Nodes.Count > 0)
        //    {
        //        Color backColor, foreColor;

        //        if ((e.State & TreeNodeStates.Selected) == TreeNodeStates.Selected)
        //        {
        //            backColor = SystemColors.Highlight;
        //            foreColor = SystemColors.HighlightText;
        //        }

        //        else if ((e.State & TreeNodeStates.Hot) == TreeNodeStates.Hot)
        //        {
        //            backColor = SystemColors.HotTrack;
        //            foreColor = SystemColors.HighlightText;
        //        }
        //        else
        //        {
        //            backColor = e.Node.BackColor;
        //            foreColor = e.Node.ForeColor;
        //        }

        //        using (SolidBrush brush = new SolidBrush(backColor))
        //        {
        //            e.Graphics.FillRectangle(brush, e.Node.Bounds);
        //        }                                               

        //        TextRenderer.DrawText(e.Graphics, e.Node.Text, treeViewCategories.Font, e.Node.Bounds, foreColor, backColor);

        //        if ((e.State & TreeNodeStates.Focused) == TreeNodeStates.Focused)
        //        {
        //            ControlPaint.DrawFocusRectangle(e.Graphics, e.Node.Bounds, foreColor, backColor);
        //        }

        //        e.DrawDefault = false;
        //    }
        //    else
        //    {
        //        e.DrawDefault = true;
        //    }
        //}

        private void treeViewCategories_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            treeViewCategories.SelectedNode = e.Node;
            UpdateSelectedNode();
        }
        #endregion

        private void SetTreeViewNodeCheckRecursive(TreeNodeCollection nodes, bool isChecked)
        {
            foreach (TreeNode node in nodes)
            {
                node.Checked = isChecked;
                SetTreeViewNodeCheckRecursive(node.Nodes, isChecked);
            }
        }

        private void treeViewCategories_AfterCheck(object sender, TreeViewEventArgs e)
        {
            TreeNode node = e.Node;

            foreach (TreeNode childNode in node.Nodes)
            {
                childNode.Checked = node.Checked;
            }
        }

    }
}
