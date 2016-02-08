using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyDnn.Modules.Support.LiveChat.Models;
using DotNetNuke.Data;
using System.IO;

namespace MyDnn.Modules.Support.LiveChat.Components.Common
{
    internal class Common
    {
        public static Common Instance
        {
            get
            {
                return new Common();
            }
        }

        internal IEnumerable<UserView> GetUsersByDisplayName(int portalID, string name)
        {
            return DataContext.Instance().ExecuteQuery<UserView>(System.Data.CommandType.Text, string.Format("Select [UserID],[DisplayName] From [vw_Users] Where ([PortalID] is null or [PortalID] = {0}) and [DisplayName] like N'%{1}%'", portalID, name));
        }

        internal string GetUserDisplayName(int userID)
        {
            return DataContext.Instance().ExecuteScalar<string>(System.Data.CommandType.Text, string.Format("Select DisplayName From Users Where [UserID]={0}", userID));
        }

        internal static string GetFileContent(string filename)
        {
            StreamReader objStreamReader = default(StreamReader);
            objStreamReader = File.OpenText(filename);
            string template = objStreamReader.ReadToEnd();
            objStreamReader.Close();
            return template;
        }
    }
}