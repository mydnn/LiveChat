using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using DotNetNuke.ComponentModel.DataAnnotations;

namespace MyDnn.Modules.Support.LiveChats.ViewModels
{
    public class DepartmentAgentViewModel
    {
        public int DepartmentID { get; set; }
        public int AgentID { get; set; }
        public int UserID { get; set; }
        public string DisplayName { get; set; }
        public string ProfilePic
        {
            get
            {
                return DotNetNuke.Common.Globals.ResolveUrl("~/dnnimagehandler.ashx?mode=profilepic&w=35&userid=" + this.UserID);
            }
        }
    }
}