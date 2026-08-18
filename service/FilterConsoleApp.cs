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
using System.Text.RegularExpressions;
using System.Text;


namespace Sift
{
    /// <summary>
    /// Debugging console interface to the filter. Used in
    /// replacement of the service installer to provide an
    /// interractive console.
    /// </summary>
    class FilterConsoleApp
    {
        
        #region MEMBERS

        public Filter filter;

        #endregion MEMBERS

        # region CONSTRUCTOR

        public FilterConsoleApp()
        {

        }

        # endregion CONSTRUCTOR

        static void Main(string[] args)
        {
            FilterConsoleApp frontEnd = new FilterConsoleApp();
            string response;
            
            frontEnd.filter = new Filter();
            
            if (frontEnd.filter.OpenConnection())
            {
                response = "";
                frontEnd.filter.LoadConfig();                
                do
                {
                    switch (response)
                    {
                        case "reload all":
                            frontEnd.filter.LoadConfig();
                            break;

                        case "reload adapter":
                            frontEnd.filter.LoadAdapters();
                            break;

                        case "reload lists":
                            frontEnd.filter.LoadLists();
                            break;                                          
                    }
                    if (Regex.IsMatch(response, "^search"))
                    {
                        Match m = Regex.Match(response, "search (.*) (.*)");

                        if (m.Success)
                        {
                            switch (m.Result("$1"))
                            {
                                case "ip":
                                    Console.WriteLine("Searching IP: " + m.Result("$2"));
                                    if (frontEnd.filter.SearchIP(m.Result("$2")))
                                    {
                                        Console.WriteLine("Found");
                                    }
                                    else
                                    {
                                        Console.WriteLine("Not Found");
                                    }
                                    break;
                                case "url":
                                    Console.WriteLine("Searching URL: " + m.Result("$2"));
                                    if (frontEnd.filter.SearchURL(m.Result("$2")))
                                    {
                                        Console.WriteLine("Found");
                                    }
                                    else
                                    {
                                        Console.WriteLine("Not Found");
                                    }
                                    break;
                                case "domain":
                                    Console.WriteLine("Searching Domain: " + m.Result("$2"));
                                    if (frontEnd.filter.SearchDomain(m.Result("$2")))
                                    {
                                        Console.WriteLine("Found");
                                    }
                                    else
                                    {
                                        Console.WriteLine("Not Found");
                                    }
                                    break;
                            }
                        }
                    }
                    Console.Write("Enter command [reload all/adapters/lists; load new; search ip/url/domain value; quit] : ");
                    response = Console.ReadLine();
                } while (response != "quit");                
                frontEnd.filter.CloseConnection();
            }
            else
            {
                Console.WriteLine("Error opening driver\n");
            }
            return;
        }

    }
}
