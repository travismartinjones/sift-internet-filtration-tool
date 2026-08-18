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

namespace Sift.Resources
{
    public class Types
    {
        #region ENUMS

        public enum ContentType
        {
            IP,
            URL,
            Domain
        }

        public enum MatchActionType
        {
            Allow,
            Block,
            Log
        }

        public enum DefaultActionType
        {
            Drop,
            Allow,
            Log
        }

        public enum LogResourceType
        {
            File,
            EventLog
        }

        public enum LogType
        {
            Information,
            Warning,
            Error
        }

        public enum LogGroupType
        {
            DriverDebug,
            ServiceDebug,
            FilterDecision,
            FilterMatch,
            FilterBlock,
            FilterAllow
        }

        public enum LogDetailType
        {
            None = 1,
            Minimal = 2,
            Moderate = 3,
            Verbose = 4
        }

        public enum ListUpdateType
        {
            Add,
            Remove
        }

        public enum ListUpdateServiceType
        {
            Category,
            List
        }
        #endregion
    }
}
