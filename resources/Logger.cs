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
using System.Text;
using System.Diagnostics; // for event viewer
using System.IO; // for write to file

namespace Sift.Resources
{
    public class Logger
    {
        #region MEMBERS

        private string _logLocation = Sift.Resources.Constants.InstallPath + @"logs\";
        private Sift.Resources.Types.LogResourceType _logResource = Sift.Resources.Types.LogResourceType.File;

        public List<Sift.Resources.Types.LogGroupType> LogGroups = new List<Sift.Resources.Types.LogGroupType>();

        private TextWriter _textWriter = null;
        private Sift.Resources.Types.LogDetailType _logDetailLevel = Sift.Resources.Types.LogDetailType.Verbose;
        private bool _isEnabled = true;
        private const int _maxLogEntriesPerFile = 50000;
        private int _currentLogEntryCount = 0;

        #endregion        

        #region PROPERTIES

        public string LogLocation
        {
            get { return _logLocation; }
            set { _logLocation = value; }
        }

        public Sift.Resources.Types.LogResourceType LogResource
        {
            get { return _logResource; }
            set { _logResource = value; }
        }

        public Sift.Resources.Types.LogDetailType LogDetailLevel
        {
            get { return _logDetailLevel; }
            set { _logDetailLevel = value; }
        }

        private TextWriter LogFile
        {
            get
            {
                if (_textWriter == null)
                {
                    // create the log directory if it doesn't exist
                    if (!Directory.Exists(_logLocation))
                        Directory.CreateDirectory(_logLocation);

                    string filename = DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss") + ".log";

                    _textWriter = new StreamWriter(_logLocation + filename, true);                    
                }

                return _textWriter;
            }
            set
            {
                _textWriter = value;
            }
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
        }

        #endregion

        #region CONSTRUCTOR DESTRUCTOR
        public Logger()
        {
            // by default, log anything
            LogGroups.Add(Types.LogGroupType.DriverDebug);
            LogGroups.Add(Types.LogGroupType.FilterAllow);
            LogGroups.Add(Types.LogGroupType.FilterBlock);
            LogGroups.Add(Types.LogGroupType.FilterDecision);
            LogGroups.Add(Types.LogGroupType.FilterMatch);
            LogGroups.Add(Types.LogGroupType.ServiceDebug);
        }

        ~Logger()
        {
        }
        #endregion

        #region METHODS

        public void Write(string message, Sift.Resources.Types.LogType type, Sift.Resources.Types.LogGroupType group, Sift.Resources.Types.LogDetailType detailLevel)
        {
            if (_isEnabled)
            {
                if (LogGroups.Contains(group) && (int)_logDetailLevel >= (int)detailLevel)
                {
                    string formattedMessage = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + " - " + type.ToString() + " - " + message;

                    switch (_logResource)
                    {
                        case Sift.Resources.Types.LogResourceType.File:
                            lock (LogFile)
                            {
                                LogFile.WriteLine(formattedMessage);
                                LogFile.Flush();

                                _currentLogEntryCount++;

                                if (_currentLogEntryCount >= _maxLogEntriesPerFile)
                                {
                                    // close out the current log file and force the creation of a new one
                                    LogFile.Close();
                                    LogFile = null;
                                    _currentLogEntryCount = 0;
                                }
                            }
                            break;
                        case Sift.Resources.Types.LogResourceType.EventLog:
                            EventLogEntryType logEntryType;

                            switch (type)
                            {
                                case Sift.Resources.Types.LogType.Information:
                                    logEntryType = EventLogEntryType.Information;
                                    break;
                                case Sift.Resources.Types.LogType.Warning:
                                    logEntryType = EventLogEntryType.Warning;
                                    break;
                                case Sift.Resources.Types.LogType.Error:
                                    logEntryType = EventLogEntryType.Error;
                                    break;
                                default:
                                    logEntryType = EventLogEntryType.Information;
                                    break;
                            }

                            EventLog.WriteEntry(Sift.Resources.Constants.ServiceName, formattedMessage, logEntryType);
                            break;
                    }

                    
                }
            }
        }

        public void Enable()
        {
            _isEnabled = true;
        }

        public void Disable()
        {
            _isEnabled = false;
        }

        #endregion
    }
}
