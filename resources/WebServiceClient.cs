using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Sift.Resources
{
    public class WebServiceClient
    {
        #region Unix DateTime Interaction
        public static DateTime ConvertFromUnixTimestamp(double timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(timestamp);
        }


        public static int ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = date - origin;
            return Convert.ToInt32(Math.Floor(diff.TotalSeconds));
        }
        #endregion

        #region Web Service Location Properties

        // this section will eventually be modified to pull from an xml file located
        // at sift.sourceforge.net, so that the web service location can be changed
        // to something faster and more dedicated, such as ASP.NET/IIS & MS-SQL 2005

        public static string ListGroupWebServiceURL
        {
            get
            {                
                return "http://sift.sourceforge.net/listupdates/updates.php?action=listGroups";
            }
        }

        public static string ListUpdateFileWebServiceURL
        {
            get
            {
                return "http://sift.sourceforge.net/listupdates/updates.php?action=listUpdateFile";
            }
        }

        public static string ListUpdatesXmlWebServiceURL
        {
            get
            {
                return "http://sift.sourceforge.net/listupdates/updates.php?action=listUpdateXML";
            }
        }

        #endregion

        #region List Categories

        public static XmlDocument GetAvailableListGroups()
        {
            string webServiceURL = ListGroupWebServiceURL;

            XmlDocument document = new XmlDocument();

            try
            {
                document.Load(XmlReader.Create(webServiceURL));
            }
            catch (System.Net.WebException)
            {
            }

            return document;
        }
        #endregion

        #region List Updates

        private static string GetListUpdatesRequestQueryString(Sift.Resources.Settings.ListCollection lists)
        {
            StringBuilder request = new StringBuilder();            

            for (int i = 1; i <= lists.Count; i++)
            {
                Sift.Resources.Settings.List list = lists[i - 1];

                request.Append("&id");
                request.Append(i);
                request.Append("=");
                request.Append(list.Id.ToString());

                request.Append("&dt");
                request.Append(i);
                request.Append("=");                
                request.Append(System.Web.HttpUtility.UrlEncode(list.LastUpdated.ToUniversalTime().ToString()));
                //request.Append(System.Web.HttpUtility.UrlEncode(new DateTime(2000, 12, 20).ToUniversalTime().ToString()));
            }

            return request.ToString();
        }
       
        public static System.IO.Stream GetListUpdateFileByList(Sift.Resources.Settings.List list, out long contentLength)
        {
            string webServiceURL = ListUpdateFileWebServiceURL;
            string listFileQueryString = "&id=" + list.Id.ToString();

            System.Net.WebRequest request = System.Net.HttpWebRequest.Create(webServiceURL + listFileQueryString);

            System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse();

            contentLength = response.ContentLength;

            return response.GetResponseStream();
        }

        public static XmlDocument GetListUpdatesXMLByLists(Sift.Resources.Settings.ListCollection lists)
        {
            string webServiceURL = ListUpdatesXmlWebServiceURL;
            string listUpdatesListIDQueryString = GetListUpdatesRequestQueryString(lists);
            
            XmlDocument document = new XmlDocument();

            try
            {
                document.Load(XmlReader.Create(webServiceURL + listUpdatesListIDQueryString));
            }
            catch (System.Net.WebException)
            {
            }

            return document;
        }

        #endregion
    }
}
