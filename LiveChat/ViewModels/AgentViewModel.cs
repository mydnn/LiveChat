using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using DotNetNuke.ComponentModel.DataAnnotations;

namespace MyDnn.Modules.Support.LiveChat.ViewModels
{
    public class AgentViewModel
    {
        public int AgentID { get; set; }
        public int UserID { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public bool Enabled { get; set; }
        public int Priority { get; set; }
        public string ProfilePic
        {
            get
            {
                return DotNetNuke.Common.Globals.ResolveUrl("~/dnnimagehandler.ashx?mode=profilepic&userid=" + this.UserID);
            }
        }
        public IEnumerable<string> Departments { get; set; }
    }
}