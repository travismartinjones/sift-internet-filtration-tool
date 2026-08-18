// WizardSheet.cs: Contributed by Shawn Wildermuth [swildermuth@adoguy.com]
// Classes for creation of Wizard-like User Interfaces
#region Copyright © 2002-2007 The Genghis Group
/*
   * This software is provided 'as-is', without any express or implied warranty.
   * In no event will the authors be held liable for any damages arising from the
   * use of this software.
   * 
   * Permission is granted to anyone to use this software for any purpose,
   * including commercial applications, subject to the following restrictions:
   * 
   * 1. The origin of this software must not be misrepresented; you must not claim
   * that you wrote the original software. If you use this software in a product,
   * an acknowledgment in the product documentation is required, as shown here:
   * 
   * Portions Copyright © 2002-2007 The Genghis Group (http://www.genghisgroup.com/).
   * 
   * 2. No substantial portion of the source code of this library may be redistributed
   * without the express written permission of the copyright holders, where
   * "substantial" is defined as enough code to be recognizably from this library. 
  */
#endregion

#region Features
/*
 * -Creation of Wizards-type User Interfaces with individual pages and groups
 * -Allows pages to be any object that derives from System.Windows.Forms.Control
 * -Handles First/Last/Finish buttons.
 * -Resizes dialog based on the pages.
 * -Base WizardPage class to allow Windows Forms Inheritance for Wizard Pages.
 * -Allows forming groups of wizard pages and switching of them on the fly.
 * -Autonumbers pages to provide feedback to the user of their location in the wizard.
 * -Supports CanFinish to determine whether a wizard has all the required elements yet.
 */
#endregion
#region Limitations
/*
 * 
*/
#endregion
#region History
/*
 * 6/22/2002:
 * -Initial Development
 * 8/31/2002
 * -Brian Hormann changed buttons to Protected to support changing the buttons
 * -Changed name of helpButton to hlpButton to make it CLS Compliant
 * -Brian Hormann Added Support for Design-Time editing of Inherited classes by 
 *  only running code in Form.Load when not in Design mode
 * -Keith Brown added support for CanFinish to the wizard and wizard pages.
*/
#endregion

#region Using directives

using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using IDesignerHost = System.ComponentModel.Design.IDesignerHost;
using System.Windows.Forms;
using System.Drawing;
using System.Text;
using Genghis.Windows.Forms;

#endregion

namespace Genghis.Windows.Forms
{
//-----------------------------------------------------------------------------
//<filedescription file="WizardSheet.cs" company="Microsoft">
//  <copyright>
//     Copyright (c) 2004 Microsoft Corporation.  All rights reserved.
//  </copyright>
//  <purpose>
//  Contains sample which shows the implementation and use of
//  WindowSerializer
//  </purpose>
//  <notes>
//  </notes>
//</filedescription>                                                                
//-----------------------------------------------------------------------------

    public partial class WizardSheet : Form
    {
        #region Constructor

        public WizardSheet()
        {
            InitializeComponent();

            // Initialize the window serializer.  This should be done before the
            // form is displayed.
        }
        /// <summary>
        /// Constructs a new Wizard Sheet specifying the name
        /// of the wizard (for the Titlebar).
        /// </summary>
        /// <param name="titleBarText">Text for the Titlebar</param>
        /// <example>To create a new WizardSheet object while specifying the dialog title:
        /// <code>
        /// Wizard sheet = new WizardSheet("My Wizard");
        /// </code>
        /// </example>
        public WizardSheet(string titleBarText)
        {
            // Designer Support
            InitializeComponent();

            // Set the titlebar text
            _TitleBarText = titleBarText;
        }
        #endregion

        #region public Interface
        /// <summary>
        /// Adds a new page
        /// </summary>
        /// <param name="page">The WizardPage object to add to the collection of pages.</param>
        public virtual void AddPage(WizardPage page)
        {
            AddPage(page as Control);
        }

        /// <summary>
        /// Adds a new Page, assigning it to a single group.
        /// </summary>
        /// <param name="page">The WizardPage object to add to the collection of pages.</param>
        /// <param name="group">The name of the group to add this page to.</param>
        public virtual void AddPage(WizardPage page, string group)
        {
            AddPage(page as Control, group);
        }

        /// <summary>
        /// Adds a new Page, assigning it to multiple groups.
        /// </summary>
        /// <param name="page">The WizardPage object to add to the collection of pages.</param>
        /// <param name="groups">An array of strings specifying which groups to add the pages to.</param>
        public virtual void AddPage(WizardPage page, string[] groups)
        {
            AddPage(page as Control, groups);
        }

        /// <summary>
        /// Add a page to the default group.
        /// </summary>
        /// <param name="page">The Control object to add to the collection of pages.</param>
        public virtual void AddPage(Control page)
        {
            // Add to default Group
            AddPage(page, new string[] { DEFAULTGROUP });
        }

        /// <summary>
        /// Adds a page to the Wizard, specifying the specific group to add it to.
        /// </summary>
        /// <param name="page">The Control object to add to the collection of pages.</param>
        /// <param name="group">The name of the group to add this page to.</param>
        public virtual void AddPage(Control page, string group)
        {
            AddPage(page, new string[] { group });
        }

        /// <summary>
        /// Adds a page to the Wizard, specifying the groups in which this new page will belong.
        /// </summary>
        /// <param name="page">The Control object to add to the collection of pages.</param>
        /// <param name="groups">An array of strings specifying which groups to add the pages to.</param>
        public virtual void AddPage(Control page, string[] groups)
        {
            // Check to make sure there is at least one group
            if (groups == null || groups.Length == 0)
            {
                throw new ArgumentNullException("Cannot send in null groups or empty group collection.  Use AddPage(page) syntax to use default group");
            }

            // If this is the first page to be added, figure out the
            // Initial Group Name
            if (_Pages.Count == 0)
            {
                _CurrentGroupName = groups[0];
            }

            // Add it to the page collection
            _Pages.Add(new PageObject(page, groups));

            if (_isLoaded)
            {
                // Calculate the Current Group
                CalculatePageGroup();

                // Resize
                ResizeDialog();
            }
        }

        /// <summary>
        /// Gets the page in the current group by ordinal number.
        /// </summary>
        /// <param name="item">The zero-based index to a page in the current group.</param>
        public Control GetPage(int item)
        {
            if (_CurrentGroup.Count <= item) throw new ArgumentException("Item number is greater than page count", "item");
            return _CurrentGroup[item] as Control;
        }

        public WizardPageCollection Pages
        {
            get
            {
                return _CurrentGroup;
            }
        }

        /// <summary>
        /// Determines whether a wizard can successfully Finish itself yet.
        /// </summary>
        /// <returns>True if wizard can be completed.</returns>
        public bool CanFinish()
        {
            return CanFinish(false);
        }

        /// <summary>
        /// Determines whether a wizard can successfully Finish itself yet.
        /// </summary>
        /// <param name="showUI">Boolean that determines if the first WizardPage that needs to be satisfied.</param>
        /// <returns>True if wizard can be completed.</returns>
        public bool CanFinish(bool showUI)
        {
            // ask all pages in current group if they can proceed
            foreach (object o in _CurrentGroup)
            {
                WizardPage validatablePage = o as WizardPage;
                if (null != o && !validatablePage.CanProceed(false))
                {
                    if (showUI)
                    {
                        // find the first page that's not ready
                        // show it, then let the page put up its UI
                        _CurrentPage = _CurrentGroup.IndexOf(validatablePage);
                        ShowPage();
                        validatablePage.CanProceed(true);
                    }

                    return false;
                }
            }
            return true;
        }

        #endregion

        #region public Members
        /// <summary>
        /// The current group of wizard pages shown in the wizard.
        /// </summary>
        /// <value>A string that matches groups that you specified while adding
        /// wizard pages.</value>
        public virtual string CurrentGroup
        {
            get
            {
                return _CurrentGroupName;
            }
            set
            {
                _CurrentGroupName = value;
                this.CalculatePageGroup();
                if (_CurrentGroup.Count == 0)
                {
                    throw new ArgumentOutOfRangeException("CurrentGroup", value, "Group does not exist");
                }

                if (_isLoaded)
                {
                    // Cache the current Page
                    Control currentPage = pnlPage.Controls[0] as Control;

                    // Set the current page if out of scope
                    if (_CurrentPage > _CurrentGroup.Count)
                    {
                        _CurrentPage = _CurrentGroup.Count - 1;
                        ShowPage();
                    }
                    else if (_CurrentGroup[_CurrentPage] != currentPage)
                    {
                        // Set Minimum Size
                        this.Size = this.MinimumSize;

                        // Resize
                        ResizeDialog();
                    }

                    // Re-show title (to fix page counts)
                    ShowTitleBar();

                    // Recalc the prev/next buttons to allow for a group of 1
                    SetActiveButtons();
                }

            }
        }

        /// <summary>
        /// The text for the titlebar of the wizard.
        /// </summary>
        public new string Text
        {
            get
            {
                return _TitleBarText;
            }
            set
            {
                _TitleBarText = value;
            }
        }

        /// <summary>
        /// The number of pages in the current group.
        /// </summary>
        public int PageCount
        {
            get
            {
                CalculatePageGroup();
                return _CurrentGroup.Count;
            }
        }

        /// <summary>
        /// Shows or hides the help button.
        /// </summary>
        public bool ShowHelpButton
        {
            get
            {
                return this.btnHelp.Visible;
            }
            set
            {
                this.btnHelp.Visible = value;
            }
        }

        /// <summary>
        /// Enables or disables the Finish button.  Usually called from
        /// the pages of the Wizard Page to ensure that some pages are viewed.
        /// </summary>
        public bool EnableFinishButton
        {
            get
            {
                return this.btnFinish.Enabled;
            }
            set
            {
                this.btnFinish.Enabled = value;
            }
        }
        #endregion

        #region protected Implementation
        /// <summary>
        /// Shows the current page in the wizard.
        /// </summary>
        protected virtual void ShowPage()
        {
            // Check for empty collection
            System.Diagnostics.Debug.Assert(_Pages.Count > 0, "Page Collection Empty");

            // Show the Page
            if (CurrentPage != null)
            {
                if (!pnlPage.Controls.Contains(CurrentPage))
                {
                    pnlPage.Controls.Clear();
                    pnlPage.Controls.Add(CurrentPage);
                    CurrentPage.Top = 0;
                    CurrentPage.Left = 0;
                    CurrentPage.Dock = DockStyle.Fill;
                }
            }

            // Show title
            ShowTitleBar();

            // Set the Active Buttons
            SetActiveButtons();

            // Activate the Page
            CurrentPage.Focus();

            // Repaint
            this.Refresh();

        }

        /// <summary>
        /// Shows the title of the page and the page numbers on the TitleBar
        /// </summary>
        protected virtual void ShowTitleBar()
        {
            // Show Title
            base.Text = string.Format("{0} ({1} of {2})", _TitleBarText, _CurrentPage + 1, _CurrentGroup.Count);
        }

        /// <summary>
        /// Enables/Disables next/prev buttons based on the pages
        /// </summary>
        protected virtual void SetActiveButtons()
        {
            // Enable/Disable next/prev buttons
            btnLast.Enabled = (_CurrentPage != 0);
            btnNext.Enabled = (_CurrentPage + 1 != _CurrentGroup.Count);

        }

        /// <summary>
        /// resizes the dialog based on the pages in the current group.
        /// </summary>
        protected virtual void ResizeDialog()
        {
            // Go through each page in the group and resize the containing dialog
            foreach (Control page in _CurrentGroup)
            {
                Size size = ClientSize;
                if (size.Width < page.Width)
                {
                    size.Width = page.Width;
                    pnlPage.Width = page.Width;
                }
                if (size.Height < page.Height + pnlBottom.Height)
                {
                    size.Height = page.Height + pnlBottom.Height;
                }
                ClientSize = size;
            }
        }

        /// <summary>
        /// Virtual method called with the help button is pressed.  The default
        /// behavior is to do nothing.
        /// </summary>
        protected virtual void OnHelp()
        {
        }
        #endregion

        #region private Methods
        /// <summary>
        /// Returns the current page in the current group.
        /// </summary>
        /// <returns>The current page as a Control object.</returns>
        private Control CurrentPage
        {
            get
            {
                return _CurrentGroup[_CurrentPage] as Control;
            }
        }

        /// <summary>
        /// Calculates the pages for the current page group.
        /// </summary>
        private void CalculatePageGroup()
        {
            // Current Page 
            int currentPointer = 0;

            // Clear the current Group
            _CurrentGroup.Clear();

            // Construct the Current Group Array
            foreach (PageObject obj in _Pages)
            {
                if (obj.Groups.Contains(_CurrentGroupName))
                {
                    _CurrentGroup.Add(obj.Page);
                    ++currentPointer;
                }
            }

            // If we can't find any pages
            if (currentPointer == 0)
            {
                // Throw an exception
                throw new Exception(string.Format("The {0} group contains no pages, please set the CurrentGroup of the WizardSheet", _CurrentGroupName));
            }
        }
        #endregion

        #region Private Fields
        // Default Group
        private const string DEFAULTGROUP = "__DefaultGroup";

        // List of Page Structures
        private WizardPageCollection _Pages = new WizardPageCollection();

        // Current Group of Pages
        private WizardPageCollection _CurrentGroup = new WizardPageCollection();

        // Page Cursor
        private int _CurrentPage = 0;

        // Current Group
        private string _CurrentGroupName = DEFAULTGROUP;

        // Titlebar Text
        private string _TitleBarText = "WizardSheet";


        // Boolean to indicate whether WizardSheet_Load has happened yet
        private bool _isLoaded = false;
        #endregion

        #region Event Handlers
        private void WizardSheet_Load(object sender, EventArgs e)
        {
            // Only setup the page in Design Mode
            if (this.DesignMode == false)
            {
                // Calculate the Group
                CalculatePageGroup();

                // Resize the Dialog
                ResizeDialog();

                // Throw the isloaded flag
                _isLoaded = true;

                // Show the current Page
                ShowPage();
            }
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            OnHelp();
        }

        private void btnLast_Click(object sender, EventArgs e)
        {
            _CurrentPage--;
            ShowPage();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            _CurrentPage++;
            ShowPage();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnFinish_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void WizardSheet_HelpRequested(object sender, System.Windows.Forms.HelpEventArgs hlpevent)
        {
            this.btnHelp_Click(sender, hlpevent as EventArgs);
        }
        #endregion
    }


    /// <summary>
    /// A simple structure to hold the page and groups that the page belongs to.
    /// </summary>
    internal class PageObject
    {
        #region Constructor
        public PageObject(Control page, string[] groups)
        {
            Page = page;
            Groups = new StringCollection();
            Groups.AddRange(groups);
        }
        #endregion

        #region Public Members
        public Control Page;
        public StringCollection Groups;
        #endregion
    }

    /// <summary>
    /// A User Control that provides a base class to derive from to create 
    /// standard looking wizard pages.  It supports a top pane that contains a 
    /// title and description of a page.
    /// </summary>
    public partial class WizardPage : System.Windows.Forms.UserControl
    {
        #region Constructor

        /// <summary>
        /// The default constructor.
        /// </summary>
        public WizardPage()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            // Set the title and description
            lblTitle.Text = string.Format("{0} Wizard", this.GetType().Name);
            lblDescription.Text = string.Format("The {0} Wizard.", this.GetType().Name);
        }

        /// <summary>
        /// Creates a new wizard page, specifying the title and description.
        /// </summary>
        /// <param name="title">The Title to be shown in the top pane of the page.</param>
        /// <param name="description">The description to be shown in the top pane of the page.</param>
        public WizardPage(string title, string description)
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            // Set the title and description
            lblTitle.Text = title;
            lblDescription.Text = description;
        }

        /// <summary>
        /// The current WizardSheet that the WizardPage is shown within.
        /// </summary>
        /// <value>A reference to the WizardSheet</value>
        public WizardSheet WizardSheet 
        {
            get
            {
                return this.ParentForm as WizardSheet;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Determines if the Wizard can pass this page.  This is called from WizardSheet.CanFinish() to see if all pages are
        /// satisfied.
        /// </summary>
        /// <param name="showUI">Boolean that indicates whether the WizardPage should show itself if the wizard cannot proceed.</param>
        /// <returns>Boolean that indicates whether a page can proceed or not.</returns>
        public virtual bool CanProceed(bool showUI)
        {
            return true;
        }

        #endregion

        #region Public Members

        /// <summary>
        /// Sets the title of the page shown within the header of the 
        /// page.
        /// </summary>
        public string PageTitle
        {
            set
            {
                lblTitle.Text = value;
            }
        }

        /// <summary>
        /// Sets the description of the page shown within the header of the page.
        /// </summary>
        public string PageDescription
        {
            set
            {
                lblDescription.Text = value;
            }
        }

        #endregion
    }

    /// <summary>
    /// Strongly Typed Collection for WizardPages (Control).
    /// </summary>
    public class WizardPageCollection : ArrayList
    {
        #region Constructor
        /// <summary>
        /// Empty Constructor that only calls 
        /// </summary>
        internal WizardPageCollection() : base()
        {
        }
        #endregion

        #region Public Members
        /// <summary>
        /// Strongly typed Indexer to return a WizardPage (as a Control object).
        /// </summary>
        public new Control this[int index]
        {
            get
            {
                return base[index] as Control;
            }
            set
            {
                base[index] = value;
            }
        }

        /// <summary>
        /// Strongly typed Indexer to return a WizardPage (as a Control object).  
        /// This indexer will retrieve the WizardPage based on the Name of the
        /// Control.
        /// </summary>
        public Control this[string name]
        {
            get
            {
                foreach (object obj in this)
                {
                    Control control = obj as Control;

                    if (control != null && control.Name == name)
                    {
                        return control;
                    }
                }

                // If not found, return null
                return null;
            }
        }

        /// <summary>
        /// Adds a new WizardPage to the collection.
        /// </summary>
        /// <param name="value">The new Wizard Page to add.</param>
        /// <returns>The ordinal number of the new page in the collection.</returns>
        public int Add(Control value)
        {
            return base.Add(value);
        }


        /// <summary>
        /// Strongly typed version of the Contains method to 
        /// test if a Wizard Page is in the collection.
        /// </summary>
        /// <param name="item">The Wizard Page to test.</param>
        /// <returns>True if the Wizard Page exists in the collection.</returns>
        public bool Contains(Control item)
        {
            return base.Contains(item);
        }

        /// <summary>
        /// Removes a specific Wizard Page from the control.
        /// </summary>
        /// <param name="control">The control to remove.</param>
        public void Remove(Control control)
        {
            base.Remove(control);
        }
        #endregion
    }
}