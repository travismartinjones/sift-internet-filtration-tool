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

using System.Net;
using NUnit.Framework;

namespace Sift.Test
{
    [TestFixture]
    public class List
    {
        private System.Collections.Generic.List<Resources.Settings.List> GetListsByListGroupActionType(Resources.Settings.ListGroup listGroup, Resources.Types.MatchActionType action)
        {
            System.Collections.Generic.List<Resources.Settings.List> lists = new System.Collections.Generic.List<Sift.Resources.Settings.List>();

            foreach (Resources.Settings.List list in listGroup.Lists)
                lists.Add(list);

            foreach (Resources.Settings.ListGroup subListGroup in listGroup.ListGroups)
                lists.AddRange(GetListsByListGroupActionType(subListGroup, action));

            return lists;
        }

        private System.Collections.Generic.List<Resources.Settings.List> GetListsByActionType(Resources.Types.MatchActionType action)
        {
            System.Collections.Generic.List<Resources.Settings.List> lists = new System.Collections.Generic.List<Resources.Settings.List>();

            foreach (Resources.Settings.ListGroup listGroup in Resources.Settings.ListSettings.Settings.ListGroups)
                lists.AddRange(GetListsByListGroupActionType(listGroup, Sift.Resources.Types.MatchActionType.Block));

            return lists;
        }

        private bool IsURLAccessible(string urlPart, Resources.Types.ContentType contentType)
        {
            bool isAccessible = true;

            WebRequest request = WebRequest.Create("http://" + urlPart);
            //request.Timeout = 1000;
            try
            {
                WebResponse response = request.GetResponse();
            }
            catch // (WebException ex)
            {
                //System.Console.WriteLine(ex.Message);
                isAccessible = false;
            }

            return isAccessible;
        }        

        [Test]
        public void BlackListTest()
        {
            System.Collections.Generic.List<Resources.Settings.List> blacklists = GetListsByActionType(Resources.Types.MatchActionType.Block);

            foreach (Resources.Settings.List blacklist in blacklists)
            {
                System.Console.WriteLine("Processing Blacklist ID " + blacklist.Id + " Path "  + blacklist.Path );
                System.IO.StreamReader fileReader = null;

                Assert.IsTrue(System.IO.File.Exists(blacklist.Path));

                fileReader = new System.IO.StreamReader(blacklist.Path);

                string line = fileReader.ReadLine();

                int lineNumber = 0;

                while (line != null)
                {
                    lineNumber++;

                    if (lineNumber % 1000 == 0)
                        System.Console.WriteLine("Processed " + lineNumber + " entries");

                    bool isAccessible = IsURLAccessible(line, blacklist.Content);

                    if(isAccessible)
                        System.Console.WriteLine("Error " + blacklist.Content.ToString() + " [" + line + "] should be blocked.");

                    Assert.IsFalse(isAccessible);

                    line = fileReader.ReadLine();
                }

                fileReader.Close();
            }
        }

        [Test]
        public void WhiteListTest()
        {
            System.Collections.Generic.List<Resources.Settings.List> whitelists = GetListsByActionType(Resources.Types.MatchActionType.Allow);

            foreach (Resources.Settings.List whitelist in whitelists)
            {
                System.Console.WriteLine("Processing whitelist ID " + whitelist.Id + " Path " + whitelist.Path);
                System.IO.StreamReader fileReader = null;

                Assert.IsTrue(System.IO.File.Exists(whitelist.Path));

                fileReader = new System.IO.StreamReader(whitelist.Path);

                string line = fileReader.ReadLine();

                while (line != null)
                {
                    bool isAccessible = IsURLAccessible(line, whitelist.Content);

                    if (!isAccessible)
                        System.Console.WriteLine("Error " + whitelist.Content.ToString() + " [" + line + "] should be allowed.");

                    Assert.IsTrue(isAccessible);

                    line = fileReader.ReadLine();
                }

                fileReader.Close();
            }
        }
    }
}
