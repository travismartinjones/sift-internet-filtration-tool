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
using System.Diagnostics;
using System.ServiceProcess;
// used for Marhsal Win32Error test - remove later
using System.Runtime.InteropServices;

namespace Sift
{

    /// <summary>
    /// Class used to build the filter application as a system
    /// service. Outputs filter.exe into the /bin subdirector.
    /// </summary>
    class FilterService : System.ServiceProcess.ServiceBase
    {

        #region MEMBERS

        public System.ServiceProcess.ServiceController serviceController;

        private System.ComponentModel.IContainer components;

        private Filter filter;       

        #endregion MEMBERS

        # region CONSTRUCTOR

        /// <summary>
        /// Initializes a new instance of the FilterService class.
        /// </summary>
        public FilterService()
        {
            // initialize service components
            components = null;
            InitializeComponent();
        }

        # endregion CONSTRUCTOR

        static void Main()
        {
            Resources.Globals.Log.Write("Service Main", Resources.Types.LogType.Information, Resources.Types.LogGroupType.ServiceDebug, Resources.Types.LogDetailType.Verbose);
            // reference and start the service
            System.ServiceProcess.ServiceBase[] servicesToRun;
            servicesToRun = new System.ServiceProcess.ServiceBase[] 
                            { new FilterService() };

            try
            {
                System.ServiceProcess.ServiceBase.Run(servicesToRun);
            }
            catch(Exception ex)
            {
                Resources.Globals.Log.Write(ex.Message, Sift.Resources.Types.LogType.Error, Sift.Resources.Types.LogGroupType.ServiceDebug, Sift.Resources.Types.LogDetailType.None);
                if (ex.StackTrace != null)
                    Resources.Globals.Log.Write(ex.StackTrace, Sift.Resources.Types.LogType.Error, Sift.Resources.Types.LogGroupType.ServiceDebug, Sift.Resources.Types.LogDetailType.None);
                if(ex.InnerException != null)
                    Resources.Globals.Log.Write(ex.InnerException.Message, Sift.Resources.Types.LogType.Error, Sift.Resources.Types.LogGroupType.ServiceDebug, Sift.Resources.Types.LogDetailType.None);
            }
        }

        /// <summary>
        /// Initializes the service controller information. Updates
        /// user viewable information such as the service name, as
        /// well as the service connection string.
        /// </summary>
        private void InitializeComponent()
        {
            Resources.Globals.Log.Write("Initialize Component", Resources.Types.LogType.Information, Resources.Types.LogGroupType.ServiceDebug, Resources.Types.LogDetailType.Verbose);
            this.serviceController = new System.ServiceProcess.ServiceController();
            // 
            // serviceController
            // 
            this.serviceController.ServiceName = "SiftFilter";
            // 
            // FilterService
            // 
            this.ServiceName = "Sift";

            this.filter = new Filter();
        }

        /// <summary>
        /// Cleans up any allocated components before service shutdown.
        /// </summary>
        /// <param name="disposing">If disposing is true and the components 
        /// are initialized, the components are disposed</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            
            base.Dispose(disposing);
        }

        /// <summary>
        /// Called when the system service is sent a Start
        /// service request.
        /// </summary>
        /// <param name="args">Command line arguments passed to
        /// the service</param>
        protected override void OnStart(string[] args)
        {            
            if (this.filter != null)
            {                
                if (this.filter.OpenConnection())
                {                                        
                    this.filter.LoadConfig();                    
                }
                else
                {
                    EventLog.WriteEntry("Sift", "Unable to open connection to Sift driver.");
                    this.Stop();
                }
            }
            else
            {
                this.Stop();
                EventLog.WriteEntry("Sift", "filter == null");
            }
        }

        /// <summary>
        /// Called when the system service is send a Stop
        /// service request.
        /// </summary>
        protected override void OnStop()
        {
            if (this.filter != null)
            {                
                EventLog.WriteEntry("Sift", "OnStop() CloseConnection()");
                this.filter.CloseConnection();
            }
            else
            {
                EventLog.WriteEntry("Sift", "OnStop() filter == null");
            }
        }

        /// <summary>
        /// Called when the system is send a custom service
        /// request.
        /// </summary>
        /// <param name="command">And integer between 128-256 indicating
        /// the specific custom request to execute.</param>
        protected override void OnCustomCommand(int command)
        {
            if (this.filter != null)
            {
                switch (command)
                {
                    // process a custom command
                    // valid range: 128-256

                    case Resources.Constants.CustomCommandLoadAdapters:
                        // the gui admin tool has changed
                        // the adapter settings, reload
                        // the XML file and update the adapters
                        this.filter.LoadAdapters();
                        Resources.Globals.Log.Write("LoadAdapters() OnCustomCommand()", Sift.Resources.Types.LogType.Information, Sift.Resources.Types.LogGroupType.ServiceDebug, Sift.Resources.Types.LogDetailType.Moderate);
                        break;

                    case Resources.Constants.CustomCommandLoadLists:
                        // the gui admin tool has changed
                        // a list entry, reload the 
                        // lists from the config file
                        this.filter.LoadLists();
                        Resources.Globals.Log.Write("LoadLists() OnCustomCommand()", Sift.Resources.Types.LogType.Information, Sift.Resources.Types.LogGroupType.ServiceDebug, Sift.Resources.Types.LogDetailType.Moderate);
                        break;
              
                    case Resources.Constants.CustomCommandDisable:
                        // disable the filtering fuctions
                        // of the filter service. sends an
                        // update to the filter driver to 
                        // allow all packets for any driver
                        this.filter.Disable();
                        Resources.Globals.Log.Write("Disable() OnCustomCommand()", Sift.Resources.Types.LogType.Information, Sift.Resources.Types.LogGroupType.ServiceDebug, Sift.Resources.Types.LogDetailType.Moderate);
                        break;

                    case Resources.Constants.CustomCommandEnable:
                        // re-enables the filtering functions
                        this.filter.Enable();
                        Resources.Globals.Log.Write("Enable() OnCustomCommand()", Sift.Resources.Types.LogType.Information, Sift.Resources.Types.LogGroupType.ServiceDebug, Sift.Resources.Types.LogDetailType.Moderate);
                        break;

                    case Resources.Constants.CustomCommandLoadListUpdates:
                        // called to load black/white list changes                        
                        this.filter.LoadListUpdates();
                        Resources.Globals.Log.Write("LoadListUpdates() OnCustomCommand()", Sift.Resources.Types.LogType.Information, Sift.Resources.Types.LogGroupType.ServiceDebug, Sift.Resources.Types.LogDetailType.Moderate);
                        break;
                }
            }       
        }
    }
}
